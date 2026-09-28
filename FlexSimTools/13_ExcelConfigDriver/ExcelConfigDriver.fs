// 13 - Excel Config Driver. Two-way sync of object properties with a CSV of
//      Object,Property,Value rows. Save the sheet from Excel as CSV.
// MODE 0 = PULL  : writes current model values into the CSV (objects chosen by PULL_PROPS)
// MODE 1 = DRYRUN: reads the CSV and reports what would change - nothing is written
// MODE 2 = PUSH  : applies the CSV to the model
// Always run DRYRUN before PUSH. Save the model first.
// FlexSim 2027: Script Console > paste > Execute.

// ---- settings ----
int    MODE = 1;
string CSV  = "C:/FlexSimTools/model_config.csv";
Array  PULL_PROPS = ["ProcessTime", "MaxContent"];   // properties to export in PULL mode
int    SELECTED_ONLY = 0;   // PULL: 1 = only selected objects
// ------------------

// ---------- PULL ----------
if (MODE == 0) {
	if (!fileopen(CSV, "w"))
		return "Could not write " + CSV;
	fpt("Object,Property,Value\n");

	int rows = 0;
	treenode root = model();
	for (int i = 1; i <= root.subnodes.length; i++) {
		treenode n = root.subnodes[i];
		if (n.dataType != DATATYPE_OBJECT)
			continue;
		Object o = n;
		if (SELECTED_ONLY && !o.flags.isSelected)
			continue;

		for (int p = 1; p <= PULL_PROPS.length; p++) {
			string prop = PULL_PROPS[p];
			Variant v = o.getProperty(prop);
			if (v == nullvar)
				continue;
			fpt(o.name + "," + prop + "," + string.fromNum(v) + "\n");
			rows++;
		}
	}
	fileclose();
	return "Pulled " + string.fromNum(rows) + " rows -> " + CSV;
}

// ---------- read the CSV for DRYRUN / PUSH ----------
if (!fileopen(CSV, "r"))
	return "Could not open " + CSV;
Array rows = [];
while (!endoffile()) {
	string line = filereadline();
	if (line.length > 2)
		rows.push(line);
}
fileclose();

int changed = 0;
int same = 0;
string missing = "";
string preview = "";

for (int r = 2; r <= rows.length; r++) {     // row 1 is the header
	Array f = rows[r].split(",");
	if (f.length < 3)
		continue;

	string nm   = f[1];
	string prop = f[2];
	double val  = stringtonum(f[3]);

	Object o = Model.find(nm);
	if (!objectexists(o)) {
		missing += nm + " ";
		continue;
	}

	double now = o.getProperty(prop);
	if (now == val) {
		same++;
		continue;
	}

	changed++;
	if (preview.length < 400)
		preview += nm + "." + prop + ": " + numtostring(now, 0, 2) + " -> " + numtostring(val, 0, 2) + "  ";

	if (MODE == 2)
		o.setProperty(prop, val);
}

if (MODE == 2)
	repaintall();

string head = MODE == 2 ? "APPLIED " : "Would change ";
string msg = head + string.fromNum(changed) + ", unchanged " + string.fromNum(same);
if (missing.length > 0)
	msg += " | not found: " + missing;
return msg + " | " + preview;
