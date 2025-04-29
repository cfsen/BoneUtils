using System.Numerics;

namespace BoneUtils.Entity.Skeleton;
public class SkeletonEntity {
	public string Name; // TODO move to parent entity
	public Vector3 WorldPosition;
	public Quaternion WorldOrientation; // TODO Quat

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
	public Matrix4x4 BoneWorldMatrix(BoneNode bone) { // TODO unit test
		var m = Matrix4x4.CreateScale(bone.Transform.Scale)
			* Matrix4x4.CreateFromQuaternion(WorldOrientation)
			* bone.Transform.Rotation.ToMatrix()
			* Matrix4x4.CreateTranslation(WorldPosition)
			* Matrix4x4.CreateTranslation(bone.Transform.Position);
		return m;
	}
	public void ResetTransforms() {
		RootNode.Reset();
	}
}
