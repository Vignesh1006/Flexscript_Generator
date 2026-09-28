// 20 - Model Cleanup. Reports things that look unused. Deletes nothing unless you ask twice.
// FlexSim 2027: Script Console > paste > Execute.

// ---- settings ----
int    DELETE = 0;    // 0 = report only (always start here), 1 = actually delete
string REPORT = "C:/FlexSimTools/cleanup_report.csv";
// ------------------

Array lines = [];
int candidates = 0;
int deleted = 0;

// ---------- 1. objects with no connections at all ----------
treenode root = model();
Array orphans = [];
for (int i = 1; i <= root.subnodes.length; i++) {
	treenode n = root.subnodes[i];
	if (n.dataType != DATATYPE_OBJECT)
		continue;
	Object o = n;
	if (o.outObjects.length == 0 && o.inObjects.length == 0 && o.centerObjects.length == 0) {
		orphans.push(o);
		lines.push("object," + o.name + ",no connections");
		candidates++;
	}
}

// ---------- 2. global tables never named in any code ----------
treenode tables = node("MODEL:/Tools/GlobalTables");
if (objectexists(tables)) {
	for (int t = 1; t <= tables.subnodes.length; t++) {
		treenode tab = tables.subnodes[t];
		string nm = tab.name;

		// search every string in the model tree for the table name
		int used = 0;
		Array stack = [root];
		while (stack.length > 0 && !used) {
			treenode cur = stack.pop();
			for (int s = 1; s <= cur.subnodes.length; s++) {
				treenode kid = cur.subnodes[s];
				if (kid.dataType == DATATYPE_STRING) {
					string v = kid.value;
					if (v.includes(nm))
						used = 1;
				}
				stack.push(kid);
			}
		}
		if (!used) {
			lines.push("table," + nm + ",name not found in any code");
			candidates++;
		}
	}
}

// ---------- delete ----------
if (DELETE) {
	for (int i = 1; i <= orphans.length; i++) {
		Object o = orphans[i];
		destroyobject(o);
		deleted++;
	}
	repaintall();
}

if (fileopen(REPORT, "w")) {
	fpt("Kind,Name,Why\n");
	for (int i = 1; i <= lines.length; i++)
		fpt(lines[i] + "\n");
	fileclose();
}

string msg = string.fromNum(candidates) + " cleanup candidates -> " + REPORT;
if (DELETE)
	msg += " | DELETED " + string.fromNum(deleted) + " orphan objects (tables were left alone)";
else
	msg += " | nothing deleted - review the report, then set DELETE = 1";
return msg;
