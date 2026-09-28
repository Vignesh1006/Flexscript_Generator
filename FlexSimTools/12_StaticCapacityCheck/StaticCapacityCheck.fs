// 12 - Static Capacity Check. Compares required takt against each station's process time,
// before you run anything. Changes nothing.
// FlexSim 2027: Script Console > paste > Execute.

// ---- settings ----
string REPORT     = "C:/FlexSimTools/capacity_check.csv";
double DEMAND     = 1000;    // units required
double SHIFT_SECS = 28800;   // seconds available (8 h)
double EFFICIENCY = 0.85;    // realistic uptime factor, 1.0 = perfect
// ------------------

double takt = SHIFT_SECS * EFFICIENCY / DEMAND;   // seconds per unit allowed

Array lines = [];
int over = 0;
int checked = 0;

treenode root = model();
for (int i = 1; i <= root.subnodes.length; i++) {
	treenode n = root.subnodes[i];
	if (n.dataType != DATATYPE_OBJECT)
		continue;
	Object o = n;
	if (o.classname != "Processor")            // widen this list once it works
		continue;

	double pt = o.getProperty("ProcessTime");  // if this errors on 2027, tell me the message
	checked++;

	double capacity = pt > 0 ? SHIFT_SECS * EFFICIENCY / pt : 0;
	string verdict = pt > takt ? "BOTTLENECK" : "ok";
	if (pt > takt)
		over++;

	lines.push(o.name + "," + numtostring(pt, 0, 2) + "," + numtostring(takt, 0, 2) + "," + numtostring(capacity, 0, 0) + "," + verdict);
}

if (fileopen(REPORT, "w")) {
	fpt("Station,ProcessTime_s,Takt_s,Capacity_units,Verdict\n");
	for (int i = 1; i <= lines.length; i++)
		fpt(lines[i] + "\n");
	fileclose();
}

return "Takt " + numtostring(takt, 0, 2) + " s/unit. " + string.fromNum(checked) + " stations checked, " + string.fromNum(over) + " too slow -> " + REPORT;
