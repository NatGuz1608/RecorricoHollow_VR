# Recorrido Hollow VR (Hollow Knight VR Tour)

An interactive Virtual Reality (VR) museum and tribute experience inspired by Team Cherry's *Hollow Knight*, developed for standalone Meta Quest headsets. Players explore themed exhibition spaces using comfort-oriented teleportation locomotion, interact with character lore lecterns featuring 3D spatial voice-over narration, solve a charm-socket altar puzzle, and complete a timed crystal-placement mini-game.

---

## Technologies Used

* **Game Engine:** Unity 6.3 LTS (Universal Render Pipeline – URP)
* **Programming Language:** C# (documented with standard XML `<summary>` tags)
* **XR & VR Frameworks:**
  * **XR Interaction Toolkit (XRI):** `XRGrabInteractable`, `XRSocketInteractor`, `XRSimpleInteractable`, parabolic teleportation locomotion (`Teleportation Area`), and XR Device Simulator
  * **OpenXR Plugin:** Cross-platform XR runtime integration
  * **Meta Quest SDK:** Standalone Android VR deployment and controller tracking/input support
* **UI & Audio:** TextMeshPro (TMP) and Unity 3D Spatial Audio
* **Version Control & Asset Storage:** Git & Git LFS (tracking `.glb` 3D assets)

---

## How to Build and Install the APK

### Prerequisites
* **Unity Hub** with **Unity 6.3 LTS** and the **Android Build Support** module (including OpenJDK and Android SDK & NDK Tools).
* **Git LFS** installed locally so large `.glb` 3D models download properly when cloning.
* A **Meta Quest** headset (Quest 2, Quest 3, or Quest Pro) with **Developer Mode** enabled via the Meta Horizon mobile app.
* **ADB** (Android Debug Bridge), **SideQuest**, or **Meta Quest Developer Hub (MQDH)** for sideloading the APK.

### Step-by-Step Build Instructions
1. **Clone the Repository:**
   ```bash
   git lfs install
   git clone [https://github.com/NatGuz1608/RecorricoHollow_VR.git](https://github.com/NatGuz1608/RecorricoHollow_VR.git)
   ```
2. **Open in Unity:**
   * Open **Unity Hub**, click **Add > Add project from disk**, and select the cloned project folder.
   * Launch the project using **Unity 6.3 LTS** and wait for assets and shaders to compile.
3. **Configure Build Settings:**
   * Go to **File > Build Profiles** (or **File > Build Settings**).
   * Select **Android** and click **Switch Platform**.
   * Ensure both the main menu scene and the primary scene (`Principal`) are added to the **Scene List** in the correct order (Menu scene first).
4. **Verify XR Plug-in Management:**
   * Navigate to **Edit > Project Settings > XR Plug-in Management**.
   * Under the **Android** tab, verify that **OpenXR** is checked and the **Meta Quest** feature group is enabled without validation errors.
5. **Generate the APK:**
   * Click **Build** (to export an `.apk` file to your computer) or connect your Meta Quest via USB and click **Build and Run** to install and launch automatically.
6. **Manual Installation (Optional):**
   * If you exported the `.apk` manually, install it onto your connected headset via ADB:
     ```bash
     adb install -r RecorridoHollowVR.apk
     ```

---

## Known Issues and Future Improvements

### Known Issues

#### Code & XR Interaction Architecture
* **Cross-Scene XR Controller & Input Freeze:** Transitioning from the main menu scene to the `Principal` gameplay scene initially caused controller rays to lock up and prevented grabbing objects without throwing console errors, due to conflicting `XRInteractionManager` and `InputActionAsset` states across scenes. This is currently handled via a workaround in `ControladorMenuPrincipal.cs` (disabling the menu's `XR Origin` and `XRInteractionManager` prior to loading) paired with a one-frame delayed coroutine (`ReactivarSistemaXREnEscena` in `GestorMinijuegoCristales.cs`) that re-enables input assets, cycles the `XR Origin`, and rebinds all interactors and interactables.
* **Unstable Crystal Physics:** The grabbable luminous crystals occasionally exhibit erratic `Rigidbody` behavior, collision jitter, or unintended velocity impulses when grabbed, released, or placed near altar sockets. A distance and altitude failsafe (`RespawnSeguro.cs`) is included to reset crystals that clip or fall out of bounds.

#### 3D Asset Management & Environment Geometry
* **Missing Environment Colliders:** Certain zones and decorative structures in the imported 3D environment lack proper collider coverage, allowing players or dropped objects to pass through surfaces.
* **Large 3D Asset Footprint & Frame Drops:** High-polygon `.glb` environment and character assets (such as `final_setting_-_hollow_knight.glb` at ~106 MB and `hollow_knight_-_quirrel.glb` at ~57 MB) exceed GitHub's 100 MB standard file limit—requiring **Git LFS**—and introduce occasional frame skips ("stuttering") during scene traversal on standalone Meta Quest hardware.

#### Camera, Tracking & Spatial Audio
* **Camera Micro-Jitter & Choppy Rotation:** Experimental VR testing revealed a slight camera tremor/jitter even when the player remains completely stationary, as well as non-smooth, choppy movement during camera turns.
* **Spatial Audio & Background Music Glitches:** Inconsistencies in 3D spatial audio rolloff and background music playback/resumption occur when triggering character lore narrations and moving across zones.

### Future Improvements
* **Persistent XR Rig Architecture:** Refactor the `XR Origin`, `EventSystem`, and `XRInteractionManager` into a persistent bootstrap scene using additive scene loading to prevent cross-scene reference loss and remove runtime re-binding coroutines.
* **Environment Collider Audit:** Perform a comprehensive pass over all 3D environment meshes to add missing primitive/convex colliders and refine `Teleportation Area` boundaries.
* **Physics & Grabbing Stabilization:** Configure `Rigidbody` interpolation, `Continuous Dynamic` collision detection, and refined `XRGrabInteractable` movement/velocity tracking parameters to eliminate crystal physics instability.
* **3D Asset & Rendering Optimization:** Apply mesh decimation, Draco geometry compression, KTX2/ASTC texture compression, and baked lighting to reduce APK size, lower memory overhead, and eliminate frame drops and camera jitter on standalone Quest headsets.
* **Audio Mixer & Spatialization Refactor:** Route voice-overs, SFX, and ambient music through a Unity `AudioMixer` using smooth volume ducking/fading transitions and calibrated 3D spatial blend curves.

---

## AI Tools Used (Declaration Summary)

During the development of this project, the Artificial Intelligence tool **Google Gemini** was used as a technical collaboration assistant. The use of this technology was strictly limited to the following support areas: code debugging and optimization, architectural guidance, and documentation support. All design decisions, practical implementation within the Unity development environment, validation of experimental VR tests, and intellectual authorship of the project are entirely the responsibility of the author, who reviewed, debugged, and approved every component integrated into the final system.

Specifically, AI assistance supported:
* Diagnosing and resolving the cross-scene XR Interaction Toolkit controller freeze (`ControladorMenuPrincipal.cs` and `GestorMinijuegoCristales.cs`).
* Evaluating comfort-oriented VR locomotion alternatives (selecting teleportation over continuous movement with tunneling vignette to prevent motion sickness).
* Standardizing C# inline comments into English XML documentation.
