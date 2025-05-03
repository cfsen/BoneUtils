using BoneUtils.Entity.Skeleton;
using BoneUtils.Helpers;
using BoneUtils.Math;
using Raylib_cs;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.Metadata;

namespace BoneUtils.RayLib.RayLibDemos;
public class DemoSimpleSpine :DemoBase {
	private RaylibRenderer Renderer;
	private SkeletonEntity Spine;
	private SkeletonEntityOps SkelOps;
	private Quat q = Quat.Normalize(Quat.Create(MathF.PI/2, Vector3.UnitX));
	private Quat qAnimate = Quat.Normalize(Quat.Create(MathHelper.DegToRad(0.2f), Vector3.UnitX));
	private Quat qAnimate2 = Quat.Normalize(Quat.Create(MathHelper.DegToRad(0.2f), Vector3.UnitY));
 
	public DemoSimpleSpine(SkeletonEntityOps skeops, RaylibRenderer renderer) {
		SkelOps = skeops;
		Spine = ConstructSkeleton();
		Renderer = renderer;
	}

	public override void Draw3D() {
		DrawBoneNodeRendered(Spine, Renderer);
		//DrawBoneNodeNetwork(Spine, true);
		//DrawQuaternionOrientation(Spine);
	}
	public override void Draw2D() {
		Raylib.DrawText("Basic rotation propagation.", 10, 50, 20, Color.White);
		Raylib.DrawText("Press 1,2,3,4 to rotate the root, SpineA, SpineB, SpineC bones.", 10, 75, 20, Color.White);
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
	public override void Update(float deltaTime) {
		AnimateRotation();
	}
	private void Spin(string node, Quat q) {
		Spine.Bones[node].Rotate(q);
	}
	private void AnimateRotation() {
		foreach(var bone in Spine.Bones.Values) {
			bone.Rotate(qAnimate);
			bone.Rotate(qAnimate2);
		}
	}
	private SkeletonEntity ConstructSkeleton() { 
		var spine = Mock_Spine();
		SkelOps.PreProcessSkeleton(ref spine, [
			SkelOps.ValidateBoneNodeTree,
			SkelOps.BoneNodeTreeBuildRenderLists
			]);
		return spine;
	}
}
