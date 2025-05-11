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
}
