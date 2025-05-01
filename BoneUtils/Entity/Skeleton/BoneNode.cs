using BoneUtils.Helpers;
using BoneUtils.Math;
using System.Diagnostics;
using System.Numerics;

namespace BoneUtils.Entity.Skeleton;
public class BoneNode {
	public string Name;
	public BoneNode? ParentBone;
	public SkeletonEntity? ParentEntity;
	public ParentRelativePosition? ParentRelativePosition;

	public int? TreeDepth;
	public Dictionary<string, BoneNode> Children = [];
	public List<BoneNode> RenderChildren = [];
	public int RenderChildrenCount = 0;

	public Transform Transform;
	public BoneNodeXfmBuffer TransformBuffer;

	public delegate Vector3 RotateXfmHandler(BoneNode node, Vector3 nodePosition, Quat newOrientation, Vector3 origin); 
	public delegate Matrix4x4 SetXfmHandler(BoneNode node, List<(BoneNode, Transform)>? Transforms = null);

	public bool Branching => Children.Count > 1;
	public bool HasChildren => Children.Count > 0;

	public BoneNode(string name, Transform transform, BoneNode? parent = null, Dictionary<string, BoneNode>? children = null,
				 SkeletonEntity? parentEntity = null) {
		Name = name;
		ParentBone = parent;
		Transform = transform;
		Children = children ?? [];
		TransformBuffer = new BoneNodeXfmBuffer();
	}


	public void Translate(Vector3 offset) {
		Transform.Position += offset;
		foreach (var child in Children.Values) 
			child.Translate(offset);
	}
	public bool PrepareTransformBuffer() {
		if(TransformBuffer.Active) return false;
		
		TransformBuffer.Translation = Transform.Position;
		TransformBuffer.Rotation = Transform.Rotation;
		TransformBuffer.Scale = Transform.Scale;

		return true;
	}
	public bool ApplyTransformBuffer() {
		if(!TransformBuffer.Complete) return false;

		Transform.Position = TransformBuffer.Translation;
		Transform.Rotation = TransformBuffer.Rotation;
		Transform.Scale = TransformBuffer.Scale;

		TransformBuffer.Reset();

		return true;
	}
	public void Rotate(Quat rotation, RotateXfmHandler? xfmHandler = null, Vector3? origin = null) {
		origin ??= Transform.Position;
		xfmHandler ??= XfmHandlerFallbacks.BoneNodeRotateFallback;

		Transform.SetPositionAndRotation(xfmHandler(this, Transform.Position, rotation, origin.Value), rotation);

		foreach (var child in Children.Values)
			child.Rotate(rotation, xfmHandler, origin);
	}
	public void SetTransform(SetXfmHandler? xfmHandler) {
		xfmHandler ??= XfmHandlerFallbacks.BoneNodeSetTransformFallback;

		Transform.SetTransform(xfmHandler(this));

		foreach (var child in Children.Values) 
			child.SetTransform(xfmHandler);
	}
	public void Reset(bool propagate = true) {
		Transform.SetTransform(Transform.InitialState);

		Debug.WriteLine($"Reset: {Name}");

		if(propagate)
			foreach (var child in Children.Values)
				child.Reset();
	}
}
public struct ParentRelativePosition {
	public Quat ParentOrientation; // Orientation of the parent node
	public Vector3 NodePosition; // Position of the node in the parents local space
	public float Distance;
}
