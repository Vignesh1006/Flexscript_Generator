// 13 - Excel Config Driver (v4, .xlsx). Two-way sync of object properties AND labels
//      with an Excel sheet laid out as a table: one row per object, one column per property / label.
//      Row 1:  Object | Type | MaxContent | ProcessTime | SetupTime | label:LoadingTime ...
//      Type (Processor, Queue, Source...) is for reference only - PUSH ignores it.
// MODE 0 = PULL   : writes current model values into the sheet (builds the columns for you)
// MODE 1 = DRYRUN : reads the sheet and reports what would change - nothing is written
// MODE 2 = PUSH   : applies the sheet to the model
// Blank cell = leave that value as is. Label columns start with  label:
// Properties set by custom code are written as (code) - they have no single value to edit.
// PUSH skips (code) cells; change those values where the code reads them (e.g. a Global Table).
// The workbook lives in the same folder as the model's .fsm file (save the model first).
// One-time setup: put an empty workbook named XLSX_NAME there, with a sheet named as in SHEET.
// Close the workbook in Excel before running. Reset the model before PULL.
// Always run DRYRUN before PUSH. Save the model first.
// FlexSim 2027: Script Console > paste > Execute.

// ---- settings ----
int    MODE  = 0;
string XLSX_NAME = "model_config.xlsx";   // file name only - the folder is the .fsm's folder
string SHEET = "Sheet1";            // letters, digits and _ only
Array  PULL_PROPS = ["MaxContent", "ProcessTime", "SetupTime",
                     "OutputPriority", "MaxDwellTime"];   // names not found on any object are dropped
int    PULL_LABELS   = 1;   // PULL: 1 = add a column for every number/text label
int    SELECTED_ONLY = 0;   // PULL: 1 = only selected objects
string CODE_MARK = "(code)"; // PULL writes this for properties set by custom code; PUSH skips it
// -------------------

// workbook path = folder of the open .fsm + XLSX_NAME
string MODEL_DIR = modeldir();
if (MODEL_DIR == "")
	return "Save the model (.fsm) first - the workbook is kept next to it";
string XLSX = MODEL_DIR + XLSX_NAME;

// ---------- PULL ----------
if (MODE == 0) {
	treenode root = model();
	Array found = [];
	for (int q = 1; q <= PULL_PROPS.length; q++)
		found.push(0);
	Array lcols = [];                       // label names, in the order first seen

	// pass 1: discover which columns exist
	for (int i = 1; i <= root.subnodes.length; i++) {
		treenode n = root.subnodes[i];
		if (n.dataType != DATATYPE_OBJECT)
			continue;
		Object obj = n;
		if (SELECTED_ONLY && !switch_selected(obj, -1))
			continue;

		for (int p = 1; p <= PULL_PROPS.length; p++)
			if (obj.getProperty(PULL_PROPS[p]) != nullvar)
				found[p] = 1;

		treenode lbls = obj.find(">labels");
		if (PULL_LABELS && objectexists(lbls)) {
			for (int k = 1; k <= lbls.subnodes.length; k++) {
				treenode L = lbls.subnodes[k];
				Variant lv = L.value;
				if (lv.type != VAR_TYPE_NUMBER && lv.type != VAR_TYPE_STRING)
					continue;              // pointer / array / table labels
				int known = 0;
				for (int u = 1; u <= lcols.length; u++)
					if (lcols[u] == L.name)
						known = 1;
				if (!known)
					lcols.push(L.name);
			}
		}
	}

	// column list
	Array cols = [];
	for (int p2 = 1; p2 <= PULL_PROPS.length; p2++)
		if (found[p2])
			cols.push(PULL_PROPS[p2]);
	for (int u2 = 1; u2 <= lcols.length; u2++) {
		string ln = lcols[u2];
		cols.push("label:" + ln);
	}

	excelopen(XLSX);
	excelsetsheet(SHEET);

	// clear whatever an earlier PULL left in the sheet
	int oldC = 0;
	while (oldC < 500 && excelreadstr(1, oldC + 1) != "")
		oldC++;
	int oldR = 0;
	while (oldR < 20000 && excelreadstr(oldR + 1, 1) != "")
		oldR++;
	for (int cr = 1; cr <= oldR; cr++)
		for (int cc = 1; cc <= oldC; cc++)
			excelwritestr(cr, cc, "");

	// header row
	excelwritestr(1, 1, "Object");
	excelwritestr(1, 2, "Type");
	for (int h = 1; h <= cols.length; h++) {
		string hc = cols[h];
		excelwritestr(1, h + 2, hc);
	}

	// pass 2: one row per object
	int written = 0;
	for (int i2 = 1; i2 <= root.subnodes.length; i2++) {
		treenode n2 = root.subnodes[i2];
		if (n2.dataType != DATATYPE_OBJECT)
			continue;
		Object ob = n2;
		if (SELECTED_ONLY && !switch_selected(ob, -1))
			continue;

		// object type = name of its library class (Processor, Queue, Source...)
		string typ = "";
		treenode cl = ob.find(">classes");
		if (objectexists(cl) && cl.subnodes.length > 0) {
			treenode c1 = cl.subnodes[1];
			Variant cref = c1.value;
			if (cref.type == VAR_TYPE_NODE) {
				treenode cnode = cref;
				if (objectexists(cnode)) {
					treenode cown = ownerobject(cnode);
					if (objectexists(cown))
						typ = cown.name;
				}
			}
			if (typ == "")
				typ = c1.name;
		}

		int rowN = written + 2;
		int any = 0;
		treenode lb = ob.find(">labels");
		for (int h2 = 1; h2 <= cols.length; h2++) {
			string col = cols[h2];
			Variant cv = nullvar;
			Array lpp = col.split(":");
			if (lpp.length == 2 && lpp[1] == "label") {
				string lnm = lpp[2];
				if (objectexists(lb))
					for (int k2 = 1; k2 <= lb.subnodes.length; k2++)
						if (lb.subnodes[k2].name == lnm)
							cv = lb.subnodes[k2].value;
			} else
				cv = ob.getProperty(col);

			if (cv.type == VAR_TYPE_NUMBER) {
				excelwritenum(rowN, h2 + 2, cv);
				any = 1;
			} else if (cv.type == VAR_TYPE_STRING) {
				string s = cv;
				// custom code (multi-line, or a /** picklist header) has no single value -
				// write a marker instead, and PUSH leaves marked properties alone
				Array lines = s.split("\n");
				Array hdrSplit = s.split("/**");
				if (lines.length > 1 || (hdrSplit.length > 1 && hdrSplit[1] == ""))
					s = CODE_MARK;
				excelwritestr(rowN, h2 + 2, s);
				any = 1;
			}
		}
		if (!any)
			continue;                             // nothing to configure on this object
		excelwritestr(rowN, 1, ob.name);
		excelwritestr(rowN, 2, typ);
		written++;
	}
	excelclose(1);                                // save and close

	string notFound = "";
	for (int m = 1; m <= PULL_PROPS.length; m++)
		if (!found[m])
			notFound += PULL_PROPS[m] + " ";

	string pmsg = "Pulled " + string.fromNum(written) + " objects x " + string.fromNum(cols.length) + " columns -> " + XLSX + " [" + SHEET + "]";
	if (notFound.length > 0)
		pmsg += " | dropped (not a property on any object): " + notFound;
	return pmsg;
}

// ---------- read the sheet for DRYRUN / PUSH ----------
excelopen(XLSX);
excelsetsheet(SHEET);

int nc = 0;                                   // columns = filled header cells in row 1
while (nc < 500 && excelreadstr(1, nc + 1) != "")
	nc++;
int nr = 1;                                   // rows = filled object names in column 1
while (nr < 20000 && excelreadstr(nr + 1, 1) != "")
	nr++;
Array hdr = [];
for (int hh = 1; hh <= nc; hh++)
	hdr.push(excelreadstr(1, hh));

int changed = 0;
int same = 0;
int codeKept = 0;
string missing = "";
string noProp = "";
string preview = "";

for (int r = 2; r <= nr; r++) {
	string nm = excelreadstr(r, 1);
	Object o = Model.find(nm);
	if (!objectexists(o)) {
		missing += nm + " ";
		continue;
	}

	for (int j = 2; j <= nc; j++) {
		string prop = hdr[j];
		if (prop == "" || prop == "Type")
			continue;                        // Type is reference only

		// read the cell: text, number, or blank (= leave as is)
		string cs = excelreadstr(r, j);
		double cn = excelreadnum(r, j);
		string digits = cs.split(",").join("");          // "1,800" -> "1800"
		string body = "x";
		Array sg = digits.split("-");
		if (sg.length == 1)
			body = digits;
		else if (sg.length == 2 && sg[1] == "")
			body = sg[2];
		string rest = body;
		for (int d = 0; d <= 9; d++)
			rest = rest.split(string.fromNum(d)).join("");
		int isNumText = body != "" && body != "." && (rest == "" || rest == ".");

		if (cs == CODE_MARK) {
			codeKept++;
			continue;                        // property is custom code - leave it as is
		}

		Variant val = nullvar;
		string shown = cs;
		if (cs != "") {
			if (isNumText)
				val = stringtonum(digits);
			else
				val = cs;                    // expression / distribution / text label
		} else if (cn != 0) {
			val = cn;
			shown = string.fromNum(cn);
		} else
			continue;                        // blank cell = leave as is

		Array lp = prop.split(":");
		int isLabel = lp.length == 2 && (lp[1] == "label" || lp[1] == "Label");
		string lname = "";
		if (isLabel)
			lname = lp[2];

		Variant now = nullvar;
		if (isLabel)
			now = getlabel(o, lname);
		else {
			now = o.getProperty(prop);
			if (now == nullvar) {
				if (noProp.length < 300)
					noProp += nm + "." + prop + " ";
				continue;
			}
		}

		if (now == val) {
			same++;
			continue;
		}

		changed++;
		if (preview.length < 400) {
			string was = "(none)";
			if (now.type == VAR_TYPE_NUMBER)
				was = string.fromNum(now);
			else if (now.type == VAR_TYPE_STRING)
				was = now;
			preview += nm + "." + prop + ": " + was + " -> " + shown + "  ";
		}

		if (MODE == 2) {
			if (isLabel)
				setlabel(o, lname, val);
			else
				o.setProperty(prop, val);
		}
	}
}
excelclose(0);                                // close without saving

if (MODE == 2)
	repaintall();

string head = MODE == 2 ? "APPLIED " : "Would change ";
string msg = head + string.fromNum(changed) + ", unchanged " + string.fromNum(same) + ", custom code kept " + string.fromNum(codeKept);
if (missing.length > 0)
	msg += " | objects not found: " + missing;
if (noProp.length > 0)
	msg += " | no such property: " + noProp;
return msg + " | " + preview;
