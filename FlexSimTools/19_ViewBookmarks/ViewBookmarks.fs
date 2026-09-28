// 19 - View Bookmarks. Saves named camera positions and jumps back to them.
//
// MOST UNCERTAIN TOOL IN THE SET. FlexSim stores the camera on the view node, and I could not
// verify the 2027 attribute names without the software. So run MODE 3 (probe) FIRST and send me
// what it prints - then I will finish save/restore properly.
//
// MODE 0 = save current view as BOOKMARK
// MODE 1 = restore BOOKMARK
// MODE 2 = list saved bookmarks
// MODE 3 = probe: print the active view's camera attributes
// FlexSim 2027: Script Console > paste > Execute.

// ---- settings ----
int    MODE     = 3;
string BOOKMARK = "Overview";
string HOLDER   = "View Bookmarks";   // hidden object that stores them as labels
// ------------------

treenode view = activeview();          // if this errors, that is useful information - tell me

// ---------- probe ----------
if (MODE == 3) {
	if (!objectexists(view))
		return "activeview() returned nothing - click the 3D view first, then re-run";
	string dump = "view path: " + view.getPath(0, 1) + " | children: ";
	for (int i = 1; i <= view.subnodes.length; i++) {
		treenode s = view.subnodes[i];
		dump += s.name;
		if (s.dataType == DATATYPE_NUMBER)
			dump += "=" + numtostring(s.value, 0, 2);
		dump += "  ";
		if (dump.length > 900)
			break;
	}
	return dump;
}

// holder object that keeps the bookmarks as labels on itself
Object holder = Model.find(HOLDER);
if (!objectexists(holder)) {
	holder = Object.create("VisualTool");
	holder.name = HOLDER;
	holder.size = Vec3(0.2, 0.2, 0.2);
	holder.setLocation(0, 0, 0, 0.5, 0.5, 0);
}

// ---------- list ----------
if (MODE == 2) {
	string list = "";
	treenode labs = holder.labels;
	for (int i = 1; i <= labs.subnodes.length; i++)
		list += labs.subnodes[i].name + "  ";
	if (list.length < 1)
		return "No bookmarks saved yet";
	return "Bookmarks: " + list;
}

// The camera lives in the view's attributes. These names are the part I need the probe for.
// Expected: viewpoint x/y/z and the view angles. Adjust after the probe.
if (!objectexists(view))
	return "Click the 3D view first, then re-run";

if (MODE == 0) {
	string packed = "";
	for (int i = 1; i <= view.subnodes.length; i++) {
		treenode s = view.subnodes[i];
		if (s.dataType == DATATYPE_NUMBER)
			packed += s.name + "=" + numtostring(s.value, 0, 4) + ";";
	}
	holder.labels.assert(BOOKMARK, "").value = packed;
	return "Saved bookmark '" + BOOKMARK + "' (" + string.fromNum(packed.length) + " chars)";
}

if (MODE == 1) {
	treenode lab = holder.labels.find(BOOKMARK);
	if (!objectexists(lab))
		return "No bookmark named '" + BOOKMARK + "'";
	string packed = lab.value;
	Array parts = packed.split(";");
	int applied = 0;
	for (int i = 1; i <= parts.length; i++) {
		if (parts[i].length < 3)
			continue;
		Array kv = parts[i].split("=");
		if (kv.length < 2)
			continue;
		treenode s = view.find(kv[1]);
		if (objectexists(s) && s.dataType == DATATYPE_NUMBER) {
			s.value = stringtonum(kv[2]);
			applied++;
		}
	}
	repaintall();
	return "Restored '" + BOOKMARK + "': " + string.fromNum(applied) + " camera values";
}

return "Set MODE to 0, 1, 2 or 3";
