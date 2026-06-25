# Unity Slot Machine Game Assignment

A 2D Unity slot machine game showcasing modular script architecture, interactive UI controls, and randomized reel loops.

## 🎰 Game Features
* **Dual-Script Reel System:** Managed independently using structured loops for reel rotation.
* **Dynamic Stakes:** Adjustable betting configurations mapped to text updates.
* **Responsive Layout:** Designed cleanly for local testing and deployment.

## 🛠️ Code Structure
The core engine is driven by two main production scripts located in `Assets/Scripts/`:
1. `GameControl.cs` — Controls the global game state, event delegates, total balance calculus, and interactive button listeners.
2. `Row.cs` — Dictates individual column reel components, independent pacing physics, and sprite randomized loops.

## 🚀 Development Setup
1. Clone this repository to your local system.
2. Open the directory via **Unity Hub**.
3. Load the `SampleScene.unity` asset file to test inside the editor layout.