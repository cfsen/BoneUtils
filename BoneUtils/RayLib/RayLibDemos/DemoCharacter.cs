using BoneUtils.Entity.Skeleton;
using BoneUtils.Helpers;
using BoneUtils.Math;
using Raylib_cs;
using System.Numerics;

namespace BoneUtils.RayLib.RayLibDemos;
public class DemoCharacter :DemoBase {
	private RaylibRenderer Renderer;

	private SkeletonEntity Character;
	private SkeletonEntityOps SkelOps;

	private bool IsWaving = false;
	private Quat qNegZ = Quat.Create(MathHelper.DegToRad(-2), Vector3.UnitX);
	private Quat qPosZ = Quat.Create(MathHelper.DegToRad(2), Vector3.UnitX);
	private bool WaveDirection = false;

	public DemoCharacter(SkeletonEntityOps skeops, RaylibRenderer renderer) {
		SkelOps = skeops;
		Character = ConstructSkeleton();
		Renderer = renderer;
		renderMode = RenderMode.Fancy ^ RenderMode.QuatOrientation ^ RenderMode.Connectors;
	}
	public override void Draw2D() {
		Raylib.DrawText("Press 1 to wave :) | R to reset", 10, 50, 20, Color.White);
	}
	public override void Draw3D() {
		Render(Character, Renderer);
	}
	public override void HandleDemoInput() {
		if (Raylib.IsKeyPressed(KeyboardKey.One))
			IsWaving = !IsWaving;
		if (Raylib.IsKeyPressed(KeyboardKey.R)) {
			Character.ResetTransforms();
			// flip skeleton to face camera, since this isn't part of saved composition state
			Character.RootNode.Rotate(Quat.Create(MathF.PI/2, Vector3.UnitY));
		}
	}
	public override void Update(float deltaTime) {
		if (IsWaving) {
			if(MathHelper.QuatToEuler(Character.Bones["L_Shoulder"].Transform.Rotation).X > MathF.PI/2
			|| MathHelper.QuatToEuler(Character.Bones["L_Shoulder"].Transform.Rotation).X < -MathF.PI/2)
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
			SkelOps.BoneNodeTreeBuildRenderLists
			]);

		sken.RootNode.Rotate(Quat.Create(MathF.PI/2, Vector3.UnitY));

		return sken;
	}
}
