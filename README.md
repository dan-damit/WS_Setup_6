# 🖥️ ADVTECH WS Setup Tool

**Author:** Dan Damit  
**Email:** dan@thedamits.com  
**Platform:** Windows  
**Tech Stack:** C#, .NET 8.0, WPF, MahApps.Metro, WiX

---

## 🚀 Getting Started
1. The latest release tag has the most up-te-date version.
2. Run `WS.Setup.exe` from the latest [Release link](https://github.com/dan-damit/WS_Setup_6/releases/tag/latest) on the main page.
3. Ensure internet access for Onboarding tasks.

---

## 🎯 Purpose

This is a personal project designed to streamline the onboarding process for new workstations. Originally built in PowerShell with Windows.Forms, it has since been fully rewritten in C# using WPF and MahApps.Metro for a modern, polished UI.

---

## ⚙️ Goals

- **Automation:** Eliminate manual setup steps through scripted workflows  
- **Consistency:** Ensure every workstation is configured identically  
- **Efficiency:** Reduce setup time and avoid configuration drift  
- **Scalability:** Support any workstation model — not hardware-specific

---

## 📦 Features

- GUI-driven setup flow with branded visuals
- Find Agent button auto-seeds the Ninja Agent installer from Desktop
- Also auto-seeds from Desktop if Ninja Agent installer is present before running
- Live Execution Log as well as logging to file  
- Automated certificate installation and runtime provisioning  
- Domain-aware configuration inputs with watermarks and tooltips
- Runs Baseline configuration based on the Baseline.ps1 script converted to yaml
- DSCv3 applies Baseline - the WS_Config file (in Assets)  
- Uninstaller logic with silent and interactive fallback modes  
- Optional purge of OEM remnants (e.g., leftover Dell software)  
- Indeterminate progress indicators for clean UX feedback    
- WiX-based installer bundle for clean deployment

---

## 🧠 Why It Matters

Manual workstation setup is error-prone and time-consuming. This tool ensures every machine is onboarded with precision, eliminating missed steps and reducing variability. Whether you're provisioning one device or a fleet, this app brings clarity, speed, and repeatability to the process.

---

## 🚀 Compatibility

- Works across all Windows workstation models  
- Requires internet access for some installer downloads  
- English language support (for now)

---

## 📁 Repo Structure (Simplified)

- Assets - (Images, WS_Config files, Icons, etc)
- Bundle - Final Package (WS Setup.exe)
- Common - Logger
- Core - AppInventoryService, BaselineService, OnboardService, UninstallService, etc.
- MSI - Initial wrapper for loose files from build 
- UI - All the UI elements 

---

Feel free to fork, adapt, or reach out with questions. This project is built for clarity, speed, and deployment-grade reliability.

---

<img width="900" height="601" alt="image" src="https://github.com/user-attachments/assets/92dc0ba4-234f-4fde-a263-4fb003bebae8" />
<img width="900" height="599" alt="image" src="https://github.com/user-attachments/assets/c6747499-29c7-447e-a235-a2ff1f64241b" />
<img width="901" height="600" alt="image" src="https://github.com/user-attachments/assets/2a7b20dd-09f6-472c-9555-925205450d30" />
<img width="900" height="599" alt="image" src="https://github.com/user-attachments/assets/841f5048-bffa-4bc2-ab35-0eeb692c656b" />
<img width="901" height="599" alt="image" src="https://github.com/user-attachments/assets/e66fd054-335f-4f69-a833-ee2c4e090cc6" />
