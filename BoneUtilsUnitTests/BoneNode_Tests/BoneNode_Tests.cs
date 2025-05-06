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
}
