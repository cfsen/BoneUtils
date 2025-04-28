using BoneUtils.Helpers;
using BoneUtils.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BoneUtils.Entity.Skeleton;
/*
These are fallback transform handlers (default behavior) for BoneNode.Rotate and BoneNode.SetTransform.

Both Rotate and SetTransform propagates throughout a BoneNode's children, applying these to each node.

They are context aware via the node passed to them, and custom handlers can use this to apply different
transforms to individual nodes in the tree.

See RayLibDemos/DemoWave.cs for a basic example.
*/
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
	}
}
