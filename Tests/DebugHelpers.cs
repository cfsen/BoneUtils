using BoneUtils.Entity.Skeleton;
using System.Diagnostics;
using System.Numerics;

namespace BoneUtils.Tests;
public abstract class DebugHelpers {
	internal void DbgOutEx(Exception ex) {
		var caller = new StackTrace()?.GetFrame(1)?.GetMethod()?.Name ?? "Unknown (DebugOutput() was called without an exception occuring)";
		Debug.WriteLine($"Exception in method: {caller}");
		Debug.WriteLine($"Exception Message: {ex.Message}");
		Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
	}
	internal void DbgOutOk(string msg) {
		Debug.WriteLine($"Test passed: {msg}");
	}
	internal void DbgRecurseTree(BoneNode node, int maxDepth = 10, int depth = 0) {
		Debug.WriteLine($"{node.Name}		{node.Transform.Position}		{node.Transform.Rotation}");
		if (node.Children.Count > 0) {
			depth += 1;
			foreach (var child in node.Children)
				DbgRecurseTree(child.Value, maxDepth, depth);
		}
	}
	internal void DbgPrintMatrix4x4(Matrix4x4 m) {
		Debug.WriteLine($"{m.M11}	|	{m.M12}	|	{m.M13}	|	{m.M14}");
		Debug.WriteLine($"{m.M21}	|	{m.M22}	|	{m.M23}	|	{m.M24}");
		Debug.WriteLine($"{m.M31}	|	{m.M32}	|	{m.M33}	|	{m.M34}");
		Debug.WriteLine($"{m.M41}	|	{m.M42}	|	{m.M43}	|	{m.M44}");
	}

}
