# FlexSim Tools

One folder per tool. **One user library (`.fsl`) per tool — never combined**, so each can be
updated, loaded or shared on its own.

Built for **FlexSim 2027 (27.0.2 build 356)**.

| # | Tool | Delivery | Status |
|---|------|----------|--------|
| 01 | Measuring Tool — distance labels on every A (solid blue) and S (dashed orange) connection | Object + `.fsl` | **working** |
| 02 | Quick Connect — chains the selection with A or S connections | Script | **working** |
| 03 | Align & Distribute — straighten, spread evenly, or fixed gap | Script | testing |
| 04 | Bulk Rename — pattern + running numbers, ordered by position | Script | planned |
| 05 | Model from Spreadsheet — build, name and connect from a sheet | Script + template | planned |
| 06 | Cell Builder — standard cells from parameters | Script | planned |
| 07 | Mirror & Array — mirrored or repeated copies with names | Script | planned |
| 08 | CAD Background Calibrator — scale a DWG/image layout by two clicks | Script + object | planned |
| 09 | Model Auditor — finds unconnected objects, defaults, duplicate names | Script | planned |
| 10 | Connection Integrity — crossed/missing ports, port map | Script | planned |
| 11 | Clearance Checker — overlaps and aisle widths | Script + overlay | planned |
| 12 | Static Capacity Check — takt vs capacity before running | Script | planned |
| 13 | Excel Config Driver — two-way property sync with dry run | Script + template | planned |
| 14 | Bulk Property Grid — edit the selection in one table | Script + GUI | planned |
| 15 | Scenario Snapshots — save, restore and compare parameter sets | Script | planned |
| 16 | Travel Distance Report — connection + actual travel distances, spaghetti diagram | Script + overlay | planned |
| 17 | Flow Heatmap — utilization colors, flow-weighted link thickness | Object | planned |
| 18 | Area Tagging — zones, color, filter, per-zone rollups | Script | planned |
| 19 | View Bookmarks — named camera positions | Script | planned |
| 20 | Model Cleanup — remove orphaned tools and dead logic | Script | planned |
| 21 | Auto Report — KPIs, charts and screenshots to Excel/PowerPoint | Script + template | planned |

Parked as later extensions: dimension annotations (grows out of 01), version stamper,
batch runner, operator workload balancer.

## Status key

**working** = verified in FlexSim. **testing** = written, waiting on a test. Everything else is
written but **never run** — expect errors on the first paste and send them over.

Tools 04–21 were written in one pass on 2026-09-28 without access to FlexSim. They reuse only
calls proven by 01–03 where possible; the risky ones are called out in each file's header:
`split()`, `stringtonum()`, `getProperty`/`setProperty`, `createcopy`, `classname`, `stats.*`,
`activeview()`, `viewtofile()`, `destroyobject()`, `Color()`.

Scripts that write files need their output folder to exist first:
`C:\FlexSimTools\`, `C:\FlexSimTools\scenarios\`, `C:\FlexSimTools\reports\`.

## Install pattern (same for every tool)

1. Open the tool's `Install_*.fs` in Notepad, copy all of it.
2. FlexSim: **Scripting → Script Console**, paste, **Execute**.
3. Object tools only: right-click the created object in the 3D view → **Add to User Library** → a new library.
4. **Save Library As** → `C:\Program Files\Autodesk\FlexSim 2027\libraries\<ToolName>.fsl`
5. **File → Global Preferences → Libraries** → add that `.fsl` to the startup list.

Script tools have nothing to install — paste and run when needed.

## Updating a tool

Re-running an installer updates the object **in the open model only**. The library keeps a
snapshot, so after testing: add to the library again (replacing the old item) → **Save Library**.

## 2027 API notes (learned the hard way)

- `tool.attrs.assert("OnDraw", "")` — `attrs` has no `subnodes`, and `assertattribute` reports
  "not enough parameters".
- `string.fromNum(x)` takes no extra width/precision arguments in 2027.
- `fileexists()` was reported as an unknown command; test file access via `fileopen()`'s return.
- A Visual Tool made by `Object.create("VisualTool")` has no shape and cannot be clicked in the
  3D view. End the draw code with `return 0`, or drag a Visual Tool in from the Library.
- `contextdragconnection(a, b, "A")` and `"S"` both work — confirmed by tool 02.
- Number + string concatenation fails; wrap numbers in `string.fromNum()`.
