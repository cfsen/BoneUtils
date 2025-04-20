using BoneUtils.Entity.Skeleton;
using BoneUtils.RayLib.RayLibDemos;
using BoneUtils.Tests;
using Raylib_cs;

namespace BoneUtils.RayLib;
public class RayLibVisualTest :MockDataBuilder{
	private SkeletonEntityOps SkelOps;
	private int DemoSelector = 0;
	private List<IDemo> Demos = [];
	private bool EnableHelpOverlay = true;

	public RayLibVisualTest() {
		SkelOps = new();
		InitDemos();
	}

	public IDemo ActiveDemo => Demos[DemoSelector];
	public void DrawDemo() => ActiveDemo.Draw();

	private void InitDemos() {
		Demos.Add(new DemoSimpleSpine(SkelOps));
		Demos.Add(new DemoCharacter(SkelOps));
	}
	public void DrawHelpOverlay() {
		if(EnableHelpOverlay) {
			ActiveDemo.DrawHelpOverlay();
		}
	}
	public void HandleInput() {
		if(Raylib.IsKeyPressed(KeyboardKey.F1))
			EnableHelpOverlay = !EnableHelpOverlay;
		if(Raylib.IsKeyPressed(KeyboardKey.F2))
			DemoSelector = 0;
		if(Raylib.IsKeyPressed(KeyboardKey.F3))
			DemoSelector = 1;
		//if(Raylib.IsKeyPressed(KeyboardKey.F2))
	}
	public void DrawHelp() {
		Raylib.DrawText("Press F1 for controls.", 290, 10, 20, Color.Yellow);
	}
}

