using BoneUtils.Entity.Skeleton;
using BoneUtils.Helpers;
using BoneUtils.Math;
using Raylib_cs;
using System.Numerics;

namespace BoneUtils.RayLib.RayLibDemos;
public class DemoCharacter :DemoBase {

	private SkeletonEntity Character;
	private SkeletonEntityOps SkelOps;

	private bool IsWaving = false;
	private Quat qNegZ = new Quat(Quaternion.CreateFromYawPitchRoll(0.0f, MathHelper.DegToRad(-2), 0.0f)); // TODO Quat
	private Quat qPosZ = new Quat(Quaternion.CreateFromYawPitchRoll(0.0f, MathHelper.DegToRad(2), 0.0f)); // TODO Quat
	private bool WaveDirection = false;

	public DemoCharacter(SkeletonEntityOps skeops) {
		SkelOps = skeops;
		Character = ConstructSkeleton();
		Character.RootNode.Rotate(new Quat(Quaternion.CreateFromYawPitchRoll(MathF.PI/2, 0.0f, 0.0f))); // TODO Quat
	}
	public override void Draw2D() {
		Raylib.DrawText("Press 1 to wave :)", 10, 50, 20, Color.Red);
	}
	public override void Draw3D() {
		DrawBoneNodeNetwork(Character);
		DrawQuaternionOrientation(Character);
	}
	public override void HandleDemoInput() {
		if (Raylib.IsKeyPressed(KeyboardKey.One))
			IsWaving = !IsWaving;
	}
	public override void Update(float deltaTime) {
		if (IsWaving) {
			// TODO Quat
			if(MathHelper.QuaternionToEuler(Character.Bones["L_Shoulder"].Transform.Rotation.ToQuaternion()).X > MathF.PI/2
			|| MathHelper.QuaternionToEuler(Character.Bones["L_Shoulder"].Transform.Rotation.ToQuaternion()).X < -MathF.PI/2)
				WaveDirection = !WaveDirection;
				
			Character.Bones["L_Shoulder"].Rotate(WaveDirection ? qNegZ : qPosZ);
			Character.Bones["R_Shoulder"].Rotate(WaveDirection ? qPosZ : qNegZ);
		}
	}
	private SkeletonEntity ConstructSkeleton() { 
		var sken = Mock_TestEntity01();
		sken.WorldPosition = new Vector3(-6, 0, 0);

		SkelOps.PreProcessSkeleton(ref sken, [
			SkelOps.ValidateBoneNodeTree,
			SkelOps.LabelDepthBoneNodeTree,
			SkelOps.BoneNodeTreeBuildRenderLists
			]);

		return sken;
	}
}
