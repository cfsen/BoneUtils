using BoneUtils.Math;
using System.Numerics;

namespace BoneUtils.Entity.Skeleton;
public class Transform {
	private Quat rotation;
	private Vector3 position;
	private Vector3 scale;
	private Matrix4x4 matrix;

	public Quat Rotation {
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
	public Matrix4x4 InitialState { get; private set; }

	public Transform(Vector3? scale = null, Quat? rotation = null, Vector3? position = null) {
		this.scale = scale ?? Vector3.One;
		this.rotation = rotation ?? Quat.Identity();
		this.position = position ?? Vector3.Zero;

		// Outlining: setting up default behavior for SetTransform();
		RebuildMatrix();
		this.InitialState = this.matrix;
	}

	public void BatchRotatePropagation(Vector3 position, Quat rotation) {
		this.rotation = Quat.Normalize(rotation * this.rotation);
		//this.rotation = Quat.Normalize(this.rotation * rotation);
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
		//matrix *= Matrix4x4.CreateFromQuaternion(rotation);
		matrix *= rotation.ToMatrix();
		matrix *= Matrix4x4.CreateTranslation(position);
	}

	public void SetTransform(Matrix4x4 xfmHandlerOutput) {
		Quaternion q = Quaternion.Identity;
		Matrix4x4.Decompose(xfmHandlerOutput, out this.scale, out q, out this.position);
		this.rotation = Quat.FromQuaternion(q); // TODO Quat
		this.matrix = xfmHandlerOutput;
	}
}
