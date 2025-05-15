using BoneUtils.Math;
using System.Diagnostics;
using System.Numerics;

namespace BoneUtils.Helpers;
/*
	Quaternion to euler ported from C++ snippet found on wikipedia:
	https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles#Quaternion_to_Euler_angles_(in_3-2-1_sequence)_conversion
*/
public static class MathHelper {
	/// <summary>
	/// Converts a Quat to euler angles
	/// </summary>
	/// <param name="q">Quat to convert</param>
	/// <returns>Vector3 of angles in radians</returns>
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
	/// Clamps the length of a Vector3.
	/// </summary>
	/// <param name="position">Vector3 to clamp</param>
	/// <param name="clampLength">Length to clamp at</param>
	/// <returns>Clamped Vector.</returns>
	public static Vector3 ClampToLength(Vector3 position, float clampLength) {
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(clampLength);

		float length = position.Length();
		if(length == clampLength)
			return position;
		if(length == 0)
			length = 1;

		return position*(clampLength/length);
	}

	/// <summary>
	/// Rotates a Vector3 using a Quat and origin Vector3, correcting for floating point errors.
	/// </summary>
	/// <param name="position">Head of vector</param>
	/// <param name="rotation">Rotation to apply</param>
	/// <param name="origin">Origin of vector</param>
	/// <returns>Reoriented vector</returns>
	public static Vector3 RotateWithDriftCorrection(Vector3 position, Quat rotation, Vector3 origin) {
		Vector3 u = Quat.RotateVector(rotation, position-origin);
		return FPCorrection(u, (position-origin).Length())+origin;
	}

	/// <summary>
	/// Corrects for floating point errors by rounding at 6 decimal points and adjusting the vectors
	/// length with scalar multiplication.
	/// </summary>
	/// <param name="u">Vector to correct.</param>
	/// <param name="length">Intended length of vector.</param>
	/// <returns>Corrected vector.</returns>
	public static Vector3 FPCorrection(Vector3 u, float length) {
		#warning Fix edge case in FPCorrection
		// TODO edge case: corrections overshooting when provided length is less than len*0.9
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
	/// <param name="orientation">Points where direction vectors will end</param>
	/// <param name="length">Length of indicating vectors (distance from position)</param>
	public static void CreateLocalDirectionVectors(Quat q, Vector3 origin, ref QuatOrientationVectors orientation, float length = 0.5f) { // TODO consider using out for result
		orientation.X = FPCorrection(Quat.RotateVector(q, Vector3.UnitX), length) + origin;
		orientation.Y = FPCorrection(Quat.RotateVector(q, Vector3.UnitY), length) + origin;
		orientation.Z = FPCorrection(Quat.RotateVector(q, Vector3.UnitZ), length) + origin;
	}

	// Radian conversion

	public static float DegToRad(float deg) => deg/180.0f*MathF.PI;
	public static float RadToDeg(float rad) => rad*180.0f/MathF.PI;
	public static double DegToRad(double deg) => deg/180.0f*MathF.PI;
	public static double RadToDeg(double rad) => rad*180.0f/MathF.PI;

	// Structs

	public ref struct QuatOrientationVectors(Vector3 x, Vector3 y, Vector3 z) {
		public Vector3 X = x;
		public Vector3 Y = y;
		public Vector3 Z = z;
	}
}
