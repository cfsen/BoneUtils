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
[TestCategory("BoneNode transform buffering")]
public class BoneNode_BufferTests :MockDataBuilder{
	[TestMethod]
	public void BoneNode_TransformBuffer() {
		SkeletonEntity sk = Mock_Spine();

		Vector3 translate = new(1,2,3);
		Vector3 scale = new(2,2,2);
		Quat q0 = Quat.Create(MathF.PI/2, Vector3.UnitY);
		Quat q1 = Quat.Create(MathF.PI/2, Vector3.UnitZ);

		Quat expect_rotation = sk.RootNode.Transform.Rotation*q0*q1;
		Vector3 expect_scale = sk.RootNode.Transform.Scale+scale+scale;
		Vector3 expect_position = sk.RootNode.Transform.Position+translate+translate;
		
		// Check invalid order of operation
		Assert.IsFalse(sk.RootNode.ApplyTransformBuffer(), "Buffer should not apply when empty.");
		Assert.IsFalse(sk.RootNode.TransformBuffer.Accumulate(translate), "Buffer should not allow accumulation before Begin()");
		Assert.IsFalse(sk.RootNode.TransformBuffer.End(), "Buffer should not allow End() before Begin()");

		// Prepare buffer
		Assert.IsTrue(sk.RootNode.PrepareTransformBuffer(), "Buffer should prepare correctly.");

		// Verify buffer state is set correctly
		Assert.AreEqual(sk.RootNode.Transform.Position, sk.RootNode.TransformBuffer.Translation);
		Assert.AreEqual(sk.RootNode.Transform.Rotation, sk.RootNode.TransformBuffer.Rotation);
		Assert.AreEqual(sk.RootNode.Transform.Scale, sk.RootNode.TransformBuffer.Scale);

		// Begin buffering
		Assert.IsTrue(sk.RootNode.TransformBuffer.Begin(), "Buffer should allow accumulation to begin");

		// Invalid order of operations after Begin()
		Assert.IsFalse(sk.RootNode.PrepareTransformBuffer(), "Should not allow prepare again after Begin() is called."); 
		Assert.IsFalse(sk.RootNode.TransformBuffer.Begin(), "Buffer should not allow Begin() after Begin(), unless End() or Reset() is called.");

		// Accumulate rotation and validate
		Assert.IsTrue(sk.RootNode.TransformBuffer.Accumulate(q0), "Quats should accumulate.");
		Assert.AreEqual(sk.RootNode.Transform.Rotation*q0, sk.RootNode.TransformBuffer.Rotation, "First buffered rotation should accumulate.");
		Assert.IsTrue(sk.RootNode.TransformBuffer.Accumulate(q1), "Quats should be accumulated multiple times");
		Assert.AreEqual(expect_rotation, sk.RootNode.TransformBuffer.Rotation, "First buffered rotation should accumulate.");

		// Accumulate translations and validate
		Assert.IsTrue(sk.RootNode.TransformBuffer.Accumulate(translate), "Translation should accumulate.");
		Assert.AreEqual(sk.RootNode.Transform.Position+translate, sk.RootNode.TransformBuffer.Translation, "First buffered translation should accumulate.");
		Assert.IsTrue(sk.RootNode.TransformBuffer.Accumulate(translate), "Translation should accumulate.");
		Assert.AreEqual(expect_position, sk.RootNode.TransformBuffer.Translation, "Second buffered translation should accumulate.");

		// Accumulate scale and validate
		Assert.IsTrue(sk.RootNode.TransformBuffer.AccumulateScale(scale), "Scale should accumulate.");
		Assert.AreEqual(sk.RootNode.Transform.Scale+scale, sk.RootNode.TransformBuffer.Scale, "First buffered scale should accumulate.");
		Assert.IsTrue(sk.RootNode.TransformBuffer.AccumulateScale(scale), "Scale should accumulate.");
		Assert.AreEqual(expect_scale, sk.RootNode.TransformBuffer.Scale, "Second buffered scale should accumulate.");

		// Validate the buffer
		Assert.AreEqual(expect_position, sk.RootNode.TransformBuffer.Translation, "Buffer should have expected translation before finalizing.");
		Assert.AreEqual(expect_rotation, sk.RootNode.TransformBuffer.Rotation, "Buffer should have expected rotation before finalizing.");
		Assert.AreEqual(expect_scale, sk.RootNode.TransformBuffer.Scale, "Buffer should have expected scale before finalizing.");

		// Close buffer and write to transform
		Assert.IsTrue(sk.RootNode.TransformBuffer.End(), "Buffer should close.");
		Assert.IsTrue(sk.RootNode.ApplyTransformBuffer(), "Buffer should apply.");

		// Validate new transform
		Assert.AreEqual(expect_position, sk.RootNode.Transform.Position, "Transform should have expected translation after setting buffer.");
		Assert.AreEqual(expect_rotation, sk.RootNode.Transform.Rotation, "Transform should have expected rotation after setting buffer.");
		Assert.AreEqual(expect_scale, sk.RootNode.Transform.Scale, "Transform should have expected scale after setting buffer.");
	}
}
