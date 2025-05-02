using BoneUtils.Entity.Skeleton;
using BoneUtils.RayLib.RayLibDemos;
using Raylib_cs;
using System.Numerics;

namespace BoneUtils.RayLib;
public class RaylibDemoRunner {
	public Camera3D Camera;

	private SkeletonEntityOps SkelOps;
	private int DemoSelector = 0;
	private List<IDemo> Demos = [];
	private bool EnableOverlay2D = true;

	private float deltaTime = 0.0f;
	private float deltaTimeHigh = 0.0f;

	public RaylibDemoRunner() {
		SkelOps = new();
		InitCamera();
		InitDemos();
	}
	
	// Active demo interfaces

	public IDemo ActiveDemo => Demos[DemoSelector];
	public void DrawDemo3D() => ActiveDemo.Draw3D();
	public void DrawDemo2D() {
		if(EnableOverlay2D) 
			ActiveDemo.Draw2D();
	}

	// Init

	private void InitDemos() {
		Demos.Add(new DemoHelix(SkelOps));
		Demos.Add(new DemoWave(SkelOps));
		Demos.Add(new DemoSimpleSpine(SkelOps));
		Demos.Add(new DemoCharacter(SkelOps));
	}
	private void InitCamera() {
		Camera = new Camera3D {
			Position = new Vector3(6.0f, 2.0f, 0.0f),
			Target = new Vector3(0.0f, 2.0f, 0.0f),
			Up = new Vector3(0.0f, 1.0f, 0.0f),
			FovY = 60.0f,
			Projection = CameraProjection.Perspective
		};
	}
	
	// Input, update handling

	public void HandleInput() {
		if(Raylib.IsKeyPressed(KeyboardKey.F1))
			EnableOverlay2D = !EnableOverlay2D;
		if(Raylib.IsKeyPressed(KeyboardKey.F2))
			DemoSelector = 0;
		if(Raylib.IsKeyPressed(KeyboardKey.F3))
			DemoSelector = 1;
		if(Raylib.IsKeyPressed(KeyboardKey.F4))
			DemoSelector = 2;
		if(Raylib.IsKeyPressed(KeyboardKey.F5))
			DemoSelector = 3;
		ActiveDemo.HandleDemoInput();
	}
	public void Update(float dt) {
		ActiveDemo.Update(dt);
		DeltaTimeTracker(dt);
	}

	// Utility

	public void DeltaTimeTracker(float dt) {
		deltaTime = dt;
		if(deltaTime > deltaTimeHigh) deltaTimeHigh = deltaTime;
	}

	// 2D overlay

	public void DrawRunner2D() {
		DrawHelp();
		DrawFPSTiming();
	}

	private void DrawHelp() {
		Raylib.DrawText("F1: Show help | Demos: F2, F3, F4, F5", 220, 10, 20, Color.Yellow);
	}
	private void DrawFPSTiming() {
		Raylib.DrawFPS(10, 10);
		Raylib.DrawText($"dt: {deltaTime}", 100, 10, 16, Color.Green);
		Raylib.DrawText($"high: {deltaTimeHigh}", 100, 26, 16, Color.Green);
	}
}

