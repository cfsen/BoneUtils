using BoneUtils.Entity.Skeleton.Animation;
﻿using System.ComponentModel;
using System.Numerics;

namespace BoneUtils.Entity.Skeleton;
public class SkeletonEntity {
	public string Name; // TODO move to owner of skeleton (eg. GameEntity)
	public SkeletonAnimator? Animator; // Set with SkeletonEntityOps mutator
	public List<string> Mutators = new();
	public Vector3 WorldPosition;
	public BoneNode RootNode;
	public Dictionary<string, BoneNode> Bones = [];
	public int RenderBoneCount = 0;
	public List<BoneNode> RenderBones = [];
	public int? BoneDepth;


	public SkeletonEntity(string name, BoneNode root, Dictionary<string, BoneNode>? bones = null) {
		Name = name;
		RootNode = root;
		if (bones != null)
			Bones = bones;
	}

	public Vector3 BoneWorldPosition(BoneNode bone) {
		return bone.Transform.Position + WorldPosition;
	}
	public Matrix4x4 BoneWorldMatrix(BoneNode bone) {
		if(bone == RootNode) // ParentEntity always null
			return bone.Transform.GetMatrix() * Matrix4x4.CreateTranslation(WorldPosition);
		if(bone.ParentEntity == null) // Mutator for setting Parent wasn't called at compose
			throw new InvalidOperationException("BoneNode does not have a valid reference to ParentEntity, add BoneNodeTreeSetParentEntity mutator at compose time.");

		var m = Matrix4x4.CreateScale(bone.Transform.Scale)
			* bone.ParentEntity.RootNode.Transform.Rotation.ToMatrix()
			* bone.Transform.Rotation.ToMatrix()
			* Matrix4x4.CreateTranslation(WorldPosition)
			* Matrix4x4.CreateTranslation(bone.Transform.Position);
		return m;
	}
	public void ResetTransforms() {
		RootNode.Reset();
	}
}
