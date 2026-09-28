// 05 - Model from Spreadsheet. Builds, places, names and connects objects from a CSV.
// Save the sheet from Excel as CSV. Column order must be:
//   Type,Name,X,Y,Z,RZ,SizeX,SizeY,SizeZ,ConnectTo,SConnectTo
// ConnectTo / SConnectTo may list several targets separated by ;
// Re-runnable: an existing name is updated, not duplicated.
// FlexSim 2027: Script Console > paste > Execute.

// ---- settings ----
string CSV        = "C:/FlexSimTools/model_layout.csv";
int    HAS_HEADER = 1;   // 1 = first line is column titles
int    DO_CONNECT = 1;   // 0 = place only, no connections
// ------------------

if (!fileopen(CSV, "r"))
	return "Could not open " + CSV;

Array rows = [];
while (!endoffile()) {
	string line = filereadline();
	if (line.length > 2)
		rows.push(line);
}
fileclose();

int first = HAS_HEADER ? 2 : 1;
if (rows.length < first)
	return "No data rows in " + CSV;

// ---- pass 1: create / update objects ----
int made = 0;
int updated = 0;
string bad = "";
for (int r = first; r <= rows.length; r++) {
	string line = rows[r];
	Array f = line.split(",");     // if split() errors on 2027, tell me and I will hand-parse
	if (f.length < 4)
		continue;

	string type = f[1];
	string nm   = f[2];

	Object o = Model.find(nm);
	if (!objectexists(o)) {
		o = Object.create(type);
		o.name = nm;
		made++;
	} else {
		updated++;
	}

	double x  = stringtonum(f[3]);
	double y  = stringtonum(f[4]);
	double z  = f.length >= 5 ? stringtonum(f[5]) : 0;
	double rz = f.length >= 6 ? stringtonum(f[6]) : 0;
	o.setLocation(x, y, z, 0.5, 0.5, 0);
	o.rotation = Vec3(0, 0, rz);

	if (f.length >= 9) {
		double sx = stringtonum(f[7]);
		double sy = stringtonum(f[8]);
		double sz = stringtonum(f[9]);
		if (sx > 0 && sy > 0 && sz > 0)
			o.size = Vec3(sx, sy, sz);
	}
}

// ---- pass 2: connections ----
int links = 0;
if (DO_CONNECT) {
	for (int r = first; r <= rows.length; r++) {
		Array f = rows[r].split(",");
		if (f.length < 10)
			continue;
		Object a = Model.find(f[2]);
		if (!objectexists(a))
			continue;

		for (int mode = 0; mode <= 1; mode++) {
			int col = mode == 0 ? 10 : 11;
			if (f.length < col)
				continue;
			string targets = f[col];
			if (targets.length < 1)
				continue;

			Array names = targets.split(";");
			for (int t = 1; t <= names.length; t++) {
				Object b = Model.find(names[t]);
				if (!objectexists(b)) {
					bad += names[t] + " ";
					continue;
				}
				if (mode == 0)
					contextdragconnection(a, b, "A");
				else
					contextdragconnection(a, b, "S");
				links++;
			}
		}
	}
}

repaintall();
string msg = "Created " + string.fromNum(made) + ", updated " + string.fromNum(updated) + ", connections " + string.fromNum(links);
if (bad.length > 0)
	msg += " | names not found: " + bad;
return msg;
