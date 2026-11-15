# BPCL AR Engine Simulator

An interactive **Augmented Reality (AR) Engine Simulator** built with Unity 6 (`6000.3.5f1`) and AR Foundation for Android devices. Developed by **Clocean MR**, this application demonstrates the performance difference between **MAK Supreme Synthetic (BPCL)** engine oil and **Local Mineral Oil** using interactive 3D V8 engine mechanics, accelerometer-driven tilt-to-pour interactions, real-time particle systems, and spatial UI feedback.

---

## Key Features

- **AR Plane Detection & Placement**: Automatically scans horizontal surfaces (floors, tables) using ARCore and spawns a 3D animated V8 engine alongside two interactive oil bottles.
- **Touch Selection Mechanic**: Tap an oil bottle (**MAK Supreme Synthetic** or **Local Mineral Oil**) to select/deselect it with visual outline highlights.
- **Accelerometer Tilt-to-Pour**: Tilt the mobile device forward (`~30°`) while within range of the engine to trigger a multi-phase pouring sequence (approach, tilt, active fluid particle stream, and retract).
- **Comparative Engine Simulation**:
  - **MAK Supreme Synthetic (BPCL)**: Smooth high-RPM piston/crankshaft animation, clean exhaust (no smoke), smooth engine audio loop, and an auto-scrolling green **Benefits** panel.
  - **Local Mineral Oil**: Heavy engine vibration/jitter, thick black exhaust smoke particles, rough engine audio loop, and an auto-scrolling red **Demerits** panel.
- **Pour Quality Scoring System**: Evaluates each pour across **Duration**, **Stability** (tilt variance), **Control** (distance from engine), and **Tilt Drops** to compute a final performance score.
- **Procedural Engine Kinematics**: Includes `AdvancedEngineAnimator.cs` for mathematical crank-rod physics, phase offsets, RPM acceleration/deceleration, and throttle gauge visualization.

---

## Tech Stack & Dependencies

| Component | Version / Details |
| :--- | :--- |
| **Unity Version** | `6000.3.5f1` (Unity 6) |
| **Render Pipeline** | Universal Render Pipeline (URP `17.3.0`) |
| **AR Framework** | AR Foundation (`6.3.2`), ARCore XR Plugin (`6.3.2`), ARKit XR Plugin (`6.3.2`) |
| **XR Interaction** | XR Interaction Toolkit (`3.3.1`) |
| **Input Handling** | Unity Input System (`1.17.0`) + Legacy Input (`Both` active) |
| **UI System** | Unity UI (`uGUI 2.0.0`) & TextMeshPro |
| **Target Platform** | Android (ARM64, API Level 24+ / Android 7.0+, tested on Android 11+ / Samsung S23) |

---

## Project Structure

```text
├── Assets/
│   ├── EngineV8/                        # V8 engine meshes, materials, and textures
│   ├── MobileARTemplateAssets/          # AR template assets and UI helpers
│   ├── PREFAB/                          # Core prefabs (animated_engine_v8, BPCL_Bottle, Local_Bottle, OilPouringParticles)
│   ├── Resources/                       # Runtime-loaded assets
│   ├── Scenes/
│   │   └── AR Oil Engine.unity          # Primary production AR scene
│   ├── SCRIPT/                          # Core C# scripts (AR_OilInteraction, AdvancedEngineAnimator, UI & debug helpers)
│   ├── Settings/                        # URP & XR settings
│   ├── SOUND/                           # Engine audio loops (Idle, Normal Oil, BPCL Oil)
│   └── VFX/                             # Visual effects and particle configurations
├── Packages/                            # Unity Package Manager manifest and lockfile
├── ProjectSettings/                     # Unity project configuration
└── Project_Documentation.md             # Deep-dive technical documentation
```

---

## Architecture & Interaction Flow

The core experience is governed by `AR_OilInteraction.cs` using an event-driven state machine:

```text
Initializing → WaitingForPlane → Spawning → Idle → Holding → Pouring → PourComplete / PourFailed
