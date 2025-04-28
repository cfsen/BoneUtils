using BoneUtils.Math;
using System.Diagnostics;
using System.Numerics;

namespace BoneUtils.Helpers;
/*
	Quaternion to euler ported from C++ snippet found on wikipedia:
	https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles#Quaternion_to_Euler_angles_(in_3-2-1_sequence)_conversion
*/
public static class MathHelper {
	[Obsolete("Moving off the native Quaternion type. Use Quat instead.")]
	public static Vector3 QuaternionToEuler(Quaternion q) {
		return new Vector3 {
			X = MathF.Atan2(
				2 * (q.W * q.X + q.Y * q.Z),
				1 - 2 * (q.X * q.X + q.Y * q.Y)),
			Y = 2 * MathF.Atan2(
				MathF.Sqrt(1 + 2 * (q.W * q.Y - q.X * q.Z)),
				MathF.Sqrt(1 - 2 * (q.W * q.Y - q.X * q.Z))
				) - MathF.PI / 2,
			Z = MathF.Atan2(
				2 * (q.W * q.Z + q.X * q.Y),
				1 - 2 * (q.Y * q.Y + q.Z * q.Z)),
		};
	}
	public static Vector3 QuatToEuler(Quat q) {
		return new Vector3 {
			X = MathF.Atan2(
				2 * (q.W * q.X + q.Y * q.Z),
				1 - 2 * (q.X * q.X + q.Y * q.Y)),
			Y = 2 * MathF.Atan2(
				MathF.Sqrt(1 + 2 * (q.W * q.Y - q.X * q.Z)),
				MathF.Sqrt(1 - 2 * (q.W * q.Y - q.X * q.Z))
				) - MathF.PI / 2,
			Z = MathF.Atan2(
				2 * (q.W * q.Z + q.X * q.Y),
				1 - 2 * (q.Y * q.Y + q.Z * q.Z)),
		};
	}
	/// <summary>
	/// Determines angles of a Vector3 in its component R2 planes.
	/// </summary>
	/// <param name="u">Vector to deconstruct</param>
	/// <param name="degrees">Return result in degrees</param>
	/// <returns>Vector3 containing angles.</returns>
	public static Vector3 VectorAngles(Vector3 u) {
		Vector3 len = new() {
			X = MathF.Max(MathF.Sqrt(u.X * u.X + u.Z * u.Z), float.Epsilon),
			Y = MathF.Max(MathF.Sqrt(u.Y * u.Y + u.Z * u.Z), float.Epsilon),
			Z = MathF.Max(MathF.Sqrt(u.X * u.X + u.Y * u.Y), float.Epsilon)
		};

		Vector3 s = new() { X=float.Sign(u.X), Y=float.Sign(u.Y), Z=float.Sign(u.Z) };

		// Initial angle calc
		Vector3 o = new() {
			X = MathF.Acos(u.X / len.X),    // XZ-plane maps to rotation around Y axis, yaw = cos(x) || sin(z)
			Y = MathF.Acos(u.Z / len.Y),    // YZ-plane maps to rotation around X axis, pitch = cos(z) || sin(y)
			Z = MathF.Acos(u.X / len.Z)     // XY-plane maps to rotation around Z axis, roll = cos(x) || sin(y)
		};

		// Adjust for quad
		if (s.Y >= 0 && s.Z >= 0 || s.Y < 0 && s.Z >= 0)
			o.X *= -1.0f;
		if (s.Y < 0 && s.Z >= 0 || s.Y < 0 && s.Z < 0)
			o.Z *= -1.0f;
		if (s.X < 0) {
			o.X += MathF.PI;
			o.X *= -1.0f;
		}

		return o;
	}
	public static Vector3 RotateWithDriftCorrection(Vector3 childPosition, Quat newOrientation) {
		Vector3 u = Quat.RotateVector(newOrientation, childPosition);
		return FPCorrection(u, childPosition.Length());
	}
	[Obsolete("Moving off the native Quaternion type. Use Quat instead.")]
	public static Vector3 RotateWithDriftCorrection(Vector3 childPosition, Quaternion newOrientation) {
		Vector3 u = Vector3.Transform(childPosition, newOrientation);
		return FPCorrection(u, childPosition.Length());
	}
	public static Vector3 FPCorrection(Vector3 u, float length) {
		u.X = MathF.Round(u.X, 6);
		u.Y = MathF.Round(u.Y, 6);
		u.Z = MathF.Round(u.Z, 6);
		if (u.Length() != length)
			u += u*(length-u.Length());
		return u;
	}
	/// <summary>
	/// Create a list of Vector3 that indicate the orientation of a quaternion
	/// </summary>
	/// <param name="q">The quaternion to generate indicators for</param>
	/// <param name="origin">Position of the quaternion in world space</param>
	/// <param name="length">Length of indicating vectors (distance from position)</param>
	public static Dictionary<string, Vector3> CreateLocalDirectionVectors(Quat q, Vector3 origin, float length = 0.5f) {
		/*
		Create unit vectors in world space for each axis
		1,0,0 - 0,1,0 - 0,0,1
		then apply rotation to each one using the quat associated with the bonenode
		then translate it into position
		 */

		Vector3 x = FPCorrection(Quat.RotateVector(q, Vector3.UnitX), length) + origin;
		Vector3 y = FPCorrection(Quat.RotateVector(q, Vector3.UnitY), length) + origin;
		Vector3 z = FPCorrection(Quat.RotateVector(q, Vector3.UnitZ), length) + origin;

		return new Dictionary<string, Vector3> { ["X"] = x, ["Y"] = y, ["Z"] = z };
	}

	// Radian conversion

	public static float DegToRad(float deg) => deg/180.0f*MathF.PI;
	public static float RadToDeg(float rad) => rad*180.0f/MathF.PI;
	public static double DegToRad(double deg) => deg/180.0f*MathF.PI;
	public static double RadToDeg(double rad) => rad*180.0f/MathF.PI;


}
