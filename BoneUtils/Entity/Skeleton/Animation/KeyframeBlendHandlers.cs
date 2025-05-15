using BoneUtils.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BoneUtils.Entity.Skeleton.Animation; 

/// <summary>
/// Keyframe blender delegate for interpolating transform state between two keyframes.
/// </summary>
/// <param name="origin">Transform state origin</param>
/// <param name="target">Transform state target</param>
/// <param name="blendFactor">Normalized time between origin and target</param>
/// <param name="context">Optional context for advanced blending.</param>
/// <returns>TransformSnapshot of interpolated state at normalized time.</returns>
public delegate TransformSnapshot xfmSnapshotBlender(TransformSnapshot origin, TransformSnapshot target, float blendFactor, object? context = null);

/// <summary>
/// Fallback blending functions for interpolating transform state between two keyframes.
/// </summary>
public static class KeyframeBlendHandlers {
	/// <summary>
	/// No blending for debugging purposes.
	/// </summary>
	/// <param name="origin">Transform state origin</param>
	/// <param name="target">Ignored.</param>
	/// <param name="blendFactor">Ignored.</param>
	/// <param name="context">Ignored.</param>
	/// <returns>origin TransformSnapshot</returns>
	public static TransformSnapshot BlendNone(TransformSnapshot origin, TransformSnapshot target, float blendFactor, object? context = null) {
		return origin;
	}

	/// <summary>
	/// Perform a basic linear blend of two transforms
	/// </summary>
	/// <param name="origin">Transform state origin</param>
	/// <param name="target">Transform state target</param>
	/// <param name="blendFactor">Normalized time (Lerp/Slerp factor)</param>
	/// <param name="context">Ignored.</param>
	/// <returns>TransformSnapshot of linear interpolation at normalized time.</returns>
	public static TransformSnapshot BlendLinear(TransformSnapshot origin, TransformSnapshot target, float blendFactor, object? context = null) {
		TransformSnapshot blend = new TransformSnapshot {
			Position = Vector3.Lerp(origin.Position, target.Position, blendFactor),
			Scale = Vector3.Lerp(origin.Scale, target.Scale, blendFactor),
			Rotation = target.Rotation,
		};
		return blend;
	}
}
