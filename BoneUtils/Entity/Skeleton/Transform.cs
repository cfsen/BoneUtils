using BoneUtils.Math;
using System.Numerics;

namespace BoneUtils.Entity.Skeleton;
public class Transform {
	private Quat rotation;
	private Vector3 position;
	private Vector3 scale;
	private Matrix4x4 matrix;

	private bool updateMatrix = false;

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
		get => this.GetMatrix(); 
		set => this.matrix = value;
	}
	public Matrix4x4 InitialState { get; private set; }

	public Transform(Vector3? scale = null, Quat? rotation = null, Vector3? position = null) {
		this.scale = scale ?? Vector3.One;
		this.rotation = rotation ?? Quat.Identity;
		this.position = position ?? Vector3.Zero;

		RebuildMatrix();
		this.InitialState = this.matrix;
	}

	public void SetPositionAndRotation(Vector3 position, Quat rotation) {
		this.rotation = Quat.Normalize(rotation * this.rotation);
		this.Position = position; // Use setter to flag matrix for rebuild
	}
	private void Set<T>(ref T field, T value) {
		if (!EqualityComparer<T>.Default.Equals(field, value)) {
			field = value;
			updateMatrix = true;
		}
	}
	public void SetTransform(Matrix4x4 xfmHandlerOutput) {
		Quaternion q = Quaternion.Identity;
		Matrix4x4.Decompose(xfmHandlerOutput, out this.scale, out q, out this.position);
		this.rotation = Quat.FromQuaternion(q); // TODO Quat
		this.matrix = xfmHandlerOutput;
	}
	public Matrix4x4 GetMatrix(){
		if(updateMatrix) RebuildMatrix();
		return this.matrix;
	}

	// Helpers

	private void RebuildMatrix() {
		matrix = Matrix4x4.CreateScale(scale);
		matrix *= rotation.ToMatrix();
		matrix *= Matrix4x4.CreateTranslation(position);
	}
}
