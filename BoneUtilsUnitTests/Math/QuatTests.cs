using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using BoneUtils.Helpers;
using BoneUtils.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace BoneUtilsUnitTests.MathTests;
[TestClass]
public class QuatTests {

// Operators

	/// <summary>
	/// Test comparator operators
	/// - Is equal (==)
	/// - Is not equal (!=)
	/// </summary>
	[TestMethod]
	public void Quat_ComparatorsTest() {
		Quat q1 = new(){ W=1, X=0, Y=0, Z=0 };
		Quat q2 = new(){ W=1, X=0, Y=0, Z=0 };
		Quat q3 = new(){ W=0, X=1, Y=0, Z=0 };

		Assert.IsTrue(q1 == q2, "q1 == q2: should return true.");
		Assert.IsFalse(q1 != q2, "q1 != q2 should return false.");
		Assert.IsFalse(q1 == q3, "q1 == q3: should return false.");
		Assert.IsTrue(q1 != q3, "q1 != q3 should return true.");
	}
	/// <summary>
	/// Test addition and subtraction
	/// - Addition (+)
	/// - Subtraction (-)
	/// </summary>
	[TestMethod]
	public void Quat_AdditionSubtractionTest() {
		Quat q1 = new(){ W=1, X=0, Y=1, Z=0 };
		Quat q2 = new(){ W=0, X=1, Y=0, Z=1 };
		Quat q3 = new(){ W=1, X=1, Y=1, Z=1 };
		Quat q4 = new(){ W=-1, X=-1, Y=-1, Z=-1 };

		Quat expect_sum_q1q2	= new(){ W=1, X=1, Y=1, Z=1 };
		Quat expect_diff_q1q2	= new(){ W=1, X=-1, Y=1, Z=-1 };
		Quat expect_sum_q3q3	= new(){ W=2, X=2, Y=2, Z=2 };
		Quat expect_sum_q4q4	= new(){ W=-2, X=-2, Y=-2, Z=-2 };
		Quat expect_diff_q4q4	= new(){ W=0, X=0, Y=0, Z=0 };

		var sum_q1q2	= q1+q2;
		var diff_q1q2	= q1-q2;
		var sum_q3q3	= q3+q3;
		var sum_q4q4	= q4+q4;
		var diff_q4q4	= q4-q4;

		Assert.IsTrue(sum_q1q2	== expect_sum_q1q2, "q1+q2 returned unexpected value.");
		Assert.IsTrue(diff_q1q2 == expect_diff_q1q2, "q1-q2 returned unexpected value.");
		Assert.IsTrue(sum_q3q3	== expect_sum_q3q3, "q3+q3 returned unexpected value.");
		Assert.IsTrue(sum_q4q4	== expect_sum_q4q4, "q4+q4 returned unexpected value.");
		Assert.IsTrue(diff_q4q4 == expect_diff_q4q4, "q4-q4 returned unexpected value.");
	}
	/// <summary>
	/// Test quaternion multiplication and behavior
	/// - Multiplication (*)
	/// </summary>
	[TestMethod]
	public void Quat_MultiplicationTest() {
		Quat q1 = new(){ W=0, X=1, Y=0, Z=0 };
		Quat q2 = new(){ W=0, X=0, Y=1, Z=0 };
		Quat q3 = new(){ W=0, X=0, Y=0, Z=1 };
		Quat identity = new(){ W=1, X=0, Y=0, Z=0 };

		// left handed
		Quat expect_q1q2		= new(){ W=0, X=0, Y=0, Z=-1 };
		Quat expect_q1q2q2		= new(){ W=0, X=-1, Y=0, Z=0 };
		Quat expect_q1q2q2q2	= new(){ W=0, X=0, Y=0, Z=1 };
		Quat expect_identity	= new(){ W=1, X=0, Y=0, Z=0 }; 
		Quat expect_cyclic		= new(){ W=-1, X=0, Y=0, Z=0 };

		Assert.IsTrue(q1*q2 == expect_q1q2, "q1*q2 returned unexpected orientation.");
		Assert.IsTrue(q1*q2*q2 == expect_q1q2q2, "q1*q2*q2 returned unexpected orientation.");
		Assert.IsTrue(q1*q2*q2*q2 == expect_q1q2q2q2, "q1*q2*q2*q2 returned unexpected orientation.");

		Assert.IsTrue(q1*q2*q3 == expect_identity, "q1*q2*q3 should return identity.");
		Assert.IsTrue(q1*identity == q1, "q1*identity should return q1.");
		Assert.IsTrue(identity*q1 == q1, "q1*identity should return q1.");

		Assert.IsTrue(q1*q1 == expect_cyclic, "cyclic property failed, should return W=-1");
		Assert.IsTrue(q2*q2 == expect_cyclic, "cyclic property failed, should return W=-1");
		Assert.IsTrue(q3*q3 == expect_cyclic, "cyclic property failed, should return W=-1");

		Assert.IsFalse(q1*q2 == q2*q1, "Quaternion multiplication should not be commutative.");
	}
	[TestMethod]
	public void Quat_DivisionTest() {
		Quat q1 = new(){ W=0, X=1, Y=0, Z=0 };
		Quat q2 = new(){ W=0, X=0, Y=1, Z=0 };
		Quat q3 = new(){ W=0, X=0, Y=0, Z=1 };
		Quat identity = new(){ W=1, X=0, Y=0, Z=0 };

		Assert.IsTrue(q1/q1 == identity, "q1/q1 should return identity");
		Assert.IsTrue(q2/q2 == identity, "q2/q2 should return identity");
		Assert.IsTrue(q3/q3 == identity, "q3/q3 should return identity");
	}

// Native type conversion

	/// <summary>
	/// Test native Quaternion type conversion 
	/// - Quat.FromQuaternion
	/// - Quat.ToQuaternion
	/// </summary>
	[TestMethod]
	public void Quat_CanConvertNative() {
		float w=1, x=0, y=0 ,z=0;
		Quat q = new Quat { W=w, X=x, Y=y, Z=z };
		Quaternion qnative = new Quaternion { W=w, X=x, Y=y, Z=z };

		var q_ToQuaternion = Quat.ToQuaternion(q);
		var qnative_ToQuat = Quat.FromQuaternion(qnative);

		Assert.AreEqual(q_ToQuaternion.W, qnative.W, "ToQuaternion failed, W not identical");
		Assert.AreEqual(q_ToQuaternion.X, qnative.X, "ToQuaternion failed, X not identical");
		Assert.AreEqual(q_ToQuaternion.Y, qnative.Y, "ToQuaternion failed, Y not identical");
		Assert.AreEqual(q_ToQuaternion.Z, qnative.Z, "ToQuaternion failed, Z not identical");

		Assert.AreEqual(qnative_ToQuat.W, q.W, "ToQuat failed, W not identical");
		Assert.AreEqual(qnative_ToQuat.X, q.X, "ToQuat failed, X not identical");
		Assert.AreEqual(qnative_ToQuat.Y, q.Y, "ToQuat failed, Y not identical");
		Assert.AreEqual(qnative_ToQuat.Z, q.Z, "ToQuat failed, Z not identical");

	}
	/// <summary>
	/// Tests conversion to the native Matrix4x4 type
	/// - Quat.ToMatrix4x4
	/// </summary>
	[TestMethod]
	public void Quat_CanConvertToMatrix4x4() {
		Quat q = Quat.Identity();
		Assert.IsTrue(q.ToMatrix() == Matrix4x4.Identity, "ToMatrix failed to produce a Matrix4x4.Identity");
	}

// Functions

	[TestMethod]
	public void Quat_CanConjugate() {
		Quat q = new(){ W = 1, X = 1, Y = 1, Z = 1 };
		Quat q_conjugated = new(){ W = 1, X = -1, Y = -1, Z = -1 };
	}
}
