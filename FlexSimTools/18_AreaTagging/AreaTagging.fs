// 18 - Area Tagging. Puts objects into named zones, colors them by zone, and totals per zone.
// MODE 0 = tag the selection with AREA
// MODE 1 = color every tagged object by its area
// MODE 2 = report objects and output per area
// FlexSim 2027: Script Console > paste > Execute.

// ---- settings ----
int    MODE = 0;
string AREA = "Packing";
string LABEL = "Area";
string REPORT = "C:/FlexSimTools/area_report.csv";
// ------------------

treenode root = model();

// ---------- tag ----------
if (MODE == 0) {
	int tagged = 0;
	for (int i = 1; i <= root.subnodes.length; i++) {
		treenode n = root.subnodes[i];
		if (n.dataType != DATATYPE_OBJECT)
			continue;
		Object o = n;
		if (!o.flags.isSelected)
			continue;
		o.labels.assert(LABEL, "").value = AREA;
		tagged++;
	}
	if (tagged < 1)
		return "Select the objects that belong to '" + AREA + "' first";
	return "Tagged " + string.fromNum(tagged) + " objects as '" + AREA + "'";
}

// collect the distinct areas in the order they appear
Array areas = [];
for (int i = 1; i <= root.subnodes.length; i++) {
	treenode n = root.subnodes[i];
	if (n.dataType != DATATYPE_OBJECT)
		continue;
	Object o = n;
	treenode lab = o.labels.find(LABEL);
	if (!objectexists(lab))
		continue;
	string a = lab.value;
	int known = 0;
	for (int k = 1; k <= areas.length; k++)
		if (areas[k] == a) known = 1;
	if (!known)
		areas.push(a);
}

if (areas.length < 1)
	return "No objects are tagged yet - run MODE 0 on a selection first";

// ---------- color ----------
if (MODE == 1) {
	int colored = 0;
	for (int i = 1; i <= root.subnodes.length; i++) {
		treenode n = root.subnodes[i];
		if (n.dataType != DATATYPE_OBJECT)
			continue;
		Object o = n;
		treenode lab = o.labels.find(LABEL);
		if (!objectexists(lab))
			continue;

		int idx = 1;
		for (int k = 1; k <= areas.length; k++)
			if (areas[k] == lab.value) idx = k;

		// spread hues around the wheel so neighbouring areas look different
		double h = (idx - 1) * 1.0 / areas.length;
		double r = Math.fabs(h * 6 - 3) - 1;
		double g = 2 - Math.fabs(h * 6 - 2);
		double b = 2 - Math.fabs(h * 6 - 4);
		o.color = Color(Math.min(Math.max(r,0),1), Math.min(Math.max(g,0),1), Math.min(Math.max(b,0),1));
		colored++;
	}
	repaintall();
	return "Colored " + string.fromNum(colored) + " objects across " + string.fromNum(areas.length) + " areas";
}

// ---------- report ----------
Array counts = [];
Array outputs = [];
for (int k = 1; k <= areas.length; k++) {
	counts.push(0);
	outputs.push(0);
}
for (int i = 1; i <= root.subnodes.length; i++) {
	treenode n = root.subnodes[i];
	if (n.dataType != DATATYPE_OBJECT)
		continue;
	Object o = n;
	treenode lab = o.labels.find(LABEL);
	if (!objectexists(lab))
		continue;
	for (int k = 1; k <= areas.length; k++) {
		if (areas[k] == lab.value) {
			counts[k] = counts[k] + 1;
			outputs[k] = outputs[k] + o.stats.output.value;
		}
	}
}

if (fileopen(REPORT, "w")) {
	fpt("Area,Objects,Output\n");
	for (int k = 1; k <= areas.length; k++)
		fpt(areas[k] + "," + string.fromNum(counts[k]) + "," + numtostring(outputs[k], 0, 0) + "\n");
	fileclose();
}

string msg = "";
for (int k = 1; k <= areas.length; k++)
	msg += areas[k] + " (" + string.fromNum(counts[k]) + " obj, " + numtostring(outputs[k], 0, 0) + " out)  ";
return msg + "-> " + REPORT;
