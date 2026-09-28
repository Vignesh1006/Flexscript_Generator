// 17 - Flow Heatmap installer. Creates the overlay object and loads its draw code.
// FlexSim 2027: Script Console > paste all of this > Execute. Run the model to see flow.

string toolName = "Flow Heatmap";

string code = "/**Flow Heatmap: draws each A connection with thickness and color by the volume that flowed through it*/\n// Goes in a Visual Tool's On Draw trigger. Run the model first - with no run there is no flow.\nObject current = ownerobject(c);\ntreenode view = param(1);\n\n// ---- settings ----\ndouble MAX_WIDTH = 0.60;   // metres: line width at the busiest link\nint    SHOW_QTY  = 1;      // 1 = print the item count on each link\ndouble TEXT_H    = 0.30;\ndouble LIFT      = 0.03;\n// ------------------\n\ndouble toM = getmodelunit(LENGTH_MULTIPLE);\ndouble maxW = MAX_WIDTH / toM;\ndouble textH = TEXT_H / toM;\ndouble lift = LIFT / toM;\n\n// pass 1: find the busiest link so colors are relative to this model\ndouble peak = 0;\ntreenode root = model();\nfor (int i = 1; i <= root.subnodes.length; i++) {\n	treenode n = root.subnodes[i];\n	if (n.dataType != DATATYPE_OBJECT || n == current)\n		continue;\n	Object a = n;\n	double q = a.stats.output.value;\n	if (q > peak)\n		peak = q;\n}\nif (peak <= 0)\n	peak = 1;\n\nfglDisable(GL_LIGHTING);\n\n// pass 2: draw\nfor (int i = 1; i <= root.subnodes.length; i++) {\n	treenode n = root.subnodes[i];\n	if (n.dataType != DATATYPE_OBJECT || n == current)\n		continue;\n	Object a = n;\n\n	double qty = a.stats.output.value;\n	double share = qty / peak;\n	if (share <= 0)\n		continue;\n\n	for (int p = 1; p <= a.outObjects.length; p++) {\n		Object b = a.outObjects[p];\n		if (!objectexists(b))\n			continue;\n\n		Vec3 pa = a.getLocation(0.5, 0.5, 0).project(a.up, current);\n		Vec3 pb = b.getLocation(0.5, 0.5, 0).project(b.up, current);\n		Vec3 flat = Vec3(pb.x - pa.x, pb.y - pa.y, 0);\n		double len = flat.magnitude;\n		if (len <= 0)\n			continue;\n\n		Vec3 u = flat / len;\n		Vec3 v = Vec3(-u.y, u.x, 0);\n		double z = Math.max(pa.z, pb.z) + lift;\n\n		// cool blue (quiet) to hot red (busy)\n		double cr = share;\n		double cg = 0.25;\n		double cb = 1 - share;\n\n		// thickness by stacking parallel lines\n		double w = maxW * share;\n		int strands = 1 + (int)(w / (0.02 / toM));\n		if (strands > 20)\n			strands = 20;\n		for (int s = 0; s < strands; s++) {\n			double off = (s - (strands - 1) / 2.0) * (w / Math.max(strands, 1));\n			drawline(view, pa.x + v.x*off, pa.y + v.y*off, z, pb.x + v.x*off, pb.y + v.y*off, z, cr, cg, cb);\n		}\n\n		if (SHOW_QTY) {\n			double angle = Math.degrees(Math.atan2(u.y, u.x));\n			Vec3 td = u;\n			if (angle > 90 || angle < -90) {\n				angle += 180;\n				td = Vec3(-u.x, -u.y, 0);\n			}\n			Vec3 tn = Vec3(-td.y, td.x, 0);\n			string label = numtostring(qty, 0, 0);\n			double halfW = label.length * textH * 0.55 / 2;\n			double tx = (pa.x + pb.x) / 2 - td.x*halfW + tn.x*(w + textH*0.6);\n			double ty = (pa.y + pb.y) / 2 - td.y*halfW + tn.y*(w + textH*0.6);\n			drawtext(view, label, tx, ty, z, 0, textH, 0, 0, 0, angle, 0.1, 0.1, 0.1, 1);\n		}\n	}\n}\n\nfglEnable(GL_LIGHTING);\nreturn 0;\n";

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
return "Flow Heatmap ready - reset and run the model, then look at the links";
