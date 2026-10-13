// 13 - Excel Check. Finds out why the Excel Config Driver does not open or update the workbook.
// Opens the workbook, selects the sheet, writes a test cell, reads it back, saves and closes,
// and reports what each Excel command returned. Close the workbook in Excel first.
// FlexSim 2027: Script Console > paste > Execute. Send the whole result line back.

// ---- settings (same values as in ExcelConfigDriver.fs) ----
string XLSX_NAME = "model_config.xlsx";   // kept in the same folder as the .fsm
string SHEET     = "Sheet1";
// ------------------------------------------------------------

string MODEL_DIR = modeldir();
if (MODEL_DIR == "")
	return "Save the model (.fsm) first - the workbook is kept next to it";
string XLSX = MODEL_DIR + XLSX_NAME;

string out = "path = " + XLSX + " | ";

// 1. can FlexScript reach the folder at all? (plain text file next to the workbook)
string probe = MODEL_DIR + "flexsim_write_test.txt";
if (fileopen(probe, "w")) {
	fpt("ok");
	fileclose();
	out += "folder writable: yes | ";
} else
	out += "folder writable: NO | ";

// 2. open the workbook
Variant r1 = excelopen(XLSX);
string s1 = "(no value)";
if (r1.type == VAR_TYPE_NUMBER)
	s1 = string.fromNum(r1);
out += "excelopen = " + s1 + " | ";

// 3. select the sheet
Variant r2 = excelsetsheet(SHEET);
string s2 = "(no value)";
if (r2.type == VAR_TYPE_NUMBER)
	s2 = string.fromNum(r2);
out += "excelsetsheet = " + s2 + " | ";

// 4. what is in A1 now?
out += "A1 before = '" + excelreadstr(1, 1) + "' | ";

// 5. write a test cell (Z1) and read it back before saving
excelwritestr(1, 26, "FlexSim test OK");
out += "Z1 read back = '" + excelreadstr(1, 26) + "' | ";

// 6. save and close
Variant r3 = excelclose(1);
string s3 = "(no value)";
if (r3.type == VAR_TYPE_NUMBER)
	s3 = string.fromNum(r3);
out += "excelclose(save) = " + s3;

return out;
