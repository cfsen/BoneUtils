using BoneUtils.Entity.Skeleton;
using BoneUtils.Entity.Skeleton.Animation;
using BoneUtils.Math;
using Raylib_cs;
using System.Numerics;
using System.Runtime.InteropServices;

namespace BoneUtils.RayLib.RayLibDemos;
public class DemoAnimatorBasic :DemoBase {
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
		Raylib.DrawText("Keyframe animation (Proof of concept)", 10, 50, 20, Color.Red);
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

		// Create an animation
		var animation = CreateBasicAnimation(sken);

		// Set up the animation owner (keyframe selection, blending)
		var animationInstance = new AnimationInstance(animation);

		// Load the animation into the manager
		// Can safely assume not null due to AddSkeletonAnimator mutator
		sken.Animator!.Load(animationInstance);

		return sken;
	}
	private AnimationContainer CreateBasicAnimation(SkeletonEntity sken) {

		Vector3[] translation = [
			new(2,0,0),
			new(0,0,2),
			new(-2,0,0),
			new(0,0,-2)
			];

		// Set up transforms to target
		TransformSnapshot xfm0 = new(sken.RootNode.Transform);
		TransformSnapshot xfm1 = xfm0 with { Position = translation[0] };
		TransformSnapshot xfm2 = xfm0 with { Position = translation[1] };
		TransformSnapshot xfm3 = xfm0 with { Position = translation[2] };
		TransformSnapshot xfm4 = xfm0 with { Position = translation[3] };

		// Set up keyframes for transforms 
		//AnimationKeyframe key0 = AnimationKeyframe.Create(sken.RootNode, xfm0, 0.0f);

		AnimationKeyframe key1 = AnimationKeyframe.Create(sken.RootNode, xfm1, 0.0f);
		AnimationKeyframe key2 = AnimationKeyframe.Create(sken.RootNode, xfm2, 1.0f);
		AnimationKeyframe key3 = AnimationKeyframe.Create(sken.RootNode, xfm3, 2.0f);
		AnimationKeyframe key4 = AnimationKeyframe.Create(sken.RootNode, xfm4, 3.0f);

		//AnimationKeyframe key5 = AnimationKeyframe.Create(sken.RootNode, xfm4, 4.0f);

		AnimationBuilder builder = new AnimationBuilder();
		builder.XfmType = AnimationXfmType.Static;

		builder.StartSequence(key1, key2, AnimationBlendType.Linear);
		builder.BuildSequence(key3, AnimationBlendType.Linear);
		builder.BuildSequence(key4, AnimationBlendType.Linear);

		//builder.BuildSequence(key5, AnimationBlendType.Linear);

		builder.EndSequence();

		return builder.Export();
	}
}
