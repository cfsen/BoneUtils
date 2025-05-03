using BoneUtils.Entity.Skeleton;
using BoneUtils.Helpers;
using BoneUtils.Mockups;
using Raylib_cs;
using System.Numerics;

namespace BoneUtils.RayLib.RayLibDemos;
public abstract class DemoBase :MockDataBuilder, IDemo{

	// Interface contracts

	public virtual void Draw3D() { }
	public virtual void Draw2D() { }
	public virtual void HandleDemoInput() { }
	public virtual void Update(float deltaTime) { }

	// Skeleton visualizers
	internal void DrawBoneNodeRendered(SkeletonEntity sken, RaylibRenderer rr) { 
		for(var i = 0; i < sken.RenderBoneCount; i++)
			rr.DrawSphere(sken.BoneWorldPosition(sken.RenderBones[i]));
	}

	internal void DrawBoneNodeNetwork(SkeletonEntity sken, bool drawConnectors = true) {
		DrawBoneNode(sken);
		if(drawConnectors) 
			DrawBoneNodeConnectors(sken);
	}
	internal void DrawBoneNode(SkeletonEntity sken) {
		for(var i = 0; i < sken.RenderBoneCount; i++)
			Raylib.DrawSphere(sken.BoneWorldPosition(sken.RenderBones[i]), 0.20f, Color.Red);
	}
	internal void DrawBoneNodeConnectors(SkeletonEntity sken) {
		for(var i = 0; i < sken.RenderBoneCount; i++) {
			if (sken.RenderBones[i].ParentBone != null)
				Raylib.DrawLine3D(
					sken.BoneWorldPosition(sken.RenderBones[i].ParentBone!),
					sken.BoneWorldPosition(sken.RenderBones[i]),
					Color.Red);
		}
	}
	internal void DrawQuaternionOrientation(SkeletonEntity sken) {
		Dictionary<string,Vector3> indicator = [];

		Vector3 originBone = Vector3.Zero;
		Vector3 worldXfm = Vector3.Zero;
		for(var i = 0; i < sken.RenderBoneCount; i++) {
			originBone = sken.RenderBones[i].Transform.Position;
			worldXfm = sken.WorldPosition;

			indicator = MathHelper.CreateLocalDirectionVectors(
				sken.RenderBones[i].Transform.Rotation, 
				originBone);

			originBone += worldXfm;

			Raylib.DrawLine3D(originBone, worldXfm+indicator["X"], Color.Orange);
			Raylib.DrawLine3D(originBone, worldXfm+indicator["Y"], Color.Green);
			Raylib.DrawLine3D(originBone, worldXfm+indicator["Z"], Color.Blue);
		}
	}

	// Text overlay

	internal void DrawQuatDebug(BoneNode node) {
		List<string> dbgLines = [];
		var euler = MathHelper.QuatToEuler(node.Transform.Rotation);

		dbgLines.Add($"Quat: {node.Name}");
		dbgLines.Add($"W: {node.Transform.Rotation.W}");
		dbgLines.Add($"X: {node.Transform.Rotation.X}");
		dbgLines.Add($"Y: {node.Transform.Rotation.Y}");
		dbgLines.Add($"Z: {node.Transform.Rotation.Z}");

		dbgLines.Add("");

		dbgLines.Add("euler:");
		dbgLines.Add($"X: {MathHelper.RadToDeg(euler.X)}");
		dbgLines.Add($"Y: {MathHelper.RadToDeg(euler.Y)}");
		dbgLines.Add($"Z: {MathHelper.RadToDeg(euler.Z)}");

		DrawDebug(dbgLines);
	}
	internal void DrawDebug(List<string> dbgLines) {
		int i = 0;
		foreach(var dbgLine in dbgLines) {
			Raylib.DrawText(dbgLine, 10, 100+i*18, 16, Color.White);
			i++;
		}
	}
	internal void DrawQuatInfo(BoneNode node, Camera3D camera) { 
		// needs access to camera
		//Raylib.GetWorldToScreen(node.Transform.Position)	
	}
}
