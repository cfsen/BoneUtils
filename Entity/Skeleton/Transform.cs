using System.Numerics;

namespace BoneUtils.Entity.Skeleton;
public class Transform {
	private Quaternion rotation;
	private Vector3 position;
	private Vector3 scale;
	private Matrix4x4 matrix;

	public Quaternion Rotation {
		get => rotation;
		set => Set(ref rotation, value);
	}
	public Vector3 Position {
		get => position;
		set => Set(ref position, value);
	}
	public Vector3 Scale {
		get => scale;
		set => Set(ref scale, value);
	}
	public Matrix4x4 Matrix {
		get { return matrix; }
		set { matrix = value; }
	}
	public Transform(Vector3? scale = null, Quaternion? rotation = null, Vector3? position = null) {
		this.scale = scale ?? Vector3.One;
		this.rotation = rotation ?? Quaternion.Identity;
		this.position = position ?? Vector3.Zero;
	}
	public void BatchRotatePropagation(Vector3 position, Quaternion rotation) {
		this.rotation = Quaternion.Normalize(this.rotation * rotation);
		Position = position;
	}
	private void Set<T>(ref T field, T value) {
		if (!EqualityComparer<T>.Default.Equals(field, value)) {
			field = value;
			RebuildMatrix();
		}
	}
	private void RebuildMatrix() {
		matrix = Matrix4x4.CreateScale(scale);
		matrix *= Matrix4x4.CreateFromQuaternion(rotation);
		matrix *= Matrix4x4.CreateTranslation(position);
	}
}
