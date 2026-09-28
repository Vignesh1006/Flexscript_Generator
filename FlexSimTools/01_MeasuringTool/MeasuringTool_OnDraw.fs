/**Measuring Tool: center-to-center plan distance in meters for every A and S connection*/
// Paste into a Visual Tool's On Draw trigger (Custom Code).
// Visual Tool: size 1,1,1. Its position/rotation do not matter - everything is projected into its space.
Object current = ownerobject(c);
treenode view = param(1);

// ---- settings (all in meters) ----
int    SHOW_A   = 1;     // A connections (flow): solid blue
int    SHOW_S   = 1;     // S connections (center ports, e.g. operators): dashed orange
int    USE_3D   = 0;     // 0 = plan (XY) distance like a 2D layout drawing, 1 = true 3D distance
int    DECIMALS = 2;
double TEXT_H   = 0.30;  // label height
double TICK     = 0.20;  // half-length of the end ticks
double GAP      = 0.10;  // label offset from the dimension line
double LIFT     = 0.02;  // draw just above the floor so the line isn't hidden
double DASH     = 0.40;  // dash length for S connections
double DASHGAP  = 0.25;  // gap between dashes
// ----------------------------------

double toM  = getmodelunit(LENGTH_MULTIPLE); // meters per model length unit
double textH = TEXT_H / toM;
double tick  = TICK / toM;
double gap   = GAP / toM;
double lift  = LIFT / toM;
double dash  = DASH / toM;
double dgap  = DASHGAP / toM;

fglDisable(GL_LIGHTING);

treenode root = model();
for (int i = 1; i <= root.subnodes.length; i++) {
	treenode n = root.subnodes[i];
	if (n.dataType != DATATYPE_OBJECT || n == current)
		continue;

	Object a = n;
	for (int mode = 0; mode <= 1; mode++) {
		if (mode == 0 && !SHOW_A) continue;
		if (mode == 1 && !SHOW_S) continue;

		Array targets = mode == 0 ? a.outObjects.toArray() : a.centerObjects.toArray();

		for (int p = 1; p <= targets.length; p++) {
			Object b = targets[p];
			if (!objectexists(b))
				continue;
			// S connections appear on both objects - draw each pair once
			if (mode == 1 && b.rank < a.rank)
				continue;

			// true geometric centers (FlexSim's location is a corner, so use the 0.5 factors)
			Vec3 pa = a.getLocation(0.5, 0.5, 0).project(a.up, current);
			Vec3 pb = b.getLocation(0.5, 0.5, 0).project(b.up, current);

			Vec3 flat = Vec3(pb.x - pa.x, pb.y - pa.y, 0);
			double planLen = flat.magnitude;
			if (planLen <= 0)
				continue;

			double dist = USE_3D ? (pb - pa).magnitude : planLen;
			string label = numtostring(dist * toM, 0, DECIMALS) + " m";

			Vec3 u = flat / planLen;          // along the line
			Vec3 v = Vec3(-u.y, u.x, 0);      // perpendicular
			double z = Math.max(pa.z, pb.z) + lift;

			double cr = mode == 0 ? 0.10 : 0.95;   // A = blue, S = orange
			double cg = mode == 0 ? 0.35 : 0.50;
			double cb = mode == 0 ? 0.80 : 0.10;

			if (mode == 0) {
				// solid dimension line + end ticks
				drawline(view, pa.x, pa.y, z, pb.x, pb.y, z, cr, cg, cb);
				drawline(view, pa.x - v.x*tick, pa.y - v.y*tick, z, pa.x + v.x*tick, pa.y + v.y*tick, z, cr, cg, cb);
				drawline(view, pb.x - v.x*tick, pb.y - v.y*tick, z, pb.x + v.x*tick, pb.y + v.y*tick, z, cr, cg, cb);
			} else {
				// dashed line
				double pos = 0;
				while (pos < planLen) {
					double e = Math.min(pos + dash, planLen);
					drawline(view, pa.x + u.x*pos, pa.y + u.y*pos, z, pa.x + u.x*e, pa.y + u.y*e, z, cr, cg, cb);
					pos = e + dgap;
				}
			}

			// keep the text readable (never upside down)
			double angle = Math.degrees(Math.atan2(u.y, u.x));
			Vec3 td = u;
			if (angle > 90 || angle < -90) {
				angle += 180;
				td = Vec3(-u.x, -u.y, 0);
			}
			Vec3 tn = Vec3(-td.y, td.x, 0);   // "up" side of the text

			double halfW = label.length * textH * 0.55 / 2;
			double tx = (pa.x + pb.x) / 2 - td.x*halfW + tn.x*gap;
			double ty = (pa.y + pb.y) / 2 - td.y*halfW + tn.y*gap;

			double tr = mode == 0 ? 0.05 : 0.45;
			double tg = mode == 0 ? 0.05 : 0.22;
			double tb = mode == 0 ? 0.05 : 0.00;
			drawtext(view, label, tx, ty, z, 0, textH, 0, 0, 0, angle, tr, tg, tb, 1);
		}
	}
}

fglEnable(GL_LIGHTING);
return 0; // 0 = also draw the Visual Tool's own shape, so it stays clickable
