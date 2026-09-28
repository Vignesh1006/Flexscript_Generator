// 07 - Mirror & Array. Copies the selection, either repeated at a pitch or mirrored about a line.
// FlexSim 2027: select the objects, then Script Console > paste > Execute.
// Save the model first - this creates objects.

// ---- settings ----
int    MODE      = 0;      // 0 = array (repeat), 1 = mirror
int    AXIS      = 1;      // 0 = along/about X, 1 = along/about Y
int    COPIES    = 2;      // array only: how many copies
double PITCH     = 10.0;   // array only: metres between copies
double MIRROR_AT = 0.0;    // mirror only: the axis value to mirror about (metres)
string SUFFIX    = "_B";   // added to copied names; array copies also get a number
// ------------------

double toM = getmodelunit(LENGTH_MULTIPLE);
double pitch = PITCH / toM;
double mirrorAt = MIRROR_AT / toM;

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
	return "Select the objects you want to copy first";

int total = 0;
int rounds = MODE == 0 ? COPIES : 1;

for (int c = 1; c <= rounds; c++) {
	for (int i = 1; i <= objs.length; i++) {
		Object src = objs[i];
		Object cp = createcopy(src, model());   // if createcopy errors on 2027, tell me
		Vec3 sc = src.getLocation(0.5, 0.5, 0);

		double x = sc.x;
		double y = sc.y;
		if (MODE == 0) {
			if (AXIS == 0) x = sc.x + pitch * c;
			else           y = sc.y + pitch * c;
			cp.name = src.name + SUFFIX + string.fromNum(c);
		} else {
			if (AXIS == 0) x = 2 * mirrorAt - sc.x;
			else           y = 2 * mirrorAt - sc.y;
			cp.name = src.name + SUFFIX;
		}

		cp.setLocation(x, y, sc.z, 0.5, 0.5, 0);
		if (MODE == 1) {
			Vec3 r = src.rotation;
			cp.rotation = Vec3(r.x, r.y, -r.z);   // mirrored orientation
		}
		total++;
	}
}

repaintall();
return "Created " + string.fromNum(total) + " copies. Connections are NOT copied - run Quick Connect on them.";
