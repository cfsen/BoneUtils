using BoneUtils.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BoneUtils.Entity.Skeleton.Animation;

/// <summary>
/// Represents an animation sequence for bones, containing keyframes and blend information.
/// </summary>
public struct AnimationContainer {
	public required List<AnimationKeyframe> Keyframes; 
	public required List<AnimationBlend> FrameBlends;
	public required float TotalDuration; // Complete duration of animation, in seconds
	public required AnimationType Type; // should transforms be treated as absolute, or relative to current position (canned anim. vs. procedural)
}

/// <summary>
/// Defines the animation method for BonesAnimation.
/// </summary>
public enum AnimationType {
	Static,
	Relative,
	Custom // prep for IoC 
}

/// <summary>
/// Defines a keyframe in an animation, containing the bone, transformation state, and time position.
/// </summary>
public readonly struct AnimationKeyframe {
	public readonly required BoneNode Bone { get; init; }
	public readonly required TransformSnapshot TransformState { get; init; }
	public readonly required float TimelinePosition { get; init; } // Relative to TotalDuration, 0 = at the beginning of the animation, = Duration at the end
}

/// <summary>
/// Defines a blend between two keyframes, with various types of blending.
/// </summary>
public readonly struct AnimationBlend {
	public readonly required AnimationBlendType BlendType { get; init; }

	// Keyframe indexes in AnimationContainer.Keyframes
	public readonly required int OriginIndex { get; init; }
	public readonly required int TargetIndex { get; init; }

	// use blendfactor as scalar/exponent/log for linear/exponential/log movement s=vt => v=v*x, v^x, logx(v) etc
	public readonly required float BlendFactor { get; init; } 

	// Slow motion essentially. s=vt => t = t*TimeFactor
	public readonly required float TimeFactor { get; init; } 

	// how long should this take to complete
	public readonly required float Time { get; init; } 
}

/// <summary>
/// Defines the different types of blending strategies between keyframes.
/// </summary>
public enum AnimationBlendType {
	None,
	Linear,
	Exponential,
	Logarithmic,
	Oscillating,
	Custom // prep for IoC
}

/// <summary>
/// Immutable struct for animation transforms
/// </summary>
public readonly struct TransformSnapshot {
    public readonly Vector3 Position { get; init; }
    public readonly Quat Rotation { get; init; }
    public readonly Vector3 Scale { get; init; }

	public TransformSnapshot(Transform xfm) {
		Position = xfm.Position;
		Rotation = xfm.Rotation;
		Scale = xfm.Scale;
	}
	public static implicit operator TransformSnapshot(Transform xfm) => new(xfm);
}


