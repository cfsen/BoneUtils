using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using BoneUtils.Helpers;
using BoneUtils.Math;

namespace Tests.Math_Tests;
[TestClass]
[TestCategory("Math")]
public class MathHelpers_Tests {

	/// <summary>
	/// Checks Vector3 length clamping.
	/// </summary>
	[TestMethod]
	public void Vector3_ClampToLength() {
		Vector3 v0 = new(1,0,0);
		Vector3 v1 = new(2,0,0);
		Vector3 v2 = new(1,1,0);
		Vector3 v3 = new(0,-1,0);
		Vector3 v4 = new(0,-10,0);

		float clampLength0 = 1.0f;
		float clampLength1 = 3.0f;
		float clampLength_illegal = -3.0f;

		Vector3 expect_v0_length0 = new(1,0,0);
		Vector3 expect_v0_length1 = new(3,0,0);
		Vector3 expect_v1_length0 = new(1,0,0);
		Vector3 expect_v1_length1 = new(3,0,0);
		Vector3 expect_v2_length0 = new(0.70710677f,0.70710677f,0);
		Vector3 expect_v2_length1 = new(2.1213205f,2.1213205f,0);
		Vector3 expect_v3_length0 = new(0,-1,0);
		Vector3 expect_v3_length1 = new(0,-3,0);
		Vector3 expect_v4_length0 = new(0,-1,0);
		Vector3 expect_v4_length1 = new(0,-3,0);

		Assert.AreEqual(expect_v0_length0, MathHelper.ClampToLength(v0, clampLength0));
		Assert.AreEqual(expect_v0_length1, MathHelper.ClampToLength(v0, clampLength1));

		Assert.AreEqual(expect_v1_length0, MathHelper.ClampToLength(v1, clampLength0));
		Assert.AreEqual(expect_v1_length1, MathHelper.ClampToLength(v1, clampLength1));

		Assert.AreEqual(expect_v2_length0, MathHelper.ClampToLength(v2, clampLength0));
		Assert.AreEqual(expect_v2_length1, MathHelper.ClampToLength(v2, clampLength1));

		Assert.AreEqual(expect_v3_length0, MathHelper.ClampToLength(v3, clampLength0));
		Assert.AreEqual(expect_v3_length1, MathHelper.ClampToLength(v3, clampLength1));

		Assert.AreEqual(expect_v4_length0, MathHelper.ClampToLength(v4, clampLength0));
		Assert.AreEqual(expect_v4_length1, MathHelper.ClampToLength(v4, clampLength1));

		Assert.ThrowsException<ArgumentOutOfRangeException>(() => MathHelper.ClampToLength(v0, clampLength_illegal));
	}
	/// <summary>
	/// Checks correction approach for floating point errors
	/// </summary>
	[TestMethod]
	public void Vector3_FPCorrection() {
		/*
		TODO edge case for FPCorrection:
		- If the length provided is <0.9 times the length returned from Vector3.Length()
		the correction will overshoot, resulting in a negative vector.
		- Larger lengths seem to be handled correctly.
		 */

		// Define vectors
		Vector3 v0 = new(0,0,0);
		Vector3 v1 = new(1,0,0);
		Vector3 v2 = new(1,1,1);
		Vector3 v3 = new(0,1,0);
		
		// Define vector lengths
		float len_v0 = 1.0f;
		float len_v1 = 1/3f;
		float len_v2 = v2.Length()*0.9f;
		float len_v3 = 10.0f;

		// Define expected returns
		Vector3 expect_v0 = new(0,0,0);
		Vector3 expect_v1 = new(0.33333333f,0,0);
		Vector3 expect_v2 = new(0.82679486f,0.82679486f,0.82679486f);
		Vector3 expect_v3 = new(0, 10.0f, 0);

		// Define tolerances for floating point correction
		float tolerance_v1 = 5E-8f;
		float tolerance_v2 = 5E-8f;

		// Perform correction
		Vector3 result_v0 = MathHelper.FPCorrection(v0, len_v0);
		Vector3 result_v1 = MathHelper.FPCorrection(v1, len_v1);
		Vector3 result_v2 = MathHelper.FPCorrection(v2, len_v2);
		Vector3 result_v3 = MathHelper.FPCorrection(v3, len_v3);


		Assert.AreEqual(expect_v0, result_v0, "[v0] Should not change vector.");

		Assert.AreEqual(expect_v1.X, result_v1.X, tolerance_v1, "[v1] Should change vector X component within tolerance.");
		Assert.AreEqual(expect_v1.Y, result_v1.Y, tolerance_v1, "[v1] Should not change vector Y component.");
		Assert.AreEqual(expect_v1.Z, result_v1.Z, tolerance_v1, "[v1] Should not change vector Z component.");

		Assert.AreEqual(expect_v2.X, result_v2.X, tolerance_v2, "[v2] Should change vector X component within tolerance.");
		Assert.AreEqual(expect_v2.Y, result_v2.Y, tolerance_v2, "[v2] Should not change vector Y component.");
		Assert.AreEqual(expect_v2.Z, result_v2.Z, tolerance_v2, "[v2] Should not change vector Z component.");

		Assert.AreEqual(expect_v3.X, result_v3.X, "[v3] Should not change vector X component.");
		Assert.AreEqual(expect_v3.Y, result_v3.Y, "[v3] Should change vector Y component.");
		Assert.AreEqual(expect_v3.Z, result_v3.Z, "[v3] Should not change vector Z component.");
	}
	/// <summary>
	/// Checks Vector3 rotation with FP drift correction
	/// </summary>
	[TestMethod]
	public void Vector3_RotateWithDriftCorrection(){ 
		Vector3 v0 = Vector3.UnitX;
		Quat q0 = Quat.Create(MathF.PI/2, Vector3.UnitY);
		Quat q1 = Quat.Create(MathF.PI, Vector3.UnitY);
		Quat q2 = Quat.Create(MathF.PI/6, Vector3.UnitY);
		
		Vector3 expect_v0q0 = new(0,0,1);
		Vector3 expect_v0q1 = new(-1,0,0);
		Vector3 expect_v0q2 = new(0.866025f,0,0.499999f);

		Vector3 result_v0q0 = MathHelper.RotateWithDriftCorrection(v0, q0, Vector3.Zero);
		Vector3 result_v0q1 = MathHelper.RotateWithDriftCorrection(v0, q1, Vector3.Zero);
		Vector3 result_v0q2 = MathHelper.RotateWithDriftCorrection(v0, q2, Vector3.Zero);

		float tolerance_v0q2 = 5E-6f;

		Assert.AreEqual(expect_v0q0, result_v0q0, "Should not induce rounding errors on simple orientations.");
		Assert.AreEqual(expect_v0q1, result_v0q1, "Should not induce rounding errors on simple orientations.");

		Assert.AreEqual(expect_v0q2.X, result_v0q2.X, tolerance_v0q2, "X component should change within tolerance.");
		Assert.AreEqual(expect_v0q2.Y, result_v0q2.Y, tolerance_v0q2, "Y component should change within tolerance.");
		Assert.AreEqual(expect_v0q2.Z, result_v0q2.Z, tolerance_v0q2, "Z component should change within tolerance.");
	}
	/// <summary>
	/// Checks Quat to euler angle conversion
	/// </summary>
	[TestMethod]
	public void Quat_ToEuler() {
		Quat q0 = Quat.Create(MathF.PI/2, Vector3.UnitX);
		Quat q1 = Quat.Create(MathF.PI/2, Vector3.UnitY);
		Quat q2 = Quat.Create(MathF.PI/2, Vector3.UnitZ);

		Vector3 v0 = MathHelper.QuatToEuler(q0);
		Vector3 v1 = MathHelper.QuatToEuler(q1);
		Vector3 v2 = MathHelper.QuatToEuler(q2);

		// Expected angles in radians
		Vector3 expect_v0 = new(1.570f,0,0);
		Vector3 expect_v1 = new(0,1.570f,0);
		Vector3 expect_v2 = new(0,0,1.570f);

		float tolerance = 5e-3f;

		Assert.AreEqual(expect_v0.X, v0.X, tolerance, "[v0] X component should be the expected angle in radians.");
		Assert.AreEqual(expect_v0.Y, v0.Y, tolerance, "[v0] Y component should be the expected angle in radians.");
		Assert.AreEqual(expect_v0.Z, v0.Z, tolerance, "[v0] Z component should be the expected angle in radians.");

		Assert.AreEqual(expect_v1.X, v1.X, tolerance, "[v1] X component should be the expected angle in radians.");
		Assert.AreEqual(expect_v1.Y, v1.Y, tolerance, "[v1] Y component should be the expected angle in radians.");
		Assert.AreEqual(expect_v1.Z, v1.Z, tolerance, "[v1] Z component should be the expected angle in radians.");

		Assert.AreEqual(expect_v2.X, v2.X, tolerance, "[v2] X component should be the expected angle in radians.");
		Assert.AreEqual(expect_v2.Y, v2.Y, tolerance, "[v2] Y component should be the expected angle in radians.");
		Assert.AreEqual(expect_v2.Z, v2.Z, tolerance, "[v2] Z component should be the expected angle in radians.");
	}
	/// <summary>
	/// Checks the Quat helper function for drawing direction vectors.
	/// </summary>
	[TestMethod]
	public void Quat_CreateLocalDirectionVectors() {
		Quat q0 = Quat.Create(0, Vector3.UnitX);
		Quat q1 = Quat.Create(MathF.PI/2, Vector3.UnitY);
		Quat q2 = Quat.Create(-MathF.PI/2, Vector3.UnitX);

		MathHelper.QuatOrientationVectors res_q0 = new();
		MathHelper.QuatOrientationVectors res_q1 = new();
		MathHelper.QuatOrientationVectors res_q2 = new();

		MathHelper.CreateLocalDirectionVectors(q0, Vector3.Zero, ref res_q0, 1.0f);
		MathHelper.CreateLocalDirectionVectors(q1, Vector3.Zero, ref res_q1, 1.0f);
		MathHelper.CreateLocalDirectionVectors(q2, Vector3.Zero, ref res_q2, 1.0f);

		Vector3 expect_q0_X = new(1,0,0);
		Vector3 expect_q0_Y = new(0,1,0);
		Vector3 expect_q0_Z = new(0,0,1);

		Vector3 expect_q1_X = new(0,0,1);
		Vector3 expect_q1_Y = new(0,1,0);
		Vector3 expect_q1_Z = new(-1,0,0);

		Vector3 expect_q2_X = new(1,0,0);
		Vector3 expect_q2_Y = new(0,0,1);
		Vector3 expect_q2_Z = new(0,-1,0);

		Assert.AreEqual(expect_q0_X, res_q0.X, "[q0.X]");
		Assert.AreEqual(expect_q0_Y, res_q0.Y, "[q0.Y]");
		Assert.AreEqual(expect_q0_Z, res_q0.Z, "[q0.Z]");

		Assert.AreEqual(expect_q1_X, res_q1.X, "[q1.X]");
		Assert.AreEqual(expect_q1_Y, res_q1.Y, "[q1.Y]");
		Assert.AreEqual(expect_q1_Z, res_q1.Z, "[q1.Z]");

		Assert.AreEqual(expect_q2_X, res_q2.X, "[q2.X]");
		Assert.AreEqual(expect_q2_Y, res_q2.Y, "[q2.Y]");
		Assert.AreEqual(expect_q2_Z, res_q2.Z, "[q2.Z]");
	}
}
