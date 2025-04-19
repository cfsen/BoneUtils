using BoneUtils.Entity.Skeleton;
using System.ComponentModel;
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
		nodeTemplate.Add(NewNode("Root", "SpineA",			(0, 1, 0),				(0.0f, MathF.PI/2, 0.0f)));
		nodeTemplate.Add(NewNode("SpineA", "SpineB",		(0, 2, 0),				(0.0f, MathF.PI/2, 0.0f)));
		nodeTemplate.Add(NewNode("SpineB", "SpineC",		(0, 3, 0),				(0.0f, MathF.PI/2, 0.0f)));

		var nodes = ConstructBoneDictFromList(nodeTemplate);
		return new SkeletonEntity("TestEntity01", nodes["Root"], nodes);
	}
	internal SkeletonEntity Mock_TestEntity01() {
		List<(string, string, Transform)> nodeTemplate = []; // parent, child
		nodeTemplate.Add(NewNode("Root", "SpineA",			(0,0,0),				(0, MathF.PI/2, 0)));
		nodeTemplate.Add(NewNode("SpineA", "SpineB",		(0,1,0),				(0, MathF.PI/2, 0)));
		nodeTemplate.Add(NewNode("SpineB", "SpineC",		(0,2,0),				(0, MathF.PI/2, 0)));
		nodeTemplate.Add(NewNode("SpineC", "L_Shoulder",	(-1, 2, 0),				(-MathF.PI/2, 0.0f, 0.0f)));
		nodeTemplate.Add(NewNode("SpineC", "R_Shoulder",	(1, 2, 0),				(MathF.PI/2, 0.0f, 0.0f)));
		nodeTemplate.Add(NewNode("L_Shoulder", "L_Elbow",	(-2.5f, 2, 0),			(-MathF.PI/2, 0.0f, 0.0f)));
		nodeTemplate.Add(NewNode("R_Shoulder", "R_Elbow",	(2.5f, 2, 0),			(MathF.PI/2, 0.0f, 0.0f)));
		nodeTemplate.Add(NewNode("L_Elbow", "L_Wrist",		(-4, 2, 0),				(-MathF.PI/2, 0.0f, 0.0f)));
		nodeTemplate.Add(NewNode("R_Elbow", "R_Wrist",		(4, 2, 0),				(MathF.PI/2, 0.0f, 0.0f)));
		nodeTemplate.Add(NewNode("SpineC", "Neck",			(0, 3, 0),				(0.0f, MathF.PI/2, 0.0f)));
		nodeTemplate.Add(NewNode("Neck", "Head",			(0, 4, 0),				(0.0f, MathF.PI/2, 0.0f)));

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

	private (string, string, Transform) NewNode(string parent, string name, (float, float, float) position, (float, float, float) q) {
		return (parent, name, new Transform(
			position: new Vector3(position.Item1, position.Item2, position.Item3), 
			rotation: Quaternion.CreateFromYawPitchRoll(q.Item1, q.Item2, q.Item3))
			);
	}

}
