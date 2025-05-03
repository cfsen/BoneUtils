using BoneUtils.Entity.Skeleton;
using BoneUtils.Math;
using Raylib_cs;
using System.Numerics;

namespace BoneUtils.RayLib.RayLibDemos;
public class DemoBoilerplate :DemoBase {
	private RaylibRenderer Renderer;

	private SkeletonEntity Spine;
	private SkeletonEntityOps SkelOps;

	public DemoBoilerplate(SkeletonEntityOps skeops, RaylibRenderer renderer) {
		SkelOps = skeops;
		Spine = ConstructSkeleton();
		Renderer = renderer;
		renderMode = RenderMode.Fancy;
	}
	public override void Draw3D() {
		Render(Spine, Renderer);
	}
	public override void Draw2D() {
		Raylib.DrawText("Helpful hints here!", 10, 50, 20, Color.Red);
	}
	public override void HandleDemoInput() {
		//if(Raylib.IsKeyPressed(KeyboardKey.One))
	}
	public override void Update(float deltaTime) {
		
	}
	private SkeletonEntity ConstructSkeleton() { 
		var sken = Mock_Spine();
		SkelOps.PreProcessSkeleton(ref sken, [
			SkelOps.ValidateBoneNodeTree,
			SkelOps.BoneNodeTreeBuildRenderLists
			]);

		return sken;
	}
}
