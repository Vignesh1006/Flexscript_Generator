// 15 - Scenario Snapshots. Saves the model's key settings under a scenario name,
//      restores them later, and lists what differs between two scenarios.
// MODE 0 = save, 1 = restore, 2 = compare SCENARIO against COMPARE_WITH.
// Snapshots are plain CSV files, one per scenario, so they are easy to keep and diff.
// FlexSim 2027: Script Console > paste > Execute. Save the model before restoring.

// ---- settings ----
int    MODE         = 0;
string SCENARIO     = "baseline";
string COMPARE_WITH = "option_a";
string FOLDER       = "C:/FlexSimTools/scenarios/";
Array  PROPS        = ["ProcessTime", "MaxContent"];
// ------------------

string path = FOLDER + SCENARIO + ".csv";

// ---------- save ----------
if (MODE == 0) {
	if (!fileopen(path, "w"))
		return "Could not write " + path + " - does the folder exist?";
	fpt("Object,Property,Value\n");
	int rows = 0;
	treenode root = model();
	for (int i = 1; i <= root.subnodes.length; i++) {
		treenode n = root.subnodes[i];
		if (n.dataType != DATATYPE_OBJECT)
			continue;
		Object o = n;
		for (int p = 1; p <= PROPS.length; p++) {
			Variant v = o.getProperty(PROPS[p]);
			if (v == nullvar)
				continue;
			fpt(o.name + "," + PROPS[p] + "," + string.fromNum(v) + "\n");
			rows++;
		}
	}
	fileclose();
	return "Snapshot '" + SCENARIO + "' saved, " + string.fromNum(rows) + " values -> " + path;
}

// ---------- load a snapshot into arrays ----------
Array names = [];
Array props = [];
Array vals = [];
if (!fileopen(path, "r"))
	return "Could not open " + path;
int firstLine = 1;
while (!endoffile()) {
	string line = filereadline();
	if (firstLine) { firstLine = 0; continue; }
	if (line.length < 3)
		continue;
	Array f = line.split(",");
	if (f.length < 3)
		continue;
	names.push(f[1]);
	props.push(f[2]);
	vals.push(stringtonum(f[3]));
}
fileclose();

// ---------- restore ----------
if (MODE == 1) {
	int applied = 0;
	for (int i = 1; i <= names.length; i++) {
		Object o = Model.find(names[i]);
		if (!objectexists(o))
			continue;
		o.setProperty(props[i], vals[i]);
		applied++;
	}
	repaintall();
	return "Restored '" + SCENARIO + "': " + string.fromNum(applied) + " values applied";
}

// ---------- compare ----------
string other = FOLDER + COMPARE_WITH + ".csv";
if (!fileopen(other, "r"))
	return "Could not open " + other;
Array on = [];
Array op = [];
Array ov = [];
firstLine = 1;
while (!endoffile()) {
	string line = filereadline();
	if (firstLine) { firstLine = 0; continue; }
	if (line.length < 3)
		continue;
	Array f = line.split(",");
	if (f.length < 3)
		continue;
	on.push(f[1]);
	op.push(f[2]);
	ov.push(stringtonum(f[3]));
}
fileclose();

int diffs = 0;
string list = "";
for (int i = 1; i <= names.length; i++) {
	for (int j = 1; j <= on.length; j++) {
		if (names[i] == on[j] && props[i] == op[j]) {
			double a = vals[i];
			double b = ov[j];
			if (a != b) {
				diffs++;
				if (list.length < 500)
					list += names[i] + "." + props[i] + ": " + numtostring(a, 0, 2) + " vs " + numtostring(b, 0, 2) + "  ";
			}
		}
	}
}

return SCENARIO + " vs " + COMPARE_WITH + ": " + string.fromNum(diffs) + " differences. " + list;
