/**Quick Connect: chains the selected objects with A or S connections. Set label Run = 1 to connect.*/
// On Draw trigger of the Quick Connect Visual Tool. Settings are this tool's labels (Properties > Labels):
//   Link = A or S          Sort = X, Y or Tree          Run = 1 connects the selection, then resets to 0
Object current = ownerobject(c);
treenode view = param(1);

double toM = getmodelunit(LENGTH_MULTIPLE); // meters per model length unit

string link = current.Link;
string sortBy = current.Sort;

int LINK = -1;   // 0 = A, 1 = S
if (link == "A" || link == "a") LINK = 0;
if (link == "S" || link == "s") LINK = 1;
int SORT = -1;   // 0 = X, 1 = Y, 2 = model tree order
if (sortBy == "X" || sortBy == "x") SORT = 0;
if (sortBy == "Y" || sortBy == "y") SORT = 1;
if (sortBy == "Tree" || sortBy == "tree" || sortBy == "TREE") SORT = 2;

if (current.Run == 1) {
	current.Run = 0;   // reset first so it runs exactly once

	Array objs = [];
	treenode root = model();
	for (int i = 1; i <= root.subnodes.length; i++) {
		treenode n = root.subnodes[i];
		if (n.dataType != DATATYPE_OBJECT || n == current)
			continue;
		Object o = n;
		if (!o.flags.isSelected)
			continue;
		objs.push(o);
	}

	if (LINK < 0 || SORT < 0) {
		current.LastResult = "Not run: Link must be A or S, Sort must be X, Y or Tree";
	} else if (objs.length < 2) {
		current.LastResult = "Not run: Ctrl-select at least 2 objects first";
	} else {
		// insertion sort by center X or Y
		if (SORT < 2) {
			for (int i = 2; i <= objs.length; i++) {
				Object key = objs[i];
				Vec3 kc = key.getLocation(0.5, 0.5, 0);
				double kv = SORT == 0 ? kc.x : kc.y;
				int j = i - 1;
				while (j >= 1) {
					Object cmp = objs[j];
					Vec3 cc = cmp.getLocation(0.5, 0.5, 0);
					double cv = SORT == 0 ? cc.x : cc.y;
					if (cv <= kv)
						break;
					objs[j + 1] = cmp;
					j--;
				}
				objs[j + 1] = key;
			}
		}

		int made = 0;
		int skipped = 0;
		for (int i = 1; i < objs.length; i++) {
			Object a = objs[i];
			Object b = objs[i + 1];

			// already connected? (S connections show up on both objects)
			Array existing = LINK == 0 ? a.outObjects.toArray() : a.centerObjects.toArray();
			int already = 0;
			for (int k = 1; k <= existing.length; k++) {
				Object e = existing[k];
				if (e == b)
					already = 1;
			}

			if (already) {
				skipped++;
			} else {
				if (LINK == 0)
					contextdragconnection(a, b, "A");
				else
					contextdragconnection(a, b, "S");
				made++;
			}
		}
		Object first = objs[1];
		Object last = objs[objs.length];
		current.LastResult = "Made " + string.fromNum(made) + " connections, skipped " + string.fromNum(skipped) + " (" + first.name + " ... " + last.name + ")";
	}
}

// ---- status text on the floor next to the tool ----
double h = 0.35 / toM;
double z = 0.02 / toM;
string lastMsg = current.LastResult;

fglDisable(GL_LIGHTING);
drawtext(view, "QUICK CONNECT   Link: " + link + "   Sort: " + sortBy, 0, -1.5 * h, z, 0, h, 0, 0, 0, 0, 0.10, 0.35, 0.80, 1);
drawtext(view, "Ctrl-select objects, then set label Run = 1", 0, -3 * h, z, 0, h * 0.8, 0, 0, 0, 0, 0.05, 0.05, 0.05, 1);
drawtext(view, lastMsg, 0, -4.5 * h, z, 0, h * 0.8, 0, 0, 0, 0, 0.45, 0.22, 0.00, 1);
fglEnable(GL_LIGHTING);
return 0; // 0 = also draw the Visual Tool's own shape, so it stays clickable
