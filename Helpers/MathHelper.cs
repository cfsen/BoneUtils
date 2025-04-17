using System.Numerics;

namespace BoneUtils.Helpers;
public static class MathHelper {
	public static Vector3 QuaternionToEuler(Quaternion q, bool degrees = false, int? rounding = null) {
		Vector3 res = Vector3.Zero;

		res.X = MathF.Atan2(
			2 * (q.W * q.X + q.Y * q.Z),
			1 - 2 * (q.X * q.X + q.Y * q.Y));

		res.Y = 2 * MathF.Atan2(
			MathF.Sqrt(1 + 2 * (q.W * q.Y - q.X * q.Z)),
			MathF.Sqrt(1 - 2 * (q.W * q.Y - q.X * q.Z))
			) - MathF.PI / 2;

		res.Z = MathF.Atan2(
			2 * (q.W * q.Z + q.X * q.Y),
			1 - 2 * (q.Y * q.Y + q.Z * q.Z));

		if (degrees) {
			res.X = RadToDeg(res.X);
			res.Y = RadToDeg(res.Y);
			res.Z = RadToDeg(res.Z);
		}

		if (rounding != null) {
			res.X = MathF.Round(res.X, Math.Clamp(rounding.Value, 0, 6));
			res.Y = MathF.Round(res.Y, Math.Clamp(rounding.Value, 0, 6));
			res.Z = MathF.Round(res.Z, Math.Clamp(rounding.Value, 0, 6));
		}

		return res;
	}
	/// <summary>
	/// Determines angles of a Vector3 in its component R2 planes.
	/// </summary>
	/// <param name="u">Vector to deconstruct</param>
	/// <param name="degrees">Return result in degrees</param>
	/// <returns>Vector3 containing angles.</returns>
	public static Vector3 VectorAngles(Vector3 u, bool degrees = false) {
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
			Z = MathF.Acos(u.X / len.Z)     // XY-plane mapts to rotation around Z axis, roll = cos(x) || sin(y)
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

		// Conversion
		if (degrees) {
			o.X *= 180 / MathF.PI;
			o.Y *= 180 / MathF.PI;
			o.Z *= 180 / MathF.PI;
		}

		return o;
	}
	public static Vector3 RotateWithDriftCorrection(Vector3 childOriginPos, Quaternion newOrientation) {
		Vector3 u = Vector3.Transform(childOriginPos, newOrientation);
		u.X = MathF.Round(u.X, 6);
		u.Y = MathF.Round(u.Y, 6);
		u.Z = MathF.Round(u.Z, 6);
		if (u.Length() != childOriginPos.Length())
			return u+u*(childOriginPos.Length()-u.Length());
		else return u;
	}

	// Radian conversion

	public static float DegToRad(float deg) => deg/180.0f*MathF.PI;
	public static float RadToDeg(float rad) => rad*180.0f/MathF.PI;
	public static double DegToRad(double deg) => deg/180.0f*MathF.PI;
	public static double RadToDeg(double rad) => rad*180.0f/MathF.PI;


}
