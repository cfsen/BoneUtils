using BoneUtils.Entity.Skeleton;
using System.Numerics;

namespace BoneUtils.Tests;
public abstract class MockDataBuilder :DebugHelpers {
	internal BoneNode Mock_BoneNode(string name = "Default") => new(name, new Transform());
	internal Dictionary<string, BoneNode> Mock_BoneNodeTree() {
		List<(string, string, Transform)> nodeTemplate = []; // parent, child
		nodeTemplate.Add(("Root", "SpineA", new Transform()));
		nodeTemplate.Add(("SpineA", "SpineB", new Transform()));
		nodeTemplate.Add(("SpineB", "SpineC", new Transform()));
		nodeTemplate.Add(("SpineC", "L_Shoulder", new Transform()));
		nodeTemplate.Add(("SpineC", "R_Shoulder", new Transform()));
		nodeTemplate.Add(("SpineC", "Neck", new Transform()));

		return ConstructBoneDictFromList(nodeTemplate);
	}
	internal SkeletonEntity Mock_Spine() {
		List<(string, string, Transform)> nodeTemplate = []; // parent, child
		nodeTemplate.Add((
			"Root", "SpineA", new Transform(
				position: new Vector3(0, 1, 0),
				rotation: Quaternion.CreateFromYawPitchRoll(0.0f, MathF.PI/2, 0.0f)
				)
			));
		nodeTemplate.Add((
			"SpineA", "SpineB", new Transform(
				position: new Vector3(0, 2, 0),
				rotation: Quaternion.CreateFromYawPitchRoll(0.0f, MathF.PI/2, 0.0f)
				)
			));
		nodeTemplate.Add((
			"SpineB", "SpineC", new Transform(
				position: new Vector3(0, 3, 0),
				rotation: Quaternion.CreateFromYawPitchRoll(0.0f, MathF.PI/2, 0.0f)
				)
			));
		var nodes = ConstructBoneDictFromList(nodeTemplate);
		return new SkeletonEntity("TestEntity01", nodes["Root"], nodes);
	}
	internal SkeletonEntity Mock_TestEntity01() {
		List<(string, string, Transform)> nodeTemplate = []; // parent, child
		nodeTemplate.Add((
			"Root", "SpineA", new Transform(
				position: Vector3.Zero,
				rotation: Quaternion.CreateFromYawPitchRoll(0.0f, MathF.PI/2, 0.0f)
				)
			));
		nodeTemplate.Add((
			"SpineA", "SpineB", new Transform(
				position: new Vector3(0, 1, 0),
				rotation: Quaternion.CreateFromYawPitchRoll(0.0f, MathF.PI/2, 0.0f)
				)
			));
		nodeTemplate.Add((
			"SpineB", "SpineC", new Transform(
				position: new Vector3(0, 2, 0),
				rotation: Quaternion.CreateFromYawPitchRoll(0.0f, MathF.PI/2, 0.0f)
				)
			));
		nodeTemplate.Add((
			"SpineC", "L_Shoulder", new Transform(
				position: new Vector3(-1, 2, 0),
				rotation: Quaternion.CreateFromYawPitchRoll(-MathF.PI, 0.0f, 0.0f)
				)
			));
		nodeTemplate.Add((
			"SpineC", "R_Shoulder", new Transform(
				position: new Vector3(1, 2, 0),
				rotation: Quaternion.CreateFromYawPitchRoll(MathF.PI/2, 0.0f, 0.0f)
				)
			));
		nodeTemplate.Add((
			"SpineC", "Neck", new Transform(
				position: new Vector3(0, 3, 0),
				rotation: Quaternion.CreateFromYawPitchRoll(0.0f, MathF.PI/2, 0.0f)
				)
			));

		var nodes = ConstructBoneDictFromList(nodeTemplate);
		return new SkeletonEntity("TestEntity01", nodes["Root"], nodes);
	}

	// Construction helpers

	private Dictionary<string, BoneNode> ConstructBoneDictFromList(List<(string, string, Transform)> nodeTemplate) {
		BoneNode root = Mock_BoneNode("Root");
		Dictionary<string, BoneNode> nodes = [];
		nodes.Add("Root", root);

		foreach (var x in nodeTemplate) {
			BoneNode bnode = new(x.Item2, x.Item3);
			nodes.Add(x.Item2, bnode);
		}

		foreach (var(parent, child, xfm) in nodeTemplate) {
			// add children
			nodes[parent].Children.Add(child, nodes[child]);
			// add parent
			nodes[child].ParentBone = nodes[parent];
		}
		return nodes;
	}

}
