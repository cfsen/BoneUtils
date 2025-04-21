using BoneUtils.Entity.Skeleton;
using BoneUtils.Tests;
using Raylib_cs;

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
}
