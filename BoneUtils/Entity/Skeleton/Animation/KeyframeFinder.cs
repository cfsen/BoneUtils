using System.Diagnostics;

namespace BoneUtils.Entity.Skeleton.Animation; 
/// <summary>
/// Finds keyframes in an AnimationInstance at an arbitraty runtime.
/// </summary>
public class KeyframeFinder {
	/// <summary>
	/// Searches the keyframes of an AnimationInstance at a given time,
	/// returning a transform for the bone to animate.
	/// </summary>
	/// <param name="runTime">runtime managed by SkeletonAnimator</param>
	/// <param name="inst">AnimationInstance to search</param>
	/// <returns>
	/// On success: true, node to transform, transform state at runTime. 
	/// On failure: false, null, null
	/// </returns>
	public static (bool valid, BoneNode? node, TransformSnapshot? state) GetKeyframe(float runTime, AnimationInstance inst, xfmSnapshotBlender? blender = null) { 
		// For non looping animations, return the last frame
		if(runTime > inst.Animation.TotalDuration && !inst.Loop) 
			return lastKeyframe();

		if(isAbsoluteAndLoop()) // transforms are set directly without accumulation
			runTime %= inst.Animation.TotalDuration; // Wrap time around if looping

		// TODO
		// reconsider if this feature is desired in the first place
		// relative (accumulated) transforms are hard to conceptualize and use
		// and might be better handled by simply transforming static animations
		// extend AnimationInstance to store initial position, and implement logic to restart the animation here
		if(isAccumulateAndLoop()) // transforms are set and propagated by bonenode translate/rotate
			return (false, null, null);

		// Fetch frames
		var (valid, origin, target) = GetKeyframes_FromLastHit(runTime, inst); // sequential lookup
		if(!valid) (valid, origin, target) = GetKeyframes_HybridSearch(runTime, inst); // interpolated binary search
		if(!valid) (valid, origin, target) = GetKeyframes_Linear(runTime, inst); // last resort linear search
		if(!valid) return (false, null, null);
		
		inst.LastOrigin = origin;
		inst.LastTarget = target;

		// Blending
		blender ??= GetAssignedBlendMode(inst, origin);

		return (
			true, 
			inst.Animation.Keyframes[origin].Bone, 
			blender(
				inst.Animation.Keyframes[origin].TransformState,
				inst.Animation.Keyframes[target].TransformState,
				CalculateKeyframeNormalizedTime(inst, runTime, origin, target)
				)
			);

		// Locals
		bool isAbsoluteAndLoop() 
			=> inst.Loop && runTime > inst.Animation.TotalDuration && inst.Animation.Type == AnimationXfmType.Static;
		bool isAccumulateAndLoop()
			=> inst.Loop && runTime > inst.Animation.TotalDuration && inst.Animation.Type == AnimationXfmType.Relative;
		(bool, BoneNode?, TransformSnapshot?) lastKeyframe() 
			=> (true, inst.Animation.Keyframes[inst.KeyframeCount-1].Bone, inst.Animation.Keyframes[inst.KeyframeCount-1].TransformState);
	}

	// Fetches blend handler for blend mode assigned at composition time.
	private static xfmSnapshotBlender GetAssignedBlendMode(AnimationInstance inst, int origin) {
		// Leverage FrameBlends & Keyframes being synchronized ordered lists
		// FrameBlends[origin] always maps to originIdx=Keyframes[origin] && targetIdx=Keyframes[origin+1]
		return inst.Animation.FrameBlends[origin].BlendType switch {
			AnimationBlendType.Linear => KeyframeBlendHandlers.BlendLinear,
			_ => KeyframeBlendHandlers.BlendNone
		};
	}
	// Calculates the normalized time between origin and target keyframe
	private static float CalculateKeyframeNormalizedTime(AnimationInstance inst, float runTime, int origin, int target) {
		return (runTime - inst.Animation.Keyframes[origin].TimelinePosition)
			/ (inst.Animation.Keyframes[target].TimelinePosition - inst.Animation.Keyframes[origin].TimelinePosition);
	}
	// Checks if the last frame lookup is still valid for passed runtime
	private static (bool valid, int origin, int target) GetKeyframes_FromLastHit(float runTime, AnimationInstance inst) {
		if(IsLastOriginOutOfBounds()) return (false, -1, -1);
		if(IsLastTargetOutOfBounds()) return (false, -1, -1);

		if(CanReuseLastOrigin())	return (true, inst.LastOrigin, inst.LastTarget);
		if(PeekNextFrame())			return (true, inst.LastTarget, inst.LastTarget+1);

		return (false, -1, -1);

		// Locals
		bool IsLastOriginOutOfBounds() 
			=> inst.LastOrigin < 0 || inst.LastOrigin >= inst.KeyframeCount;
		bool IsLastTargetOutOfBounds()
			=> inst.LastTarget < 0 || inst.LastTarget >= inst.KeyframeCount;
		bool CanReuseLastOrigin()
			=> inst.Animation.Keyframes[inst.LastOrigin].TimelinePosition <= runTime 
			&& inst.Animation.Keyframes[inst.LastTarget].TimelinePosition > runTime;
		bool PeekNextFrame()
			=> inst.Animation.Keyframes[inst.LastTarget].TimelinePosition <= runTime 
			&& inst.Animation.Keyframes[int.Min(inst.LastTarget+1, inst.KeyframeCount-1)].TimelinePosition > runTime;
	}
	// Searches for valid frames at passed runtime
	private static (bool valid, int origin, int target) GetKeyframes_HybridSearch(float runTime, AnimationInstance inst) {
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
				// edge case: when i == inst.KeyframeCount-1 
				if(i == inst.KeyframeCount-1)	
					return (true, i, 0);		// link last frame to first frame

				return (true, i, i+1);			// link to next frame | exact match
			}
			else if(IsCursorLate(i, runTime)) {
				if(PeekEarlierFrame(i, runTime)){
					// edge case: when i == 0
					if(i == 0)
						return (true, inst.KeyframeCount-1, i); // link last frame to first frame

					return(true, i-1, i);	// link from frame ahead of cursor | match: i + 1 < runTime < i
				}
				else {
					// expand search, match is earlier in timeline
					keyframesRemaining -= 2;
					i = (int)MathF.Round((i-2)/2);
				}
			}
			else if(IsCursorEarly(i, runTime)) { 
				if(PeekLaterFrame(i, runTime)) {
					// edge case: when i == inst.KeyframeCount-1
					if(i == inst.KeyframeCount-1)	
						return (true, i, 0);		// link last frame to first frame

					return (true, i, i+1);			// link cursor to next frame | match: i < runTime < i + 1
				}
				else {
					// expand search, match is later in timeline
					keyframesRemaining -= 2;
					i = (int)MathF.Round((inst.KeyframeCount-i-1)/2)+i;
				}
			}
		}
		return (false, -1, -1);

		// Locals
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
	// Linearly searches for keyframes at passed runtime
	private static (bool valid, int origin, int target) GetKeyframes_Linear(float runTime, AnimationInstance inst) {
		int i = inst.LastLookupTime < runTime ? inst.LastLookupKeyframe : 1;
		inst.LastLookupTime = runTime;

		for (int j = i; j < inst.KeyframeCount; j++)
			if (inst.Animation.Keyframes[j].TimelinePosition > runTime) {
				inst.LastLookupKeyframe = j;
				return (true, j-1, j);
			}
		return (false, -1, -1);
	}
}
