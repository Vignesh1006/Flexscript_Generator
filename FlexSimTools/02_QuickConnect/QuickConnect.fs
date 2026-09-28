// Quick Connect - chains the selected objects together with A or S connections.
// FlexSim 2027: select the objects in the 3D view, then Script Console > paste > Execute.

// ---- settings ----
int SORT = 0;   // 0 = left to right (X), 1 = bottom to top (Y), 2 = model tree order
int LINK = 0;   // 0 = A connection (flow), 1 = S connection (center ports)
// ------------------

Array objs = [];
treenode root = model();
for (int i = 1; i <= root.subnodes.length; i++) {
	treenode n = root.subnodes[i];
	if (n.dataType != DATATYPE_OBJECT)
		continue;
	Object o = n;
	if (!o.flags.isSelected)
		continue;
	objs.push(o);
}

if (objs.length < 2)
	return "Select at least 2 objects in the 3D view first";

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
string chain = "";
for (int i = 1; i < objs.length; i++) {
	Object a = objs[i];
	Object b = objs[i + 1];
	if (LINK == 0)
		contextdragconnection(a, b, "A");
	else
		contextdragconnection(a, b, "S");
	made++;
	chain += a.name + " > ";
}
Object last = objs[objs.length];
chain += last.name;

repaintall();
return "Connected " + string.fromNum(made) + " links: " + chain;
