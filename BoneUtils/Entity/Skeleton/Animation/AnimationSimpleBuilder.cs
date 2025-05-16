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

	public AnimationSimpleBuilder (BoneNode bone, AnimationXfmType xfmType = AnimationXfmType.RotatePropagate) {
		this.xfmType = xfmType;
		this.boneNode = bone;
		this.animationBuilder = new(){ 
			XfmType = xfmType,	
		};
	}

	public AnimationSimpleBuilder CaptureInitial() {
		TransformSnapshot xfm = new(boneNode.Transform);
		AnimationKeyframe key = AnimationKeyframe.Create(boneNode, xfm, 0.0f);
		initialState = key;

		return this;
	}
	public AnimationSimpleBuilder ApplyInitial(float time, AnimationBlendType blendType = AnimationBlendType.Linear) {
		if(initialState != null) {
			AnimationKeyframe updateTime = initialState.Value with {
				TimelinePosition = time,
			};
			bufferKeyframes.Add((updateTime, blendType)); 
		}

		return this;
	}
	public AnimationSimpleBuilder Rotate(float degrees, Axis axis, float time, AnimationBlendType blendType = AnimationBlendType.Testing) {
		if(xfmType != AnimationXfmType.RotatePropagate) return this;

		Quat q = Quat.Create(MathHelper.DegToRad(degrees), axisResolve(axis));
		TransformSnapshot xfm = new(boneNode.Transform){ Rotation=q };
		AnimationKeyframe key = AnimationKeyframe.Create(boneNode, xfm, time);
		bufferKeyframes.Add((key, blendType));

		return this;
	}
	public AnimationSimpleBuilder Translate(float units, Axis axis, float time, AnimationBlendType blendType = AnimationBlendType.Linear) {
		if(xfmType != AnimationXfmType.TranslatePropagate) return this;

		Vector3 translate = axisTranslate(units, axis);
		TransformSnapshot xfm = new(boneNode.Transform){ Position=translate };
		AnimationKeyframe key = AnimationKeyframe.Create(boneNode, xfm, time);
		bufferKeyframes.Add((key, blendType));

		return this;
	}
	public AnimationContainer Finish() {
		if(!builderCheckAndStartSequence())
			throw new Exception("Failed to start sequence!");
		builderCompleteSequence();
		animationBuilder.EndSequence();
		return animationBuilder.Export();
	}
	private bool builderCheckAndStartSequence() {
		if (bufferKeyframes.Count < 2) return false;

		animationBuilder.StartSequence(bufferKeyframes[0].key, bufferKeyframes[1].key, bufferKeyframes[0].blend);
		return true;
	}
	private void builderCompleteSequence() {
		if(bufferKeyframes.Count <= 2) return; 
		
		for (int i = 2; i < bufferKeyframes.Count-1; i++) 
			animationBuilder.BuildSequence(bufferKeyframes[i].key, bufferKeyframes[i].blend);
	}
	private Vector3 axisResolve(Axis axis) {
		return axis switch {
			Axis.X => Vector3.UnitX,
			Axis.Y => Vector3.UnitY,
			Axis.Z => Vector3.UnitZ,
			_ => Vector3.UnitX
		};
	}
	private Vector3 axisTranslate(float units, Axis axis) {
		return axis switch {
			Axis.X => new Vector3(units, 0, 0),
			Axis.Y => new Vector3(0, units, 0),
			Axis.Z => new Vector3(0, 0, units),
			_ => Vector3.Zero
		};
	}
}
