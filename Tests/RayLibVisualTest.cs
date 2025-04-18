using BoneUtils.Entity.Skeleton;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BoneUtils.Tests;
public class RayLibVisualTest :MockDataBuilder{
	public SkeletonEntityOps skeops = new();
	public SkeletonEntity SimpleSpine() { 
		var spine = Mock_Spine();

		if(!skeops.ValidateBoneNodeTree(spine)) 
			throw new FormatException("BoneNode tree is invalid: check for duplicates or circular relationships.");
		if(skeops.LabelDepthBoneNodeTree(spine) == null)
			throw new Exception("BoneNode tree is too deep.");

		return spine;
	}

	public void HandleInput(SkeletonEntity spine, Quaternion q) {
		if(Raylib.IsKeyPressed(KeyboardKey.One)) {
			spine.RootNode.Rotate(q);
		}
		if(Raylib.IsKeyPressed(KeyboardKey.Two)) {
			spine.Bones["SpineA"].Rotate(q);
		}
		if(Raylib.IsKeyPressed(KeyboardKey.Three)) {
			spine.Bones["SpineB"].Rotate(q);
		}
		if(Raylib.IsKeyPressed(KeyboardKey.Four)) {
			spine.Bones["SpineC"].Rotate(q);
		}
	}
}

