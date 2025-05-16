using BoneUtils.Entity.Skeleton;
using BoneUtils.Entity.Skeleton.Animation;
using BoneUtils.Helpers;
using BoneUtils.Math;
using Raylib_cs;
using System.Diagnostics;
using System.Numerics;

namespace BoneUtils.RayLib.RayLibDemos;
public class DemoAnimationPropagate :DemoBase {
	private const string text_Title = "Keyframe animation with skeleton propagation (proof of concept)";

	private RaylibRenderer Renderer;

	private SkeletonEntity Spine;
	private SkeletonEntityOps SkelOps;

	public DemoAnimationPropagate(SkeletonEntityOps skeops, RaylibRenderer renderer) {
		SkelOps = skeops;
		Spine = ConstructSkeleton();
		Renderer = renderer;
		renderMode = RenderMode.Fancy ^ RenderMode.QuatOrientation;
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

		var translationContainer = CreateTranslation(sken, sken.RootNode);
		var rotation_spinea = CreateRotatePropagate(sken, sken.Bones["SpineA"]);

		// Set up the animation owner (keyframe selection, blending)
		AnimationInstance animTranslate = new(translationContainer) {
			Loop = true
		};

		AnimationInstance animRotate = new(rotation_spinea) {
			Loop = true 
		};

		// Load the animation into the manager
		// Can safely assume not null due to AddSkeletonAnimator mutator
		//sken.Animator!.Load(animTranslate);
		sken.Animator!.Load(animRotate);

		return sken;
	}
	private AnimationContainer CreateRotatePropagate(SkeletonEntity sken, BoneNode node) {
		Quat q0 = Quat.Create(MathHelper.DegToRad(0.0f), Vector3.UnitX);
		Quat q1 = Quat.Create(MathHelper.DegToRad(-90.0f), Vector3.UnitY);
		Quat q2 = Quat.Create(MathHelper.DegToRad(-180.1f), Vector3.UnitY);
		Quat q3 = Quat.Create(MathHelper.DegToRad(0.0f), Vector3.UnitY);

		TransformSnapshot xfm0 = new(node.Transform);
		TransformSnapshot xfm1 = xfm0 with { Rotation = node.Transform.Rotation };
		TransformSnapshot xfm2 = xfm0 with { Rotation = q0 };
		TransformSnapshot xfm3 = xfm0 with { Rotation = q1 };
		TransformSnapshot xfm4 = xfm0 with { Rotation = q2 };
		TransformSnapshot xfm5 = xfm0 with { Rotation = q3 };
		
		AnimationKeyframe key1 = AnimationKeyframe.Create(node, xfm1, 0.0f);
		AnimationKeyframe key2 = AnimationKeyframe.Create(node, xfm2, 1.0f);
		AnimationKeyframe key3 = AnimationKeyframe.Create(node, xfm3, 2.0f);
		AnimationKeyframe key4 = AnimationKeyframe.Create(node, xfm4, 3.0f);
		AnimationKeyframe key5 = AnimationKeyframe.Create(node, xfm5, 4.0f);
		AnimationKeyframe key6 = AnimationKeyframe.Create(node, xfm0, 7.0f);

		AnimationBuilder builder = new() {
			XfmType = AnimationXfmType.RotatePropagate
		};

		builder.StartSequence(key1, key2, AnimationBlendType.Testing);
		builder.BuildSequence(key3, AnimationBlendType.Testing);
		builder.BuildSequence(key4, AnimationBlendType.Testing);
		builder.BuildSequence(key5, AnimationBlendType.Testing);
		builder.BuildSequence(key6, AnimationBlendType.Testing);
		builder.EndSequence();

		return builder.Export();

	}
	private AnimationContainer CreateAddtiveRotation(SkeletonEntity sken, BoneNode node) {
		Quat q0 = Quat.Create(MathHelper.DegToRad(1.0f), Vector3.UnitX);
		Quat q1 = Quat.Create(MathHelper.DegToRad(1.0f), Vector3.UnitZ);
		Quat q2 = Quat.Create(MathHelper.DegToRad(5.0f), Vector3.UnitY);

		TransformSnapshot xfm0 = new(node.Transform);
		TransformSnapshot xfm1 = xfm0 with { Rotation = node.Transform.Rotation };
		TransformSnapshot xfm2 = xfm0 with { Rotation = q0 };
		TransformSnapshot xfm3 = xfm0 with { Rotation = q1 };
		TransformSnapshot xfm4 = xfm0 with { Rotation = q2 };
		
		AnimationKeyframe key1 = AnimationKeyframe.Create(node, xfm1, 0.0f);
		AnimationKeyframe key2 = AnimationKeyframe.Create(node, xfm2, 2.0f);
		AnimationKeyframe key3 = AnimationKeyframe.Create(node, xfm3, 4.0f);
		AnimationKeyframe key4 = AnimationKeyframe.Create(node, xfm4, 6.0f);

		AnimationBuilder builder = new() {
			XfmType = AnimationXfmType.AdditiveRotation
		};

		builder.StartSequence(key1, key2, AnimationBlendType.Linear);
		builder.BuildSequence(key3, AnimationBlendType.Linear);
		builder.BuildSequence(key4, AnimationBlendType.Linear);
		builder.EndSequence();

		return builder.Export();

	}
	private AnimationContainer CreateTranslation(SkeletonEntity sken, BoneNode node) {
		// Define a translation rotating around the xz plane
		Vector3[] translation = [
			new(2,0,0),
			new(0,0.5f,2),
			new(-2,0,0),
			new(0,-0.5f,-2)
			];

		// Set up transforms to target
		TransformSnapshot xfm0 = new(node.Transform);
		TransformSnapshot xfm1 = xfm0 with { Position = translation[0] };
		TransformSnapshot xfm2 = xfm0 with { Position = translation[1] };
		TransformSnapshot xfm3 = xfm0 with { Position = translation[2] };
		TransformSnapshot xfm4 = xfm0 with { Position = translation[3] };

		// Set up keyframes for transforms 
		AnimationKeyframe key1 = AnimationKeyframe.Create(node, xfm1, 0.0f);
		AnimationKeyframe key2 = AnimationKeyframe.Create(node, xfm2, 2.0f);
		AnimationKeyframe key3 = AnimationKeyframe.Create(node, xfm3, 4.0f);
		AnimationKeyframe key4 = AnimationKeyframe.Create(node, xfm4, 6.0f);
		// The final frame is set at the same position as the intial frame, creating a smooth loop
		AnimationKeyframe key5 = AnimationKeyframe.Create(node, xfm1, 8.0f);

		// Build the sequence
		AnimationBuilder builder = new() {
			XfmType = AnimationXfmType.TranslatePropagate
		};

		builder.StartSequence(key1, key2, AnimationBlendType.Linear);
		builder.BuildSequence(key3, AnimationBlendType.Linear);
		builder.BuildSequence(key4, AnimationBlendType.Linear);
		builder.BuildSequence(key5, AnimationBlendType.Linear);

		builder.EndSequence();

		return builder.Export();
	}
}
