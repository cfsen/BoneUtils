# BoneUtils
A modular C# library for managing skeletal hierarchies, built for portability, flexibility and extensibility in animation and simulation systems.

### 🧱 Design Goals

- **Modular by default** – Every feature is opt-in, favoring composition over inheritance.
- **Data-first** – Structures are declarative and validated before execution.
- **Built for integration** – Designed to slot into larger systems without friction.

---

### ✅ Current Features

- 🔧 **Declarative skeleton composition**  
  Compose and validate skeletal structures using mutator delegates. Ensures structure integrity through early validation passes like circular reference checks.

- 🔁 **Transform operations with automatic propagation**  
  Common operations like rotation and translation are recursively applied through child nodes, maintaining consistent spatial hierarchies. Allows advanced control over transforms through behavior injection.

- 🧪 **Interactive visual demos (Raylib)**  
  Real-time visualizations built with [raylib_cs](https://github.com/Raylib-cs/Raylib-cs) demonstrate bone behavior and transformation flow for testing and debugging.

### 🚀 Upcoming Features

- 🎞️ **Builder-based animation sequencing system**  
  Currently implementing a `AnimationBuilder` API that simplifies creation of frame sequences and their associated blend transitions. Prioritizes ease of use while allowing advanced behavior injection and flexible sequencing.

- 🧱 **Composable behavior injection**  
  Support for injecting custom blend behavior or transformation logic into the animation system via delegates or interfaces, enabling non-linear and domain-specific animation behaviors.

- 🔄 **Robust validation and export pipeline**  
  Ensures all built animations are validated for timeline consistency, transform types, and blending integrity before exporting into the runtime `AnimationContainer` format.

- 🧠 **Intelligent state tracking during build**  
  Internal promise-based blending management allows deferred frame resolution and error-resistant sequencing.

### 💡 Planned Features

Features that are scoped for near-future implementation but not currently in development.

- **IK System** – Inverse kinematics for bone constraints and target-based posing.  
- **Physics Simulation** – Allow physically accurate bone/transform behavior.  
- **Extensive Testing** – Broader unit and integration test coverage (within reason).
