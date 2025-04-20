using BoneUtils.Entity.Skeleton;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace BoneUtils.Tests;

[TestClass]
public class SkeletonEntityConstructionTests :MockDataBuilder {
	[TestMethod]
	public void BoneNode_CanConstruct() {
		try {
			BoneNode node = Mock_BoneNode();
			Assert.IsInstanceOfType<BoneNode>(node);
			Assert.IsInstanceOfType<Transform>(node.Transform);
			Assert.IsTrue(node.Transform.Position == Vector3.Zero);
			Assert.IsTrue(node.Transform.Rotation == Quaternion.Identity);
			Assert.IsTrue(node.Transform.Scale == Vector3.One);

			DbgOutOk("BoneNode_CanConstruct");
		}
		catch (Exception ex) {
			DbgOutEx(ex);
			throw;
		}
	}
	[TestMethod]
	public void Entity_CanConstruct() {
		try {
			BoneNode node = Mock_BoneNode();
			SkeletonEntity en = new("Test Entity", node);

			Assert.IsInstanceOfType<SkeletonEntity>(en);
			Assert.IsInstanceOfType<BoneNode>(en.RootNode);

			DbgOutOk("Entity_CanConstruct");
		}
		catch (Exception ex) {
			DbgOutEx(ex);
			throw;
		}
	}
	[TestMethod]
	public void Entity_CanConstructComplex() {
		try {
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

			DbgOutOk("Entity_CanConstructComplex");
		}
		catch (Exception ex) {
			DbgOutEx(ex);
			throw;
		}
	}
	[TestMethod]
	public void Entity_FailCircularTreeTest() {
		try {
			var sken = Mock_FailCircularTree();
			var skeops = new SkeletonEntityOps();
			sken.Bones["SpineB"].Children.Add("SpineA", sken.Bones["SpineA"]);

			Assert.IsFalse(skeops.ValidateBoneNodeTree(sken), "BoneNode tree is circular and should not be valid.");

			DbgOutOk("Entity_FailCircularTreeTest");
		}
		catch(Exception ex) {
			DbgOutEx(ex);
			//throw;
		}
	}
	[TestMethod]
	public void Entity_ConstructMockSpine() {
		try {
			var sken = Mock_Spine();
			var skeops = new SkeletonEntityOps();

			Assert.IsTrue(skeops.ValidateBoneNodeTree(sken), "BoneNode tree is not circular and should be valid.");

			DbgOutOk("Entity_ConstructMockSpine");
		}
		catch (Exception ex) {
			DbgOutEx(ex);
		}
	}
	[TestMethod]
	public void Entity_ConstructEntity01() {
		try {
			var sken = Mock_TestEntity01();
			var skeops = new SkeletonEntityOps();

			Assert.IsTrue(skeops.ValidateBoneNodeTree(sken), "BoneNode tree is not circular and should be valid.");

			DbgOutOk("Entity_ConstructEntity01");
		}
		catch (Exception ex) {
			DbgOutEx(ex);
		}
	}
}
