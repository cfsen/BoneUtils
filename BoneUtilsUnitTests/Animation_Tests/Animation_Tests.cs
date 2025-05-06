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
[TestCategory("Animation tests")]
public class Animation_Tests :MockAnimationBuilder{
	/// <summary>
	/// Checks if a static 2 frame translation animation can be played and looped.
	/// </summary>
	[TestMethod]
	public void Animator_Play_RootNode_Translation() {
		var (sken, anim) = Mock_Skeleton_With_AnimationContainer_RootNode_Translation();
		SkeletonAnimation animation = new(anim);

		sken.Animator!.Load(animation);

		Vector3 origin = new(0,0,0);
		Vector3 target = new(2,0,0);

		// Temporary testing of basic no-blends keyframe delivery

		// Start the timeline
		sken.Animator.Play(0.0f);
		Assert.IsTrue(sken.Animator.Running, "Animator should be running after Play is called.");
		Assert.AreEqual(origin, sken.RootNode.Transform.Position, "At t=0, skeleton should be at its initial position");

		// Advance timeline by 1s, t=1.0
		sken.Animator.Play(1.0f);
		Assert.AreEqual(origin, sken.RootNode.Transform.Position, "At t=1.0, skeleton should be at its initial position");

		// Advance timeline by 1s, t=2.0
		sken.Animator.Play(1.0f);
		Assert.AreEqual(target, sken.RootNode.Transform.Position, "At t=2.0, skeleton should be at its target position");

		// Advance timeline by 1s, t=3.0
		sken.Animator.Play(1.0f);
		Assert.AreEqual(target, sken.RootNode.Transform.Position, "At t=3.0, skeleton should be at its target position");

		// Advance timeline by 1s, t=1.0
		sken.Animator.Play(0.00001f);
		Assert.AreEqual(origin, sken.RootNode.Transform.Position, "At t=3.0+0.00001s, skeleton should be back at its initial position");
	}
}
