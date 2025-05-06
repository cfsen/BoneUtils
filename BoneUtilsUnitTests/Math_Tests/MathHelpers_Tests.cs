using System;
using System.Collections.Generic;
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
}
