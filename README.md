# Zoo World

A top-down 3D food chain simulation. Frogs jump, snakes move continuously, and one animal spawns every 1–2 seconds. Prey bounce off each other; predators eat animals on contact. uGUI displays death counters and a `Tasty!` label below the predator.

[Play in your browser](https://AwsomeSauci.github.io/Zoo-world/)

## Getting started

Install **Unity 6000.6.0f1** and **Git** for package restoration. Open the project, load **Assets/Scenes/Zoo.unity**, and press Play.

Configure species, spawn weights, spawn interval, and seed in **Assets/Configurations/Zoo Settings.asset**. Movement and feeding settings are in the adjacent folders.

## Tests and builds

- Tests: **Window → General → Test Runner**, EditMode and PlayMode tabs.
- Run checks from PowerShell: `./Tools/Verify.ps1`.
- Build WebGL locally: `./Tools/Build-Web.ps1`; Unity's Web Build Support module is required.
- GitHub Actions builds WebGL on pushes to `main` or manual runs and publishes the game to GitHub Pages.

## Architecture

- **Domain** — feeding rules, steering, and locomotion.
- **Application** — animal instances, module factories, spawning, contacts, and statistics.
- **Unity** — Rigidbody integration, body pools, camera, and uGUI.
- **Composition** — ScriptableObject settings and Extenject bindings.

Domain and Application do not depend on Unity. Animals combine movement and feeding modules, with separate state for each instance. Steering is independent of locomotion. **Extenject** wires dependencies; **R3** delivers statistics and consumption events to the UI.

## Adding a species

1. Create a prefab based on Frog or Snake: unit root scale, a dynamic Rigidbody with no gravity or damping, and frozen Y position and rotation. Use the Animal layer and one SphereCollider, BoxCollider, or CapsuleCollider on the root; place the model under Visual.
2. Use **Create → Zoo World → Animal**, then assign a unique Species ID, prefab, and movement and feeding modules.
3. Add the species to Zoo Settings with a positive weight. For example, Jumping Movement and Predator produce a jumping predator without code changes.

Add new mechanics through `ILocomotionBehaviour` / `LocomotionAsset`, `ISteeringBehaviour` / `SteeringAsset`, or `IAnimalModule` / `AnimalModuleAsset`.

## Simulation rules

- When predators meet, the animal with the lower spawn ID survives.
- Simulation runs in the XZ plane; only the model moves vertically when jumping. The orthographic camera looks down and aligns with the world axes.
- Animals return to the camera bounds and try a sideways detour when stuck. This is local steering without maze pathfinding.
