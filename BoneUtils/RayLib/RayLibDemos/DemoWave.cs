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
		Raylib.DrawText("Behavior injection in rotation propagation.", 10, 50, 20, Color.White);
	}
	public override void Update(float deltaTime) {
		WaveXfmController(deltaTime);
	}
	private SkeletonEntity ConstructSkeleton(int i) { 
		var sken = Mock_Wave();
		SkelOps.PreProcessSkeleton(ref sken, [
			SkelOps.ValidateBoneNodeTree,
			SkelOps.BoneNodeTreeBuildRenderLists,
			SkelOps.BoneNodeTreeCalculateConstraints
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
	// Proof of concept/prototype (WIP)

	// 1. Controller and delegate dispatcher
	public void WaveXfmController(float deltaTime) {
		// Set up conditions for rotation. This will oscillate between +- 30 degrees of the X axis. 
		if(MathHelper.QuatToEuler(WaveMid.RootNode.Transform.Rotation).X > MathF.PI/2+MathF.PI/8 && WaveDir) {
			WaveDir = false;
		}
		else if(MathHelper.QuatToEuler(WaveMid.RootNode.Transform.Rotation).X < MathF.PI/2-MathF.PI/8 && !WaveDir) {
			WaveDir = true;
		}

		// Create the quat for the rotations
		Quat q;
		if(WaveDir)
			q = Quat.Create(MathHelper.DegToRad(0.2f), Vector3.UnitX);
		else
			q = Quat.Create(MathHelper.DegToRad(-0.2f), Vector3.UnitX);
		q.Normalize();

		// Pass the delegates to nodes, Rotate() handles propagation to children from any node in the tree.
		WaveMid.RootNode.Rotate(q, WaveXfmHandlerMid);
		WaveTop.RootNode.Rotate(q, WaveXfmHandlerBotTop);
		WaveBot.RootNode.Rotate(q, WaveXfmHandlerBotTop);
	}
	// 2a. Transform handler
	public Vector3 WaveXfmHandlerMid(BoneNode node, Vector3 pos, Quat q, Vector3 origin) {
		Quat localQuat;
		// Filtering by name enables controlling which transforms are applied to specific nodes in the (partial) tree.
		if(node.Name == "Root" || node.Name == "SpineB" || node.Name == "SpineD" || node.Name == "SpineF") { 
			// Conjugating the passed quaternion will produce a rotation that rotates oppositely for these nodes.
			localQuat = q.Conjugate();
		}
		else {
			localQuat = q;
		}

		// Finalize transforms that will be applied throughout the (partial) tree
		// This example uses the default transform handler, then clamps the length of the vector.
		origin = node.ParentBone?.Transform.Position ?? node.Transform.Position; // Set pivot point of rotation to parent node
		pos = MathHelper.RotateWithDriftCorrection(pos, localQuat, origin); // Default transform handler for rotation
		pos = MathHelper.ClampToLength(pos-origin, 1.0f)+origin; // Clamp the rotation to avoid stretching or compressing
		return pos; // Finally return the new position of the node, which will be set in Transform by BoneNode
	}
	// 2b. Similar to 2a, but less comment clutter for readability.
	public Vector3 WaveXfmHandlerBotTop(BoneNode node, Vector3 pos, Quat q, Vector3 origin) {
		Quat localQuat;
		if(node.Name == "SpineA" || node.Name == "SpineC" || node.Name == "SpineE") {
			localQuat = q.Conjugate();
		}
		else {
			localQuat = q;
		}
		origin = node.ParentBone?.Transform.Position ?? node.Transform.Position;
		pos = MathHelper.RotateWithDriftCorrection(pos, localQuat, origin);
		pos = MathHelper.ClampToLength(pos-origin, 1.0f)+origin;
		return pos; 
	}

}
