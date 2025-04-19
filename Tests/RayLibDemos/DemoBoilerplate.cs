using BoneUtils.Entity.Skeleton;
using Raylib_cs;
using System.Numerics;

namespace BoneUtils.Tests.RayLibDemos;
public class DemoBoilerplate :DemoBase {

	private SkeletonEntity Spine;
	private SkeletonEntityOps SkelOps;

	public DemoBoilerplate(SkeletonEntityOps skeops) {
		this.SkelOps = skeops;
		this.Spine = ConstructSkeleton();
	}
	public override void DrawHelpOverlay() {
		Raylib.DrawText("Helpful hints here!", 10, 50, 20, Color.Red);
	}
	public override void Draw() {
		//foreach(var bone in Spine.Bones.Values)
		//	Raylib.DrawSphere(Spine.BoneWorldPosition(bone), 0.25f, Color.Red);
	}
	public override void HandleDemoInput() {
		//if(Raylib.IsKeyPressed(KeyboardKey.One))
		//	Spin("Root", q);
	}
	private void Spin(string node, Quaternion q) {
		Spine.Bones[node].Rotate(q);
	}
	private SkeletonEntity ConstructSkeleton() { 
		var sken = Mock_Spine();

		if(!SkelOps.ValidateBoneNodeTree(sken)) 
			throw new FormatException("BoneNode tree is invalid: check for duplicates or circular relationships.");
		if(SkelOps.LabelDepthBoneNodeTree(sken) == null)
			throw new Exception("BoneNode tree is too deep.");

		return sken;
	}
}
