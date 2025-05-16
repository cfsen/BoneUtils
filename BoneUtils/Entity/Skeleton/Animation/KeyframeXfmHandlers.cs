using BoneUtils.Helpers;
using BoneUtils.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BoneUtils.Entity.Skeleton.Animation; 

public delegate void KeyframeTransformer(BoneNode bone, TransformSnapshot xfm, AnimationXfmType ac);

/// <summary>
/// Transform handlers for updating skeletons. WIP.
/// </summary>
public static class KeyframeXfmHandlers {
	/// <summary>
	/// Basic transformer for keyframes. Leverages built-ins for propagation.
	/// </summary>
	/// <param name="bone">BoneNode to mutate.</param>
	/// <param name="xfm">Transform to set.</param>
	public static void KeyframeTransformerBasic(BoneNode bone, TransformSnapshot xfm, AnimationXfmType animType) {
		if(animType == AnimationXfmType.AdditiveRotation) {
			// Leverage BoneNode propagation
			bone.Rotate(xfm.Rotation);
		}
		else if(animType == AnimationXfmType.RotatePropagate) {
			// Calculate delta between rotation we'll be setting and current state
			var delta = xfm.Rotation * Quat.Inverse(bone.Transform.Rotation);

			// Set rotation (does not propagate)
			bone.Transform.Rotation = xfm.Rotation;

			// pass delta rotation to children for propagation
			for(int i = 0; i < bone.RenderChildrenCount; i++) {
				bone.RenderChildren[i].Rotate(delta, XfmHandlerFallbacks.BoneNodeRotateFallback, bone.Transform.Position);
			}
		}
		else if(animType == AnimationXfmType.TranslatePropagate) {
			// Pass translation delta
			bone.Translate(xfm.Position - bone.Transform.Position);
		}
		else if(animType == AnimationXfmType.Absolute) {
			bone.Transform.Rotation = xfm.Rotation;
			bone.Transform.Position = xfm.Position;
			bone.Transform.Scale = xfm.Scale;
		}
	}
}
