// 11 - Clearance Checker. Finds objects that overlap or sit closer than a minimum clearance.
// Axis-aligned footprints (rotation is ignored), which is the usual case for plant layouts.
// FlexSim 2027: Script Console > paste > Execute. Changes nothing.

// ---- settings ----
string REPORT   = "C:/FlexSimTools/clearance_report.csv";
double MIN_GAP  = 1.20;   // metres of clear space required between footprints
int    SKIP_CONNECTED = 0; // 1 = ignore pairs that are A-connected (they are meant to touch)
// ------------------

double toM = getmodelunit(LENGTH_MULTIPLE);
Array lines = [];
int overlaps = 0;
int tight = 0;

treenode root = model();
Array objs = [];
for (int i = 1; i <= root.subnodes.length; i++) {
	treenode n = root.subnodes[i];
	if (n.dataType == DATATYPE_OBJECT)
		objs.push(n);
}

for (int i = 1; i <= objs.length; i++) {
	Object a = objs[i];
	Vec3 ca = a.getLocation(0.5, 0.5, 0);
	Vec3 sa = a.size;

	for (int j = i + 1; j <= objs.length; j++) {
		Object b = objs[j];

		if (SKIP_CONNECTED) {
			int connected = 0;
			for (int p = 1; p <= a.outObjects.length; p++)
				if (a.outObjects[p] == b) connected = 1;
			for (int p = 1; p <= b.outObjects.length; p++)
				if (b.outObjects[p] == a) connected = 1;
			if (connected)
				continue;
		}

		Vec3 cb = b.getLocation(0.5, 0.5, 0);
		Vec3 sb = b.size;

		// gap per axis: negative means the footprints overlap on that axis
		double gx = Math.fabs(cb.x - ca.x) - (sa.x + sb.x) / 2;
		double gy = Math.fabs(cb.y - ca.y) - (sa.y + sb.y) / 2;

		if (gx < 0 && gy < 0) {
			lines.push(a.name + "," + b.name + ",OVERLAP," + numtostring(Math.min(gx, gy) * toM, 0, 2));
			overlaps++;
			continue;
		}

		double gap = Math.max(gx, gy) * toM;   // clear distance between footprints
		if (gap < MIN_GAP) {
			lines.push(a.name + "," + b.name + ",tight," + numtostring(gap, 0, 2));
			tight++;
		}
	}
}

if (fileopen(REPORT, "w")) {
	fpt("ObjectA,ObjectB,Issue,Gap_m\n");
	for (int i = 1; i <= lines.length; i++)
		fpt(lines[i] + "\n");
	fileclose();
}

return string.fromNum(overlaps) + " overlaps, " + string.fromNum(tight) + " below " + numtostring(MIN_GAP, 0, 2) + " m -> " + REPORT;
