using BoneUtils.Helpers;
using BoneUtils.Math;
using System.Diagnostics;
using System.Numerics;

namespace BoneUtils.Entity.Skeleton;
public class BoneNode {
	public string Name;
	public BoneNode? ParentBone;
	public SkeletonEntity? ParentEntity;

	public int? TreeDepth;
	public Dictionary<string, BoneNode> Children = [];
	public List<BoneNode> RenderChildren = [];
	public int RenderChildrenCount = 0;

	public Transform Transform;

	public BoneNode(string name, Transform transform, BoneNode? parent = null, Dictionary<string, BoneNode>? children = null, SkeletonEntity? parentEntity = null) {
		Name = name;
		ParentBone = parent;
		Transform = transform;
		Children = children ?? [];
	}
	public delegate Vector3 RotateXfmHandler(BoneNode node, Vector3 nodePosition, Quat newOrientation); 
	public delegate Matrix4x4 SetXfmHandler(BoneNode node, List<(BoneNode, Transform)>? Transforms = null);
	public bool Branching => Children.Count > 1;
	public bool HasChildren => Children.Count > 0;
	public void Translate(Vector3 offset) {
		Transform.Position += offset;
		foreach (var child in Children.Values) 
			child.Translate(offset);
	}
	public void Rotate(Quat rotation, RotateXfmHandler? xfmHandler = null, Vector3? origin = null) {
		origin ??= Transform.Position;
		xfmHandler ??= XfmHandlerFallbacks.BoneNodeRotateFallback;

		Transform.BatchRotatePropagation(xfmHandler(this, Transform.Position-origin.Value, rotation) + origin.Value, rotation);

		foreach (var child in Children.Values)
			child.Rotate(rotation, xfmHandler, origin);
	}
	public void SetTransform(SetXfmHandler? xfmHandler) {
		xfmHandler ??= XfmHandlerFallbacks.BoneNodeSetTransformFallback;

		Transform.SetTransform(xfmHandler(this));

		foreach (var child in Children.Values) 
			child.SetTransform(xfmHandler);
	}
}
