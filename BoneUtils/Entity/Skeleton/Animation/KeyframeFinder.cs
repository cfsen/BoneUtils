namespace BoneUtils.Entity.Skeleton.Animation; 
public class KeyframeFinder {
	public static (bool valid, BoneNode? node, TransformSnapshot? state) GetKeyframe(float runTime, AnimationInstance inst) { 
		//Debug.WriteLine(inst.ToString());
		if(runTime > inst.Animation.TotalDuration && !inst.Loop) 
			return (false, null, null); 

		// If xfmtype is static, transforms are set directly, this will loop without reversing 
		if(inst.Loop && runTime > inst.Animation.TotalDuration && inst.Animation.Type == AnimationXfmType.Static) 
			runTime %= inst.Animation.TotalDuration; // Wrap time around if looping

		// TODO this prevents relative animations from looping until dedicated logic can be implemented
		// If xfmtype is relative, transforms are set and propagated by bonenode translate/rotate
		if(inst.Loop && runTime > inst.Animation.TotalDuration && inst.Animation.Type == AnimationXfmType.Relative)
			return (false, null, null);

		// Fetch frames
		var (valid, origin, target) = GetKeyframes_FromLastHit(runTime, inst); // sequential lookup
		if(!valid) (valid, origin, target) = GetKeyframes_HybridSearch(runTime, inst); // interpolated binary search
		if(!valid) (valid, origin, target) = GetKeyframes_Linear(runTime, inst); // last resort linear search
		if(!valid) return (false, null, null);
		
		inst.LastOrigin = origin;
		inst.LastTarget = target;

		// TODO blend and return current state
		//Debug.WriteLine($"""
		//	runTime={runTime}
		//	LastOrigin={inst.LastOrigin} | LastTarget={inst.LastTarget}
		//	origin = {origin} | target = {target}
		//	""");

		// Just return whichever frame is active for now
		return (true, inst.Animation.Keyframes[origin].Bone, inst.Animation.Keyframes[origin].TransformState);
	}

	private static (bool valid, int origin, int target) GetKeyframes_FromLastHit(float runTime, AnimationInstance inst) {
		//Debug.WriteLine("FromLastHit");
		if(inst.LastOrigin < 0 || inst.LastOrigin >= inst.KeyframeCount) return (false, -1, -1);
		if(inst.LastTarget < 0 || inst.LastTarget >= inst.KeyframeCount) return (false, -1, -1);

		if(inst.Animation.Keyframes[inst.LastOrigin].TimelinePosition <= runTime 
			&& inst.Animation.Keyframes[inst.LastTarget].TimelinePosition > runTime)
			return (true, inst.LastOrigin, inst.LastTarget);
		if(inst.Animation.Keyframes[inst.LastTarget].TimelinePosition <= runTime 
			&& inst.Animation.Keyframes[int.Min(inst.LastTarget+1, inst.KeyframeCount-1)].TimelinePosition > runTime)
			return (true, inst.LastTarget, inst.LastTarget+1);

		return (false, -1, -1);
	}

	private static (bool valid, int origin, int target) GetKeyframes_Linear(float runTime, AnimationInstance inst) {
		//Debug.WriteLine("Linear");
		int i = inst.LastLookupTime < runTime ? inst.LastLookupKeyframe : 1;
		inst.LastLookupTime = runTime;

		for (int j = i; j < inst.KeyframeCount; j++)
			if (inst.Animation.Keyframes[j].TimelinePosition > runTime) {
				inst.LastLookupKeyframe = j;
				return (true, j-1, j);
			}
		return (false, -1, -1);
	}
	private static (bool valid, int origin, int target) GetKeyframes_HybridSearch(float runTime, AnimationInstance inst) {
		//Debug.WriteLine("Hybrid");
		// Handle edge cases
		if(inst.KeyframeCount == 2)
			return (true, 0, 1);
		if(inst.KeyframeCount < 2)
			return (false, -1, -1);
		if(runTime == inst.Animation.TotalDuration)
			return (true, inst.KeyframeCount-2, inst.KeyframeCount-1);

		// hybrid interpolation-binary search
		// attempt to estimate index
		int i = (int)MathF.Round((runTime/inst.Animation.TotalDuration)*inst.KeyframeCount);
		int keyframesRemaining = inst.KeyframeCount;

		while (keyframesRemaining >= 2) {
			if(inst.Animation.Keyframes[i].TimelinePosition == runTime) {
				// edge case: when i == inst.KeyframeCount-1 | wrap back to beginning
				if(i == inst.KeyframeCount-1)
					return (true, i, 0);
				// exact match
				return (true, i, i+1);
			}
			else if(inst.Animation.Keyframes[i].TimelinePosition > runTime) {
				if(inst.Animation.Keyframes[int.Max(0, i-1)].TimelinePosition <= runTime) { // use Max to stay inside bounds
					// edge case: when i == 0
					if(i == 0)
						return (true, inst.KeyframeCount-1, i);
					// match: i + 1 < runTime < i
					return(true, i-1, i);
				}
				else {
					// expand search, match is earlier in timeline
					keyframesRemaining -= 2;
					i = (int)MathF.Round((i-2)/2);
				}
			}
			else if(inst.Animation.Keyframes[i].TimelinePosition < runTime) { 
				if(inst.Animation.Keyframes[int.Min(inst.KeyframeCount, i+1)].TimelinePosition > runTime) { // use Min to stay inside bounds
					// TODO is this desired?
					// edge case: when i == inst.KeyframeCount-1
					if(i == inst.KeyframeCount-1)
						return (true, i, 0); // link last frame to first frame

					// match i < runTime < i + 1
					return (true, i, i+1);
				}
				else {
					// expand search, match is later in timeline
					keyframesRemaining -= 2;
					i = (int)MathF.Round((inst.KeyframeCount-i-1)/2)+i;
				}
			}
		}
		return (false, -1, -1);
	}
}
