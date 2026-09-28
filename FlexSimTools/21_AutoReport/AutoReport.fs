// 21 - Auto Report. After a run, exports per-object KPIs to CSV and saves a 3D screenshot.
// Open the CSV in Excel and chart it, or paste straight into the deck.
// FlexSim 2027: run the model first, then Script Console > paste > Execute. Changes nothing.

// ---- settings ----
string FOLDER  = "C:/FlexSimTools/reports/";
string RUNNAME = "baseline";        // becomes part of the file names
int    SHOT    = 1;                 // 1 = also save a 3D screenshot
// ------------------

string csv = FOLDER + RUNNAME + "_kpi.csv";

if (!fileopen(csv, "w"))
	return "Could not write " + csv + " - does the folder exist?";

fpt("Object,Class,Input,Output,AvgContent,AvgStayTime_s\n");

int rows = 0;
double totalOut = 0;
treenode root = model();
for (int i = 1; i <= root.subnodes.length; i++) {
	treenode n = root.subnodes[i];
	if (n.dataType != DATATYPE_OBJECT)
		continue;
	Object o = n;

	double in  = o.stats.input.value;
	double out = o.stats.output.value;
	double content = o.stats.content.average;      // if .average errors, tell me
	double stay = o.stats.staytime.average;

	fpt(o.name + "," + o.classname + "," + numtostring(in, 0, 0) + "," + numtostring(out, 0, 0) + "," + numtostring(content, 0, 2) + "," + numtostring(stay, 0, 2) + "\n");
	totalOut += out;
	rows++;
}

fpt("\nModel time," + numtostring(Model.time, 0, 2) + "\n");
fileclose();

string shotMsg = "";
if (SHOT) {
	string png = FOLDER + RUNNAME + "_view.png";
	viewtofile(activeview(), png);     // if viewtofile errors, drop SHOT to 0 and tell me
	shotMsg = " | screenshot " + png;
}

return "Reported " + string.fromNum(rows) + " objects, total output " + numtostring(totalOut, 0, 0) + " -> " + csv + shotMsg;
