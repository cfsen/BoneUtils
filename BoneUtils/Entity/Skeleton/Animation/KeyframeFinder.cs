using System.Diagnostics;

namespace BoneUtils.Entity.Skeleton.Animation; 
public class KeyframeFinder {
	/// <summary>
	/// Searches the keyframes of an AnimationInstance at a given time,
	/// returning a transform for the bone to animate.
	/// </summary>
	/// <param name="runTime"></param>
	/// <param name="inst"></param>
	/// <returns>true, node to transform, transform state at runTime, or false, null, null</returns>
	public static (bool valid, BoneNode? node, TransformSnapshot? state) GetKeyframe(float runTime, AnimationInstance inst, xfmSnapshotBlender? blender = null) { 
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

		// Blending
		blender ??= GetBlendMode(inst, origin, target);

		float timeBetweenFrames = inst.Animation.Keyframes[target].TimelinePosition - inst.Animation.Keyframes[origin].TimelinePosition;
		float runTimeBetweenFrames = runTime - inst.Animation.Keyframes[origin].TimelinePosition;
		float normalizedTime = runTimeBetweenFrames / timeBetweenFrames;

		var blendedFrame = blender(
			inst.Animation.Keyframes[origin].TransformState, 
			inst.Animation.Keyframes[target].TransformState, 
			normalizedTime);

		return (true, inst.Animation.Keyframes[origin].Bone, blendedFrame);
		// TODO blend and return current state
		//Debug.WriteLine($"""
		//	runTime={runTime}
		//	LastOrigin={inst.LastOrigin} | LastTarget={inst.LastTarget}
		//	origin = {origin} | target = {target}
		//	""");

		// Just return whichever frame is active for now
		//return (true, inst.Animation.Keyframes[origin].Bone, inst.Animation.Keyframes[origin].TransformState);
	}

	private static xfmSnapshotBlender GetBlendMode(AnimationInstance inst, int origin, int target) {
		AnimationBlend blending = inst.Animation.FrameBlends
			.Where(x => x.OriginIndex == origin && x.TargetIndex == target).FirstOrDefault();
		return blending.BlendType switch {
			AnimationBlendType.Linear => KeyframeBlendHandlers.BlendLinear,
			_ => KeyframeBlendHandlers.BlendNone
		};
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
			if(IsMatch(i, runTime)) {
				if(i == inst.KeyframeCount-1)	// edge case: when i == inst.KeyframeCount-1 
					return (true, i, 0);		// link last frame to first frame

				// exact match
				return (true, i, i+1);			// link to next frame
			}
			else if(IsCursorLate(i, runTime)) {
				// check the frame ahead of cursor
				if(PeekEarlierFrame(i, runTime)){
					if(i == 0)									// edge case: when i == 0
						return (true, inst.KeyframeCount-1, i); // link last frame to first frame

					// match: i + 1 < runTime < i
					return(true, i-1, i);						// link from frame ahead of cursor
				}
				// expand search, match is earlier in timeline
				else {
					keyframesRemaining -= 2;
					i = (int)MathF.Round((i-2)/2);
				}
			}
			else if(IsCursorEarly(i, runTime)) { 
				// check the frame after cursor
				if(PeekLaterFrame(i, runTime)) {
					if(i == inst.KeyframeCount-1)	// edge case: when i == inst.KeyframeCount-1
						return (true, i, 0);		// link last frame to first frame

					// match i < runTime < i + 1
					return (true, i, i+1);			// link cursor to next frame
				}
				// expand search, match is later in timeline
				else {
					keyframesRemaining -= 2;
					i = (int)MathF.Round((inst.KeyframeCount-i-1)/2)+i;
				}
			}
		}
		return (false, -1, -1);

		bool PeekEarlierFrame(int i, float runTime) 
			=> inst.Animation.Keyframes[int.Max(0, i-1)].TimelinePosition <= runTime;
		bool PeekLaterFrame(int i, float runTime)
			=> inst.Animation.Keyframes[int.Min(inst.KeyframeCount-1, i+1)].TimelinePosition > runTime;
		bool IsCursorEarly(int i, float runTime)
			=> inst.Animation.Keyframes[i].TimelinePosition < runTime;
		bool IsCursorLate(int i, float runTime)
			=> inst.Animation.Keyframes[i].TimelinePosition > runTime;
		bool IsMatch(int i, float runTime)
			=> inst.Animation.Keyframes[i].TimelinePosition == runTime;
	}
}
