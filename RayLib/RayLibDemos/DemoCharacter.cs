using BoneUtils.Entity.Skeleton;
using BoneUtils.Helpers;
using Raylib_cs;
using System.Numerics;

namespace BoneUtils.RayLib.RayLibDemos;
public class DemoCharacter :DemoBase {

	private SkeletonEntity Character;
	private SkeletonEntityOps SkelOps;

	private bool IsWaving = false;
	private Quaternion qNegZ = Quaternion.CreateFromYawPitchRoll(0.0f, MathHelper.DegToRad(-2), 0.0f);
	private Quaternion qPosZ = Quaternion.CreateFromYawPitchRoll(0.0f, MathHelper.DegToRad(2), 0.0f);
	private Quaternion qSpin = Quaternion.CreateFromYawPitchRoll(MathHelper.DegToRad(1), 0, 0);
	private bool WaveDirection = false;

	public DemoCharacter(SkeletonEntityOps skeops) {
		SkelOps = skeops;
		Character = ConstructSkeleton();
		Character.RootNode.Rotate(Quaternion.CreateFromYawPitchRoll(MathF.PI/2, 0.0f, 0.0f));
	}
	public override void DrawHelpOverlay() {
		Raylib.DrawText("Press 1 to wave :)", 10, 50, 20, Color.Red);
	}
	public override void Draw() {
		DrawBoneNodeNetwork(Character);
	}
	public override void HandleDemoInput() {
		if (Raylib.IsKeyPressed(KeyboardKey.One))
			IsWaving = !IsWaving;
	}
	public override void Update(float deltaTime) {
		if (IsWaving) {
			if(MathHelper.QuaternionToEuler(Character.Bones["L_Shoulder"].Transform.Rotation).X > MathF.PI/2
			|| MathHelper.QuaternionToEuler(Character.Bones["L_Shoulder"].Transform.Rotation).X < -MathF.PI/2)
				WaveDirection = !WaveDirection;
				
			Character.Bones["L_Shoulder"].Rotate(WaveDirection ? qNegZ : qPosZ);
			Character.Bones["R_Shoulder"].Rotate(WaveDirection ? qPosZ : qNegZ);
		}
	}
	private SkeletonEntity ConstructSkeleton() { 
		var sken = Mock_TestEntity01();
		sken.WorldPosition = new Vector3(-6, 0, 0);

		if(!SkelOps.ValidateBoneNodeTree(sken)) 
			throw new FormatException("BoneNode tree is invalid: check for duplicates or circular relationships.");
		if(SkelOps.LabelDepthBoneNodeTree(sken) == null)
			throw new Exception("BoneNode tree is too deep.");

		return sken;
	}
}
