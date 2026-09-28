// Quick Connect (library tool) - Visual Tool that chains the selected objects with A or S connections.
// Self-contained: no external files. Built for FlexSim 2027 (27.0.2 build 356).
// Script Console > paste all of this > Execute. Then add the object to a user library like tool 01.
// Settings are labels on the tool (Properties > Labels): Link = A/S, Sort = X/Y/Tree, Run = 1 to connect.

string toolName = "Quick Connect";

string code = "/**Quick Connect: chains the selected objects with A or S connections. Set label Run = 1 to connect.*/\n// On Draw trigger of the Quick Connect Visual Tool. Settings are this tool's labels (Properties > Labels):\n//   Link = A or S          Sort = X, Y or Tree          Run = 1 connects the selection, then resets to 0\nObject current = ownerobject(c);\ntreenode view = param(1);\n\ndouble toM = getmodelunit(LENGTH_MULTIPLE); // meters per model length unit\n\nstring link = current.Link;\nstring sortBy = current.Sort;\n\nint LINK = -1;   // 0 = A, 1 = S\nif (link == \"A\" || link == \"a\") LINK = 0;\nif (link == \"S\" || link == \"s\") LINK = 1;\nint SORT = -1;   // 0 = X, 1 = Y, 2 = model tree order\nif (sortBy == \"X\" || sortBy == \"x\") SORT = 0;\nif (sortBy == \"Y\" || sortBy == \"y\") SORT = 1;\nif (sortBy == \"Tree\" || sortBy == \"tree\" || sortBy == \"TREE\") SORT = 2;\n\nif (current.Run == 1) {\n	current.Run = 0;   // reset first so it runs exactly once\n\n	Array objs = [];\n	treenode root = model();\n	for (int i = 1; i <= root.subnodes.length; i++) {\n		treenode n = root.subnodes[i];\n		if (n.dataType != DATATYPE_OBJECT || n == current)\n			continue;\n		Object o = n;\n		if (!o.flags.isSelected)\n			continue;\n		objs.push(o);\n	}\n\n	if (LINK < 0 || SORT < 0) {\n		current.LastResult = \"Not run: Link must be A or S, Sort must be X, Y or Tree\";\n	} else if (objs.length < 2) {\n		current.LastResult = \"Not run: Ctrl-select at least 2 objects first\";\n	} else {\n		// insertion sort by center X or Y\n		if (SORT < 2) {\n			for (int i = 2; i <= objs.length; i++) {\n				Object key = objs[i];\n				Vec3 kc = key.getLocation(0.5, 0.5, 0);\n				double kv = SORT == 0 ? kc.x : kc.y;\n				int j = i - 1;\n				while (j >= 1) {\n					Object cmp = objs[j];\n					Vec3 cc = cmp.getLocation(0.5, 0.5, 0);\n					double cv = SORT == 0 ? cc.x : cc.y;\n					if (cv <= kv)\n						break;\n					objs[j + 1] = cmp;\n					j--;\n				}\n				objs[j + 1] = key;\n			}\n		}\n\n		int made = 0;\n		int skipped = 0;\n		for (int i = 1; i < objs.length; i++) {\n			Object a = objs[i];\n			Object b = objs[i + 1];\n\n			// already connected? (S connections show up on both objects)\n			Array existing = LINK == 0 ? a.outObjects.toArray() : a.centerObjects.toArray();\n			int already = 0;\n			for (int k = 1; k <= existing.length; k++) {\n				Object e = existing[k];\n				if (e == b)\n					already = 1;\n			}\n\n			if (already) {\n				skipped++;\n			} else {\n				if (LINK == 0)\n					contextdragconnection(a, b, \"A\");\n				else\n					contextdragconnection(a, b, \"S\");\n				made++;\n			}\n		}\n		Object first = objs[1];\n		Object last = objs[objs.length];\n		current.LastResult = \"Made \" + string.fromNum(made) + \" connections, skipped \" + string.fromNum(skipped) + \" (\" + first.name + \" ... \" + last.name + \")\";\n	}\n}\n\n// ---- status text on the floor next to the tool ----\ndouble h = 0.35 / toM;\ndouble z = 0.02 / toM;\nstring lastMsg = current.LastResult;\n\nfglDisable(GL_LIGHTING);\ndrawtext(view, \"QUICK CONNECT   Link: \" + link + \"   Sort: \" + sortBy, 0, -1.5 * h, z, 0, h, 0, 0, 0, 0, 0.10, 0.35, 0.80, 1);\ndrawtext(view, \"Ctrl-select objects, then set label Run = 1\", 0, -3 * h, z, 0, h * 0.8, 0, 0, 0, 0, 0.05, 0.05, 0.05, 1);\ndrawtext(view, lastMsg, 0, -4.5 * h, z, 0, h * 0.8, 0, 0, 0, 0, 0.45, 0.22, 0.00, 1);\nfglEnable(GL_LIGHTING);\nreturn 0; // 0 = also draw the Visual Tool's own shape, so it stays clickable\n";

Object tool = Model.find(toolName);
if (!objectexists(tool)) {
	tool = Object.create("VisualTool");
}
tool.name = toolName;
tool.location = Vec3(0, -3, 0);
tool.rotation = Vec3(0, 0, 0);
tool.size = Vec3(1, 1, 1);

tool.labels.assert("Link", "A");
tool.labels.assert("Sort", "X");
tool.labels.assert("Run", 0);
tool.labels.assert("LastResult", "");

treenode trig = tool.attrs.assert("OnDraw", "");
trig.dataType = DATATYPE_STRING;
trig.value = code;
enablecode(trig, 1);
buildnodeflexscript(trig);

repaintall();
return "Quick Connect ready - set its labels Link (A/S) and Sort (X/Y/Tree), then Run = 1";
