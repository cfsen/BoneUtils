using BoneUtils.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BoneUtils.Entity.Skeleton;
public class BoneNodeXfmBuffer {
	public Vector3 Translation;
	public Vector3 Scale;
	public Quat Rotation;
	public Matrix4x4 Matrix;

	public bool Active;
	public bool Complete;
	public XfmType BufferedXfms;

	public BoneNodeXfmBuffer() {
		Reset();
	}
	public bool Begin() {
		if(Active || Complete) return false;
		Active = true;
		return true;
	}
	public bool Accumulate(Quat Rotation) => Accumulator(Rotation, XfmType.Rotate);
	public bool Accumulate(Vector3 Translation) => Accumulator(Translation, XfmType.Translate);
	public bool Accumulate(Matrix4x4 Matrix) => Accumulator(Matrix, XfmType.Matrix);
	public bool AccumulateScale(Vector3 Scale) => Accumulator(Scale, XfmType.Scale);
	private bool Accumulator(object xfm, XfmType type) {
		if(type == XfmType.None) return false;
		if(!Active) return false;

		switch (type) {
			case XfmType.Translate:
				Translation += (Vector3)xfm;
				break;
			case XfmType.Rotate:
				Rotation *= (Quat)xfm;
				break;
			case XfmType.Matrix:
				Matrix *= (Matrix4x4)xfm;
				break;
			case XfmType.Scale:
				Scale += (Vector3)xfm;
				break;
			default:
				return false;
		}

		BufferedXfms |= type;
		return true;
	}
	public bool End() {
		if(!Active) return false;
		Complete = true;
		return true;
	}
	public void Reset() {
		Translation = Vector3.Zero;
		Scale = Vector3.One;
		Rotation = Quat.Identity;
		Matrix = Matrix4x4.Identity;
		BufferedXfms = XfmType.None;
		Active = false;
		Complete = false;
	}
}
[Flags]
public enum XfmType {
	None = 0,
	Translate = 1,
	Rotate = 2,
	Scale = 4,
	Matrix = 8
}
