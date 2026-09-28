/**Flow Heatmap: draws each A connection with thickness and color by the volume that flowed through it*/
// Goes in a Visual Tool's On Draw trigger. Run the model first - with no run there is no flow.
Object current = ownerobject(c);
treenode view = param(1);

// ---- settings ----
double MAX_WIDTH = 0.60;   // metres: line width at the busiest link
int    SHOW_QTY  = 1;      // 1 = print the item count on each link
double TEXT_H    = 0.30;
double LIFT      = 0.03;
// ------------------

double toM = getmodelunit(LENGTH_MULTIPLE);
double maxW = MAX_WIDTH / toM;
double textH = TEXT_H / toM;
double lift = LIFT / toM;

// pass 1: find the busiest link so colors are relative to this model
double peak = 0;
treenode root = model();
for (int i = 1; i <= root.subnodes.length; i++) {
	treenode n = root.subnodes[i];
	if (n.dataType != DATATYPE_OBJECT || n == current)
		continue;
	Object a = n;
	double q = a.stats.output.value;
	if (q > peak)
		peak = q;
}
if (peak <= 0)
	peak = 1;

fglDisable(GL_LIGHTING);

// pass 2: draw
for (int i = 1; i <= root.subnodes.length; i++) {
	treenode n = root.subnodes[i];
	if (n.dataType != DATATYPE_OBJECT || n == current)
		continue;
	Object a = n;

	double qty = a.stats.output.value;
	double share = qty / peak;
	if (share <= 0)
		continue;

	for (int p = 1; p <= a.outObjects.length; p++) {
		Object b = a.outObjects[p];
		if (!objectexists(b))
			continue;

		Vec3 pa = a.getLocation(0.5, 0.5, 0).project(a.up, current);
		Vec3 pb = b.getLocation(0.5, 0.5, 0).project(b.up, current);
		Vec3 flat = Vec3(pb.x - pa.x, pb.y - pa.y, 0);
		double len = flat.magnitude;
		if (len <= 0)
			continue;

		Vec3 u = flat / len;
		Vec3 v = Vec3(-u.y, u.x, 0);
		double z = Math.max(pa.z, pb.z) + lift;

		// cool blue (quiet) to hot red (busy)
		double cr = share;
		double cg = 0.25;
		double cb = 1 - share;

		// thickness by stacking parallel lines
		double w = maxW * share;
		int strands = 1 + (int)(w / (0.02 / toM));
		if (strands > 20)
			strands = 20;
		for (int s = 0; s < strands; s++) {
			double off = (s - (strands - 1) / 2.0) * (w / Math.max(strands, 1));
			drawline(view, pa.x + v.x*off, pa.y + v.y*off, z, pb.x + v.x*off, pb.y + v.y*off, z, cr, cg, cb);
		}

		if (SHOW_QTY) {
			double angle = Math.degrees(Math.atan2(u.y, u.x));
			Vec3 td = u;
			if (angle > 90 || angle < -90) {
				angle += 180;
				td = Vec3(-u.x, -u.y, 0);
			}
			Vec3 tn = Vec3(-td.y, td.x, 0);
			string label = numtostring(qty, 0, 0);
			double halfW = label.length * textH * 0.55 / 2;
			double tx = (pa.x + pb.x) / 2 - td.x*halfW + tn.x*(w + textH*0.6);
			double ty = (pa.y + pb.y) / 2 - td.y*halfW + tn.y*(w + textH*0.6);
			drawtext(view, label, tx, ty, z, 0, textH, 0, 0, 0, angle, 0.1, 0.1, 0.1, 1);
		}
	}
}

fglEnable(GL_LIGHTING);
return 0;
