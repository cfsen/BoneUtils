using BoneUtils.Entity.Skeleton;
using BoneUtils.Math;
using Raylib_cs;
using System.Numerics;

namespace BoneUtils.RayLib.RayLibDemos;
public class DemoBoilerplate :DemoBase {

	private SkeletonEntity Spine;
	private SkeletonEntityOps SkelOps;

	public DemoBoilerplate(SkeletonEntityOps skeops) {
		SkelOps = skeops;
		Spine = ConstructSkeleton();
	}
	public override void Draw3D() {
		//foreach(var bone in Spine.Bones.Values)
		//	Raylib.DrawSphere(Spine.BoneWorldPosition(bone), 0.25f, Color.Red);
	}
	public override void Draw2D() {
		Raylib.DrawText("Helpful hints here!", 10, 50, 20, Color.Red);
	}
	public override void HandleDemoInput() {
		//if(Raylib.IsKeyPressed(KeyboardKey.One))
		//	Spin("Root", q);
	}
	public override void Update(float deltaTime) {
		
	}
	private void Spin(string node, Quat q) {
		Spine.Bones[node].Rotate(q); // TODO Quat
	}
	private SkeletonEntity ConstructSkeleton() { 
		var sken = Mock_Spine();
		SkelOps.PreProcessSkeleton(ref sken, [
			SkelOps.ValidateBoneNodeTree,
			SkelOps.LabelDepthBoneNodeTree,
			SkelOps.BoneNodeTreeBuildRenderLists
			]);

		return sken;
	}
}
