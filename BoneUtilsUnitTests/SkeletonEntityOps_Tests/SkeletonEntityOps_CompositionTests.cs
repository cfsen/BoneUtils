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
}
