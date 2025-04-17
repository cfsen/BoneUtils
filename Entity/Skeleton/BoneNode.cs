using BoneUtils.Helpers;
using System.Diagnostics;
using System.Numerics;

namespace BoneUtils.Entity.Skeleton;
public class BoneNode {
	public string Name;
	public BoneNode? ParentBone;
	public SkeletonEntity? ParentEntity;
	// TODO could add a leveldepth counter
	public Dictionary<string, BoneNode> Children = [];
	public Transform Transform;

	public BoneNode(string name, Transform transform, BoneNode? parent = null, Dictionary<string, BoneNode>? children = null, SkeletonEntity? parentEntity = null) {
		Name = name;
		ParentBone = parent;
		Transform = transform;
		Children = children ?? [];
	}
	public bool Branching => Children.Count > 1;
	public void Translate(Vector3 offset) {
		Transform.Position += offset;
		foreach (var child in Children) 
			child.Value.Translate(offset);
	}
	public void Rotate(
		Quaternion rotation,
		Func<Vector3, Quaternion, Vector3>? positionTransformer = null,
		Vector3? origin = null) {

		if (positionTransformer != null)
			Transform.BatchRotatePropagation(
				positionTransformer(Transform.Position-origin ?? Vector3.Zero, rotation) + origin ?? Vector3.Zero,
				rotation);
		else {
			origin = Transform.Position;
			Transform.Rotation = Quaternion.Normalize(Transform.Rotation * rotation);
			positionTransformer = MathHelper.RotateWithDriftCorrection;
		}

		foreach (var child in Children.Values)
			child.Rotate(rotation, positionTransformer, origin);
	}
}
