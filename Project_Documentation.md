# AR Oil Engine - Project Documentation & Deep Analysis

This document provides a comprehensive overview of the **AR Engine Simulator** (formerly AR Engine Android) Unity project. It encompasses the project type, build settings, core mechanics, dependency architecture, and the scene hierarchy based on a detailed analysis of the repository files.

## 1. Project Overview
**AR Engine Simulator** is an Augmented Reality (AR) demonstration application developed by **Clocean MR**. The core experience is designed for Android devices and focuses on comparing two types of engine oils—"MAK Supreme Synthetic (BPCL)" vs "Local Mineral Oil"—using interactive 3D models.

### Key Interactive Features:
- **Plane Detection**: Automatically scans for horizontal surfaces (like floors or tables) to spawn a predefined 3D Engine model and two bottles of oil.
- **Touch & Hold Mechanic**: Tapping a bottle selects it, creating a holding sensation.
- **Tilt-to-Pour**: Utilizes the device's accelerometer. Tilting the phone (~30 degrees / Y-axis value < -0.3) simulates pouring the selected oil into the engine. Distance checking ensures you are close enough to pour correctly.
- **Visual Feedback System**:
  - **MAK Supreme Synthetic (BPCL)**: Triggers a smooth engine animation, displays a green auto-scrolling benefits UI, and emits no smoke.
  - **Mineral Based (Local)**: Triggers engine vibrations, displays a red auto-scrolling demerits UI, and emits thick black smoke particles.
- **Architecture**: Employs a robust, event-driven state machine (`Initializing`, `WaitingForPlane`, `Spawning`, `Idle`, `Holding`, `Pouring`, `PourComplete`, `PourFailed`) ensuring clean, decoupled interaction flows.

---

## 2. Project Settings & Build Configuration

The project is built upon Unity's Universal Render Pipeline (URP) and properly targets modern Android devices.

*   **Company Name**: Clocean MR
*   **Product Name**: AR Engine Simulator
*   **Target Platform**: Android (e.g., Target tested on Samsung S23)
*   **Application Identifier**: `com.unity.template.ar_mobile`
*   **Minimum API Level**: Default targeted Android API Level 30 (Android 11) - setup guide references API 24 as a minimum fallback.
*   **Architecture**: ARM64 (Required for ARCore 64-bit compliance).
*   **Input System**: Configured securely to parse both Legacy and the new Unity Input System (`com.unity.inputsystem` v1.17.0).

---

## 3. Core Dependencies & Packages
Derived from the `manifest.json`, the project relies on the newest XR implementations:
*   **AR Foundation** (`com.unity.xr.arfoundation: 6.3.2`) - Base API.
*   **ARCore XR Plugin** (`com.unity.xr.arcore: 6.3.2`) - Android AR framework.
*   **ARKit XR Plugin** (`com.unity.xr.arkit: 6.3.2`) - Included for potential iOS cross-platform compatibility.
*   **XR Interaction Toolkit** (`com.unity.xr.interaction.toolkit: 3.3.1`)
*   **Universal Render Pipeline** (`com.unity.render-pipelines.universal: 17.3.0`)
*   **TextMeshPro**: Used extensively in UI feedback panels (`com.unity.ugui: 2.0.0`).

---

## 4. Scene Hierarchy
The primary functional scene is **`AR Oil Engine.unity`**. The scene executes using a standardized AR Foundation hierarchy intertwined with customized manager objects.

```text
================= SCENE HIERARCHY =================
□ AR Session
   └─ Manages the lifecycle of the AR experience
   
□ AR Session Origin (or XROrigin in newer XR versions)
   ├─ AR Camera (Main Camera for AR Passthrough)
   ├─ AR Plane Manager (Detects horizontal surfaces)
   └─ AR Raycast Manager (Handles touch-to-surface projection)
   
□ AR_OilInteraction_Manager
   │  (The central brain. Contains the 'AR_OilInteraction.cs' script)
   │  (Requires PREFABS to be attached in the Inspector: Engine, BPCL Bottle, Local Bottle)
   ├─ Particle Systems (Smoke, pour visual effects)
   └─ Audio Sources (Engine revving, pouring sounds)
   
□ AR_OilDemo_UI
   │  (Canvas handling instructions, tap prompts, and Reset buttons)
   ├─ Demerits Panel (Local Oil)
   └─ Benefits Panel (BPCL)

□ ShowObjectPositions 
   └─ (Optional Debug text display)
===================================================
```

---

## 5. System Requirements & Setup Wizard
### Wizard Tooling
To ease deployment, the developer has included an `AR Oil Demo Setup Wizard` accessible in the editor via `Window > AR Oil Demo Setup`. The wizard automatically corrects hierarchy gaps and binds specific prefabs.

### Troubleshooting Highlights
A dedicated `CRITICAL_FIXES_README.txt` specifies mandatory procedures before building:
1.  **Prefab Assignment missing**: If the app launches but nothing spawns, it is because the Inspector references on `AR_OilInteraction_Manager` are empty. The developer MUST assign `animated_engine_v8`, `BPCL_Bottle`, and `Local_Bottle` respectively.
2.  **Input Exceptions**: Previously addressed; code seamlessly captures both screen touches and accelerometers avoiding InvalidOperationExceptions.
3.  **Visibility Rendering**: To solve "invisible AR objects", a helper script force-enables all object renderers on instantiation and shifts the base engine scale to `0.5` for prompt visibility.

---

## 6. Core Implementation Architecture

### 6.1 Main Interaction Script: `AR_OilInteraction.cs`
This is the **central orchestrator** for the entire AR oil demo experience. It implements a robust state machine and handles all user interactions, physics, and visual feedback.

#### State Machine
The script defines an `InteractionState` enum with the following flow:
```
Initializing 
  ↓
WaitingForPlane (AR scanning for horizontal surface)
  ↓
Spawning (3D models instantiated: Engine + 2 Bottles)
  ↓
Idle (Waiting for user tap on a bottle)
  ↓
Holding (User has selected a bottle by tapping it)
  ↓
Pouring (User tilts phone forward > 0.65 degrees)
  ↓
PourComplete (Pour action finished)
  ↓
PourFailed (Pour failed: distance exceeded or other error)
```

#### Key Prefab Fields & Configuration
Located in the `AR_OilInteraction_Manager` Inspector:

| Field | Purpose | Default | Range |
|-------|---------|---------|-------|
| **Engine Prefab** | 3D V8 engine model | Required | N/A |
| **BPCL Bottle Prefab** | MAK Supreme Synthetic oil container | Required | N/A |
| **Local Bottle Prefab** | Mineral oil container | Required | N/A |
| **Spawn Distance** | How far from camera to spawn objects | 1.2m | 0.5–3.0m |
| **Engine Scale** | Size multiplier for engine | 0.4 | 0.1–1.0 |
| **Bottle Scale** | Size multiplier for bottles | 1.5 | 0.5–2.0 |
| **Bottle Spacing** | Distance from engine to bottle | 0.5m | 0.3–1.0m |
| **Tilt Start Threshold** | Tilt required to **initiate** pour | 0.65 | 0.5–0.9 |
| **Tilt Continue Threshold** | Tilt to **maintain** pour | 0.55 | 0.4–0.8 |
| **Pour Duration** | Max seconds at full tilt | 3.0s | 1.0–10.0s |
| **Max Pour Distance** | Distance tolerance from engine | 2.5m | 1.0–5.0m |

#### Touch Input Handling
**Touch Detection:**
- Uses Unity's **New Input System** (`com.unity.inputsystem 1.17.0`) with fallback to Legacy Input for compatibility.
- Tap detection via `EnhancedTouchSupport` and `Touch.activeTouches` (New System).
- Each frame, the script raycasts from camera through touch position to find bottles or the AR plane.

**Bottle Selection Logic:**
```csharp
if (Tap on bottle) {
  if (selectedOil == oilType) 
    Deselect();  // Double-tap deselects
  else {
    Select(oilType);  // Select new bottle
    AutoResetEngine();  // Clear previous pour state
  }
}
```

#### Accelerometer (Tilt) Input
**Device Orientation:**
- Captured via `Input.acceleration` (Legacy) or `Accelerometer.current.acceleration` (New System).
- Smoothed using exponential moving average: `smoothedTiltValue = Mathf.Lerp(smoothedTiltValue, rawTilt, accelSmoothing)`
- Sensitivity is tuned via `accelSmoothing = 0.15` (0.1=very smooth, 0.5=very responsive).

**Detect Pour Initiation:**
- If `smoothedTiltValue < -tiltStartThreshold` (approx. -0.65), pour begins.
- If maintained above `tiltContinueThreshold` (-0.55), pour continues.
- Distance check: If `distance(phone, engine) > maxPourDistance`, abort with warning.

#### Animation Sequence During Pour
1. **Phase 1: Bottle Approach** (0.6s)
   - Bottle moves from spawn position toward engine oil hole.
   - Uses `Vector3.Lerp()` and smooth animation curves.

2. **Phase 2: Tilt Bottle** (0.8s)
   - Bottle rotates to `pourTiltAngle = 120°` (inverted pour position).
   - Rotation interpolated via `Quaternion.Lerp()`.

3. **Phase 3: Active Pour** (7.0s by default)
   - Particle system emits oil stream.
   - Engine plays vibration (on Local oil) or runs smoothly (BPCL).
   - Engine animation loops based on oil type.

4. **Phase 4: Retract & Complete**
   - Bottle returns to original position.
   - Results panel displays with scoring data.

#### Particle Systems & Visual Effects
**Oil Pouring Particles:**
- Created programmatically if not assigned.
- **Emission**: 50 particles/second.
- **Lifetime**: 1.5 seconds (stream effect).
- **Velocity**: Gravity-affected (simulates pouring fluid).

**Engine Smoke Particles (Local Oil Only):**
- Found as child of engine prefab.
- Plays during and after Local oil pour.
- Disabled for BPCL (clean engine).

#### Audio System
Three audio loops supported:
- **Default Engine Loop**: Before any pour.
- **Normal Oil Engine Loop**: After Local oil pour (vibrating sound).
- **BPCL Oil Engine Loop**: After MAK oil pour (smooth sound).

Audio is controlled via `AudioSource` components; pitch and volume are modulated based on RPM.

---

### 6.2 Engine Animation: `AdvancedEngineAnimator.cs`
This script mathematically animates engine pistons, crankshaft, and connecting rods **without pre-made animations**. It simulates realistic mechanical motion.

#### Engine Types Supported
- **Inline 4** (most common)
- **Inline 6**
- **V6**, **V8**, **V12**
- **Boxer 4**, **Boxer 6**
- **Single cylinder**
- **Custom** (user-defined phase offsets)

#### Simulation Method
**Crank-Rod Physics:**
- Crankshaft rotates at the current RPM (converted to degrees/frame).
- Each piston's Y-position is calculated via: $y = \sin(\text{crank angle}) \times \text{crank radius}$
- Connecting rod length and bank angles are factored in for accurate V-engine geometry.

**Phase Offsets:**
- Each cylinder fires at a specific angle offset from TDC (Top Dead Center).
- For an Inline 4: 0°, 180°, 360°, 540° (every 180° for 4 pistons, one rotation = 2 pistons fire).
- Automatcially computed based on engine type; manually overridable via `customPhaseOffset` in editor.

#### RPM Control
**Throttle Input:**
- Mapped 0.0–1.0.
- RPM accelerates/decelerates toward target based on throttle input.
- Deceleration is faster than acceleration (engine braking effect).

```csharp
targetRPM = Mathf.Lerp(idleRPM, maxRPM, throttle);
currentRPM = Mathf.Lerp(currentRPM, targetRPM, rpmAcceleration * Time.deltaTime);
```

#### Vibration Effects
- Amplitude controlled by `vibrationIntensity = 0.001`.
- Applied to a target transform (usually engine root) as random jitter.
- Intensity increases with RPM to simulate realistic engine vibration.

#### Audio Integration
- Pitch mapped to current RPM: $\text{pitch} = \text{Lerp}(\text{minPitch}, \text{maxPitch}, \text{normalizedRPM})$
- Volume scales with throttle for realistic audio fade.

---

### 6.3 Touch Controller: `EngineTouchController.cs`
A flexible UI component for controlling engine throttle via UI interactions.

#### Control Modes
1. **Vertical Drag**: Drag up/down on a UI element to increase/decrease throttle.
2. **Horizontal Drag**: Drag left/right.
3. **Tap to Increase**: Tap button to increment throttle; auto-decreases when released.
4. **Hold to Accelerate**: Hold button to continuously accelerate; release to decelerate.

#### Implementation
- Implements `IPointerDownHandler`, `IPointerUpHandler`, `IDragHandler` (UI Event System).
- Tracks drag distance via `Vector2.Distance()` and converts to throttle percentage.
- Optional visual feedback: thumb slider animates along a track.

---

### 6.4 RPM Gauge Display: `EngineRPMGauge.cs`
A UI gauge component that visualizes current engine RPM and throttle.

#### Needle Animation
- **Min Angle** (0 RPM): 135° (pointing left).
- **Max Angle** (Max RPM): -135° (pointing right).
- Smoothly interpolated with `Mathf.Lerp()`.
- Optional bounce effect at high RPM (needle oscillates for dramatic effect).

#### Digital Readout
- Shows current RPM (e.g., "5,200 RPM").
- Shows throttle percentage (e.g., "THR: 75%").
- Text color transitions:
  - **Green** (normal): RPM < 75% of max.
  - **Yellow** (warning): 75%–90% of max.
  - **Red** (redline): > 90%, blinks at maximum.

---

### 6.5 Utility Scripts

#### `FanRotation.cs`
Simple continuous rotation script for decorative elements (fans, etc.).
```csharp
public class FanRotation {
  - rotationSpeed: degrees/second (default: 360°)
  - rotationAxis: Vector3 (default: Z-forward)
  - isRotating: toggle enable/disable
  - Methods: ToggleFan(), SetRotationSpeed(), StartFan(), StopFan()
}
```

#### Debug & Validation Helper Scripts
- **AR_SetupValidator.cs**: Checks if all required AR components are configured.
- **AR_DebugHelper.cs**: Logs detailed state machine transitions and input events.
- **ShowObjectPositions.cs**: Overlays world position vectors for debugging spatial layout.
- **FixARObjectRendering.cs**: Force-enables renderers to solve visibility issues.

---

## 7. Data & Scoring System

### Pour Quality Scoring
After each pour, the app computes a score based on:

| Metric | Calculation |
|--------|-------------|
| **Duration Score** | 100 – (|actual_duration – ideal_duration| × penalty_factor) |
| **Stability Score** | Based on tilt variance (smoother tilt = higher score, max 100) |
| **Control Score** | Based on distance from engine (farther = lower score, max 100) |
| **Tilt Drops** | Count of times tilt fell below threshold during pour (penalty per drop) |
| **Final Score** | (Duration + Stability + Control) / 3 – (Tilt Drops × 10) |

### PourScore Class
```csharp
private sealed class PourScore {
  public string Label;  // "MAK Premium" or "Local Oil"
  public bool Completed;
  public float DurationSeconds;
  public int StabilityScore;
  public int ControlScore;
  public int TimeScore;
  public int FinalScore;
  public int TiltDrops;
}
```

---

## 8. Asset Management & Prefabs

### Available Prefabs
Located in `Assets/PREFAB/`:

| Prefab | Purpose | Notes |
|--------|---------|-------|
| **animated_engine_v8** | Main V8 engine 3D model | GLB format, ~5-10MB, rigged for animation |
| **BPCL_Bottle** | MAK Supreme Synthetic oil container | Custom model, ~1-2MB |
| **Local_Bottle** | Mineral/local oil container | Custom model, ~1-2MB |
| **OilPouringParticles** | Reusable particle system prefab | Pre-configured oil stream |
| **BMW**, **2018_bmw_m5** | Full car body models | Optional, for "fade" effect |
| **car-engine** | Alternative engine model | Backup if animated_engine_v8 unavailable |

### Material & Texture Assets
- **PBR Engine 8K Textures**: High-fidelity realistic engine surfaces.
- **Smoke/Dust Particle Materials**: Located in `Assets/EffectExamples/`.
- **Ultimate GPU Particle System**: Advanced visual effects library.

---

## 9. Build & Deployment Pipeline

### Pre-Build Checklist
Before deploying to Android:

- [ ] All three prefabs assigned in `AR_OilInteraction_Manager` Inspector.
- [ ] Active Input Handling set to **"Both"** (supports old and new input system).
- [ ] ARCore XR Plugin enabled in XR Plug-in Management.
- [ ] Target API Level ≥ 24 (Android 7.0).
- [ ] Graphics API: OpenGL ES3 or Vulkan.
- [ ] Minimum Device: ARM64 architecture.

### Build Command
```bash
# Build for Android (headless)
/path/to/unity --project . --build-target Android \
  --scenes Assets/Scenes/AR\ Oil\ Engine.unity \
  -buildLinux64Player ./build/game.apk \
  -logFile buildLog.txt
```

### Testing Scenarios
1. **Plane Detection**: Point camera at floor/table; wait 2–3 seconds for AR plane visualization.
2. **Tap Interaction**: Tap a bottle; verify selection highlight (mint green outline).
3. **Tilt Pour**: Tilt phone forward ~30°; observe bottle approach and pouring animation.
4. **Distance Violation**: Move device > 2.5m away during pour; expect "Wrong Pouring!" alert.
5. **Oil Comparison**: Pour BPCL vs. Local oil; verify different engine animations and particle effects.

---

## 10. Performance Optimization Tips

### Mobile AR Optimization
1. **LOD (Level of Detail)**: Engine prefab should use LOD groups for reduced poly count at distance.
2. **Particle System Limits**: Cap particle count at 500 (configurable via `maxParticles`).
3. **Shader Complexity**: Use mobile-friendly shaders from URP library (avoid complex normal maps).
4. **Draw Calls**: AR Foundation renders plane visualization; combine static geometry where possible.

### Frame Rate Targets
- **Target FPS**: 60 FPS on Samsung S23 and equivalent devices.
- **AR Thread**: Plane detection runs on separate thread; aim for <5ms overhead.
- **Physics**: Use simple colliders for bottles; full physics not needed.

---

## 11. Known Limitations & Future Enhancements

### Current Limitations
1. **Single AR Anchor**: Only one engine spawns per session. Multi-object spawning not supported.
2. **Plane Filtering**: Only horizontal planes detected; vertical surfaces (walls) ignored.
3. **Network**: No multiplayer or cloud sync. Purely local experience.
4. **Audio**: Only one Engine loop plays at a time; no dynamic mixing.

### Potential Enhancements
- [ ] **Multi-engine spawning**: Allow user to place multiple engines and compare pours side-by-side.
- [ ] **Gesture Recognition**: Swipe gestures for UI navigation.
- [ ] **Social Sharing**: Screenshot/video export with score overlay.
- [ ] **Analytics**: Track user interactions and oil preference trends.
- [ ] **AR Anchors**: Save engine position and re-place from history.

---

## 12. Troubleshooting & Common Issues

### Issue: Nothing Spawns After Plane Detection
**Solution**: 
1. Check `AR_OilInteraction_Manager` Inspector → all three prefabs assigned.
2. Verify AR Foundation is importing correctly (check Project Settings → XR Plug-in Management).
3. Run `AR_SetupValidator.cs` to validate scene setup.

### Issue: Accelerometer Input Not Detected
**Solution**:
1. Enable device accelerometer: Settings → Devices → Sensors → Accelerometer.
2. Set Project Settings → Player → Other Settings → Active Input Handling → **Both**.
3. Ensure `Accelerometer.current` is not null (check Unity console logs).

### Issue: Pouring Particles Not Visible
**Solution**:
1. Check particle emission rate and lifetime.
2. Verify `OilPouringParticles.prefab` is assigned in Inspector.
3. Increase `arPanelScale` or `arBenefitsPanel` size if UI occludes particles.

### Issue: Engine Model Not Animating
**Solution**:
1. Verify crankshaft and cylinder list are assigned in `AdvancedEngineAnimator.cs`.
2. Check that `Animator` component exists (if using skeletal animation).
3. Run `OnValidate()` in Inspector to recalculate phase offsets.

---

## 13. Summary & Quick Reference

**Project Type**: AR Foundation mobile application targeting Android.
**Core Mechanic**: Interactive oil-pouring simulator with state machine control flow.
**Key Technologies**: XR Foundation, New Input System, Particle Systems, Advanced Animation Math.
**Main Script**: `AR_OilInteraction.cs` (orchestrator), `AdvancedEngineAnimator.cs` (simulation).
**Build Target**: Android 7.0+ (API 24+), ARM64 architecture, Samsung S23 (reference device).
**Primary Scenes**: `Assets/Scenes/AR Oil Engine.unity` (enabled in build).

This comprehensive implementation provides a solid foundation for demonstrating oil product benefits in an immersive AR experience.