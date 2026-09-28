// 08 - CAD Background Calibrator. Scales a layout image so model distances match the drawing.
// How to use:
//   1. Import your layout image as a Visual Tool (Plane) and name it as BACKGROUND below.
//   2. Pick two points on the drawing whose real distance you know (a column grid, a wall run).
//      Read their model coordinates off the status bar and type them in below.
//   3. Set REAL_DISTANCE to the true distance in metres and run.
// The plane is scaled about its own center so the two points end up REAL_DISTANCE apart.
// FlexSim 2027: Script Console > paste > Execute.

// ---- settings ----
string BACKGROUND    = "Layout Background";
Vec3   P1            = Vec3(0, 0, 0);     // model coords of the first picked point
Vec3   P2            = Vec3(10, 0, 0);    // model coords of the second picked point
double REAL_DISTANCE = 25.0;              // what that span really is, in metres
int    APPLY         = 1;                 // 0 = report the factor only, 1 = apply it
// ------------------

Object bg = Model.find(BACKGROUND);
if (!objectexists(bg))
	return "No object named '" + BACKGROUND + "'. Import the layout as a Visual Tool and rename it.";

double toM = getmodelunit(LENGTH_MULTIPLE);
double shown = Vec3(P2.x - P1.x, P2.y - P1.y, 0).magnitude * toM;
if (shown <= 0)
	return "P1 and P2 are the same point";

double factor = REAL_DISTANCE / shown;

if (!APPLY)
	return "Scale factor would be " + numtostring(factor, 0, 4) + " (drawing shows " + numtostring(shown, 0, 2) + " m, real " + numtostring(REAL_DISTANCE, 0, 2) + " m)";

Vec3 center = bg.getLocation(0.5, 0.5, 0);
Vec3 s = bg.size;
bg.size = Vec3(s.x * factor, s.y * factor, s.z);
bg.setLocation(center.x, center.y, center.z, 0.5, 0.5, 0);   // keep it centered where it was

repaintall();
return "Scaled " + BACKGROUND + " by " + numtostring(factor, 0, 4) + ". Re-measure the span to confirm it now reads " + numtostring(REAL_DISTANCE, 0, 2) + " m.";
