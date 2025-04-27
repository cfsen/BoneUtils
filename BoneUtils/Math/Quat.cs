using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BoneUtils.Math;
/*
	This was ported from math.net: 
	https://github.com/mathnet/mathnet-numerics/blob/master/src/FSharp/Quaternion.fs

	Left-handed
*/

[StructLayout(LayoutKind.Sequential)]
public struct Quat :IEquatable<Quat> {
	public float X, Y, Z, W;

	// Operators

	public static Quat operator +(Quat r, Quat q)
		=> new() {W=r.W+q.W, X=r.X+q.X, Y=r.Y+q.Y, Z=r.Z+q.Z};
	public static Quat operator -(Quat r, Quat q)
		=> new() {W=r.W-q.W, X=r.X-q.X, Y=r.Y-q.Y, Z=r.Z-q.Z};
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Quat operator *(Quat r, Quat q) {
		return new() {
			W = r.W*q.W - r.X*q.X - r.Y*q.Y - r.Z*q.Z,
			X = r.W*q.X + r.X*q.W - r.Y*q.Z + r.Z*q.Y,
			Y = r.W*q.Y + r.X*q.Z + r.Y*q.W - r.Z*q.X,
			Z = r.W*q.Z - r.X*q.Y + r.Y*q.X + r.Z*q.W
		};
	}
	public static Quat operator *(Quat r, float s) {
		return new() {
			W = r.W*s, X = r.X*s, Y = r.Y*s, Z = r.Z*s
		};
	}
	public static Quat operator *(float s, Quat r) => r * s;
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
	public static bool operator ==(Quat r, Quat q) {
		return (r.W == q.W) && (r.X == q.X) && (r.Y == q.Y) && (r.Z == q.Z);
	}
	public static bool operator !=(Quat r, Quat q) {
		return (r.W != q.W) || (r.X != q.X) || (r.Y != q.Y) || (r.Z != q.Z); 
	}

	// Constructor

	public Quat(float W = 1, float X = 0, float Y = 0, float Z = 0) {
		this.W = W;
		this.X = X;
		this.Y = Y;
		this.Z = Z;
	}
	public Quat(Quaternion q) => this = FromQuaternion(q);

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
	public static Quat Identity() 
		=> new() { W = 1, X = 0, Y = 0, Z = 0 };

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
	public static Vector3 RotateVector(Quat q, Vector3 v) {
		Quat u = q.Normalize() * new Quat(0, v.X, v.Y, v.Z) * q.Inverse();
		return new(){ X=u.X, Y=u.Y, Z=u.Z };
	}
	public static Quat Slerp(Quat q0, Quat q1, float u) {
		// https://splines.readthedocs.io/en/latest/rotation/slerp.html
		// TODO benchmark for performance vs allocating theta

		if(MathF.Sin(MathF.Acos(Quat.Dot(q0,q1))) < float.Epsilon) // Quats are nearly identical
			return q0;

		return 
			(q0 * MathF.Sin((1-u) * MathF.Acos(Quat.Dot(q0, q1))) + q1 * MathF.Sin(u * MathF.Acos(Quat.Dot(q0, q1))))
			/ MathF.Max(float.Epsilon, MathF.Sin(MathF.Acos(Quat.Dot(q0, q1))));
	}

	// Native type conversion

	public static Quat FromQuaternion(Quaternion q) 
		=> new() { W = q.W, X = q.X, Y = q.Y, Z = q.Z };
	public static Quaternion ToQuaternion(Quat q) 
		=> new() { W = q.W, X = q.X, Y = q.Y, Z=q.Z };
	public static Matrix4x4 ToMatrix(Quat q) {
		// https://www.mathworks.com/help/nav/ref/quaternion.rotmat.html
		return new() {
			M11 = 2*(q.W*q.W + q.X*q.X) - 1, 	M12 = 2*(q.X*q.Y + q.W*q.Z), 	M13 = 2*(q.X*q.Z - q.W*q.Y), 	M14 = 0,
			M21 = 2*(q.X*q.Y - q.W*q.Z),		M22 = 2*(q.W*q.W + q.Y*q.Y)-1,	M23 = 2*(q.Y*q.Z + q.W*q.X),	M24 = 0,
			M31 = 2*(q.X*q.Z + q.W*q.Y),		M32 = 2*(q.Y*q.Z - q.W*q.X),	M33 = 2*(q.W*q.W + q.Z*q.Z)-1,	M34 = 0,
			M41 = 0,							M42 = 0,						M43 = 0,						M44 = 1 
		};
	}
	public static Quat FromMatrix4x4(Matrix4x4 m) {
		// https://www.ljll.fr/~frey/papers/scientific%20visualisation/Shoemake%20K.,%20Quaternions.pdf
		throw new NotImplementedException();
	}
	public static Vector3 ToVector3(Quat q)
		=> new(){ X=q.X, Y=q.Y, Z=q.Z }; 

	// Instance methods

	public Matrix4x4 ToMatrix() 
		=> ToMatrix(this);
	public Vector3 ToVector3() 
		=> ToVector3(this);
	public Quaternion ToQuaternion() 
		=> ToQuaternion(this);
	public Quat Conjugate() 
		=> Conjugate(this);
	public float NormSquared() 
		=> NormSquared(this);
	public float EuclidianNorm() 
		=> EuclidianNorm(this);
	public Quat Normalize() 
		=> Normalize(this);
	public Quat Inverse() 
		=> Inverse(this);

	// Native integration
	
	public bool Equals(Quat other) 
		=> this == other;
	public override bool Equals(object? obj) {
		if (obj is Quat q) {
			return this == q;
		}
		return false;
	}
	public override int GetHashCode() 
		=> HashCode.Combine(W, X, Y, Z);
	public override string ToString() 
		=> $"{{X:{X} Y:{Y} Z:{Z} W:{W}}}";
}
