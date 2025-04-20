using BoneUtils.Entity.Skeleton;
using Raylib_cs;
using System.Numerics;

namespace BoneUtils.RayLib.RayLibDemos;
public class DemoCharacter :DemoBase {

	private SkeletonEntity Character;
	private SkeletonEntityOps SkelOps;

	public DemoCharacter(SkeletonEntityOps skeops) {
		SkelOps = skeops;
		Character = ConstructSkeleton();
		Character.RootNode.Rotate(Quaternion.CreateFromYawPitchRoll(MathF.PI/2, 0.0f, 0.0f));
	}
	public override void DrawHelpOverlay() {
		Raylib.DrawText("Helpful hints here!", 10, 50, 20, Color.Red);
	}
	public override void Draw() {
		DrawBoneNodeNetwork(Character);
	}
	public override void HandleDemoInput() {
		//if(Raylib.IsKeyPressed(KeyboardKey.One))
		//	Spin("Root", q);
	}
	private void Spin(string node, Quaternion q) {
		Character.Bones[node].Rotate(q);
	}
	private SkeletonEntity ConstructSkeleton() { 
		var sken = Mock_TestEntity01();

		if(!SkelOps.ValidateBoneNodeTree(sken)) 
			throw new FormatException("BoneNode tree is invalid: check for duplicates or circular relationships.");
		if(SkelOps.LabelDepthBoneNodeTree(sken) == null)
			throw new Exception("BoneNode tree is too deep.");

		return sken;
	}
}
