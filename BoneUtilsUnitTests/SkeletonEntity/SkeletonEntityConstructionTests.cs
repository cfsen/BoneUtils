using BoneUtils.Entity.Skeleton;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BoneUtils.Mockups;
using System.Numerics;
using BoneUtils.Math;

namespace BoneUtilsUnitTests.SkeletonEntityTests;

[TestClass]
public class SkeletonEntityConstructionTests :MockDataBuilder {
	[TestMethod]
	public void BoneNode_CanConstruct() {
		BoneNode node = Mock_BoneNode();
		Assert.IsInstanceOfType<BoneNode>(node);
		Assert.IsInstanceOfType<Transform>(node.Transform);
		Assert.IsTrue(node.Transform.Position == Vector3.Zero);
		Assert.IsTrue(node.Transform.Rotation == Quat.Identity());
		Assert.IsTrue(node.Transform.Scale == Vector3.One);
	}
	[TestMethod]
	public void Entity_CanConstruct() {
		BoneNode node = Mock_BoneNode();
		SkeletonEntity en = new("Test Entity", node);

		Assert.IsInstanceOfType<SkeletonEntity>(en);
		Assert.IsInstanceOfType<BoneNode>(en.RootNode);
	}
	[TestMethod]
	public void Entity_CanConstructComplex() {
		var nodes = Mock_BoneNodeTree();
		SkeletonEntity en = new("Actor", nodes["Root"]) {
			Bones = nodes
		};

		Assert.AreEqual(7, en.Bones.Count,
			$"Missing bones, expected 7, found {en.Bones.Count}");
		Assert.IsTrue(en.Bones.ContainsKey("Root"),
			"Root missing.");
		Assert.IsTrue(en.Bones.ContainsKey("SpineA"),
			"SpineA missing.");
		Assert.IsTrue(en.Bones.ContainsKey("SpineB"),
			"SpineB missing.");
		Assert.IsTrue(en.Bones.ContainsKey("SpineC"),
			"SpineC missing.");
		Assert.IsTrue(en.Bones.ContainsKey("L_Shoulder"),
			"L_Shoulder missing.");
		Assert.IsTrue(en.Bones.ContainsKey("R_Shoulder"),
			"R_Shoulder missing.");
		Assert.IsTrue(en.Bones.ContainsKey("Neck"),
			"Neck missing.");

		Assert.AreEqual(3, en.Bones["SpineC"].Children.Count,
			$"SpineC should have 3 children, has {en.Bones["SpineC"].Children.Count}");
		Assert.IsTrue(en.Bones["SpineC"].Children.ContainsKey("L_Shoulder"),
			"Missing child: L_Shoulder");
		Assert.IsTrue(en.Bones["SpineC"].Children.ContainsKey("R_Shoulder"),
			"Missing child: R_Shoulder");
		Assert.IsTrue(en.Bones["SpineC"].Children.ContainsKey("Neck"),
			"Missing child: Neck.");

		Assert.IsTrue(en.Bones["SpineC"].Branching,
			"SpineC.Branching returned false, expected true.");
		Assert.IsFalse(en.Bones["Root"].Branching,
			"Root.Branching returned true, expected false.");
	}
	[TestMethod]
	public void Entity_FailCircularTreeTest() {
		var sken = Mock_FailCircularTree();
		var skeops = new SkeletonEntityOps();
		sken.Bones["SpineB"].Children.Add("SpineA", sken.Bones["SpineA"]);

		Assert.IsFalse(skeops.ValidateBoneNodeTree(ref sken), "BoneNode tree is circular and should not be valid.");
	}
	[TestMethod]
	public void Entity_ConstructMockSpine() {
		var sken = Mock_Spine();
		var skeops = new SkeletonEntityOps();

		Assert.IsTrue(skeops.ValidateBoneNodeTree(ref sken), "BoneNode tree is not circular and should be valid.");
	}
	[TestMethod]
	public void Entity_ConstructEntity01() {
		var sken = Mock_TestEntity01();
		var skeops = new SkeletonEntityOps();

		Assert.IsTrue(skeops.ValidateBoneNodeTree(ref sken), "BoneNode tree is not circular and should be valid.");
	}
}
