using BoneUtils.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BoneUtils.Entity.Skeleton.Animation; 

public delegate TransformSnapshot xfmSnapshotBlender(TransformSnapshot origin, TransformSnapshot target, float blendFactor, object? context = null);

public static class KeyframeBlendHandlers {
	/// <summary>
	/// Returns the origin keyframe without blending.
	/// </summary>
	/// <param name="origin"></param>
	/// <param name="target"></param>
	/// <param name="blend"></param>
	/// <param name="context"></param>
	/// <returns></returns>
	public static TransformSnapshot BlendNone(TransformSnapshot origin, TransformSnapshot target, float blendFactor, object? context = null) {
		return origin;
	}

	/// <summary>
	/// Perform a basic linear blend of two transforms
	/// </summary>
	/// <param name="origin"></param>
	/// <param name="target"></param>
	/// <param name="blendFactor"></param>
	/// <param name="context"></param>
	/// <returns></returns>
	public static TransformSnapshot BlendLinear(TransformSnapshot origin, TransformSnapshot target, float blendFactor, object? context = null) {
		TransformSnapshot blend = new TransformSnapshot {
			Position = Vector3.Lerp(origin.Position, target.Position, blendFactor),
			Scale = Vector3.Lerp(origin.Scale, target.Scale, blendFactor),
			Rotation = Quat.Slerp(origin.Rotation, target.Rotation, blendFactor),
		};
		return blend;
	}
}
