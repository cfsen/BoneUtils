using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoneUtils.Entity.Skeleton.Animation; 
public struct BonesAnimation {
	public required List<BoneAnimationKeyframe> Keyframes; 
	public required List<AnimationBlend> FrameBlends;
	public required float TotalDuration; // Complete duration of animation, in seconds
	public required BonesAnimationType Type; // should transforms be treated as absolute, or relative to current position (canned anim. vs. procedural)
}
public enum BonesAnimationType {
	Static,
	Relative,
	Custom // prep for IoC 
}
public struct BoneAnimationKeyframe {
	public required BoneNode Bone;
	public required Transform TransformState;
	public required float TimelinePosition; // Relative to TotalDuration, 0 = at the beginning of the animation, = Duration at the end
}
public struct AnimationBlend {
	public required AnimationBlendType BlendType;

	public required int OriginIndex; // ref to BoneAnimationKeyframe
	public required int TargetIndex; // ref to BoneAnimationKeyframe

	public required float BlendFactor; // use blendfactor as scalar/exponent/log for linear/exponential/log movement s=vt => v=v*x, v^x, logx(v) etc
	public required float TimeFactor; // Slow motion essentially. s=vt => t = t*TimeFactor
	public required float Time; // how long should this take to complete
}
public enum AnimationBlendType {
	None,
	Linear,
	Exponential,
	Logarithmic,
	Oscillating,
	Custom // prep for IoC
}

