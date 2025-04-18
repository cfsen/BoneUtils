﻿using BoneUtils.Entity.Skeleton;
using Raylib_cs;

namespace BoneUtils.Tests.RayLibDemos;
public abstract class DemoBase :MockDataBuilder, IDemo{
	public virtual void Draw() { }
	public virtual void HandleDemoInput() { }
	public virtual void DrawHelpOverlay() { }
	internal void DrawBoneNodeNetwork(SkeletonEntity sken) {
		foreach(var bone in sken.Bones.Values) {
			Raylib.DrawSphere(sken.BoneWorldPosition(bone), 0.20f, Color.Red);
			if(bone.ParentBone != null)
				Raylib.DrawLine3D(sken.BoneWorldPosition(bone.ParentBone), sken.BoneWorldPosition(bone), Color.Red);
		}
	}
}
