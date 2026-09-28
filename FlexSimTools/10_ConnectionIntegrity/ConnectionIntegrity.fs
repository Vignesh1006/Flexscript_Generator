// 10 - Connection Integrity. Prints a port map and flags suspicious wiring. Changes nothing.
// FlexSim 2027: Script Console > paste > Execute.

// ---- settings ----
string REPORT   = "C:/FlexSimTools/port_map.csv";
double LONG_LINK = 50.0;   // metres: flag connections longer than this
// ------------------

double toM = getmodelunit(LENGTH_MULTIPLE);
Array lines = [];
int flags = 0;
int links = 0;

treenode root = model();
for (int i = 1; i <= root.subnodes.length; i++) {
	treenode n = root.subnodes[i];
	if (n.dataType != DATATYPE_OBJECT)
		continue;
	Object a = n;

	for (int p = 1; p <= a.outObjects.length; p++) {
		Object b = a.outObjects[p];
		if (!objectexists(b))
			continue;
		links++;

		Vec3 ca = a.getLocation(0.5, 0.5, 0);
		Vec3 cb = b.getLocation(0.5, 0.5, 0);
		double d = Vec3(cb.x - ca.x, cb.y - ca.y, 0).magnitude * toM;

		string note = "";
		if (d > LONG_LINK) { note = "long link"; flags++; }
		if (b == a)        { note = "connects to itself"; flags++; }

		// same pair connected more than once
		int repeats = 0;
		for (int q = 1; q <= a.outObjects.length; q++) {
			Object t = a.outObjects[q];
			if (t == b) repeats++;
		}
		if (repeats > 1) { note = "duplicate connection"; flags++; }

		lines.push(a.name + ",out port " + string.fromNum(p) + "," + b.name + "," + numtostring(d, 0, 2) + "," + note);
	}

	// objects with ports but nothing on them
	if (a.outObjects.length == 0 && a.inObjects.length > 0)
		lines.push(a.name + ",-,NO OUTPUT,0,dead end");
}

if (fileopen(REPORT, "w")) {
	fpt("From,Port,To,Distance_m,Note\n");
	for (int i = 1; i <= lines.length; i++)
		fpt(lines[i] + "\n");
	fileclose();
}

return string.fromNum(links) + " connections mapped, " + string.fromNum(flags) + " flagged -> " + REPORT;
