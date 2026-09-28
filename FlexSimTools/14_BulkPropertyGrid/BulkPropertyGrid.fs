// 14 - Bulk Property Grid. One row per selected object, one column per property,
//      exported for editing and read back. Same idea as 13 but shaped like a grid,
//      which is easier when you are editing many objects on the same few properties.
// MODE 0 = export the selection, MODE 1 = apply the file back to the model.
// Save the model before applying.
// FlexSim 2027: select objects, then Script Console > paste > Execute.

// ---- settings ----
int    MODE  = 0;
string CSV   = "C:/FlexSimTools/property_grid.csv";
Array  PROPS = ["ProcessTime", "MaxContent"];   // the columns
// ------------------

// ---------- export ----------
if (MODE == 0) {
	Array objs = [];
	treenode root = model();
	for (int i = 1; i <= root.subnodes.length; i++) {
		treenode n = root.subnodes[i];
		if (n.dataType != DATATYPE_OBJECT)
			continue;
		Object o = n;
		if (o.flags.isSelected)
			objs.push(o);
	}
	if (objs.length < 1)
		return "Select the objects you want in the grid first";

	if (!fileopen(CSV, "w"))
		return "Could not write " + CSV;

	string header = "Object";
	for (int p = 1; p <= PROPS.length; p++)
		header += "," + PROPS[p];
	fpt(header + "\n");

	for (int i = 1; i <= objs.length; i++) {
		Object o = objs[i];
		string line = o.name;
		for (int p = 1; p <= PROPS.length; p++) {
			Variant v = o.getProperty(PROPS[p]);
			line += "," + (v == nullvar ? "" : string.fromNum(v));
		}
		fpt(line + "\n");
	}
	fileclose();
	return "Exported " + string.fromNum(objs.length) + " objects -> " + CSV;
}

// ---------- apply ----------
if (!fileopen(CSV, "r"))
	return "Could not open " + CSV;
Array rows = [];
while (!endoffile()) {
	string line = filereadline();
	if (line.length > 2)
		rows.push(line);
}
fileclose();

Array cols = rows[1].split(",");
int applied = 0;
string missing = "";

for (int r = 2; r <= rows.length; r++) {
	Array f = rows[r].split(",");
	if (f.length < 2)
		continue;
	Object o = Model.find(f[1]);
	if (!objectexists(o)) {
		missing += f[1] + " ";
		continue;
	}
	for (int c = 2; c <= cols.length; c++) {
		if (f.length < c)
			continue;
		string val = f[c];
		if (val.length < 1)
			continue;
		o.setProperty(cols[c], stringtonum(val));
		applied++;
	}
}

repaintall();
string msg = "Applied " + string.fromNum(applied) + " values";
if (missing.length > 0)
	msg += " | not found: " + missing;
return msg;
