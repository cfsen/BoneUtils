using BoneUtils.Helpers;
using BoneUtils.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
	/// This is the default behavior used when no custom handler is provided.
	/// </summary>
	/// <param name="node">Node about to rotate</param>
	/// <param name="position">Position of node. Rotate() handles translating into local space.</param>
	/// <param name="rotation">New orientation, multiplicative with current orientation.</param>
	/// <param name="origin">Origin of rotation (pivot point)</param>
	/// <returns>New position for bone</returns>
	public static Vector3 BoneNodeRotateFallback(BoneNode node, Vector3 position, Quat rotation, Vector3 origin) 
		=> MathHelper.RotateWithDriftCorrection(position, rotation, origin);

	/// <summary>
	/// Fallback delegate for BoneNode.SetTransform()
	/// This is the default behavior used when no custom handler is provided.
	/// </summary>
	/// <param name="node">Node to transform.</param>
	/// <param name="Transforms">List of tuples for contextual logic and target transform.</param>
	/// <returns>The state stored in Transform.InitialState at compose time.</returns>
	public static Matrix4x4 BoneNodeSetTransformFallback(BoneNode node, List<(BoneNode, Transform)>? Transforms) {
		return node.Transform.InitialState;
	}

	/// <summary>
	/// Fallback delegate for BoneNode.PrepareTransformBuffer()
	/// This is the default behavior used when no custom handler is provided.
	/// </summary>
	/// <param name="node">Node encapsulating transform buffer</param>
	/// <returns>True</returns>
	public static bool BoneNodePrepareXfmBufferFallback(BoneNode node) {
		XfmBufferSet(ref node);

		return true; // Always returns true, allowing for custom handlers to return false if necessary
	}

	/// <summary>
	/// Fallback delegate for BoneNode.ApplyTransformBuffer()
	/// This is the default behavior used when no custom handler is provided.
	/// </summary>
	/// <param name="node">Node encapsulating transform buffer</param>
	/// <returns>True</returns>
	public static bool BoneNodeApplyXfmBufferFallback(BoneNode node) {
		XfmBufferSet(ref node, false);

		node.TransformBuffer.Reset();

		return true;
	}


	// Helpers
	// TODO move these out

	private static void XfmBufferSet(ref BoneNode node, bool toBuffer = true) {
		if(toBuffer) {
			node.TransformBuffer.Translation = node.Transform.Position;
			node.TransformBuffer.Rotation	 = node.Transform.Rotation;
			node.TransformBuffer.Scale		 = node.Transform.Scale;
		}
		else {
			node.Transform.Position	= node.TransformBuffer.Translation;
			node.Transform.Rotation	= node.TransformBuffer.Rotation;
			node.Transform.Scale	= node.TransformBuffer.Scale;
		}
	}
}
