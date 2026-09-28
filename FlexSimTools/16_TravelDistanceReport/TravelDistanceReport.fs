// 16 - Travel Distance Report. Exports every A and S connection with its center-to-center
//      distance, plus per-object totals. Run after a model run to also get the flow volume,
//      which turns distance into actual travel.
// FlexSim 2027: Script Console > paste > Execute. Changes nothing.

// ---- settings ----
string REPORT   = "C:/FlexSimTools/travel_report.csv";
int    USE_FLOW = 1;    // 1 = multiply distance by items moved (needs a completed run)
// ------------------

double toM = getmodelunit(LENGTH_MULTIPLE);
Array lines = [];
double totalDist = 0;
double totalTravel = 0;
int links = 0;

treenode root = model();
for (int i = 1; i <= root.subnodes.length; i++) {
	treenode n = root.subnodes[i];
	if (n.dataType != DATATYPE_OBJECT)
		continue;
	Object a = n;

	for (int mode = 0; mode <= 1; mode++) {
		Array targets = mode == 0 ? a.outObjects.toArray() : a.centerObjects.toArray();
		for (int p = 1; p <= targets.length; p++) {
			Object b = targets[p];
			if (!objectexists(b))
				continue;
			if (mode == 1 && b.rank < a.rank)
				continue;

			Vec3 ca = a.getLocation(0.5, 0.5, 0);
			Vec3 cb = b.getLocation(0.5, 0.5, 0);
			double d = Vec3(cb.x - ca.x, cb.y - ca.y, 0).magnitude * toM;

			double qty = 0;
			double travel = 0;
			if (USE_FLOW && mode == 0) {
				qty = a.stats.output.value;     // items that left this object
				travel = qty * d;
			}

			lines.push(a.name + "," + b.name + "," + (mode == 0 ? "A" : "S") + "," + numtostring(d, 0, 2) + "," + numtostring(qty, 0, 0) + "," + numtostring(travel, 0, 1));
			totalDist += d;
			totalTravel += travel;
			links++;
		}
	}
}

if (fileopen(REPORT, "w")) {
	fpt("From,To,Type,Distance_m,Items,Travel_m\n");
	for (int i = 1; i <= lines.length; i++)
		fpt(lines[i] + "\n");
	fpt("TOTAL,,," + numtostring(totalDist, 0, 2) + ",," + numtostring(totalTravel, 0, 1) + "\n");
	fileclose();
}

return string.fromNum(links) + " links, " + numtostring(totalDist, 0, 1) + " m of layout, " + numtostring(totalTravel / 1000, 0, 2) + " km travelled -> " + REPORT;
