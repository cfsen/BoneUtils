using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoneUtils.Entity.Skeleton.Animation; 

public delegate void KeyframeTransformer(BoneNode bone, TransformSnapshot xfm, AnimationXfmType ac);

public static class KeyframeXfmHandlers {
	/// <summary>
	/// Basic transformer for keyframes. Leverages built-ins for propagation.
	/// </summary>
	/// <param name="bone">BoneNode to mutate.</param>
	/// <param name="xfm">Transform to set.</param>
	public static void KeyframeTransformerBasic(BoneNode bone, TransformSnapshot xfm, AnimationXfmType animType) {
		bone.Transform.Scale = xfm.Scale;

		if(animType == AnimationXfmType.Relative) {
			// Leverage BoneNode propagation
			bone.Rotate(xfm.Rotation);
			bone.Translate(xfm.Position);
		}
		else if(animType == AnimationXfmType.Static) {
			bone.Transform.Rotation = xfm.Rotation;
			bone.Transform.Position = xfm.Position;
		}
	}
}
