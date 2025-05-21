using BoneUtils.Helpers;
using BoneUtils.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BoneUtils.Entity.Skeleton.Animation; 
public enum Axis {
	X, Y, Z
}
public class AnimationSimpleBuilder {
	private AnimationBuilder animationBuilder;

	// internal logic
	private List<(AnimationKeyframe key, AnimationBlendType blend)> bufferKeyframes = [];
	private AnimationXfmType xfmType;
	private BoneNode boneNode;
	private AnimationKeyframe? initialState;

	/// <summary>
	/// Simplified wrapper of AnimationBuilder.
	/// Currently supports RotatePropagate and TranslatePropagate.
	/// </summary>
	/// <param name="bone">BoneNode to animate</param>
	/// <param name="xfmType">Type of animation to build</param>
	public AnimationSimpleBuilder (BoneNode bone, AnimationXfmType xfmType) {
		this.xfmType = xfmType;
		this.boneNode = bone;
		this.animationBuilder = new(){ 
			XfmType = xfmType,	
		};
	}

	/// <summary>
	/// Captures the state of the transform at animation composition time.
	/// Must be called before ApplyInitial().
	/// </summary>
	/// <returns>The current builder to allow chaining additional bone rotation keyframes (see <see cref="AnimationSimpleBuilder"/>).</returns>
	public AnimationSimpleBuilder CaptureInitial() {
		TransformSnapshot xfm = new(boneNode.Transform);
		AnimationKeyframe key = AnimationKeyframe.Create(boneNode, xfm, 0.0f);
		initialState = key;

		return this;
	}

	/// <summary>
	/// Creates a keyframe with transform data stored by CaptureInitial().
	/// </summary>
	/// <param name="time">Timeline position of keyframe</param>
	/// <param name="blendType">Blend type for keyframe</param>
	/// <returns>The current builder to allow chaining additional bone rotation keyframes (see <see cref="AnimationSimpleBuilder"/>).</returns>
	public AnimationSimpleBuilder ApplyInitial(float time, AnimationBlendType blendType = AnimationBlendType.Linear) {
		if(initialState != null) {
			AnimationKeyframe updateTime = initialState.Value with {
				TimelinePosition = time,
			};
			bufferKeyframes.Add((updateTime, blendType)); 
		}

		return this;
	}

	/// <summary>
	/// Rotates a BoneNode in its local space.
	/// </summary>
	/// <param name="degrees">Degrees (euler) to rotate</param>
	/// <param name="axis">Axis to rotate around</param>
	/// <param name="time">Timeline position of keyframe</param>
	/// <param name="blendType">Blend type for keyframe</param>
	/// <returns>The current builder to allow chaining additional bone rotation keyframes (see <see cref="AnimationSimpleBuilder"/>).</returns>
	public AnimationSimpleBuilder Rotate(float degrees, Axis axis, float time, AnimationBlendType blendType = AnimationBlendType.Linear) {
		if(xfmType != AnimationXfmType.RotatePropagate) return this;

		Quat q = Quat.Create(MathHelper.DegToRad(degrees), AxisResolve(axis));
		TransformSnapshot xfm = new(boneNode.Transform){ Rotation=q };
		AnimationKeyframe key = AnimationKeyframe.Create(boneNode, xfm, time);
		bufferKeyframes.Add((key, blendType));

		return this;
	}

	/// <summary>
	/// Translates a BoneNode in its local space.
	/// </summary>
	/// <param name="units">Units to translate</param>
	/// <param name="axis">Axis to translate on</param>
	/// <param name="time">Timeline position of keyframe</param>
	/// <param name="blendType">Blend type for keyframe</param>
	/// <returns>The current builder to allow chaining additional bone rotation keyframes (see <see cref="AnimationSimpleBuilder"/>).</returns>
	public AnimationSimpleBuilder Translate(float units, Axis axis, float time, AnimationBlendType blendType = AnimationBlendType.Linear) {
		if(xfmType != AnimationXfmType.TranslatePropagate) return this;

		Vector3 translate = AxisTranslate(units, axis);
		TransformSnapshot xfm = new(boneNode.Transform){ Position=translate };
		AnimationKeyframe key = AnimationKeyframe.Create(boneNode, xfm, time);
		bufferKeyframes.Add((key, blendType));

		return this;
	}

	/// <summary>
	/// Finalizes the animation, passing buffered keyframes to AnimationBuilder.
	/// An animation must contain at least two keyframes.
	/// </summary>
	/// <returns><see cref="AnimationInstance"/> ready to load into <see cref="SkeletonAnimator"/></returns>
	/// <exception cref="InvalidOperationException">Thrown if less than two keyframes are defined.</exception>
	public AnimationInstance Finish() {
		if(!BuilderCheckAndStartSequence())
			throw new InvalidOperationException("Failed to start sequence!");
		BuilderCompleteSequence();
		animationBuilder.EndSequence();

		return new AnimationInstance(animationBuilder.Export());
	}
	private bool BuilderCheckAndStartSequence() {
		if (bufferKeyframes.Count < 2) return false;

		animationBuilder.StartSequence(bufferKeyframes[0].key, bufferKeyframes[1].key, bufferKeyframes[0].blend);
		return true;
	}
	private void BuilderCompleteSequence() {
		if(bufferKeyframes.Count <= 2) return; 
		
		for (int i = 2; i < bufferKeyframes.Count; i++) 
			animationBuilder.BuildSequence(bufferKeyframes[i].key, bufferKeyframes[i].blend);
	}
	private Vector3 AxisResolve(Axis axis) {
		return axis switch {
			Axis.X => Vector3.UnitX,
			Axis.Y => Vector3.UnitY,
			Axis.Z => Vector3.UnitZ,
			_ => Vector3.UnitX
		};
	}
	private Vector3 AxisTranslate(float units, Axis axis) {
		return axis switch {
			Axis.X => new Vector3(units, 0, 0),
			Axis.Y => new Vector3(0, units, 0),
			Axis.Z => new Vector3(0, 0, units),
			_ => Vector3.Zero
		};
	}
}
