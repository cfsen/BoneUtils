using BoneUtils.Math;
using System.Numerics;

namespace BoneUtilsUnitTests.MathTests;

/// <summary.>
/// Tests for the Quat struct
/// </summary>
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

		Assert.AreEqual(expect_sum_q1q2, sum_q1q2);
		Assert.AreEqual(expect_diff_q1q2, diff_q1q2);
		Assert.AreEqual(expect_sum_q3q3, sum_q3q3);
		Assert.AreEqual(expect_sum_q4q4, sum_q4q4);
		Assert.AreEqual(expect_diff_q4q4, diff_q4q4);
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

		Assert.AreEqual(expect_q1q2, q1*q2);
		Assert.AreEqual(expect_q1q2q2, q1*q2*q2);
		Assert.AreEqual(expect_q1q2q2q2, q1*q2*q2*q2);

		Assert.AreEqual(expect_identity, q1*q2*q3, "q1*q2*q3 should return identity");
		Assert.AreEqual(q1, q1*identity, "q1*identity should return q1");
		Assert.AreEqual(q1, identity*q1, "identity*q1 should return q1");

		Assert.AreEqual(expect_cyclic, q1*q1, "Cyclic property failed, should return W=-1");
		Assert.AreEqual(expect_cyclic, q2*q2, "Cyclic property failed, should return W=-1");
		Assert.AreEqual(expect_cyclic, q3*q3, "Cyclic property failed, should return W=-1");

		Assert.AreNotEqual(q1*q2, q2*q1, "Quaternion multiplication should not be commutative.");
	}
	/// <summary>
	/// Tests basic quat by quat division
	/// - Divison (/, Quat by Quat)
	/// </summary>
	[TestMethod]
	public void Quat_DivisionTest() {
		Quat q1 = new(){ W=0, X=1, Y=0, Z=0 };
		Quat q2 = new(){ W=0, X=0, Y=1, Z=0 };
		Quat q3 = new(){ W=0, X=0, Y=0, Z=1 };
		Quat identity = new(){ W=1, X=0, Y=0, Z=0 };

		Assert.AreEqual(identity, q1/q1);
		Assert.AreEqual(identity, q2/q2);
		Assert.AreEqual(identity, q3/q3);
	}
	/// <summary>
	/// Tests division by scalar
	/// - Division (/, Quat by float)
	/// </summary>
	[TestMethod]
	public void Quat_ScalarDivisionTest() {
		Quat q = new(){ W=2, X=2, Y=2, Z=2 };
		float scalar = 2.0f;

		Quat expect_scalarDiv = new(){ W=1, X=1, Y=1, Z=1 };

		Assert.AreEqual(expect_scalarDiv, (q / scalar));
	}

// Constructors

	[TestMethod]
	public void Quat_CanConstructFromQuaternion() {
		Quaternion native = new(){ W=1, X=2, Y=3, Z=4 };
		Quat q = new(native);

		Assert.AreEqual(native.W, q.W);
		Assert.AreEqual(native.X, q.X);
		Assert.AreEqual(native.Y, q.Y);
		Assert.AreEqual(native.Z, q.Z);
	}
	[TestMethod]
	public void Quat_CanConstructFromArguments() {
		Quat q = new(1,2,3,4);

		Assert.AreEqual(1, q.W);
		Assert.AreEqual(2, q.X);
		Assert.AreEqual(3, q.Y);
		Assert.AreEqual(4, q.Z);
	}

// Native type conversion

	/// <summary>
	/// Test native Quaternion type conversion 
	/// - FromQuaternion
	/// - ToQuaternion
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
	/// - ToMatrix4x4
	/// </summary>
	[TestMethod]
	public void Quat_CanConvertToMatrix4x4() {
		Quat q = Quat.Identity();

		Assert.AreEqual(Matrix4x4.Identity, q.ToMatrix());
	}

// Functions

	/// <summary>
	/// Tests quaternion conjugation 
	/// - Conjugate
	/// </summary>
	[TestMethod]
	public void Quat_CanConjugate() {
		Quat q = new(){ W = 1, X = 1, Y = 1, Z = 1 };
		Quat q_conjugated = new(){ W = 1, X = -1, Y = -1, Z = -1 };

		Assert.AreEqual(q_conjugated, q.Conjugate());
	}
	/// <summary>
	/// Tests rotating a vector by quaternions
	/// - RotateVector
	/// </summary>
	[TestMethod]
	public void Quat_CanRotateVector() {
		Vector3 v = new(1,0,0);
		Quat qAroundX = new(0,1,0,0);
		Quat qAroundY = new(0,0,1,0);
		Quat qAroundZ = new(0,0,0,1);
		Quat qHalfAroundZ = Quat.Normalize(new(0.5f,0,0,0.5f));

		Vector3 resAroundX = Quat.RotateVector(qAroundX, v);
		Vector3 resAroundY = Quat.RotateVector(qAroundY, v);
		Vector3 resAroundZ = Quat.RotateVector(qAroundZ, v);
		Vector3 resHalfAroundZ = Quat.RotateVector(qHalfAroundZ, v);

		Vector3 expected_aroundX = new(1,0,0);
		Vector3 expected_aroundY = new(-1,0,0);
		Vector3 expected_aroundZ = new(-1,0,0);
		Vector3 expected_halfAroundZ = new(0,-1,0);

		Assert.AreEqual(expected_aroundX, resAroundX);
		Assert.AreEqual(expected_aroundY, resAroundY);
		Assert.AreEqual(expected_aroundZ, resAroundZ);

		Assert.AreEqual(expected_halfAroundZ.X, resHalfAroundZ.X, 1E-6);
		Assert.AreEqual(expected_halfAroundZ.Y, resHalfAroundZ.Y, 1E-6);
		Assert.AreEqual(expected_halfAroundZ.Z, resHalfAroundZ.Z, 1E-6);
	}

}
