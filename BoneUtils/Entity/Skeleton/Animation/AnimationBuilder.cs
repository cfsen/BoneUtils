using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BoneUtils.Entity.Skeleton.Animation; 
/* 
 * TODO: Reevaluate this approach.
 * Using mutable classes for keyframe/blend staging and export to structs later is cleaner.
 */
public class AnimationBuilder {

	public List<AnimationKeyframe> Keyframes { get; private set; } = [];
	public List<AnimationBlend> FrameBlends { get; private set; }= [];
	public float TotalDuration { get; private set; } = 0;
	public AnimationXfmType XfmType { get; set; } = AnimationXfmType.None;

	public bool SequenceStarted { get; private set; } = false;
	public bool SequenceFinished { get; private set; } = false;
	// Sequence builders

	public void CreateTwoFrameAnimation() {
		// should start and finish the sequence by adding two keyframes, one blend frame
	}

	/// <summary>
	/// Starts an animation sequence with two keyframes and a blend frame
	/// </summary>
	/// <param name="firstFrame">First frame in sequence.</param>
	/// <param name="secondFrame">Second frame in sequence.</param>
	/// <param name="blendType">Blend from first to second frame.</param>
	/// <param name="originToTarget">Optional advanced blending.</param>
	/// <returns>false on failure to start sequence.</returns>
	public bool StartSequence(AnimationKeyframe firstFrame, AnimationKeyframe secondFrame, AnimationBlendType blendType, AnimationBlendType? blendPromise = null, AnimationBlend? originToTarget = null) {
		if(SequenceStarted || SequenceFinished) return false;
		SequenceStarted = true;
		AddSequence(firstFrame, secondFrame, blendType, blendPromise, originToTarget);
		return true;
	}

	/// <summary>
	/// Adds a keyframe and blend to the tail of the sequence
	/// </summary>
	/// <param name="firstFrame">Keyframe to add.</param>
	/// <param name="blendType">The blend from firstFrame to the next frame.</param>
	/// <param name="originToTarget">Optional advanced blending.</param>
	/// <returns>false on failure to add keyframe.</returns>
	public bool BuildSequence(AnimationKeyframe firstFrame, AnimationBlendType blendType, AnimationBlend? originToTarget = null) {
		if(SequenceFinished || !SequenceStarted) return false;
		var (origin, target) = GetKeyframeIndices();

		Keyframes.Add(firstFrame);

		CreatePromiseBlendSegment(blendType, origin, target, originToTarget);
		return true;
	}

	/// <summary>
	/// Add the final two keyframes and end the sequence.
	/// </summary>
	/// <param name="secondToLast">Second to last frame.</param>
	/// <param name="finalFrame">Final frame of sequence.</param>
	/// <param name="blendType">Blend type from second to last to final.</param>
	/// <param name="originToTarget">Optional advanced blending.</param>
	/// <returns>false on failure to finalize sequence.</returns>
	[Obsolete] // Will remove this in favor of always using param-free endsequence
	public bool EndSequence(AnimationKeyframe secondToLast, AnimationKeyframe finalFrame, AnimationBlendType blendType, AnimationBlend? originToTarget = null) {
		if(SequenceFinished || !SequenceStarted) return false;
		SequenceFinished = true;
		//AddSequence(secondToLast, finalFrame, blendType, originToTarget);
		return false;
	}

	/// <summary>
	/// Finishes the sequence without adding a pair of keyframes,
	/// removing the blendframe for the last keyframe.
	/// </summary>
	/// <returns>false on failure to finalize sequence.</returns>
	public bool EndSequence() {
		if(SequenceFinished || !SequenceStarted) return false;
		SequenceFinished = true;
		// Finalize sequence promise
		_sequenceHasBeenPromised = false;
		return true;
	}

	public bool AddSequence(AnimationKeyframe origin, AnimationKeyframe target, AnimationBlendType blendTypeOrigin, AnimationBlendType? blendPromiseTarget = null, AnimationBlend? originToTarget = null) {
		var (originIndex, targetIndex) = GetKeyframeIndices();
		if(_sequenceHasBeenPromised) {
			// fulfill open blend frame promise
			// TODO DRY repeated in createpromise
			var blendTime = Keyframes[originIndex].TimelinePosition - Keyframes[_sequencePromise.OriginIndex].TimelinePosition;

			AnimationBlend _fulfillment = _sequencePromise with { Time = blendTime};
			FrameBlends.Add(_fulfillment);
		}


		// Add keyframes
		Keyframes.Add(origin);
		Keyframes.Add(target);

		AnimationBlend? blend = GetBlendFrame(originIndex, targetIndex, blendTypeOrigin, originToTarget);
		if(blend == null) {
			// Failed to create blend, clean up inserted keyframes
			Keyframes.RemoveAt(targetIndex);
			Keyframes.RemoveAt(originIndex);
			return false;
		}
		FrameBlends.Add(blend.Value);

		// create blend frame promise

		CreatePromiseBlendSegment(blendPromiseTarget == null ? blendTypeOrigin : blendPromiseTarget.Value, targetIndex, targetIndex+1);

		return true;
	}

	// Sequence helpers
	private (int origin, int target) GetKeyframeIndices() => (Keyframes.Count, Keyframes.Count+1);
	private AnimationBlend? GetBlendFrame(int originIndex, int targetIndex, AnimationBlendType blendType, AnimationBlend? originToTarget = null) {
		AnimationBlend? blend;
		if(originToTarget == null) {
			blend = CreateBlendSegment(blendType, originIndex, targetIndex);
			if(blend == null)
				return null;
		}
		else
			blend = originToTarget;
		return blend.Value;
	}

	// Struct builders

	public AnimationKeyframe CreateKeyframe(BoneNode bone, Transform xfm, float timelinePosition) 
		=> CreateKeyframe(bone, xfm, timelinePosition);
	public AnimationKeyframe CreateKeyframe(BoneNode bone, TransformSnapshot xfm, float timelinePosition) {
		return new AnimationKeyframe {
			Bone = bone,
			TransformState = xfm, 
			TimelinePosition = timelinePosition,
		};
	}
	public AnimationBlend? CreateBlendSegment(AnimationBlendType blendType, int originIndex, int targetIndex) {
		if(!CheckKeyframeExists(originIndex) || !CheckKeyframeExists(targetIndex))
			return null;
		
		var blendTime = Keyframes[targetIndex].TimelinePosition - Keyframes[originIndex].TimelinePosition;
		if(blendTime <= 0) 
			return null;

		return new AnimationBlend {
			BlendType = blendType,
			OriginIndex = originIndex,
			TargetIndex = targetIndex,

			BlendFactor = 0.5f,
			TimeFactor = 1.0f,
			Time = blendTime
		};
	}
	private AnimationBlend _sequencePromise;
	private bool _sequenceHasBeenPromised = false;
	private void CreatePromiseBlendSegment(AnimationBlendType blendType, int originIndex, int promiseIndex, AnimationBlend? blend = null) {
		// TODO consider edge cases, this inherently has big sequence breaking potential.
		if(_sequenceHasBeenPromised) {
			// finalize sequence before adding the next promise
			var blendTime = Keyframes[originIndex].TimelinePosition - Keyframes[_sequencePromise.OriginIndex].TimelinePosition;

			AnimationBlend _fulfillment = _sequencePromise with { Time = blendTime};
			FrameBlends.Add(_fulfillment);
		}
		else {
			// initial promise
			_sequenceHasBeenPromised = true;
		}

		// use any passed blend frame
		if(blend != null) {
			_sequencePromise = blend.Value;
			return;
		}

		// create a promise, note that this promise will need to be fulfilled if AddSequence(frame0, frame1...) is called
		_sequencePromise = new AnimationBlend {
			BlendType = blendType,
			OriginIndex = originIndex,
			TargetIndex = promiseIndex,

			BlendFactor = 0.5f,
			TimeFactor= 1.0f,
			Time = -1.0f, // to be set on fulfilment
		};
	}

	// Deletion

	public bool DeleteKeyframe(int keyframeIndex) {
		if(!CheckKeyframeExists(keyframeIndex)) return false;

		if (!CheckKeyframeHasBlends(keyframeIndex)) {
			Keyframes.RemoveAt(keyframeIndex);
			return true;
		}

		var (hasBlends, deleteFrom, blendList) = RebuildBlendFrames(keyframeIndex);

		if(!hasBlends || deleteFrom == null || blendList == null)
			return false; // TODO failure at this time indicates state corruption, maybe throw instead of returning false

		// Reconstruct blendframes and remove keyframe
		FrameBlends.RemoveRange(deleteFrom.Value, 
			FrameBlends.Count - deleteFrom.Value);
		FrameBlends.AddRange(blendList);
		Keyframes.RemoveAt(keyframeIndex);
		return true;
	}
	private (bool hasBlends, int? delFromIdx, List<AnimationBlend>? rebuiltFrames) RebuildBlendFrames(int keyframeIndex) {
		// Find index of associated blendframe
		int? assocBlendsFromIndex = null;
		for(int i = 0; i < FrameBlends.Count; i++) {
			if(FrameBlends[i].OriginIndex == keyframeIndex) { 
				// Edge case: indices must be unique, finding another indicates state corruption
				if(assocBlendsFromIndex != null)
					return (false, null, null); 
				assocBlendsFromIndex = i;
			}
		}

		// Edge case: failed to find blend frame assosciated with keyframeIndex (Call CheckKeyframeHasBlends first)
		if(assocBlendsFromIndex == null)
			return (false, null, null); 

		// Rebuild following blend frames
		List<AnimationBlend> blendList = [];
		AnimationBlend? tmp = null;
		for(int i = 0; i < FrameBlends.Count; i++) {
			if(FrameBlends[i].OriginIndex > keyframeIndex) {
				// Any frame after the deleted index, need to have both their origin and target index reduced
				tmp = CreateBlendSegment(
					FrameBlends[i].BlendType, 
					FrameBlends[i].OriginIndex-1, 
					FrameBlends[i].TargetIndex-1
					);

				if(tmp == null) return (false, null, null); 

				blendList.Add(tmp.Value);
			}
		}

		return (true, assocBlendsFromIndex, blendList);
	}

	// Export
	private float CalculateDuration() {
		return Keyframes.Last().TimelinePosition;
	}
	public AnimationContainer Export() {
		TotalDuration = CalculateDuration();

		// Validate state before exporting
		var (valid, error) = ValidateAnimationContainer();
		if(!valid)
			throw new Exception(error); // TODO Throwing for now, consider allowing soft failure

		return new AnimationContainer {
			Keyframes = this.Keyframes,
			FrameBlends = this.FrameBlends,
			TotalDuration = CalculateDuration(),
			Type = this.XfmType
		};
	}

	// State checkers

	private bool CheckKeyframeHasBlends(int i) 
		=> FrameBlends.Any(x => x.OriginIndex == i || x.TargetIndex == i);
	private bool CheckKeyframeExists(int i) 
		=> (i >= 0 && Keyframes.Count > i);
	public (bool valid, string msg) ValidateAnimationContainer() {
		if(_sequenceHasBeenPromised)
			return (false, "Build sequence hasn't been completed. Missing EndSequence()?");
		if(XfmType == AnimationXfmType.None)
			return (false, "Animation transform type (XfmType) is None, must be set.");

		if(Keyframes.Count < 2) 
			return (false, "AnimationContainer must have at least 2 keyframes.");

		if(FrameBlends.Count < Keyframes.Count - 1)
			return (false, $"""
				Missing blend frames, count should be Keyframes.Count-1
				Blendframes expected: {Keyframes.Count-1}, is: {FrameBlends.Count}. 
				Keyframes: {Keyframes.Count}
				""");
		else if(FrameBlends.Count >= Keyframes.Count)
			return (false, "Too many blend frames, count should be Keyframes.Count-1");

		// Check if first keyframe is at the timeline beginning
		if(Keyframes.First().TimelinePosition != 0.0f) 
			return (false, $"Expected: 0.0f, is: {Keyframes.First().TimelinePosition}. First keyframe must start at 0.0");

		// Check if the last keyframe is at the timeline end
		if(Keyframes.Last().TimelinePosition != TotalDuration)
			return (false, $"Expected: {TotalDuration}, is: {Keyframes.Last().TimelinePosition}. Final keyframe must be equal to TotalDuration.");


		// Check that keyframes are an ordered list by TimelinePosition
		float timelinePos = 0.0f;
		for(int i = 1; i < Keyframes.Count; i++) {
			if(Keyframes[i].TimelinePosition > timelinePos) {
				timelinePos = Keyframes[i].TimelinePosition;
				continue;
			}
			else return (false, "Keyframes must be an ordered list by timeline.");
		}

		return (true, "AnimationContainer verified");
	}
}
