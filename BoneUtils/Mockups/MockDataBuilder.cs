using BoneUtils.Entity.Skeleton;
using BoneUtils.Helpers;
using BoneUtils.Math;
using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;

namespace BoneUtils.Mockups;
public abstract class MockDataBuilder :DebugHelpers {
	public BoneNode Mock_BoneNode(string name = "Default") => new(name, new Transform());
	public Dictionary<string, BoneNode> Mock_BoneNodeTree() {
		List<(string, string, Transform)> nodeTemplate = []; // parent, child
		nodeTemplate.Add(("Root", "SpineA", new Transform()));
		nodeTemplate.Add(("SpineA", "SpineB", new Transform()));
		nodeTemplate.Add(("SpineB", "SpineC", new Transform()));
		nodeTemplate.Add(("SpineC", "L_Shoulder", new Transform()));
		nodeTemplate.Add(("SpineC", "R_Shoulder", new Transform()));
		nodeTemplate.Add(("SpineC", "Neck", new Transform()));

		return ConstructBoneDictFromList(nodeTemplate);
	}
	public SkeletonEntity Mock_Spine() {
		List<(string, string, Transform)> nodeTemplate = []; // parent, child
		nodeTemplate.Add(NewNode("Root", "SpineA",			(0,1,0),			(0,1,0)		));
		nodeTemplate.Add(NewNode("SpineA", "SpineB",		(0,2,0),			(0,1,0)		));
		nodeTemplate.Add(NewNode("SpineB", "SpineC",		(0,3,0),			(0,1,0)		));

		var nodes = ConstructBoneDictFromList(nodeTemplate);
		return new SkeletonEntity("TestEntity01",nodes["Root"],nodes);
	}
	public SkeletonEntity Mock_TestEntity01() {
		List<(string,string,Transform)> nodeTemplate = []; // parent,child,unit vector for orientation
		nodeTemplate.Add(NewNode("Root", "SpineA",			(0,0,0),			(0,1,0)		));
		nodeTemplate.Add(NewNode("SpineA", "SpineB",		(0,1,0),			(0,1,0)		));
		nodeTemplate.Add(NewNode("SpineB", "SpineC",		(0,2,0),			(0,1,0)		));
		nodeTemplate.Add(NewNode("SpineC", "L_Shoulder",	(-1,2,0),			(-1,0,0)	));
		nodeTemplate.Add(NewNode("SpineC", "R_Shoulder",	(1,2,0),			(1,0,0)		));
		nodeTemplate.Add(NewNode("L_Shoulder", "L_Elbow",	(-2.5f,2,0),		(-1,0,0)	));
		nodeTemplate.Add(NewNode("R_Shoulder", "R_Elbow",	(2.5f,2,0),			(1,0,0)		));
		nodeTemplate.Add(NewNode("L_Elbow", "L_Wrist",		(-4,2,0),			(-1,0,0)	));
		nodeTemplate.Add(NewNode("R_Elbow", "R_Wrist",		(4,2,0),			(1,0,0)		));
		nodeTemplate.Add(NewNode("SpineC", "Neck",			(0,3,0),			(0,1,0)		));
		nodeTemplate.Add(NewNode("Neck", "Head",			(0,4,0),			(0,1,0)		));
		nodeTemplate.Add(NewNode("SpineA", "Waist",			(0,0,0),			(0,-1,0)	));
		nodeTemplate.Add(NewNode("Waist", "L_Hip",			(-1,-1,0),			(0,-1,0)	));
		nodeTemplate.Add(NewNode("Waist", "R_Hip",			(1,-1,0),			(0,-1,0)	));
		nodeTemplate.Add(NewNode("L_Hip", "L_Knee",			(-1,-2,0),			(0,-1,0)	));
		nodeTemplate.Add(NewNode("R_Hip", "R_Knee",			(1,-2,0),			(0,-1,0)	));
		nodeTemplate.Add(NewNode("L_Knee", "L_Ankle",		(-1,-2,0),			(0,-1,0)	));
		nodeTemplate.Add(NewNode("R_Knee", "R_Ankle",		(1,-2,0),			(0,-1,0)	));

		var nodes = ConstructBoneDictFromList(nodeTemplate);
		return new SkeletonEntity("TestEntity01", nodes["Root"], nodes);
	}
	public SkeletonEntity Mock_FailCircularTree() {
		List<(string,string,Transform)> nodeTemplate = []; // parent,child,unit vector for orientation
		nodeTemplate.Add(NewNode("Root", "SpineA",			(0,0,0),			(0,1,0)		));
		nodeTemplate.Add(NewNode("SpineA", "SpineB",		(0,1,0),			(0,1,0)		));

		var nodes = ConstructBoneDictFromList(nodeTemplate);
		return new SkeletonEntity("FailCircularTree", nodes["Root"], nodes);
	}

	// Construction helpers

	private Dictionary<string, BoneNode> ConstructBoneDictFromList(List<(string, string, Transform)> nodeTemplate) {
		BoneNode root = Mock_BoneNode("Root");
		Dictionary<string, BoneNode> nodes = [];
		nodes.Add("Root", root);

		// first pass composition
		foreach (var x in nodeTemplate) {
			BoneNode bnode = new(x.Item2, x.Item3);
			nodes.Add(x.Item2, bnode);
		}

		// linking pass
		foreach (var(parent, child, xfm) in nodeTemplate) {
			// add children
			nodes[parent].Children.Add(child, nodes[child]);
			// add parent
			nodes[child].ParentBone = nodes[parent];
		}
		return nodes;
	}
	/// <summary>
	/// Create a tuple for mockup bonenode data
	/// Compatible with <see cref="ConstructBoneDictFromList(System.Collections.Generic.List{System.ValueTuple{string,string,Transform}})"/>
	/// </summary>
	/// <param name="parent">Name of parent node (must be unique in tree!)</param>
	/// <param name="name">Name of node to create (must be unique in tree)</param>
	/// <param name="position">X, Y, Z position in RootNode's local space.</param>
	/// <param name="facing">Yaw, pitch, roll. Set to -1/0/-1 for 90 degrees rotation.</param>
	/// <returns>
	/// A tuple compatible with <see cref="ConstructBoneDictFromList(System.Collections.Generic.List{System.ValueTuple{string,string,Transform}})"/>
	/// </returns>
	private (string, string, Transform) NewNode(string parent, string name, (float, float, float) position, (int, int, int) facing) {
		// TODO Quat
		Quaternion qn = Quaternion.CreateFromYawPitchRoll(MathF.PI/2*facing.Item1, MathF.PI/2*facing.Item2, MathF.PI/2*facing.Item3);
		Quat q = new(qn);
		return (parent, name, new Transform(
			position: new Vector3(position.Item1, position.Item2, position.Item3), 
			rotation: q)
			);
	}

}
