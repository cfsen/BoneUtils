using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BoneUtils.Entity.Skeleton.Animation; 
// Orchestrates animations
public class SkeletonAnimator {

	public required SkeletonEntity Skeleton;
	public KeyframeFinder KeyframeFinder;
	public List<AnimationInstance> Animations { get; private set; } = [];

	// Animations state
	private int animationCount = 0;
	public bool Running = false;
	public float Runtime { get; private set; }= 0;

	// Hoisted from Play() to reduce allocations
	private bool _valid;
	private BoneNode? _node;
	private TransformSnapshot? _xfm;

	public SkeletonAnimator() {
		KeyframeFinder = new KeyframeFinder();
	}

	public int LoadedAnimations => animationCount;

	// Animation loading/unloading

	public void Load(AnimationInstance animation) {
		// TODO validate animation
		// TODO delegate assignment

		Animations.Add(animation);
		animationCount = Animations.Count;
		Debug.WriteLine($"animationCount={animationCount}");
	}
	public void Unload(AnimationInstance animation) {
		// TODO pre-remove cleanup?
		var instanceSelect = Animations.FirstOrDefault(x => x == animation);
		if(instanceSelect == null) return; // TODO handle better
		Animations.Remove(instanceSelect);
	}
	public void Clear() {
		Animations.Clear();
		animationCount = 0;
	}

	// Playback

	//TransformSnapshot? _dbgLastXfm;
	public void Scrub(float timelinePoint, KeyframeTransformer? xfmHandler = null) {
		xfmHandler ??= KeyframeXfmHandlers.KeyframeTransformerBasic;
		
		for(int i = 0; i < animationCount; i++) {
			if(!Animations[i].IsRunning && Running) {
				Animations[i].deltaTimeStarted = timelinePoint;
				Animations[i].IsRunning = true;
			}

			//Debug.WriteLine("---");
			//Debug.WriteLine($"Runtime: {Runtime}");

			(_valid, _node, _xfm) = KeyframeFinder.GetKeyframe(timelinePoint-Animations[i].deltaTimeStarted, Animations[i]);

			//Debug.WriteLine($"{_valid}, {_node}, {_xfm}");
			
			if (_valid && _node != null && _xfm.HasValue) {
				xfmHandler(_node, _xfm.Value, Animations[i].Animation.Type);

				//_dbgLastXfm ??= _xfm;
				//if (_dbgLastXfm.Value.Position != _xfm.Value.Position) {
				//	Debug.WriteLine($"Swapped nodes at global runtime: {timelinePoint}");
				//	_dbgLastXfm = _xfm;
				//}
			}
		}
	}

	public void Play(float deltaTime, KeyframeTransformer? xfmHandler = null){
		if(!Running) Running = !Running;
		Runtime += deltaTime;

		Scrub(Runtime, xfmHandler);
	}
}
