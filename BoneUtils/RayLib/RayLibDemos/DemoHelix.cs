using BoneUtils.Entity.Skeleton;
using BoneUtils.Helpers;
using BoneUtils.Math;
using Raylib_cs;
using System.Numerics;

namespace BoneUtils.RayLib.RayLibDemos;
public class DemoHelix :DemoBase {

	private SkeletonEntity Helix;
	private SkeletonEntityOps SkelOps;
	private Quat Spin = Quat.Create(MathHelper.DegToRad(0.2f), Vector3.UnitX);

	public DemoHelix(SkeletonEntityOps skeops) {
		SkelOps = skeops;
		Helix = ConstructSkeleton();
	}
	public override void Draw3D() {
		DrawBoneNodeConnectors(Helix);
	}
	public override void Draw2D() {
		Raylib.DrawText("A 4096 node skeleton.", 10, 50, 20, Color.White);
	}
	public override void Update(float deltaTime) {
		Helix.RootNode.Rotate(Spin);
	}
	private SkeletonEntity ConstructSkeleton() { 
		var sken = Mock_Helix();
		SkelOps.PreProcessSkeleton(ref sken, [
			SkelOps.ValidateBoneNodeTree,
			SkelOps.BoneNodeTreeBuildRenderLists
			]);
		sken.RootNode.Rotate(Quat.Create(MathF.PI/2, Vector3.UnitY));
		sken.RootNode.Translate(new Vector3(6,2,0));

		return sken;
	}
}
