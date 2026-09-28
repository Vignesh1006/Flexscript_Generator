# 02 - Quick Connect

**Status:** WORKING (QuickConnect.fs) / TESTING (GUI and library versions)

## What it does
Chains the selected objects with A or S connections, sorted left-to-right or bottom-to-top.

## Example
Select 12 stations, one run, 11 A-connections made.

## Delivery
- `QuickConnect.fs` - script, settings edited in the code.
- `QuickConnect_GUI.fs` - script, asks A/S and X/Y in Yes/No/Cancel dialogs.
- `Install_QuickConnect.fs` - Visual Tool object for a user library (`QuickConnect.fsl`).
  Settings are labels on the tool: `Link` = A/S, `Sort` = X/Y/Tree. Ctrl-select the objects,
  Ctrl+click the tool, set label `Run` = 1. The result is written on the floor next to the tool.
  `QuickConnect_OnDraw.fs` is the readable copy of the code the installer embeds.

All versions skip pairs that are already connected (except the original QuickConnect.fs).

## Install
See ..\README.md - same pattern for every tool. One .fsl per tool, never combined.
