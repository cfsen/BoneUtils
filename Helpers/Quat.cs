using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BoneUtils.Helpers;
/*
	Quaternion functions for:
		- multiplication
		- division
	Were ported from math.net: 
	https://github.com/mathnet/mathnet-numerics/blob/master/src/FSharp/Quaternion.fs
*/
public struct Quat {
	public float X, Y, Z, W;
	public static Quat operator *(Quat r, Quat q) {
		return new Quat {
			W = r.W*q.W - r.X*q.X - r.Y*q.Y - r.Z*q.Z,
			X = r.W*q.X + r.X*q.W - r.Y*q.Z + r.Z*q.Y,
			Y = r.W*q.Y + r.X*q.Z + r.Y*q.W - r.Z*q.X,
			Z = r.W*q.Z - r.X*q.Y + r.Y*q.X + r.Z*q.W
		};
	}
	public static Quat operator /(Quat r, Quat q) {
		float d = (MathF.Pow(r.W, 2) + MathF.Pow(r.X, 2) + MathF.Pow(r.Y, 2) + MathF.Pow(r.Z, 2));
		return new Quat {
			W = (r.W*q.W + r.X*q.X + r.Y*q.Y + r.Z*q.Z) / d,
			X = (r.W*q.X - r.X*q.W - r.Y*q.Z + r.Z*q.Y) / d,
			Y = (r.W*q.Y + r.X*q.Z - r.Y*q.W - r.Z*q.X) / d,
			Z = (r.W*q.Z - r.X*q.Y + r.Y*q.X - r.Z*q.W) / d
		};
	}
	public static Quat operator /(Quat q, float a) {
		return new Quat { W=q.W/a, X=q.X/a, Y=q.Y/a, Z=q.Z/a, };
	}
	public static Quat FromQuaternion(Quaternion q) => new Quat { W = q.W, X = q.X, Y = q.Y, Z = q.Z, };
	public static Quaternion ToQuaternion(Quat q) => new Quaternion { W = q.W, X = q.X, Y = q.Y, Z=q.Z };
}
