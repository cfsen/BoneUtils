using BoneUtils.Entity.Skeleton;
using BoneUtils.Math;
using BoneUtils.Mockups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BoneUtilsUnitTests.AnimationTests;
[TestClass]
public class AnimationTests :MockDataBuilder{
	[TestMethod]
	public void Animation_CanPrepareSkeleton() {
		SkeletonEntity sken = Mock_Spine();
		SkeletonEntityOps ops = new();

		// Premature animator attachment
		Assert.ThrowsException<Exception>(
			() => ops.PreProcessSkeleton(ref sken, [ ops.AddSkeletonAnimator ]), 
			"Animator should not attach to skeleton pre-validation");
		
		SetupSkeletonAnimator(ref sken, ref ops);
		
		// Composition tests
		Assert.IsNotNull(sken.Animator);
		Assert.IsFalse(sken.Animator.Running, "Skeleton should be inactive after composition.");
	}

	private void SetupSkeletonAnimator(ref SkeletonEntity sken, ref SkeletonEntityOps ops) {
		// Intended intialization
		ops.PreProcessSkeleton(ref sken, [ 
			ops.ValidateBoneNodeTree, 
			ops.BoneNodeTreeCalculateConstraints,
			ops.AddSkeletonAnimator 
			]);
	}
}
