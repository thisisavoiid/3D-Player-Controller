# 3D-Player-Controller (C# Unity Template)

A **Unity project template** for a **First-Person Controller** featuring basic rigid-body physics-based movement, jump, a raycast-driven interaction system, and visual feedback (crosshair and interaction label).
The architecture is divided into modular components for input, movement, interaction, and UI management.

---

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [Project Structure](#project-structure)
- [Core Mechanics & Design](#core-mechanics--design)
- [Author](#author)

---

## Features

- **Physics-based Movement** (Walk, Sprint, Jump) using `Rigidbody`.
- **Look System** with adjustable sensitivity and camera clamping.
- **Raycast-driven Interaction System** to detect objects in front of the player.
- **Visual Feedback** with a dynamic crosshair and an interaction label.
- **Modular Component Design** (`PlayerController`, `PlayerInteraction`, `CrosshairManager`).
- **Input System** integration for movement, look, and interaction actions.
- **Reusable `IInteractable` interface** for easy setup of world objects.

---

## Installation

1. Clone the repository (placeholder):

```bash
git clone [https://github.com/your-username/FirstPersonControllerTemplate.git](https://github.com/your-username/FirstPersonControllerTemplate.git)
````

2.  Navigate into the project directory:

<!-- end list -->

```bash
cd FirstPersonControllerTemplate
```

3.  Open the directory in the **Unity Editor** (2021 LTS or newer recommended).

-----

## Usage

1.  Open the **Main Scene** located in the `Scenes/` folder.
2.  Ensure the **`PlayerController`** GameObject is present and configured.
3.  Start the game by pressing **Play** in the Unity Editor.

**Controls:**

| Action | Input | Description |
| :--- | :--- | :--- |
| **Move** | **W, A, S, D** | Translates the player's position. |
| **Look** | **Mouse Movement** | Rotates the camera and player body. |
| **Sprint** | **Left Shift** | Increases movement speed. |
| **Jump** | **Space** | Executes a vertical impulse jump. |
| **Interact** | **Left Mouse Button** | Activates the `Interact()` method on objects within reach. |

-----

## Project Structure

Below is an overview of the main components of the codebase:

| Component | Files | Main Function | Detailed Responsibility |
| :--- | :--- | :--- | :--- |
| **Input & Core Control** | `PlayerController.cs` | Handles input binding, movement, and view logic. | Manages `FixedUpdate` (Movement) and `Update` (Look, Jump, Interact). Uses `Rigidbody` for physics. Contains **MovementState** (Grounded/InAir) and **MovementMode** (Walk/Sprint) enums. Initializes and enables input actions. |
| **Interaction Logic** | `PlayerInteraction.cs` | Manages player's view-based interaction detection. | Uses **Raycasting** (`CheckForInteractable`) from the camera forward vector to detect objects on the specified layer within reach. Updates the **Crosshair** and **InteractLabel** visibility based on detection. |
| **Interactable Objects** | `IInteractable.cs`, `Box.cs` | Defines the behavior for interactable world objects. | **`IInteractable`** is the contract for all interactable objects. **`Box.cs`** is a simple example implementation of this interface, logging a message upon interaction. |
| **UI/HUD Management** | `CrosshairManager.cs`, `InteractLabel.cs` | Provides visual feedback for interaction and game state. | **`CrosshairManager`** manages the color state (**Base/CanInteract**) of the crosshair via an animated **`FadeColorRoutine`** using `Color.Lerp`. **`InteractLabel`** simply hides/shows the interaction prompt GameObject. |
| **Geometry & Movement** | N/A | Core Unity components used. | Uses `CapsuleCollider` for player shape. Employs `Physics.OverlapBox` and `Physics.Raycast` for collision and ground checks. |

-----

## Core Mechanics & Design

### **Physics-Based Movement (`PlayerController.cs`)**

  * **Movement:** Movement input is read as a `Vector3` and transformed to world space direction. Velocity is applied directly to the `Rigidbody`'s `linearVelocity`.
  * **Sprinting:** The `_sprintSpeedScale` is applied to the target linear velocity if the sprint input is held.
  * **Air Control:** When in `MovementState.InAir`, the target horizontal velocity is scaled down by `_inAirSpeedScale` and smoothly interpolated (`Vector3.Lerp`) with the current horizontal velocity to simulate realistic air slowdown.
  * **Jumping:** A one-time `ForceMode.Impulse` is applied to the `Rigidbody` along the player's up vector using `_jumpPower`.
  * **Ground Check:** Ground contact is managed using `OnCollisionEnter` to set the state to `MovementState.Grounded`, which is verified by `GetGroundCollisionObjects()` using `Physics.OverlapBox`.

### **Raycast Interaction System (`PlayerInteraction.cs`)**

  * **Detection:** The system uses a **raycast** originating from the player's camera (`_playerCamera.transform.position`) pointing forward (`_playerCamera.transform.forward`).
  * **Range:** The raycast distance is limited by `_interactionReachDistance`, and it only hits objects on the configured `_interactableLayers`.
  * **State Update:** In `FixedUpdate`, **`GetInteractable()`** continuously checks for valid objects. If an object is hit and successfully retrieves the **`IInteractable`** component:
      * The **`CrosshairManager`** state is set to `CrosshairState.CanInteract`.
      * The **`InteractLabel`** is shown.
  * **Interaction Trigger:** The **`PlayerController.Interact()`** method is called when the **Interact input** is pressed (`_interact.WasPressedThisFrame()`). It calls `_interaction.GetInteractable()` once and executes the `Interact()` method on the returned object.

### **Dynamic UI Feedback (`CrosshairManager.cs` & `InteractLabel.cs`)**

  * **Crosshair Color:** The `CrosshairManager` uses a **coroutine** (`FadeColorRoutine`) to smoothly transition the crosshair's color between the `_baseColor` and the `_interactColor` whenever an interactable object enters or leaves the player's view, providing instant, non-jarring feedback.
  * **Label:** The `InteractLabel` simply toggles its GameObject's visibility to prompt the player when an interaction is possible.

-----

## Author

**Jonathan Huber** – Developer of the first person controller template project.

-----

> Built with ❤️ in C\#.
