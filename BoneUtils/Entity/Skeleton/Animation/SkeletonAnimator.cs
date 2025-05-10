using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BoneUtils.Entity.Skeleton.Animation; 
public class SkeletonAnimator {
	// Manages animation state

	public required SkeletonEntity Skeleton;

	public List<AnimationInstance> Animations { get; private set; } = [];
	//public List<SkeletonAnimation> Animations { get; private set; } = [];
	private int animationCount = 0;

	public bool Running = false;
	public float Runtime { get; private set; }= 0;

	// Hoisted from Play() to reduce allocations
	private bool _valid;
	private BoneNode? _node;
	private TransformSnapshot? _xfm;

	public delegate void KeyframeTransformer(BoneNode bone, TransformSnapshot xfm, AnimationXfmType ac);

	public int LoadedAnimations => animationCount;

	// Logic structure

	public class AnimationInstance(SkeletonAnimation animation) {
		public SkeletonAnimation SkeletonAnimation = animation;
		public bool IsRunning = false;
		public float deltaTimeStarted = 0.0f;
		public delegate TransformSnapshot XfmBlender(TransformSnapshot xfmOrigin, TransformSnapshot xfmTarget);
	}

	// Animation loading/unloading

	public void Load(SkeletonAnimation animation) {
		// TODO validate animation
		// TODO delegate assignment

		Animations.Add(new(animation));
		animationCount = Animations.Count;
	}
	public void Unload(SkeletonAnimation animation) {
		// TODO pre-remove cleanup?
		var logicSelect = Animations.FirstOrDefault(x => x.SkeletonAnimation == animation);
		if(logicSelect == null) return;
		Animations.Remove(logicSelect);
	}
	public void Clear() {
		Animations.Clear();
		animationCount = 0;
	}

	// Playback

	//TransformSnapshot? _dbgLastXfm;
	public void Scrub(float timelinePoint, KeyframeTransformer? xfmHandler = null) {
		xfmHandler ??=KeyframeTransformerBasic;
		
		for(int i = 0; i < animationCount; i++) {
			if(!Animations[i].IsRunning && Running) {
				Animations[i].deltaTimeStarted = timelinePoint;
				Animations[i].IsRunning = true;
			}

			(_valid, _node, _xfm) = Animations[i].SkeletonAnimation.GetKeyframe(timelinePoint-Animations[i].deltaTimeStarted);
			
			if (_valid && _node != null && _xfm.HasValue) {
				xfmHandler(_node, _xfm.Value, Animations[i].SkeletonAnimation.Animation.Type);

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


	// TODO consider moving this out of animator
	/// <summary>
	/// Basic transformer for keyframes. Leverages built-ins for propagation.
	/// </summary>
	/// <param name="bone">BoneNode to mutate.</param>
	/// <param name="xfm">Transform to set.</param>
	public void KeyframeTransformerBasic(BoneNode bone, TransformSnapshot xfm, AnimationXfmType animType) {

		bone.Transform.Scale = xfm.Scale;

		if(animType == AnimationXfmType.Relative) {
			// Leverage BoneNode propagation
			bone.Rotate(xfm.Rotation);
			bone.Translate(xfm.Position);
		}
		else if(animType == AnimationXfmType.Static) {
			bone.Transform.Rotation = xfm.Rotation;
			bone.Transform.Position = xfm.Position;
		}
	}
}
