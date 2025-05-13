using BoneUtils.Entity.Skeleton;
using BoneUtils.Entity.Skeleton.Animation;
using BoneUtils.Math;
using Raylib_cs;
using System.Numerics;

namespace BoneUtils.RayLib.RayLibDemos;
public class DemoAnimatorBasic :DemoBase {
	private const string text_Title = "Keyframe animation (proof of concept)";

	private RaylibRenderer Renderer;

	private SkeletonEntity Spine;
	private SkeletonEntityOps SkelOps;

	public DemoAnimatorBasic(SkeletonEntityOps skeops, RaylibRenderer renderer) {
		SkelOps = skeops;
		Spine = ConstructSkeleton();
		Renderer = renderer;
		renderMode = RenderMode.Fancy;
	}
	public override void Draw3D() {
		Render(Spine, Renderer);
	}
	public override void Draw2D() {
		Raylib.DrawText(text_Title, 10, 50, 20, Color.Red);
	}
	public override void HandleDemoInput() {
		//if(Raylib.IsKeyPressed(KeyboardKey.One))
	}
	public override void Update(float deltaTime) {
		Spine.Animator?.Play(deltaTime);
	}
	private SkeletonEntity ConstructSkeleton() { 
		var sken = Mock_Spine();
		SkelOps.PreProcessSkeleton(ref sken, [
			SkelOps.ValidateBoneNodeTree,
			SkelOps.BoneNodeTreeCalculateConstraints,
			SkelOps.BoneNodeTreeBuildRenderLists,
			SkelOps.AddSkeletonAnimator
			]);

		// Animate all bones in the simple mockup spine
		int offset = 0;
		foreach(var node in sken.Bones) {
			// Create an animation
			var animation = CreateBasicAnimation(sken, node.Value, offset);

			// Set up the animation owner (keyframe selection, blending)
			var animationInstance = new AnimationInstance(animation);

			// Load the animation into the manager
			// Can safely assume not null due to AddSkeletonAnimator mutator
			sken.Animator!.Load(animationInstance);
			offset++;
		}

		return sken;
	}
	private AnimationContainer CreateBasicAnimation(SkeletonEntity sken, BoneNode node, int offset) {
		// Define a translation rotating around the xz plane
		Vector3[] translation = [
			new(2,offset,0),
			new(0,offset,2),
			new(-2,offset,0),
			new(0,offset,-2)
			];

		// Creating an array of shuffled indexes
		// Lets us reuse translation for all bones, while staggering their motion
		int[] j = new int[4];
		if(offset == 0) {
			// Baseline access of translation[]
			j = [0,1,2,3];
		} 
		else {
			// Populates j[] with 1,2,3,0 / 2,3,0,1 / etc
			for(int i = 0; i < 4; i++) {
				j[i] = (i + offset) % 4;
			}
		}

		// Set up transforms to target
		TransformSnapshot xfm0 = new(node.Transform);
		TransformSnapshot xfm1 = xfm0 with { Position = translation[j[0]] };
		TransformSnapshot xfm2 = xfm0 with { Position = translation[j[1]] };
		TransformSnapshot xfm3 = xfm0 with { Position = translation[j[2]] };
		TransformSnapshot xfm4 = xfm0 with { Position = translation[j[3]] };

		// Set up keyframes for transforms 
		AnimationKeyframe key1 = AnimationKeyframe.Create(node, xfm1, 0.0f);
		AnimationKeyframe key2 = AnimationKeyframe.Create(node, xfm2, 1.0f);
		AnimationKeyframe key3 = AnimationKeyframe.Create(node, xfm3, 2.0f);
		AnimationKeyframe key4 = AnimationKeyframe.Create(node, xfm4, 3.0f);
		// The final frame is set at the same position as the intial frame, creating a smooth loop
		AnimationKeyframe key5 = AnimationKeyframe.Create(node, xfm1, 4.0f);

		// Build the sequence
		AnimationBuilder builder = new() {
			XfmType = AnimationXfmType.Static
		};

		builder.StartSequence(key1, key2, AnimationBlendType.Linear);
		builder.BuildSequence(key3, AnimationBlendType.Linear);
		builder.BuildSequence(key4, AnimationBlendType.Linear);
		builder.BuildSequence(key5, AnimationBlendType.Linear);

		builder.EndSequence();

		return builder.Export();
	}
}
