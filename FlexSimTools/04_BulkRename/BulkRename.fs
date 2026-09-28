// 04 - Bulk Rename. Renames the selected objects to a pattern with running numbers.
// FlexSim 2027: select objects in the 3D view, then Script Console > paste > Execute.

// ---- settings ----
string PREFIX = "LINE1_PR";   // name prefix
int    START  = 1;            // first number
int    DIGITS = 2;            // 2 -> 01, 3 -> 001
int    SORT   = 0;            // 0 = left to right (X), 1 = bottom to top (Y), 2 = tree order
// ------------------

Array objs = [];
treenode root = model();
for (int i = 1; i <= root.subnodes.length; i++) {
	treenode n = root.subnodes[i];
	if (n.dataType != DATATYPE_OBJECT)
		continue;
	Object o = n;
	if (o.flags.isSelected)
		objs.push(o);
}

if (objs.length < 1)
	return "Select the objects you want to rename first";

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

int num = START;
string list = "";
for (int i = 1; i <= objs.length; i++) {
	Object o = objs[i];
	string s = string.fromNum(num);
	while (s.length < DIGITS)
		s = "0" + s;
	o.name = PREFIX + s;
	list += o.name + "  ";
	num++;
}

repaintall();
return "Renamed " + string.fromNum(objs.length) + ": " + list;
