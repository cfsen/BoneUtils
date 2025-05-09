using BoneUtils.Entity.Skeleton;
using BoneUtils.RayLib.RayLibDemos;
using Raylib_cs;
using System.Numerics;

namespace BoneUtils.RayLib;
public class RaylibDemoRunner {
	public Camera3D Camera;

	private SkeletonEntityOps SkelOps;
	public RaylibRenderer RayRender;

	private int DemoSelector = 0;
	private List<IDemo> Demos = [];
	private bool EnableOverlay2D = true;

	private float deltaTime = 0.0f;
	private float deltaTimeHigh = 0.0f;

	private GuiText guiText = new();
	private string guiDt = string.Empty;
	private string guiDtHigh = string.Empty;

	public RaylibDemoRunner() {
		SkelOps = new();
		Camera = InitCamera();
		RayRender = new(Camera);
	}
	
	// Active demo interfaces

	public IDemo ActiveDemo => Demos[DemoSelector];
	public void DrawDemo3D() => ActiveDemo.Draw3D();
	public void DrawDemo2D() {
		if(EnableOverlay2D) 
			ActiveDemo.Draw2D();
	}

	// Init

	public void InitDemos() {
		Demos.Add(new DemoAnimatorBasic(SkelOps, RayRender));
		Demos.Add(new DemoHelix(SkelOps));
		Demos.Add(new DemoWave(SkelOps));
		Demos.Add(new DemoSimpleSpine(SkelOps, RayRender));
		Demos.Add(new DemoCharacter(SkelOps, RayRender));
	}
	private Camera3D InitCamera() {
		return new Camera3D {
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
		if(deltaTime > deltaTimeHigh) {
			deltaTimeHigh = deltaTime;
			guiDtHigh = deltaTimeHigh.ToString(); // can be optimized further
		}
		guiDt = dt.ToString();
	}

	// 2D overlay

	public void DrawRunner2D() {
		DrawHelp();
		DrawFPSTiming();
	}

	private void DrawHelp() {
		Raylib.DrawText(guiText.Help, 220, 10, 20, Color.Yellow);
	}
	private void DrawFPSTiming() {
		Raylib.DrawFPS(10, 10);
		Raylib.DrawText(guiText.DeltaTime, 100, 10, 16, Color.Green);
		Raylib.DrawText(guiText.DeltaTimeHigh, 100, 26, 16, Color.Green);
		Raylib.DrawText(guiDt, 140, 10, 16, Color.Green); 
		Raylib.DrawText(guiDtHigh, 140, 26, 16, Color.Green);
	}
	private struct GuiText {
		public string Help = "F1: Show help | Demos: F2, F3, F4, F5";
		public string DeltaTime = "dt:";
		public string DeltaTimeHigh ="high:";
		public GuiText(){}
	}
}

