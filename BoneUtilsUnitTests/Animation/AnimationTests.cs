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
	[TestMethod]
	public void Animation_CanLoadAnimation() {
		SkeletonEntity sken = Mock_Spine();
		SkeletonEntityOps ops = new();
		SetupSkeletonAnimator(ref sken, ref ops);

		// Check if animator is attached
		Assert.IsNotNull(sken.Animator);

		// Compose mock animation
		Vector3 expect_position = new(1,0,1);
		BonesAnimation animationGroup = SetupBasicTranslationAnimation(sken, expect_position);
		SkeletonAnimation animation = new(animationGroup);

		sken.Animator.Load(animation);

		// Validate animation load
		Assert.AreEqual(1, sken.Animator.LoadedAnimations, "The created animation should be loaded.");
	}

	// Composition helpers

	private BonesAnimation SetupBasicTranslationAnimation(SkeletonEntity sken, Vector3 expect_position) {
		// frame 0
		BoneAnimationKeyframe f0 = new BoneAnimationKeyframe {
			Bone = sken.RootNode,
			TransformState = sken.RootNode.Transform,
			TimelinePosition = 0.0f
		};

		Transform xfm = new() {
			Rotation = new Quat( // quat is a struct, a reference type, so manual copy
				sken.RootNode.Transform.Rotation.W,
				sken.RootNode.Transform.Rotation.X,
				sken.RootNode.Transform.Rotation.Y,
				sken.RootNode.Transform.Rotation.Z
				),
			Position = expect_position
		};

		// frame 1
		BoneAnimationKeyframe f1 = new BoneAnimationKeyframe {
			Bone = sken.RootNode,
			TransformState = xfm,
			TimelinePosition = 3.0f
		};

		// blend setup
		AnimationBlend blend_f0f1 = new AnimationBlend {
			BlendType = AnimationBlendType.None,
			OriginIndex = 0,
			TargetIndex = 1,
			BlendFactor = 0.5f,
			TimeFactor = 1.0f,
			Time = 3.0f // f0.TimelinePosition + f1.TimelinePosition
		};

		// group frames
		BonesAnimation animationGroup = new BonesAnimation {
			Keyframes = [f0, f1],
			FrameBlends = [blend_f0f1],
			TotalDuration = 3.0f,
			Type = BonesAnimationType.Relative,
		};

		return animationGroup;
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
