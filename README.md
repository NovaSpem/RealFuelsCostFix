# RealFuels Cost Fix

A plugin for the Kerbal Space Program, which corrects cost errors in career mode when using the **RealFuels** mod.

## 🛠️ The Problem
When using the **RealFuels** mod in Kerbal Space Program's Career mode, the stock fuel cost calculation completely breaks, resulting in two severe economic bugs:
1. **Price is not increasing upon filling the tank:** RealFuels fuel resources often register as 0 funds in the editor. As a result, filling the tank to 100% does not add any value to the rocket, making fueling practically free.
2. **Tank cost becomes negative depending on fuel cost:** When you modify the fuel configuration or empty the tank, the stock KSP calculator tries to subtract phantom vanilla resources from the part. This pushes the base tank shell price below zero into negative numbers, completely ruining the Career mode budget balance.

## 🚀 The Solution
**RealFuels** Cost Fix обходит этот баг ванильного калькулятора KSP и принудительно внедряет математически формулу расчета стоимости через  IPartCostModifier.

### Key Features:
* **Perfect Cost Symmetry:** Fueling a tank increases the price correctly based on real Community Resource Pack (CRP) liter costs. Emptying a tank smoothly transitions the cost, locking the dry tank shell exactly at its factory value.
* **Utilization Support:** Fully compatible with the "Tank Volume / Utilization" slider. The tank shell price remains rock-solid and predictable no matter how you scale your inner tank configuration.
* **Multi-Fuel Component UI:** Adds a clean, dynamic indicator to the Part Action Window (PAW) right-click menu showing the overall fill percentage and individual percentages/liters for up to 5 loaded fuel types simultaneously (e.g., `LqdOxygen: 73.7% (1474 / 2000 L)`).

## 📦 Installation & Dependencies
1. Install **ModuleManager** and **RealFuels** (required).
2. Download the latest release from the [Releases](https://github.com/NovaSpem/RemoteTechBlackoutFix/releases) tab.
3. Unzip the archive and place the `RealFuelsCostFix` folder inside your `GameData/` directory.

## 📜 License
This project is licensed under the [MIT License](https://github.com/NovaSpem/RealFuelsCostFix/raw/refs/heads/main/LICENSE). Feel free to fork, modify, and distribute.
