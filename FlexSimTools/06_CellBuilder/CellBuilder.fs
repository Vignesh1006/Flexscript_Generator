// 06 - Cell Builder. Creates a standard cell: named, spaced and connected in one run.
// Copy this file per cell type and change the settings block.
// FlexSim 2027: Script Console > paste > Execute.

// ---- settings ----
string PREFIX  = "PACK_A";                 // names become PACK_A_01, PACK_A_02, ...
Array  TYPES   = ["Source", "Queue", "Processor", "Processor", "Sink"];
double SPACING = 3.0;                      // metres between object centers
int    AXIS    = 0;                        // 0 = build along X, 1 = build along Y
Vec3   ORIGIN  = Vec3(0, 0, 0);            // center of the first object
int    CONNECT = 1;                        // 1 = A-connect the chain in order
string OPERATOR = "";                      // optional: S-connect every object to this operator
// ------------------

double toM = getmodelunit(LENGTH_MULTIPLE);
double step = SPACING / toM;

Array made = [];
for (int i = 1; i <= TYPES.length; i++) {
	string nm = PREFIX + "_" + (i < 10 ? "0" : "") + string.fromNum(i);

	Object o = Model.find(nm);
	if (!objectexists(o)) {
		o = Object.create(TYPES[i]);
		o.name = nm;
	}

	double along = step * (i - 1);
	double x = AXIS == 0 ? ORIGIN.x + along : ORIGIN.x;
	double y = AXIS == 0 ? ORIGIN.y : ORIGIN.y + along;
	o.setLocation(x, y, ORIGIN.z, 0.5, 0.5, 0);
	made.push(o);
}

int links = 0;
if (CONNECT) {
	for (int i = 1; i < made.length; i++) {
		Object a = made[i];
		Object b = made[i + 1];
		contextdragconnection(a, b, "A");
		links++;
	}
}

if (OPERATOR.length > 0) {
	Object op = Model.find(OPERATOR);
	if (objectexists(op)) {
		for (int i = 1; i <= made.length; i++) {
			Object a = made[i];
			contextdragconnection(a, op, "S");
		}
	}
}

repaintall();
return "Cell " + PREFIX + ": " + string.fromNum(made.length) + " objects, " + string.fromNum(links) + " links";
