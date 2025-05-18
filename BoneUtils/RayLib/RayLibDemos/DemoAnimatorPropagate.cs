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
	private RaylibAnimationUI AnimationUI;

	public DemoAnimationPropagate(SkeletonEntityOps skeops, RaylibRenderer renderer) {
		SkelOps = skeops;
		Spine = ConstructSkeleton();
		AnimationUI = new(Spine.Animator!);
		Renderer = renderer;
		renderMode = RenderMode.Fancy ^ RenderMode.QuatOrientation;
	}
	public override void Draw3D() {
		Render(Spine, Renderer);
	}
	public override void Draw2D() {
		Raylib.DrawText(text_Title, 10, 50, 20, Color.Red);
		AnimationUI.Draw2DTimeline();
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

		var anim_rotation_spinea = CreateRotatePropagate(sken.Bones["SpineA"]);
		var anim_rotation_spineb = CreateAddtiveRotation(sken.Bones["SpineB"]);
		var anim_translate_root = CreateTranslatePropagate(sken.RootNode);

		// Set up the animation owner (keyframe selection, blending)
		AnimationInstance anim_spineb = new(anim_rotation_spineb) {
			Loop = true
		};

		// Load the animations into the manager
		sken.Animator!.Load(anim_rotation_spinea);
		sken.Animator!.Load(anim_spineb);
		sken.Animator!.Load(anim_translate_root);

		return sken;
	}
	private AnimationInstance CreateRotatePropagate(BoneNode node) {
		// Simple animations can be built with the limited AnimationSimpleBuilder:
		AnimationSimpleBuilder simplebuilder = new(node, AnimationXfmType.RotatePropagate);
		simplebuilder
			.CaptureInitial()
			.ApplyInitial(				0.0f, AnimationBlendType.Testing)
			.Rotate(0.0f,		Axis.X, 0.5f)
			.Rotate(-90.0f,		Axis.Y, 4.0f)
			.Rotate(-180.0f,	Axis.Y, 6.0f)
			.ApplyInitial(				7.0f, AnimationBlendType.Testing)
			.ApplyInitial(				8.0f, AnimationBlendType.Testing)
			.Rotate(0.0f,		Axis.Y, 9.0f)
			.ApplyInitial(				13.0f, AnimationBlendType.Testing);
		return simplebuilder.Finish();
	}
	private AnimationContainer CreateAddtiveRotation(BoneNode node) {
		// Consuming AnimationBuilder directly offers granular control:
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
	private AnimationInstance CreateTranslatePropagate(BoneNode node) {
		// Simple translation animation
		AnimationSimpleBuilder simplebuilder = new(node, AnimationXfmType.TranslatePropagate);
		simplebuilder
			.CaptureInitial()
			.ApplyInitial(0.0f)
			.Translate(2.0f, Axis.Z, 2.0f)
			.Translate(-2.0f, Axis.Z, 6.0f)
			.ApplyInitial(8.0f);
		return simplebuilder.Finish();
	}
}
