using BoneUtils.Entity.Skeleton;
using BoneUtils.Helpers;
using BoneUtils.Mockups;
using Raylib_cs;
using System.Numerics;

namespace BoneUtils.RayLib.RayLibDemos;
public abstract class DemoBase :MockDataBuilder, IDemo{
	public virtual void Draw() { }
	public virtual void DrawHelpOverlay() { }
	public virtual void HandleDemoInput() { }
	public virtual void Update(float deltaTime) { }
	internal void DrawBoneNodeNetwork(SkeletonEntity sken) {
		for(var i = 0; i < sken.RenderBoneCount; i++) {
			Raylib.DrawSphere(sken.BoneWorldPosition(sken.RenderBones[i]), 0.20f, Color.Red);
			if(sken.RenderBones[i].ParentBone != null)
				Raylib.DrawLine3D(
					sken.BoneWorldPosition(sken.RenderBones[i].ParentBone!), 
					sken.BoneWorldPosition(sken.RenderBones[i]),
					Color.Red);
		}
	}
	internal void DrawDebug(List<string> dbgLines) {
		int i = 0;
		foreach(var dbgLine in dbgLines) {
			Raylib.DrawText(dbgLine, 10, 100+i*18, 16, Color.White);
			i++;
		}
	}
	internal void DrawQuaternionOrientation(SkeletonEntity sken) {
		// TODO improve visual clarity
		Vector3 pos = Vector3.Zero;
		Vector3 ang = Vector3.Zero;
		List<Vector3> directionals = [];
		for(var i = 0; i < sken.RenderBoneCount; i++) {
			pos = sken.BoneWorldPosition(sken.RenderBones[i]);
			ang = MathHelper.QuaternionToEuler(sken.RenderBones[i].Transform.Rotation);

			Raylib.DrawCircle3D(pos, 0.30f, Vector3.UnitX, MathHelper.RadToDeg(ang.X), Color.Orange);
			Raylib.DrawCircle3D(pos, 0.30f, Vector3.UnitY, MathHelper.RadToDeg(ang.Y), Color.Green);
			Raylib.DrawCircle3D(pos, 0.30f, Vector3.UnitZ, MathHelper.RadToDeg(ang.Z), Color.Blue);
		}
	}
}
