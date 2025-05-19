using BoneUtils.Entity.Skeleton;
using BoneUtils.Math;
using BoneUtils.Mockups;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace Tests.SkeletonEntityOps_Tests;
[TestClass]
[TestCategory("SkeletonEntityOps composition")]
public class SkeletonEntityOps_CompositionTests :MockDataBuilder {
	[TestMethod]
	public void SkeletonEntityOps_Mutator_Guards() {
		SkeletonEntity en = Mock_Spine();
		SkeletonEntityOps ops = new();

		// Check rejection of DFS/BFS/recursive mutators pre-validation
		Assert.IsFalse(ops.BoneNodeTreeBuildRenderLists(ref en), "Should not allow recursive mutator before validation.");
		Assert.IsFalse(ops.BoneNodeTreeCalculateConstraints(ref en), "Should not allow BFS mutator before validation.");
		Assert.IsFalse(ops.LabelDepthBoneNodeTree(ref en), "Should not allow BFS mutator before validation.");

		// Simple mutators such as iterators should be allowed pre-validation
		Assert.IsTrue(ops.BoneNodeTreeSetParentEntity(ref en), "Should allow iterator mutator before validation.");

		// Validate skeleton
		Assert.IsTrue(ops.ValidateBoneNodeTree(ref en), "Should validate correctly composed skeletons.");

		// Check rejection of DFS/BFS/recursive mutators pre-validation
		Assert.IsFalse(ops.BoneNodeTreeBuildRenderLists(ref en), "Should not allow recursive mutator without validator registered in skeleton.");
		Assert.IsFalse(ops.BoneNodeTreeCalculateConstraints(ref en), "Should not allow BFS mutator without validator registered in skeleton.");
		Assert.IsFalse(ops.LabelDepthBoneNodeTree(ref en), "Should not allow BFS mutator without validator registered in skeleton.");

		// Register validator in skeleton (normally handled by ops.PreProcess)
		en.Mutators.Add("ValidateBoneNodeTree");

		// Run mutators 
		Assert.IsTrue(ops.BoneNodeTreeBuildRenderLists(ref en), "Should allow recursive mutator after validation registration.");
		Assert.IsTrue(ops.BoneNodeTreeCalculateConstraints(ref en), "Should allow BFS mutator after validation registration.");
		Assert.IsTrue(ops.LabelDepthBoneNodeTree(ref en), "Should allow BFS mutator after validation registration.");

	}
	[TestMethod]
	public void SkeletonEntityOps_Mutator_RenderList() {
		SkeletonEntity sken = Mock_Spine();
		SkeletonEntityOps ops = new();
		
		Assert.AreEqual(0, sken.RenderBones.Count, "Should be 0 before preprocessing.");

		ops.PreProcessSkeleton(ref sken, [
			ops.ValidateBoneNodeTree,
			ops.BoneNodeTreeBuildRenderLists,
			]);

		Assert.AreEqual(4, sken.RenderBones.Count, "Should equal the number of bones in skeleton.");

		// Check that all bones were added
		HashSet<string> registered = [.. sken.Bones.Keys];
		foreach(var bone in sken.RenderBones)
			if(!registered.Contains(bone.Name))
				Assert.Fail($"Missing render bone: {bone.Name}");

	}
	[TestMethod]
	public void SkeletonEntityOps_Mutator_LabelDepthBoneNodeTree() {
		SkeletonEntityOps ops = new();
		SkeletonEntity spine = Mock_Spine();
		SkeletonEntity chara = Mock_TestEntity01();

		ops.PreProcessSkeleton(ref spine, [
			ops.ValidateBoneNodeTree, 
			ops.LabelDepthBoneNodeTree
			]);

		ops.PreProcessSkeleton(ref chara, [
			ops.ValidateBoneNodeTree, 
			ops.LabelDepthBoneNodeTree
			]);

		// Non-branching skeleton
		Assert.AreEqual(0, spine.RootNode.TreeDepth, "RootNode should be depth 0.");
		Assert.AreEqual(1, spine.Bones["SpineA"].TreeDepth, "SpineA should be depth 1.");
		Assert.AreEqual(2, spine.Bones["SpineB"].TreeDepth, "SpineB should be depth 2.");
		Assert.AreEqual(3, spine.Bones["SpineC"].TreeDepth, "SpineC should be depth 3.");

		// Branching skeleton
		Assert.AreEqual(0, chara.RootNode.TreeDepth, "RootNode should be depth 0.");
		Assert.AreEqual(1, chara.Bones["SpineA"].TreeDepth, "SpineA should be depth 1.");
		Assert.AreEqual(2, chara.Bones["SpineB"].TreeDepth, "SpineB should be depth 2.");
		Assert.AreEqual(3, chara.Bones["SpineC"].TreeDepth, "SpineC should be depth 3.");

		// Branches into L_Shoulder, R_Shoulder, Neck
		Assert.AreEqual(4, chara.Bones["L_Shoulder"].TreeDepth, "L_Shoulder should be depth 4.");
		Assert.AreEqual(4, chara.Bones["R_Shoulder"].TreeDepth, "R_Shoulder should be depth 4.");
		Assert.AreEqual(4, chara.Bones["Neck"].TreeDepth, "Neck should be depth 4.");

		Assert.AreEqual(5, chara.Bones["Head"].TreeDepth, "Head should be depth 5.");
	}
	[TestMethod]
	public void SkeletonEntityOps_Mutator_CalculateConstraints() {
		SkeletonEntityOps ops = new();
		SkeletonEntity spine = Mock_Spine();

		ops.PreProcessSkeleton(ref spine, [
			ops.ValidateBoneNodeTree, 
			ops.BoneNodeTreeCalculateConstraints
			]);

		float expect_constraint = 1.0f;
		Vector3 expect_position = new(0,1,0);

		// Null check
		Assert.IsNotNull(spine.Bones["SpineA"].ParentRelativePosition, "Should not be null.");
		Assert.IsNotNull(spine.Bones["SpineB"].ParentRelativePosition, "Should not be null.");
		Assert.IsNotNull(spine.Bones["SpineC"].ParentRelativePosition, "Should not be null.");

		// Check vector length calculation
		Assert.AreEqual(expect_constraint, spine.Bones["SpineA"].ParentRelativePosition!.Value.Distance, "Distance should be 1.0f");
		Assert.AreEqual(expect_constraint, spine.Bones["SpineB"].ParentRelativePosition!.Value.Distance, "Distance should be 1.0f");
		Assert.AreEqual(expect_constraint, spine.Bones["SpineC"].ParentRelativePosition!.Value.Distance, "Distance should be 1.0f");

		// Check relative position from parent node
		Assert.AreEqual(expect_position, spine.Bones["SpineA"].ParentRelativePosition!.Value.NodePosition, "Parent relative position should be (0,1,0)");
		Assert.AreEqual(expect_position, spine.Bones["SpineB"].ParentRelativePosition!.Value.NodePosition, "Parent relative position should be (0,1,0)");
		Assert.AreEqual(expect_position, spine.Bones["SpineC"].ParentRelativePosition!.Value.NodePosition, "Parent relative position should be (0,1,0)");

		// Check orientation copy
		Assert.AreEqual(
			spine.Bones["Root"].Transform.Rotation, 
			spine.Bones["SpineA"].ParentRelativePosition!.Value.ParentOrientation, 
			"Should be equal to Root.Transform.Orientation"
			);
		Assert.AreEqual(spine.Bones["SpineA"].Transform.Rotation, 
			spine.Bones["SpineB"].ParentRelativePosition!.Value.ParentOrientation, 
			"Should be equal to SpineA.Transform.Orientation"
			);
		Assert.AreEqual(spine.Bones["SpineB"].Transform.Rotation, 
			spine.Bones["SpineC"].ParentRelativePosition!.Value.ParentOrientation, 
			"Should be equal to SpineB.Transform.Orientation"
			);
	}
	[TestMethod]
	public void SkeletonEntityOps_Mutator_SetParent() {
		SkeletonEntityOps ops = new();
		SkeletonEntity Spine = Mock_Spine();

		ops.PreProcessSkeleton(ref Spine, [ops.BoneNodeTreeSetParentEntity]);

		var expect_spinea_parent = Spine.RootNode;
		var expect_spineb_parent = Spine.Bones["SpineA"];
		var expect_spinec_parent = Spine.Bones["SpineB"];

		Assert.AreEqual(expect_spinea_parent, Spine.Bones["SpineA"].ParentBone, "SpineA.ParentBone should be Root");
		Assert.AreEqual(expect_spineb_parent, Spine.Bones["SpineB"].ParentBone, "SpineB.ParentBone should be SpineA");
		Assert.AreEqual(expect_spinec_parent, Spine.Bones["SpineC"].ParentBone, "SpineC.ParentBone should be SpineB");
	}
}
