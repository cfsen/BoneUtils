using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BoneUtils.Helpers;
/*
	This was ported from math.net: 
	https://github.com/mathnet/mathnet-numerics/blob/master/src/FSharp/Quaternion.fs
*/

public struct Quat {
	public float X, Y, Z, W;

	// Operators

	public static Quat operator +(Quat r, Quat q)
		=> new() {W=r.W+q.W, X=r.X+q.X, Y=r.Y+q.Y, Z=r.Z+q.Z};
	public static Quat operator -(Quat r, Quat q)
		=> new() {W=r.W-q.W, X=r.X-q.X, Y=r.Y-q.Y, Z=r.Z-q.Z};
	public static Quat operator *(Quat r, Quat q) {
		return new() {
			W = r.W*q.W - r.X*q.X - r.Y*q.Y - r.Z*q.Z,
			X = r.W*q.X + r.X*q.W - r.Y*q.Z + r.Z*q.Y,
			Y = r.W*q.Y + r.X*q.Z + r.Y*q.W - r.Z*q.X,
			Z = r.W*q.Z - r.X*q.Y + r.Y*q.X + r.Z*q.W
		};
	}
	public static Quat operator /(Quat r, Quat q) {
		float d = NormSquared(r);
		return new() {
			W = (r.W*q.W + r.X*q.X + r.Y*q.Y + r.Z*q.Z) / d,
			X = (r.W*q.X - r.X*q.W - r.Y*q.Z + r.Z*q.Y) / d,
			Y = (r.W*q.Y + r.X*q.Z - r.Y*q.W - r.Z*q.X) / d,
			Z = (r.W*q.Z - r.X*q.Y + r.Y*q.X - r.Z*q.W) / d
		};
	}
	public static Quat operator /(Quat q, float a) {
		return new Quat { W=q.W/a, X=q.X/a, Y=q.Y/a, Z=q.Z/a, };
	}

	// Initializers

	/// <summary>
	/// Create a Quat using a unit vector and an angle
	/// </summary>
	/// <param name="angle">Angle in radians</param>
	/// <param name="u">Unit vector</param>
	/// <returns></returns>
	public static Quat Create(float angle, Vector3 u) {
		// TODO benchmark allocating sin vs current implementation
		float uInv = 1.0f / MathF.Sqrt(u.X*u.X + u.Y*u.Y + u.Z*u.Z);
		return new() {
			W = MathF.Cos(angle*0.5f),
			X= u.X * uInv * MathF.Sin(angle*0.5f),
			Y= u.Y * uInv * MathF.Sin(angle*0.5f),
			Z= u.Z * uInv * MathF.Sin(angle*0.5f)
		};
	}

	// Functions

	public static Quat Conjugate(Quat q) 
		=> new() {W=q.W, X=-q.X, Y=-q.Y, Z=-q.Z };
	public static float NormSquared(Quat q) 
		=> q.W*q.W + q.X*q.X + q.Y*q.Y + q.Z*q.Z;
	public static float EuclidianNorm(Quat q) 
		=> MathF.Sqrt(NormSquared(q));
	public static Quat Normalize(Quat q) {
		float invNorm = 1.0f/EuclidianNorm(q);
		return new() { W=q.W*invNorm, X=q.X*invNorm, Y=q.Y*invNorm, Z=q.Z*invNorm };
	}
	public static Quat Inverse(Quat q) 
		=> Conjugate(q)/NormSquared(q);
	public static float Dot(Quat q1, Quat q2) 
		=> q1.W*q2.W + q1.X*q2.X + q1.Y*q2.Y + q1.Z*q2.Z;

	// Native type conversion

	public static Quat FromQuaternion(Quaternion q) 
		=> new() { W = q.W, X = q.X, Y = q.Y, Z = q.Z };
	public static Quaternion ToQuaternion(Quat q) 
		=> new() { W = q.W, X = q.X, Y = q.Y, Z=q.Z };
}
