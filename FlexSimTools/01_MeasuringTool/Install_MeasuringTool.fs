// Measuring Tool - labels center-to-center distance of every A and S connection.
// Self-contained: no external files. Built for FlexSim 2027 (27.0.2 build 356).
// Script Console > paste all of this > Execute. Rename the tool via toolName below.

string toolName = "Measuring Tool";

string code = "/**Measuring Tool: center-to-center plan distance in meters for every A and S connection*/\n// Paste into a Visual Tool's On Draw trigger (Custom Code).\n// Visual Tool: size 1,1,1. Its position/rotation do not matter - everything is projected into its space.\nObject current = ownerobject(c);\ntreenode view = param(1);\n\n// ---- settings (all in meters) ----\nint    SHOW_A   = 1;     // A connections (flow): solid blue\nint    SHOW_S   = 1;     // S connections (center ports, e.g. operators): dashed orange\nint    USE_3D   = 0;     // 0 = plan (XY) distance like a 2D layout drawing, 1 = true 3D distance\nint    DECIMALS = 2;\ndouble TEXT_H   = 0.30;  // label height\ndouble TICK     = 0.20;  // half-length of the end ticks\ndouble GAP      = 0.10;  // label offset from the dimension line\ndouble LIFT     = 0.02;  // draw just above the floor so the line isn't hidden\ndouble DASH     = 0.40;  // dash length for S connections\ndouble DASHGAP  = 0.25;  // gap between dashes\n// ----------------------------------\n\ndouble toM  = getmodelunit(LENGTH_MULTIPLE); // meters per model length unit\ndouble textH = TEXT_H / toM;\ndouble tick  = TICK / toM;\ndouble gap   = GAP / toM;\ndouble lift  = LIFT / toM;\ndouble dash  = DASH / toM;\ndouble dgap  = DASHGAP / toM;\n\nfglDisable(GL_LIGHTING);\n\ntreenode root = model();\nfor (int i = 1; i <= root.subnodes.length; i++) {\n	treenode n = root.subnodes[i];\n	if (n.dataType != DATATYPE_OBJECT || n == current)\n		continue;\n\n	Object a = n;\n	for (int mode = 0; mode <= 1; mode++) {\n		if (mode == 0 && !SHOW_A) continue;\n		if (mode == 1 && !SHOW_S) continue;\n\n		Array targets = mode == 0 ? a.outObjects.toArray() : a.centerObjects.toArray();\n\n		for (int p = 1; p <= targets.length; p++) {\n			Object b = targets[p];\n			if (!objectexists(b))\n				continue;\n			// S connections appear on both objects - draw each pair once\n			if (mode == 1 && b.rank < a.rank)\n				continue;\n\n			// true geometric centers (FlexSim's location is a corner, so use the 0.5 factors)\n			Vec3 pa = a.getLocation(0.5, 0.5, 0).project(a.up, current);\n			Vec3 pb = b.getLocation(0.5, 0.5, 0).project(b.up, current);\n\n			Vec3 flat = Vec3(pb.x - pa.x, pb.y - pa.y, 0);\n			double planLen = flat.magnitude;\n			if (planLen <= 0)\n				continue;\n\n			double dist = USE_3D ? (pb - pa).magnitude : planLen;\n			string label = numtostring(dist * toM, 0, DECIMALS) + \" m\";\n\n			Vec3 u = flat / planLen;          // along the line\n			Vec3 v = Vec3(-u.y, u.x, 0);      // perpendicular\n			double z = Math.max(pa.z, pb.z) + lift;\n\n			double cr = mode == 0 ? 0.10 : 0.95;   // A = blue, S = orange\n			double cg = mode == 0 ? 0.35 : 0.50;\n			double cb = mode == 0 ? 0.80 : 0.10;\n\n			if (mode == 0) {\n				// solid dimension line + end ticks\n				drawline(view, pa.x, pa.y, z, pb.x, pb.y, z, cr, cg, cb);\n				drawline(view, pa.x - v.x*tick, pa.y - v.y*tick, z, pa.x + v.x*tick, pa.y + v.y*tick, z, cr, cg, cb);\n				drawline(view, pb.x - v.x*tick, pb.y - v.y*tick, z, pb.x + v.x*tick, pb.y + v.y*tick, z, cr, cg, cb);\n			} else {\n				// dashed line\n				double pos = 0;\n				while (pos < planLen) {\n					double e = Math.min(pos + dash, planLen);\n					drawline(view, pa.x + u.x*pos, pa.y + u.y*pos, z, pa.x + u.x*e, pa.y + u.y*e, z, cr, cg, cb);\n					pos = e + dgap;\n				}\n			}\n\n			// keep the text readable (never upside down)\n			double angle = Math.degrees(Math.atan2(u.y, u.x));\n			Vec3 td = u;\n			if (angle > 90 || angle < -90) {\n				angle += 180;\n				td = Vec3(-u.x, -u.y, 0);\n			}\n			Vec3 tn = Vec3(-td.y, td.x, 0);   // \"up\" side of the text\n\n			double halfW = label.length * textH * 0.55 / 2;\n			double tx = (pa.x + pb.x) / 2 - td.x*halfW + tn.x*gap;\n			double ty = (pa.y + pb.y) / 2 - td.y*halfW + tn.y*gap;\n\n			double tr = mode == 0 ? 0.05 : 0.45;\n			double tg = mode == 0 ? 0.05 : 0.22;\n			double tb = mode == 0 ? 0.05 : 0.00;\n			drawtext(view, label, tx, ty, z, 0, textH, 0, 0, 0, angle, tr, tg, tb, 1);\n		}\n	}\n}\n\nfglEnable(GL_LIGHTING);\nreturn 0; // 0 = also draw the Visual Tool's own shape, so it stays clickable\n";

Object tool = Model.find(toolName);
if (!objectexists(tool)) {
	tool = Object.create("VisualTool");
}
tool.name = toolName;
tool.location = Vec3(0, 0, 0);
tool.rotation = Vec3(0, 0, 0);
tool.size = Vec3(1, 1, 1);

treenode trig = tool.attrs.assert("OnDraw", "");
trig.dataType = DATATYPE_STRING;
trig.value = code;
enablecode(trig, 1);
buildnodeflexscript(trig);

repaintall();
return "Measuring Tool ready - A connections solid blue, S connections dashed orange";
