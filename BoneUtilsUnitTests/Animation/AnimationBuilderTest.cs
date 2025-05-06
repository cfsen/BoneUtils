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
[TestCategory("Animation builder operations")]
public class AnimationBuilderTests :MockAnimationBuilder{
	[TestMethod]
	public void CreateBasicAnimation() {
		var (sken, ops) = SetupSkeletonWithAnimator(Mock_Spine);
		AnimationBuilder builder = new();

		Vector3 translation = new(2, 0, 0);
		TransformSnapshot xfm0 = sken.RootNode.Transform;
		TransformSnapshot xfm1 = new Transform { 
			Position = sken.RootNode.Transform.Position+translation, 
			Rotation = sken.RootNode.Transform.Rotation,
			Scale = sken.RootNode.Transform.Scale
			};

		var frame0 = builder.CreateKeyframe(sken.RootNode, xfm0, 0.0f);
		var frame1 = builder.CreateKeyframe(sken.RootNode, xfm1, 3.0f);
		var blend0 = builder.AddSequence(frame0, frame1, AnimationBlendType.Linear);
		var animation = builder.Export();

		// Deep copy check
		Assert.AreNotEqual(sken.RootNode.Transform, frame0.TransformState, "Transform should not be passed by reference");

		// Transform state check
		Assert.AreEqual(sken.RootNode.Transform.Position, frame0.TransformState.Position, "Cloned transform should not change position.");
		Assert.AreEqual(sken.RootNode.Transform.Rotation, frame0.TransformState.Rotation, "Cloned transform should not change rotation.");
		Assert.AreEqual(sken.RootNode.Transform.Scale, frame0.TransformState.Scale, "Cloned transform should not change scale.");

		Assert.AreEqual(sken.RootNode.Transform.Position+translation, frame1.TransformState.Position, "Cloned transform should not change position.");
		Assert.AreEqual(sken.RootNode.Transform.Rotation, frame1.TransformState.Rotation, "Cloned transform should not change rotation.");
		Assert.AreEqual(sken.RootNode.Transform.Scale, frame1.TransformState.Scale, "Cloned transform should not change scale.");

		// Sequence validation
		Assert.IsTrue(blend0, "Keyframes should be able to form a sequence.");
		Assert.AreEqual(frame0, animation.Keyframes[0]);
		Assert.AreEqual(frame1, animation.Keyframes[1]);


	}
}
