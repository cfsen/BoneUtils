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

	// SkeletonEntity helpers

	public (SkeletonEntity sken, SkeletonEntityOps ops) SetupSkeletonWithAnimator(Func<SkeletonEntity> mockupGenerator) {
		SkeletonEntity mockup = mockupGenerator();
		SkeletonEntityOps ops = new();
		SetupSkeletonAnimator(ref mockup, ref ops);
		return (mockup, ops);
	}

	/// <summary>
	/// Sets up a skeleton ready for animations
	/// </summary>
	/// <param name="sken">SkeletonEntity to add animator to</param>
	/// <param name="ops">SkeletonEntityOps for mutating skeleton</param>
	public void SetupSkeletonAnimator(ref SkeletonEntity sken, ref SkeletonEntityOps ops) {
		// Intended intialization for animated skeletons
		ops.PreProcessSkeleton(ref sken, [ 
			ops.ValidateBoneNodeTree, 
			ops.BoneNodeTreeCalculateConstraints,
			ops.AddSkeletonAnimator 
			]);
	}

	// Helpers: AnimationBuilder

	/// <summary>
	/// Performs the same setup as AB_CreateBasicAnimation
	/// </summary>
	/// <returns>SkeletonEntity, AnimationContainer</returns>
	public (SkeletonEntity, AnimationContainer) Mock_Skeleton_With_AnimationContainer_RootNode_Translation() {
		var (sken, ops) = SetupSkeletonWithAnimator(Mock_Spine);
		AnimationBuilder builder = new();

		// Set transform type
		builder.XfmType = AnimationXfmType.Static;

		// Describe a translation animation
		Vector3 translation = new(2, 0, 0);
		var (xfm0, xfm1) = CreateKeyframePair_BoneNode_Translation(sken.RootNode, translation);

		// Create keyframes
		var frame0 = builder.CreateKeyframe(sken.RootNode, xfm0, 0.0f);
		var frame1 = builder.CreateKeyframe(sken.RootNode, xfm1, 3.0f);
		var blend0 = builder.StartSequence(frame0, frame1, AnimationBlendType.Linear);

		// Export animation
		builder.EndSequence();
		AnimationContainer animation = builder.Export();

		return (sken, animation);
	}

	// Helpers: Keyframe

	/// <summary>
	/// Creates a pair of keyframe for a basic translation animation 
	/// </summary>
	/// <param name="node">Node to animate</param>
	/// <param name="translation">Translation target</param>
	/// <returns>Keyframes for origin and target transforms</returns>
	public (TransformSnapshot, TransformSnapshot) CreateKeyframePair_BoneNode_Translation(BoneNode node, Vector3 translation) {
		// Describe a translation animation
		TransformSnapshot xfm0 = node.Transform;
		TransformSnapshot xfm1 = new Transform { 
			Position = node.Transform.Position+translation, 
			Rotation = node.Transform.Rotation,
			Scale = node.Transform.Scale
			};
		return (xfm0, xfm1);
	}

	// Data structure hardcoded mockups

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
			Type = AnimationXfmType.Relative,
		};

		return animationGroup;
	}


}
