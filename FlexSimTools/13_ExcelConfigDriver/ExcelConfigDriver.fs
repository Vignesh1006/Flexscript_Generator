// 13 - Excel Config Driver (v4, .xlsx). Two-way sync of object properties AND labels
//      with an Excel sheet laid out as a table: one row per object, one column per property / label.
//      Row 1:  Object | Type | MaxContent | ProcessTime | SetupTime | label:LoadingTime ...
//      Type (Processor, Queue, Source...) is for reference only - PUSH ignores it.
// MODE 0 = PULL   : writes current model values into the sheet (builds the columns for you)
// MODE 1 = DRYRUN : reads the sheet and reports what would change - nothing is written
// MODE 2 = PUSH   : applies the sheet to the model
// Blank cell = leave that value as is. Label columns start with  label:
// Properties set by custom code:
//   code that reads one Global Table cell, e.g.  Table("Timings")[1][2]  -> PULL writes that cell's value,
//       PUSH writes the new value back INTO THE TABLE (the code itself is never touched)
//   any other code -> EVAL_CODE = 1 runs it once and writes the result (labels it sets are restored);
//       read-only, PUSH skips it. EVAL_CODE = 0 writes (code) instead.
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
int    EVAL_CODE     = 1;   // PULL: custom code that is not a plain Table("X")[r][c] lookup -
                            //       1 = run the code once and write its result (read-only), 0 = write (code)
string CODE_MARK = "(code)"; // written for custom code that cannot be resolved; PUSH skips it
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
	int fromTable = 0;
	int evaluated = 0;
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

			// custom code (multi-line, or a /** picklist header): find the value it produces
			if (cv.type == VAR_TYPE_STRING) {
				string s = cv;
				Array lines = s.split("\n");
				Array hdrSplit = s.split("/**");
				if (lines.length > 1 || (hdrSplit.length > 1 && hdrSplit[1] == "")) {
					// --- one Table("Name")[row][col] lookup? ---
					int tOk = 0;
					string tName = "";
					Variant tRow = nullvar;
					Variant tCol = nullvar;
					Array tp = s.split("Table(\"");
					if (tp.length == 2) {
						string t2 = tp[2];
						Array ta = t2.split("\")[");
						if (ta.length >= 2) {
							tName = ta[1];
							string t3 = ta[2];
							Array tb = t3.split("][");
							if (tb.length >= 2) {
								string rS = tb[1];
								string t4 = tb[2];
								Array tc = t4.split("]");
								string cS = tc[1];
								string rQ = rS.split("\"").join("");
								string cQ = cS.split("\"").join("");
								string rRest = rQ;
								string cRest = cQ;
								for (int dd = 0; dd <= 9; dd++) {
									rRest = rRest.split(string.fromNum(dd)).join("");
									cRest = cRest.split(string.fromNum(dd)).join("");
								}
								int rOk = rQ != "" && (rQ != rS || rRest == "");   // quoted name or plain number
								int cOk = cQ != "" && (cQ != cS || cRest == "");
								if (rOk && cOk && objectexists(Model.find("Tools/GlobalTables/" + tName))) {
									if (rQ != rS) tRow = rQ; else tRow = stringtonum(rQ);
									if (cQ != cS) tCol = cQ; else tCol = stringtonum(cQ);
									tOk = 1;
								}
							}
						}
					}

					if (tOk) {
						cv = Table(tName)[tRow][tCol];
						fromTable++;
					} else if (EVAL_CODE) {
						// run the code once from a temporary label on the object (so current = this object),
						// then put every label back the way it was
						Array lNames = [];
						Array lVals = [];
						treenode lbs = ob.find(">labels");
						if (objectexists(lbs))
							for (int k3 = 1; k3 <= lbs.subnodes.length; k3++) {
								lNames.push(lbs.subnodes[k3].name);
								lVals.push(lbs.subnodes[k3].value);
							}
						treenode tmp = ob.labels.assert("__xcd_eval", "");
						tmp.dataType = DATATYPE_STRING;
						tmp.value = s;
						enablecode(tmp, 1);
						buildnodeflexscript(tmp);
						Variant res = tmp.evaluate();
						tmp.destroy();
						lbs = ob.find(">labels");
						if (objectexists(lbs))
							for (int k4 = lbs.subnodes.length; k4 >= 1; k4--) {
								treenode L2 = lbs.subnodes[k4];
								int idx = 0;
								for (int k5 = 1; k5 <= lNames.length; k5++)
									if (lNames[k5] == L2.name)
										idx = k5;
								if (idx > 0)
									L2.value = lVals[idx];
								else
									L2.destroy();          // label created by the code
							}
						cv = res;
						evaluated++;
					} else
						cv = CODE_MARK;
				}
			}

			if (cv.type == VAR_TYPE_NUMBER) {
				excelwritenum(rowN, h2 + 2, cv);
				any = 1;
			} else if (cv.type == VAR_TYPE_STRING) {
				string sv = cv;
				excelwritestr(rowN, h2 + 2, sv);
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
	if (fromTable > 0)
		pmsg += " | " + string.fromNum(fromTable) + " values read from Global Tables";
	if (evaluated > 0)
		pmsg += " | " + string.fromNum(evaluated) + " values from running custom code (read-only)";
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

		// property is custom code? then the cell belongs to the Global Table cell it reads, if any
		if (!isLabel && now.type == VAR_TYPE_STRING) {
			string ns = now;
			Array nLines = ns.split("\n");
			Array nHdr = ns.split("/**");
			if (nLines.length > 1 || (nHdr.length > 1 && nHdr[1] == "")) {
				int tOkP = 0;
				string tNameP = "";
				string tRef = "";
				Variant tRowP = nullvar;
				Variant tColP = nullvar;
				Array tpP = ns.split("Table(\"");
				if (tpP.length == 2) {
					string t2P = tpP[2];
					Array taP = t2P.split("\")[");
					if (taP.length >= 2) {
						tNameP = taP[1];
						string t3P = taP[2];
						Array tbP = t3P.split("][");
						if (tbP.length >= 2) {
							string rSP = tbP[1];
							string t4P = tbP[2];
							Array tcP = t4P.split("]");
							string cSP = tcP[1];
							string rQP = rSP.split("\"").join("");
							string cQP = cSP.split("\"").join("");
							string rRestP = rQP;
							string cRestP = cQP;
							for (int d2 = 0; d2 <= 9; d2++) {
								rRestP = rRestP.split(string.fromNum(d2)).join("");
								cRestP = cRestP.split(string.fromNum(d2)).join("");
							}
							int rOkP = rQP != "" && (rQP != rSP || rRestP == "");
							int cOkP = cQP != "" && (cQP != cSP || cRestP == "");
							if (rOkP && cOkP && objectexists(Model.find("Tools/GlobalTables/" + tNameP))) {
								if (rQP != rSP) tRowP = rQP; else tRowP = stringtonum(rQP);
								if (cQP != cSP) tColP = cQP; else tColP = stringtonum(cQP);
								tRef = tNameP + "[" + rQP + "][" + cQP + "]";
								tOkP = 1;
							}
						}
					}
				}

				if (!tOkP) {
					codeKept++;                  // other custom code - read-only, never overwritten
					continue;
				}

				Variant tNow = Table(tNameP)[tRowP][tColP];
				if (tNow == val) {
					same++;
					continue;
				}
				changed++;
				if (preview.length < 400) {
					string tWas = "(none)";
					if (tNow.type == VAR_TYPE_NUMBER)
						tWas = string.fromNum(tNow);
					else if (tNow.type == VAR_TYPE_STRING)
						tWas = tNow;
					preview += nm + "." + prop + " via " + tRef + ": " + tWas + " -> " + shown + "  ";
				}
				if (MODE == 2)
					Table(tNameP)[tRowP][tColP] = val;
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
string msg = head + string.fromNum(changed) + ", unchanged " + string.fromNum(same) + ", custom code not pushed " + string.fromNum(codeKept);
if (missing.length > 0)
	msg += " | objects not found: " + missing;
if (noProp.length > 0)
	msg += " | no such property: " + noProp;
return msg + " | " + preview;
