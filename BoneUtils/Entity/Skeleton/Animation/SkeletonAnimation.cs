using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace BoneUtils.Entity.Skeleton.Animation; 
public class SkeletonAnimation {
	// Animation owner 

	public AnimationContainer Animation;
	public List<AnimationKeyframe> Keyframes;

	public float TotalDuration; // seconds
	public bool Loop = true;

	private int KeyframeCount = 0;

	// for sequential lookup optimization
	private float LastLookupTime = 0;
	private int LastLookupKeyframe = 1; 
	private int LastOrigin = -1;
	private int LastTarget = -1;

	public SkeletonAnimation(AnimationContainer animationContainer) {
		Animation = animationContainer;
		Keyframes = animationContainer.Keyframes;
		TotalDuration = animationContainer.TotalDuration;
		KeyframeCount = Keyframes.Count;
		if(KeyframeCount < 2)
			throw new Exception("Animations must have at least two keyframes."); // TODO find appropriate built in exception
	}
	public (bool valid, BoneNode? node, TransformSnapshot? state) GetKeyframe(float runTime) { 
		if(runTime > TotalDuration && !Loop) return (false, null, null); 
		if(Loop && runTime > TotalDuration) runTime %= TotalDuration; // Wrap time around if looping

		// Fetch frames
		var (valid, origin, target) = CheckLastFrames(runTime);
		if(!valid) (valid, origin, target) = GetSequentialKeyframes(runTime);
		if(!valid) (valid, origin, target) = GetKeyframes(runTime);
		if(!valid) return (false, null, null);

		LastOrigin = origin;
		LastTarget = target;

		// TODO blend and return current state

		// Just return whichever frame is closer for now
		if(runTime-Keyframes[origin].TimelinePosition >= Keyframes[target].TimelinePosition-runTime)
			return (true, Keyframes[target].Bone, Keyframes[target].TransformState);
		return (true, Keyframes[origin].Bone, Keyframes[origin].TransformState);
	}

	private (bool valid, int origin, int target) CheckLastFrames(float runTime) {
		if(LastOrigin < 0 || LastOrigin >= KeyframeCount) return (false, -1, -1);
		if(LastTarget < 0 || LastTarget >= KeyframeCount) return (false, -1, -1);

		if(Keyframes[LastOrigin].TimelinePosition < runTime && Keyframes[LastTarget].TimelinePosition > runTime)
			return (true, LastOrigin, LastTarget);

		return (false, -1, -1);
	}

	private (bool valid, int origin, int target) GetSequentialKeyframes(float runTime) {
		int i = LastLookupTime < runTime ? LastLookupKeyframe : 1;
		LastLookupTime = runTime;

		for (int j = i; j < KeyframeCount; j++)
			if (Keyframes[j].TimelinePosition > runTime) {
				LastLookupKeyframe = j;
				return (true, j-1, j);
			}
		return (false, -1, -1);
	}
	private (bool valid, int origin, int target) GetKeyframes(float runTime) {
		int origin = -1, target = -1;
		for(int i = 0; i < KeyframeCount; i++) 
			if(Keyframes[i].TimelinePosition < runTime) 
				origin = i;

		for(int j = KeyframeCount - 1; j >= 0; j--)
			if(Keyframes[j].TimelinePosition > runTime)
				target = j;

		return (!(origin == -1 || target == -1), origin, target);
	}
}
