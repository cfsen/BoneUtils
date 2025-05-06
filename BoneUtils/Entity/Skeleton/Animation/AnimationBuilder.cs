using System;
using System.Collections.Generic;
using System.Linq;
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

	// Sequence builders

	// Primary method for adding a pair of keyframes with a blend
	// setting AnimationBlend is intended for advanced use
	public bool AddSequence(AnimationKeyframe origin, AnimationKeyframe target, AnimationBlendType blendType, AnimationBlend? originToTarget = null) {
		int originIndex = Keyframes.Count;
		int targetIndex = originIndex + 1;

		// Add keyframes
		Keyframes.Add(origin);
		Keyframes.Add(target);

		AnimationBlend? blend;
		if(originToTarget == null) {
			blend = CreateBlendSegment(blendType, originIndex, targetIndex);
			if(blend == null) {
				// Failed to create blend, clean up inserted keyframes
				Keyframes.RemoveAt(targetIndex);
				Keyframes.RemoveAt(originIndex);
				return false;
			}
		}
		else
			blend = originToTarget;
		FrameBlends.Add(blend.Value);

		return true;
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
		if(XfmType == AnimationXfmType.None)
			return (false, "Animation transform type (XfmType) is None, must be set.");

		if(Keyframes.Count < 2) 
			return (false, "AnimationContainer must have at least 2 keyframes.");

		if(FrameBlends.Count < Keyframes.Count - 1)
			return (false, "Missing blend frames, count should be Keyframes.Count-1");
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
