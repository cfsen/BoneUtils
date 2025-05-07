using BoneUtils.Entity.Skeleton;
using BoneUtils.Math;
using BoneUtils.Mockups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Tests.BoneNode_Tests;
[TestClass]
[TestCategory("BoneNode construction")]
public class BoneNode_Tests :MockDataBuilder{
	[TestMethod]
	public void BoneNode_CanConstruct() {
		BoneNode node = Mock_BoneNode();

		Assert.IsInstanceOfType<BoneNode>(node);
		Assert.IsInstanceOfType<Transform>(node.Transform);

		// Check for default values 
		Assert.IsTrue(node.Transform.Position == Vector3.Zero);
		Assert.IsTrue(node.Transform.Rotation == Quat.Identity);
		Assert.IsTrue(node.Transform.Scale == Vector3.One);
	}
	[TestMethod]
	public void BoneNode_CanReset() {
		BoneNode node = Mock_BoneNode();
		BoneNode identical_node = Mock_BoneNode();

		Vector3 newPosition = new(1,2,3), newScale = new(2,2,2);
		Quat newOrientation = Quat.Create(MathF.PI/2, Vector3.UnitY);

		node.Rotate(Quat.Create(MathF.PI/2, Vector3.UnitY));
		node.Translate(newPosition);
		node.Transform.Scale = newScale;
		
		// TODO write a checker that accounts for rounding in Quat multiplication
		Quat expected_orientation = new (0.7071068f, 0f, 0.7071068f, 0f);

		// Check that node moved
		Assert.AreEqual(newPosition, node.Transform.Position, "Node should move");
		Assert.AreEqual(expected_orientation, node.Transform.Rotation, "Node should rotate");
		Assert.AreEqual(newScale, node.Transform.Scale, "Node should scale");

		node.Reset();

		// Check for reset
		Assert.AreEqual(identical_node.Transform.Position, node.Transform.Position, "Transform position should reset");
		Assert.AreEqual(identical_node.Transform.Rotation, node.Transform.Rotation, "Transform rotatino should reset");
		Assert.AreEqual(identical_node.Transform.Scale, node.Transform.Scale, "Transform scale should reset");

	}
}
