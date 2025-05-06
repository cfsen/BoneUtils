using BoneUtils.Entity.Skeleton;
using BoneUtils.Math;
using BoneUtils.Mockups;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace Tests.Skeleton_Tests;
[TestClass]
[TestCategory("SkeletonEntity transforms")]
public class SkeletonEntity_TransformTests :MockDataBuilder {
	[TestMethod]
	public void Entity_WorldTranslateVectors() {
		SkeletonEntity en = Mock_Spine();

		en.WorldPosition = new(1, 1, 1);

		var worldRoot = en.BoneWorldPosition(en.RootNode);
		var worldSpineA = en.BoneWorldPosition(en.Bones["SpineA"]);
		var worldSpineB = en.BoneWorldPosition(en.Bones["SpineB"]);
		var worldSpineC = en.BoneWorldPosition(en.Bones["SpineC"]);

		Assert.AreEqual(Vector3.Zero, en.RootNode.Transform.Position, "RootNode (local): Unexpected position.");
		Assert.AreEqual(new Vector3(1,1,1), worldRoot, "RootNode (world): Unexpected position.");

		Assert.AreEqual(new Vector3(0,1,0), en.Bones["SpineA"].Transform.Position, "SpineA (local): Unexpected position");
		Assert.AreEqual(new Vector3(1,2,1), worldSpineA, "SpineA (world): Unexpected position.");

		Assert.AreEqual(new Vector3(0,2,0), en.Bones["SpineB"].Transform.Position, "SpineB (local): Unexpected position");
		Assert.AreEqual(new Vector3(1,3,1), worldSpineB, "SpineB (world): Unexpected position.");

		Assert.AreEqual(new Vector3(0,3,0), en.Bones["SpineC"].Transform.Position, "SpineC (local): Unexpected position");
		Assert.AreEqual(new Vector3(1,4,1), worldSpineC, "SpineC (world): Unexpected position.");
	}
	[TestMethod]
	public void Entity_RotateSpine() {
		SkeletonEntity en = Mock_Spine();

		Quat q = Quat.Create(MathF.PI/2, Vector3.UnitZ);
		en.RootNode.Rotate(q);

		// Expected normalized and corrected results from BoneNodes
		Quat q_root = new(X: 0, Y: 0, Z: 0.7071068f, W: 0.7071068f);
		Quat q_nodes = new(X: 0.5f, Y: -0.5f, Z: 0.5f, W: 0.5f);
		Vector3 p_root = Vector3.Zero;
		Vector3 p_nodes = new(1, 0, 0);

		Assert.AreEqual(p_root, en.Bones["Root"].Transform.Position, "Root: Position moved, should remain in place.");
		Assert.AreEqual(q_root, en.Bones["Root"].Transform.Rotation, "Root: Unexpected orientation.");

		Assert.AreEqual(p_nodes, en.Bones["SpineA"].Transform.Position, "SpineA: Unexpected position.");
		Assert.AreEqual(q_nodes, en.Bones["SpineA"].Transform.Rotation, "SpineA: Unexpected orientation.");

		Assert.AreEqual(p_nodes*2, en.Bones["SpineB"].Transform.Position, "SpineB: Unexpected position.");
		Assert.AreEqual(q_nodes, en.Bones["SpineB"].Transform.Rotation, "SpineB: Unexpected orientation.");

		Assert.AreEqual(p_nodes*3, en.Bones["SpineC"].Transform.Position, "SpineC: Unexpected position.");
		Assert.AreEqual(q_nodes, en.Bones["SpineC"].Transform.Rotation, "SpineC: Unexpected orientation.");

	}
	[TestMethod]
	public void Entity_TranslateSpine() {
		SkeletonEntity en = Mock_Spine();

		Vector3 translate = new(1, 1, 1);
		en.RootNode.Translate(translate);

		Assert.AreEqual(new Vector3(1,1,1), en.RootNode.Transform.Position, "Root: Unexpected position.");
		Assert.AreEqual(new Vector3(1,2,1), en.Bones["SpineA"].Transform.Position, "SpineA: Unexpected position.");
		Assert.AreEqual(new Vector3(1,3,1), en.Bones["SpineB"].Transform.Position, "SpineB: Unexpected position.");
		Assert.AreEqual(new Vector3(1,4,1), en.Bones["SpineC"].Transform.Position, "SpineC: Unexpected position.");
	}
}
