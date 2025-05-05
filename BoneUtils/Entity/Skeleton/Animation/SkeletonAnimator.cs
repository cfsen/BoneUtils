using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoneUtils.Entity.Skeleton.Animation; 
public class SkeletonAnimator {
	// Manages animation state

	public required SkeletonEntity Skeleton;

	private List<SkeletonAnimation> animations = [];
	private int animationCount = 0;

	public bool Running = false;
	private float runtime = 0;

	public int LoadedAnimations => animationCount;

	// Animation loading/unloading

	public void Load(SkeletonAnimation animation) {
		// TODO validate animation

		animations.Add(animation);
		animationCount = animations.Count;
	}
	public void Unload(SkeletonAnimation animation) {
		// TODO pre-remove cleanup?
		animations.Remove(animation);
	}
	public void Clear() {
		animations.Clear();
		animationCount = 0;
	}

	// Playback

	public void Play(float deltaTime){
		if(!Running) Running = !Running;
		runtime += deltaTime;
		bool valid;
		BoneNode? node;
		Transform? xfm;
		for(int i = 0; i < animationCount; i++) {
			(valid, node, xfm) = animations[i].GetKeyframe(runtime);
			if (valid) {
				node!.Transform = xfm!;
			}
		}
	}
	public void Scrub(float timelinePoint) {

	}
}
