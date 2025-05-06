using BoneUtils.Entity.Skeleton;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BoneUtils.Mockups;
using System.Numerics;
using BoneUtils.Math;

namespace Tests.Skeleton_Tests;

[TestClass]
[TestCategory("SkeletonEntity construction")]
public class SkeletonEntity_ConstructionTests :MockDataBuilder {
	[TestMethod]
	public void Entity_CanConstruct() {
		BoneNode node = Mock_BoneNode();

		SkeletonEntity en = new("Test Entity", node);

		Assert.IsInstanceOfType<SkeletonEntity>(en);
		Assert.IsInstanceOfType<BoneNode>(en.RootNode);

		// Check if node attached correctly
		Assert.AreEqual(node, en.RootNode, "Created node should be set as RootNode.");
	}
	[TestMethod]
	public void Entity_CanConstructComplex() {
		var nodes = Mock_BoneNodeTree();
		SkeletonEntity en = new("Actor", nodes["Root"]) {
			Bones = nodes
		};

		// Check if bones were added
		Assert.AreEqual(7, en.Bones.Count, "Mockup should have 7 bones.");
		Assert.IsTrue(en.Bones.ContainsKey("Root"), "Root missing.");
		Assert.IsTrue(en.Bones.ContainsKey("SpineA"), "SpineA missing.");
		Assert.IsTrue(en.Bones.ContainsKey("SpineB"), "SpineB missing.");
		Assert.IsTrue(en.Bones.ContainsKey("SpineC"), "SpineC missing.");
		Assert.IsTrue(en.Bones.ContainsKey("L_Shoulder"), "L_Shoulder missing.");
		Assert.IsTrue(en.Bones.ContainsKey("R_Shoulder"), "R_Shoulder missing.");
		Assert.IsTrue(en.Bones.ContainsKey("Neck"), "Neck missing.");

		// Check if child bones were attached
		Assert.AreEqual(3, en.Bones["SpineC"].Children.Count, "SpineC should have 3 children.");
		Assert.IsTrue(en.Bones["SpineC"].Children.ContainsKey("L_Shoulder"), "Missing child: L_Shoulder");
		Assert.IsTrue(en.Bones["SpineC"].Children.ContainsKey("R_Shoulder"), "Missing child: R_Shoulder");
		Assert.IsTrue(en.Bones["SpineC"].Children.ContainsKey("Neck"), "Missing child: Neck.");

		// Check class properties behavior
		Assert.IsTrue(en.Bones["SpineC"].Branching, "SpineC.Branching returned false, expected true.");
		Assert.IsFalse(en.Bones["Root"].Branching, "Root.Branching returned true, expected false.");
	}
	[TestMethod]
	public void Entity_FailCircularTreeTest() {
		var sken = Mock_FailCircularTree();
		var skeops = new SkeletonEntityOps();

		// Perform illegal operation, link child back to parent
		sken.Bones["SpineB"].Children.Add("SpineA", sken.Bones["SpineA"]);

		Assert.IsFalse(skeops.ValidateBoneNodeTree(ref sken), "BoneNode tree is circular and should not be valid.");
	}
	[TestMethod]
	public void Entity_ValidateMockupTrees() {
		var spine = Mock_Spine();
		var testEntity01 = Mock_TestEntity01();
		var wave = Mock_Wave();
		var helix = Mock_Helix();
		var skeops = new SkeletonEntityOps();

		// Check for tree circularity
		Assert.IsTrue(skeops.ValidateBoneNodeTree(ref testEntity01), "TestEntity01 tree is not circular and should be valid.");
		Assert.IsTrue(skeops.ValidateBoneNodeTree(ref spine), "Spine tree is not circular and should be valid.");
		Assert.IsTrue(skeops.ValidateBoneNodeTree(ref wave), "Wave tree is not circular and should be valid.");
		Assert.IsTrue(skeops.ValidateBoneNodeTree(ref helix), "Helix tree is not circular and should be valid.");
	}
}
