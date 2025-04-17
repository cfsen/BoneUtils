using System.Numerics;

namespace BoneUtils.Entity.Skeleton;
public class SkeletonEntity {
	public string Name; // TODO move to parent entity
	public Vector3 WorldPosition;
	public Quaternion WorldOrientation;

	public BoneNode RootNode;
	public Dictionary<string, BoneNode> Bones = [];

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
		var m = Matrix4x4.CreateScale(bone.Transform.Scale)
			* Matrix4x4.CreateFromQuaternion(WorldOrientation)
			* Matrix4x4.CreateFromQuaternion(bone.Transform.Rotation)
			* Matrix4x4.CreateTranslation(WorldPosition)
			* Matrix4x4.CreateTranslation(bone.Transform.Position);
		return m;
	}
}
