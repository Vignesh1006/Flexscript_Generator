// Quick Connect (GUI) - chains the selected objects together with A or S connections.
// FlexSim 2027: select the objects in the 3D view, then run this (Script Console or a toolbar button).
// Asks two questions - connection type, then order - and skips pairs that are already connected.

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

if (objs.length < 2) {
	msg("Quick Connect", "Select at least 2 objects in the 3D view first (Ctrl+click, or Ctrl+drag a box).");
	return "Select at least 2 objects in the 3D view first";
}

// ---- question 1: connection type ----
int ans = msg("Quick Connect - connection type", string.fromNum(objs.length) + " objects selected.\n\nYes = A connection (flow: output to input)\nNo = S connection (center ports, e.g. operators)\nCancel = stop", 1);
if (ans != 1 && ans != 0)
	return "Cancelled";
int LINK = 0;   // 0 = A, 1 = S
if (ans == 0)
	LINK = 1;

// ---- question 2: order ----
ans = msg("Quick Connect - order", "Chain the objects in which order?\n\nYes = left to right (X)\nNo = bottom to top (Y)\nCancel = stop", 1);
if (ans != 1 && ans != 0)
	return "Cancelled";
int SORT = 0;   // 0 = X, 1 = Y
if (ans == 0)
	SORT = 1;

// insertion sort by center X or Y
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

int made = 0;
int skipped = 0;
string chain = "";
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
	chain += a.name + " > ";
}
Object last = objs[objs.length];
chain += last.name;

repaintall();

string type = "A";
if (LINK == 1)
	type = "S";
string result = "Made " + string.fromNum(made) + " " + type + " connections, skipped " + string.fromNum(skipped) + " already connected.\n\n" + chain;
msg("Quick Connect - done", result);
return result;
