# 🖥️ ADVTECH WS Setup Tool

**Author:** Dan Damit  
**Email:** dan@thedamits.com  
**Platform:** Windows  
**Tech Stack:** C#, .NET 8.0, WPF, MahApps.Metro

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
- Automated certificate installation and runtime provisioning  
- Domain-aware configuration inputs with watermarks and tooltips  
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

<img width="900" height="601" alt="image" src="https://github.com/user-attachments/assets/92dc0ba4-234f-4fde-a263-4fb003bebae8" />

