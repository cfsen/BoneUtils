using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoneUtils.Entity.Skeleton.Animation; 
public class AnimationBuilder {
	public AnimationContainer BonesAnimation;

	public AnimationBuilder() {
		BonesAnimation = new AnimationContainer{
			Keyframes = [],
			FrameBlends = [],
			TotalDuration = 0,
			Type = AnimationType.Static,
		};
	}

	// Sequence builders

	// Primary method for adding a pair of keyframes with a blend
	// setting AnimationBlend is intended for advanced use
	public bool AddSequence(AnimationKeyframe origin, AnimationKeyframe target, AnimationBlendType blendType, AnimationBlend? originToTarget = null) {
		int originIndex = BonesAnimation.Keyframes.Count;
		int targetIndex = originIndex + 1;

		// Add keyframes
		BonesAnimation.Keyframes.Add(origin);
		BonesAnimation.Keyframes.Add(target);

		AnimationBlend? blend;
		if(originToTarget == null) {
			blend = CreateBlendSegment(blendType, originIndex, targetIndex);
			if(blend == null) {
				// Failed to create blend, clean up inserted keyframes
				BonesAnimation.Keyframes.RemoveAt(targetIndex);
				BonesAnimation.Keyframes.RemoveAt(originIndex);
				return false;
			}
		}
		else
			blend = originToTarget;
		BonesAnimation.FrameBlends.Add(blend.Value);

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
		
		var blendTime = BonesAnimation.Keyframes[targetIndex].TimelinePosition - BonesAnimation.Keyframes[originIndex].TimelinePosition;
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
			BonesAnimation.Keyframes.RemoveAt(keyframeIndex);
			return true;
		}

		var (hasBlends, deleteFrom, blendList) = RebuildBlendFrames(keyframeIndex);

		if(!hasBlends || deleteFrom == null || blendList == null)
			return false; // TODO failure at this time indicates state corruption, maybe throw instead of returning false

		// Reconstruct blendframes and remove keyframe
		BonesAnimation.FrameBlends.RemoveRange(deleteFrom.Value, 
			BonesAnimation.FrameBlends.Count - deleteFrom.Value);
		BonesAnimation.FrameBlends.AddRange(blendList);
		BonesAnimation.Keyframes.RemoveAt(keyframeIndex);
		return true;
	}
	private (bool hasBlends, int? delFromIdx, List<AnimationBlend>? rebuiltFrames) RebuildBlendFrames(int keyframeIndex) {
		// Find index of associated blendframe
		int? assocBlendsFromIndex = null;
		for(int i = 0; i < BonesAnimation.FrameBlends.Count; i++) {
			if(BonesAnimation.FrameBlends[i].OriginIndex == keyframeIndex) { 
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
		for(int i = 0; i < BonesAnimation.FrameBlends.Count; i++) {
			if(BonesAnimation.FrameBlends[i].OriginIndex > keyframeIndex) {
				// Any frame after the deleted index, need to have both their origin and target index reduced
				tmp = CreateBlendSegment(
					BonesAnimation.FrameBlends[i].BlendType, 
					BonesAnimation.FrameBlends[i].OriginIndex-1, 
					BonesAnimation.FrameBlends[i].TargetIndex-1
					);

				if(tmp == null) return (false, null, null); 

				blendList.Add(tmp.Value);
			}
		}

		return (true, assocBlendsFromIndex, blendList);
	}

	// Export

	public AnimationContainer Export() => BonesAnimation;

	// State checkers

	private bool CheckKeyframeHasBlends(int i) 
		=> BonesAnimation.FrameBlends.Any(x => x.OriginIndex == i || x.TargetIndex == i);
	private bool CheckKeyframeExists(int i) 
		=> (i >= 0 && BonesAnimation.Keyframes.Count > i);
	public bool Verify() {
		return false;
	}
}
