using BoneUtils.Entity.Skeleton;
using BoneUtils.Entity.Skeleton.Animation;
using BoneUtils.Math;
using BoneUtils.Mockups;
using Microsoft.Testing.Platform.Extensions.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BoneUtilsUnitTests.AnimationTests;
[TestClass]
[TestCategory("Animation builder (AB) operations")]
public class AnimationBuilder_Tests :MockAnimationBuilder{
	[TestMethod]
	public void AB_CreateBasicAnimation() {
		var (sken, ops) = SetupSkeletonWithAnimator(Mock_Spine);
		AnimationBuilder builder = new();

		// Describe a translation animation
		Vector3 translation = new(2, 0, 0);
		var (xfm0, xfm1) = CreateKeyframePair_BoneNode_Translation(sken.RootNode, translation);
		builder.XfmType = AnimationXfmType.Static;

		// Create keyframes
		var frame0 = builder.CreateKeyframe(sken.RootNode, xfm0, 0.0f);
		var frame1 = builder.CreateKeyframe(sken.RootNode, xfm1, 3.0f);
		var blend0 = builder.AddSequence(frame0, frame1, AnimationBlendType.Linear);

		// Sequence composition check 
		Assert.IsTrue(blend0, "Keyframes should be able to form a sequence.");

		// Export animation
		AnimationContainer animation = builder.Export();

		// Deep copy check
		Assert.AreNotSame(sken.RootNode.Transform, frame0.TransformState, "Transform should not be passed by reference");
		Assert.AreNotSame(frame0, animation.Keyframes[0], "Keyframe 0 should not be passed by reference.");
		Assert.AreNotSame(frame1, animation.Keyframes[1], "Keyframe 1 should not be passed by reference.");
		Assert.AreNotSame(sken.RootNode.Transform.Rotation, frame0.TransformState.Rotation, "Cloned Quat for frame0 rotation should not be passed by reference.");
		Assert.AreNotSame(sken.RootNode.Transform.Rotation, frame1.TransformState.Rotation, "Cloned Quat for frame1 rotation should not be passed by reference.");
		// Omitting native value type properties (Vector3)

		// Transform state check
		Assert.AreEqual(sken.RootNode.Transform.Position, frame0.TransformState.Position, "Cloned transform should not change position.");
		Assert.AreEqual(sken.RootNode.Transform.Rotation, frame0.TransformState.Rotation, "Cloned transform should not change rotation.");
		Assert.AreEqual(sken.RootNode.Transform.Scale, frame0.TransformState.Scale, "Cloned transform should not change scale.");

		Assert.AreEqual(sken.RootNode.Transform.Position+translation, frame1.TransformState.Position, "Cloned transform should not change position.");
		Assert.AreEqual(sken.RootNode.Transform.Rotation, frame1.TransformState.Rotation, "Cloned transform should not change rotation.");
		Assert.AreEqual(sken.RootNode.Transform.Scale, frame1.TransformState.Scale, "Cloned transform should not change scale.");

		// Container check
		Assert.AreEqual(3.0f, animation.TotalDuration, "Animation duration should be set.");
		Assert.AreEqual(AnimationXfmType.Static, animation.Type, "Animation type should be Relative.");

		// TODO check amount of blendframes
	}
	/// <summary>
	/// Creates an AnimationContainer with 1000 keyframes
	/// </summary>
	[TestMethod]
	public void AB_CreateLongAnimation() {
		var (sken, ops) = SetupSkeletonWithAnimator(Mock_Spine);
		AnimationBuilder ab = new();
		ab.XfmType = AnimationXfmType.Static;

		// This sequence forms a rotation around origo on the XZ-plane
		Vector3[] translation = [
			new(2,0,0),
			new(0,0,2),
			new(-2,0,0),
			new(0,0,-2)
			];

		// Set up composition
		int targetKeyframePairs = 100000;
		float time = 0f;
		float timeInc = 1.0f;

		// Build initial frames
		var (xfm0, xfm1) = CreateKeyframePair_BoneNode_Translation(sken.RootNode, translation[0]);
		var frame0 = ab.CreateKeyframe(sken.RootNode, xfm0, time);
		time += timeInc;
		var frame1 = ab.CreateKeyframe(sken.RootNode, xfm1, time);
		time += timeInc;

		ab.StartSequence(frame0, frame1, AnimationBlendType.Linear);
		// Create and add to builder
		for(int i = 0; i < targetKeyframePairs; i+=2) {
			(xfm0, xfm1) = CreateKeyframePair_BoneNode_Translation(sken.RootNode, translation[i % 4]);
			frame0 = ab.CreateKeyframe(sken.RootNode, xfm0, time);
			time += timeInc;
			frame1 = ab.CreateKeyframe(sken.RootNode, xfm1, time);
			time += timeInc;
			ab.BuildSequence(frame0, AnimationBlendType.Linear);
			ab.BuildSequence(frame1, AnimationBlendType.Linear);
		}
		ab.EndSequence();

		AnimationContainer ac = ab.Export();
		
	}
}
