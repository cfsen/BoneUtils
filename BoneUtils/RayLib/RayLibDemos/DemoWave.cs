using BoneUtils.Entity.Skeleton;
using BoneUtils.Helpers;
using BoneUtils.Math;
using Raylib_cs;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Numerics;

namespace BoneUtils.RayLib.RayLibDemos;
public class DemoWave :DemoBase {

	private SkeletonEntity WaveTop;
	private SkeletonEntity WaveMid;
	private SkeletonEntity WaveBot;
	private SkeletonEntityOps SkelOps;

	private bool WaveDir = false;

	public DemoWave(SkeletonEntityOps skeops) {
		SkelOps = skeops;
		WaveTop = ConstructSkeleton(0);
		WaveMid = ConstructSkeleton(1);
		WaveBot = ConstructSkeleton(2);
	}
	public override void Draw3D() {
		DrawBoneNodeNetwork(WaveTop);
		DrawBoneNodeNetwork(WaveMid);
		DrawBoneNodeNetwork(WaveBot);

		DrawQuaternionOrientation(WaveMid);
	}
	public override void Draw2D() {
		Raylib.DrawText("Work in progress.", 10, 50, 20, Color.Red);
		DrawQuatDebug(WaveMid.RootNode);
	}
	public override void HandleDemoInput() {
	}
	public override void Update(float deltaTime) {
		WaveXfmController(deltaTime);
	}
	private SkeletonEntity ConstructSkeleton(int i) { 
		var sken = Mock_Spine();
		SkelOps.PreProcessSkeleton(ref sken, [
			SkelOps.ValidateBoneNodeTree,
			SkelOps.LabelDepthBoneNodeTree,
			SkelOps.BoneNodeTreeBuildRenderLists
			]);

		Quat rot = Quat.Create(MathF.PI/2, Vector3.UnitX);
		sken.RootNode.Rotate(rot);
		if(i == 1)
			sken.RootNode.Translate(new(0, 2, 3));
		else if(i == 2)
			sken.RootNode.Translate(new(0, 0, 3));
		else
			sken.RootNode.Translate(new(0, 4, 3));

		return sken;
	}

	// xfmHandler example
	// WIP disclaimer: this is a proof of concept/prototype

	// TODO origin handling is a bit clunky to deal with, consider offering a dedicated handler for this

	// 1. set up a controlling function with arbitrary logic
	public void WaveXfmController(float deltaTime) {
		if(MathHelper.QuatToEuler(WaveMid.RootNode.Transform.Rotation).X > MathF.PI/2+MathF.PI/8 && WaveDir) {
			WaveDir = false;
		}
		else if(MathHelper.QuatToEuler(WaveMid.RootNode.Transform.Rotation).X < MathF.PI/2-MathF.PI/8 && !WaveDir) {
			WaveDir = true;
		}
	
		Quat q;
		if(WaveDir)
			q = Quat.Create(MathHelper.DegToRad(0.2f), Vector3.UnitX);
		else
			q = Quat.Create(MathHelper.DegToRad(-0.2f), Vector3.UnitX);

		q.Normalize();
		WaveMid.RootNode.Rotate(q, WaveXfmHandlerMid); // 2. pass the custom xfm handler to Rotate()
		WaveTop.RootNode.Rotate(q, WaveXfmHandlerBotTop);
		WaveBot.RootNode.Rotate(q, WaveXfmHandlerBotTop);
	}
	// 3. the following function will then be ran on each individual node in the rootnodes tree.
	public Vector3 WaveXfmHandlerMid(BoneNode node, Vector3 pos, Quat q, Vector3 origin) {
		Quat localQuat;
		if(node.Name == "Root" || node.Name == "SpineB") {
			// arbitrarily apply any transform logic
			localQuat = q.Conjugate();
		}
		else {
			localQuat = q;
		}
		origin = node.ParentBone?.Transform.Position ?? node.Transform.Position;
		pos = MathHelper.RotateWithDriftCorrection(pos, localQuat, origin);
		return pos; // finally return the new position of the node
	}
	// 4. add as many as needed, with or without their own controlling logic
	public Vector3 WaveXfmHandlerBotTop(BoneNode node, Vector3 pos, Quat q, Vector3 origin) {
		Quat localQuat;
		if(node.Name == "SpineA" || node.Name == "SpineC") {
			localQuat = q.Conjugate();
		}
		else {
			localQuat = q;
		}
		origin = node.ParentBone?.Transform.Position ?? node.Transform.Position;
		pos = MathHelper.RotateWithDriftCorrection(pos, localQuat, origin);
		return pos; // return the new node position
	}

}
