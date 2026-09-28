// 09 - Model Auditor. Lists suspicious things in the model. Changes nothing.
// Writes a CSV report and returns a one-line summary.
// FlexSim 2027: Script Console > paste > Execute.

// ---- settings ----
string REPORT   = "C:/FlexSimTools/audit_report.csv";
double FLOOR_Z  = 0.0;    // expected floor height in metres
double MAX_XY   = 500.0;  // anything beyond this many metres from origin is suspect
// ------------------

double toM = getmodelunit(LENGTH_MULTIPLE);
Array lines = [];
int issues = 0;

treenode root = model();
Array objs = [];
for (int i = 1; i <= root.subnodes.length; i++) {
	treenode n = root.subnodes[i];
	if (n.dataType == DATATYPE_OBJECT)
		objs.push(n);
}

for (int i = 1; i <= objs.length; i++) {
	Object o = objs[i];
	string cls = o.classname;                 // if classname errors, tell me
	Vec3 c = o.getLocation(0.5, 0.5, 0);

	int outs = o.outObjects.length;
	int ins  = o.inObjects.length;
	int cens = o.centerObjects.length;

	// 1. completely unconnected
	if (outs == 0 && ins == 0 && cens == 0) {
		lines.push(o.name + ",unconnected,no A or S connections at all");
		issues++;
	}
	// 2. nothing feeds it (sources and executers excluded by having no input ports anyway)
	else if (ins == 0 && outs > 0 && cls != "Source") {
		lines.push(o.name + ",no input,has outputs but nothing feeds it");
		issues++;
	}
	// 3. dead end
	else if (outs == 0 && ins > 0 && cls != "Sink") {
		lines.push(o.name + ",dead end,receives items but sends nowhere");
		issues++;
	}

	// 4. default name never changed
	if (o.name.includes(cls))                 // e.g. "Processor12" still contains "Processor"
		lines.push(o.name + ",default name,still has its library name");

	// 5. off the floor or far from origin
	if (Math.fabs(c.z * toM - FLOOR_Z) > 0.01) {
		lines.push(o.name + ",off floor,z = " + numtostring(c.z * toM, 0, 2) + " m");
		issues++;
	}
	if (Math.fabs(c.x * toM) > MAX_XY || Math.fabs(c.y * toM) > MAX_XY) {
		lines.push(o.name + ",far away," + numtostring(c.x * toM, 0, 1) + " / " + numtostring(c.y * toM, 0, 1));
		issues++;
	}

	// 6. duplicate names
	for (int j = i + 1; j <= objs.length; j++) {
		Object p = objs[j];
		if (p.name == o.name) {
			lines.push(o.name + ",duplicate name,two objects share this name");
			issues++;
		}
	}
}

if (fileopen(REPORT, "w")) {
	fpt("Object,Issue,Detail\n");
	for (int i = 1; i <= lines.length; i++)
		fpt(lines[i] + "\n");
	fileclose();
}

return "Audited " + string.fromNum(objs.length) + " objects, " + string.fromNum(issues) + " issues, " + string.fromNum(lines.length) + " report rows -> " + REPORT;
