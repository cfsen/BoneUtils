using BoneUtils.Entity.Skeleton.Animation;
using BoneUtils.Entity.Skeleton;
using BoneUtils.Math;
using System.Numerics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoneUtils.Mockups; 
public abstract class MockAnimationBuilder :MockDataBuilder {

	// Composition helpers

	public (SkeletonEntity sken, SkeletonEntityOps ops) SetupSkeletonWithAnimator(Func<SkeletonEntity> mockupGenerator) {
		SkeletonEntity mockup = mockupGenerator();
		SkeletonEntityOps ops = new();
		SetupSkeletonAnimator(ref mockup, ref ops);
		return (mockup, ops);
	}

	public void SetupSkeletonAnimator(ref SkeletonEntity sken, ref SkeletonEntityOps ops) {
		// Intended intialization
		ops.PreProcessSkeleton(ref sken, [ 
			ops.ValidateBoneNodeTree, 
			ops.BoneNodeTreeCalculateConstraints,
			ops.AddSkeletonAnimator 
			]);
	}

	/// <summary>
	/// Manually constructed animation for testing the structure. 
	/// This is not the intended way to set up animations. Use AnimationBuilder.
	/// </summary>
	/// <param name="sken"></param>
	/// <param name="expect_position"></param>
	/// <returns></returns>
	public AnimationContainer SetupBasicTranslationAnimation(SkeletonEntity sken, Vector3 expect_position) {
		// frame 0
		AnimationKeyframe f0 = new AnimationKeyframe {
			Bone = sken.RootNode,
			TransformState = sken.RootNode.Transform,
			TimelinePosition = 0.0f
		};

		TransformSnapshot xfm = new() {
			Rotation = new Quat( 
				sken.RootNode.Transform.Rotation.W,
				sken.RootNode.Transform.Rotation.X,
				sken.RootNode.Transform.Rotation.Y,
				sken.RootNode.Transform.Rotation.Z
				),
			Position = expect_position,
			Scale = new(1,1,1)
		};

		// frame 1
		AnimationKeyframe f1 = new() {
			Bone = sken.RootNode,
			TransformState = xfm,
			TimelinePosition = 3.0f
		};

		// blend setup
		AnimationBlend blend_f0f1 = new() {
			BlendType = AnimationBlendType.None,
			OriginIndex = 0,
			TargetIndex = 1,
			BlendFactor = 0.5f,
			TimeFactor = 1.0f,
			Time = 3.0f // f0.TimelinePosition + f1.TimelinePosition
		};

		// group frames
		AnimationContainer animationGroup = new AnimationContainer {
			Keyframes = [f0, f1],
			FrameBlends = [blend_f0f1],
			TotalDuration = 3.0f,
			Type = AnimationType.Relative,
		};

		return animationGroup;
	}


}
