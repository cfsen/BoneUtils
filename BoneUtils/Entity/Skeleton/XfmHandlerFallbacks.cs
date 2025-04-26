using BoneUtils.Helpers;
using BoneUtils.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BoneUtils.Entity.Skeleton;
public static class XfmHandlerFallbacks {
	/// <summary>
	/// Fallback delegate for BoneNode.Rotate()
	/// </summary>
	/// <param name="node">Node about to rotate</param>
	/// <param name="position">Position of node. Rotate() handles translating into local space.</param>
	/// <param name="newOrientation">New orientation, multiplicative with current orientation.</param>
	/// <returns></returns>
	public static Vector3 BoneNodeRotateFallback(BoneNode node, Vector3 position, Quat newOrientation) 
		=> MathHelper.RotateWithDriftCorrection(position, newOrientation);

	/// <summary>
	/// Fallback delegate for BoneNode.SetTransform()
	/// </summary>
	/// <param name="node">Noe to transform.</param>
	/// <param name="Transforms">List of tuples for contextual logic and target transform.</param>
	/// <returns></returns>
	public static Matrix4x4 BoneNodeSetTransformFallback(BoneNode node, List<(BoneNode, Transform)>? Transforms) {
		return node.Transform.InitialState;
		/* 
		This is where contextual logic for propagated transforms could be added.

		SetTransform() walks the tree through children (using names as a human readable example):
			if(BoneNode.Name == "Shoulder") ... do stuff to the shoulder
			else if(BoneNode.Name == "Elbow") ... do stuff to the elbow

		 Must return a Matrix4x4 for the Transform object associated with node.
		*/
	}
}
