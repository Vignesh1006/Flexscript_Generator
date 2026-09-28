// Align & Distribute - tidies the selected objects into a straight, evenly spaced row or column.
// FlexSim 2027: select the objects in the 3D view, then Script Console > paste > Execute.
// Save the model first - this moves objects.

// ---- settings ----
int    MODE  = 0;    // 0 = align only, 1 = align + spread evenly end to end, 2 = align + fixed gap
int    AXIS  = 0;    // 0 = row along X (aligns Y), 1 = column along Y (aligns X)
int    LINE  = 0;    // where the line sits: 0 = average, 1 = lowest, 2 = highest
double GAP   = 2.0;  // metres of clear space between objects, MODE 2 only
// ------------------

double toM = getmodelunit(LENGTH_MULTIPLE);
double gap = GAP / toM;

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

// sort along the travel axis, by center
for (int i = 2; i <= objs.length; i++) {
	Object key = objs[i];
	Vec3 kc = key.getLocation(0.5, 0.5, 0);
	double kv = AXIS == 0 ? kc.x : kc.y;
	int j = i - 1;
	while (j >= 1) {
		Object cmp = objs[j];
		Vec3 cc = cmp.getLocation(0.5, 0.5, 0);
		double cv = AXIS == 0 ? cc.x : cc.y;
		if (cv <= kv)
			break;
		objs[j + 1] = cmp;
		j--;
	}
	objs[j + 1] = key;
}

// the cross-axis value every object will share
double lineVal = 0;
double lo = 0;
double hi = 0;
for (int i = 1; i <= objs.length; i++) {
	Object o = objs[i];
	Vec3 c = o.getLocation(0.5, 0.5, 0);
	double cross = AXIS == 0 ? c.y : c.x;
	lineVal += cross;
	if (i == 1 || cross < lo) lo = cross;
	if (i == 1 || cross > hi) hi = cross;
}
lineVal = lineVal / objs.length;
if (LINE == 1) lineVal = lo;
if (LINE == 2) lineVal = hi;

// first and last positions along the travel axis stay put; the rest are placed between them
Object firstObj = objs[1];
Object lastObj = objs[objs.length];
Vec3 fc = firstObj.getLocation(0.5, 0.5, 0);
Vec3 lc = lastObj.getLocation(0.5, 0.5, 0);
double startPos = AXIS == 0 ? fc.x : fc.y;
double endPos = AXIS == 0 ? lc.x : lc.y;
double step = objs.length > 1 ? (endPos - startPos) / (objs.length - 1) : 0;

double cursor = startPos;
for (int i = 1; i <= objs.length; i++) {
	Object o = objs[i];
	Vec3 c = o.getLocation(0.5, 0.5, 0);
	double along = AXIS == 0 ? c.x : c.y;

	if (MODE == 1)
		along = startPos + step * (i - 1);

	if (MODE == 2) {
		double half = AXIS == 0 ? o.size.x / 2 : o.size.y / 2;
		if (i == 1) {
			along = startPos;
			cursor = along + half;
		} else {
			along = cursor + gap + half;
			cursor = along + half;
		}
	}

	double x = AXIS == 0 ? along : lineVal;
	double y = AXIS == 0 ? lineVal : along;
	o.setLocation(x, y, c.z, 0.5, 0.5, 0);
}

repaintall();
string what = MODE == 0 ? "aligned" : (MODE == 1 ? "aligned and spread evenly" : "aligned with fixed gap");
return string.fromNum(objs.length) + " objects " + what;
