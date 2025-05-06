using BoneUtils.Entity.Skeleton;
using BoneUtils.Entity.Skeleton.Animation;
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
[TestCategory("Animator operations")]
public class Animator_Tests :MockAnimationBuilder{
	/// <summary>
	/// Checks if Animator can attach to SkeletonEntity
	/// </summary>
	[TestMethod]
	public void Animator_AttachTo_Skeleton() {
		SkeletonEntity sken = Mock_Spine();
		SkeletonEntityOps ops = new();

		// Premature animator attachment
		Assert.ThrowsException<Exception>(
			() => ops.PreProcessSkeleton(ref sken, [ ops.AddSkeletonAnimator ]), 
			"Animator should not attach to skeleton pre-validation");
		
		SetupSkeletonAnimator(ref sken, ref ops);
		
		// Composition tests
		Assert.IsNotNull(sken.Animator);
		Assert.IsFalse(sken.Animator.Running, "Skeleton should be inactive.");
		Assert.AreEqual(0, sken.Animator.LoadedAnimations, "Skeleton should have no loaded animations.");
	}

	/// <summary>
	/// Checks AnimationContainer compositon without AnimationBuilder
	/// </summary>
	[TestMethod]
	public void Animator_LoadAnimation_NoBuilder() {
		var (sken, ops) = SetupSkeletonWithAnimator(Mock_Spine);

		// Check if animator is attached
		Assert.IsNotNull(sken.Animator);

		// Compose mock animation
		Vector3 expect_position = new(1,0,1);
		AnimationContainer animationGroup = SetupBasicTranslationAnimation(sken, expect_position);
		SkeletonAnimation animation = new(animationGroup);

		sken.Animator.Load(animation);

		// Validate animation load
		Assert.AreEqual(1, sken.Animator.LoadedAnimations, "The created animation should be loaded.");
	}

	/// <summary>
	/// Checks loading up an animation
	/// </summary>
	[TestMethod]
	public void Animator_LoadAnimation() {
		var (sken, animContainer) = Mock_Skeleton_With_AnimationContainer_RootNode_Translation();
		SkeletonAnimation animation = new(animContainer);
		sken.Animator!.Load(animation);
		
		Assert.AreEqual(1, sken.Animator.LoadedAnimations, "The created animation should be loaded.");
	}
}
