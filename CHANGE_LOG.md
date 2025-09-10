# 📦 ADVTECH WS Setup Changelog

**Author:** Dan Damit  
**Email:** dan@thedamits.com  
**Platform:** Windows  
**Tech Stack:** C#, .NET 8.0, WPF, MahApps.Metro, WiX

---

## 🛠 Project Timeline

### 2025-07-02 — Project Kickoff
- Idea originated in January 2025
- Initial prototype in PowerShell + Windows.Forms

---

## 🚀 Milestones & Updates

### 2025-07-09 — First Working Version
- Finalized initial build
- Internet required for some installers (consider bundling)

### 2025-08-24 — Service Refactor
- Extracted Baseline Service to its own class library
- Added “Purge OEM Leftovers” button to Uninstall tab
- Expanded Dell uninstall targets

### 2025-08-26 — Bootstrap & Asset Strategy
- Excluded Assets from single-file exe to improve cold starts
- Bootstrapper renamed to `Setup.exe`
- Assets copied to `C:\Working\Assets` at runtime
- Migrated from PowerShell to NSIS bootstrapper to avoid loose files

### 2025-08-27 to 2025-08-31 — WiX Packaging
- Migrated from NSIS to WiX
- No longer uses `C:\Working\`
- Built MSI packages and bundle installer
- Refined licensing and UI logic

### 2025-09-04 — Dell Uninstall Logic
- Identified two apps requiring user interaction
- Release `6.7.1` published

### 2025-09-06 — Interactive Uninstall Flow
- ViewModel now detects non-silent apps and prompts UI

### 2025-09-08 — Purge Logic Refactor
- Moved `ForceDeleteRemnants()` to its own button

### 2025-09-09 — UI Polish
- Added tooltips and watermarks
- Converted progress bars to indeterminate mode
- Improved font sizing for readability
