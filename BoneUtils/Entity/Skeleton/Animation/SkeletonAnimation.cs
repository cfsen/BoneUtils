using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace BoneUtils.Entity.Skeleton.Animation; 
public class SkeletonAnimation {
	// Animation owner 

	public AnimationContainer Animation { get; private set; }
	public List<AnimationKeyframe> Keyframes { get; private set; }
	public float TotalDuration { get; private set; } // seconds
	public bool Loop = true;
	private int KeyframeCount = 0;

	// for sequential lookup optimization
	private float LastLookupTime = 0;
	private int LastLookupKeyframe = 1; 
	private int LastOrigin = -1;
	private int LastTarget = -1;

	// Debugging
	public bool _dbgEnable = true;

	public SkeletonAnimation(AnimationContainer animationContainer) {
		Animation = animationContainer;
		Keyframes = animationContainer.Keyframes;
		TotalDuration = animationContainer.TotalDuration;
		KeyframeCount = Keyframes.Count;
		if(KeyframeCount < 2)
			throw new Exception("Animations must have at least two keyframes."); // TODO find appropriate built in exception
	}
	public (bool valid, BoneNode? node, TransformSnapshot? state) GetKeyframe(float runTime) { 
		if(runTime > TotalDuration && !Loop) 
			return (false, null, null); 

		// If xfmtype is static, transforms are set directly, this will loop without reversing 
		if(Loop && runTime > TotalDuration && Animation.Type == AnimationXfmType.Static) 
			runTime %= TotalDuration; // Wrap time around if looping

		// TODO this prevents relative animations from looping until dedicated logic can be implemented
		// If xfmtype is relative, transforms are set and propagated by bonenode translate/rotate
		if(Loop && runTime > TotalDuration && Animation.Type == AnimationXfmType.Relative)
			return (false, null, null);

		// Fetch frames
		var (valid, origin, target) = GetKeyframes_FromLastHit(runTime); // sequential lookup
		if(!valid) (valid, origin, target) = GetKeyframes_HybridSearch(runTime); // interpolated binary search
		if(!valid) (valid, origin, target) = GetKeyframes_Linear(runTime); // last resort linear search
		if(!valid) return (false, null, null);
		
		LastOrigin = origin;
		LastTarget = target;

		if(_dbgEnable) Debug.WriteLine($"{runTime} : {origin} : {target}");

		// TODO blend and return current state

		// Just return whichever frame is active for now
		return (true, Keyframes[origin].Bone, Keyframes[origin].TransformState);
	}

	private (bool valid, int origin, int target) GetKeyframes_FromLastHit(float runTime) {
		if(LastOrigin < 0 || LastOrigin >= KeyframeCount) return (false, -1, -1);
		if(LastTarget < 0 || LastTarget >= KeyframeCount) return (false, -1, -1);

		if(Keyframes[LastOrigin].TimelinePosition <= runTime 
			&& Keyframes[LastTarget].TimelinePosition > runTime)
			return (true, LastOrigin, LastTarget);
		if(Keyframes[LastTarget].TimelinePosition <= runTime 
			&& Keyframes[int.Min(LastTarget+1, KeyframeCount-1)].TimelinePosition > runTime)
			return (true, LastTarget, LastTarget+1);

		if(_dbgEnable) Debug.WriteLine($"Failed at runtime: {runTime} (LastOrigin: {LastOrigin} | LastTarget: {LastTarget}");
		return (false, -1, -1);
	}

	private (bool valid, int origin, int target) GetKeyframes_Linear(float runTime) {
		int i = LastLookupTime < runTime ? LastLookupKeyframe : 1;
		LastLookupTime = runTime;

		for (int j = i; j < KeyframeCount; j++)
			if (Keyframes[j].TimelinePosition > runTime) {
				LastLookupKeyframe = j;
				return (true, j-1, j);
			}
		return (false, -1, -1);
	}
	private (bool valid, int origin, int target) GetKeyframes_HybridSearch(float runTime) {
		// Handle edge cases
		if(KeyframeCount == 2)
			return (true, 0, 1);
		if(KeyframeCount < 2)
			return (false, -1, -1);
		if(runTime == TotalDuration)
			return (true, KeyframeCount-2, KeyframeCount-1);

		// hybrid interpolation-binary search
		// attempt to estimate index
		int i = (int)MathF.Round((runTime/TotalDuration)*KeyframeCount);
		int keyframesRemaining = KeyframeCount;

		while (keyframesRemaining >= 2) {
			if(Keyframes[i].TimelinePosition == runTime) {
				// edge case: when i == KeyframeCount-1 | wrap back to beginning
				if(i == KeyframeCount-1)
					return (true, i, 0);
				// exact match
				return (true, i, i+1);
			}
			else if(Keyframes[i].TimelinePosition > runTime) {
				if(Keyframes[int.Max(0, i-1)].TimelinePosition <= runTime) { // use Max to stay inside bounds
					// edge case: when i == 0
					if(i == 0)
						return (true, KeyframeCount-1, i);
					// match: i + 1 < runTime < i
					return(true, i-1, i);
				}
				else {
					// expand search, match is earlier in timeline
					keyframesRemaining -= 2;
					i = (int)MathF.Round((i-2)/2);
				}
			}
			else if(Keyframes[i].TimelinePosition < runTime) { 
				if(Keyframes[int.Min(KeyframeCount, i+1)].TimelinePosition > runTime) { // use Min to stay inside bounds
					// TODO is this desired?
					// edge case: when i == KeyframeCount-1
					if(i == KeyframeCount-1)
						return (true, i, 0); // link last frame to first frame

					// match i < runTime < i + 1
					return (true, i, i+1);
				}
				else {
					// expand search, match is later in timeline
					keyframesRemaining -= 2;
					i = (int)MathF.Round((KeyframeCount-i-1)/2)+i;
				}
			}
		}
		return (false, -1, -1);
	}
}
