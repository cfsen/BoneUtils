using BoneUtils.Entity.Skeleton;
using Raylib_cs;
using System.Numerics;
using System.Reflection.Metadata;

namespace BoneUtils.RayLib.RayLibDemos;
public class DemoSimpleSpine :DemoBase {

	private SkeletonEntity Spine;
	private SkeletonEntityOps SkelOps;
	private Quaternion q = Quaternion.CreateFromYawPitchRoll(0.0f, MathF.PI/2, 0.0f);

	public DemoSimpleSpine(SkeletonEntityOps skeops) {
		SkelOps = skeops;
		Spine = ConstructSkeleton();
	}

	public override void DrawHelpOverlay() {
		Raylib.DrawText("Press 1,2,3,4 to rotate the root, SpineA, SpineB, SpineC bones.", 10, 50, 20, Color.Red);
	}
	public override void Draw() {
		DrawBoneNodeNetwork(Spine);
	}
	public override void HandleDemoInput() {
		if(Raylib.IsKeyPressed(KeyboardKey.One))
			Spin("Root", q);
		if(Raylib.IsKeyPressed(KeyboardKey.Two))
			Spin("SpineA", q);
		if(Raylib.IsKeyPressed(KeyboardKey.Three))
			Spin("SpineB", q);
		if(Raylib.IsKeyPressed(KeyboardKey.Four))
			Spin("SpineC", q);
	}
	private void Spin(string node, Quaternion q) {
		Spine.Bones[node].Rotate(q);
	}
	private SkeletonEntity ConstructSkeleton() { 
		var spine = Mock_Spine();

		if(!SkelOps.ValidateBoneNodeTree(spine)) 
			throw new FormatException("BoneNode tree is invalid: check for duplicates or circular relationships.");
		if(SkelOps.LabelDepthBoneNodeTree(spine) == null)
			throw new Exception("BoneNode tree is too deep.");

		return spine;
	}
}
