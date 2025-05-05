using BoneUtils.Math;
using System.Numerics;

namespace BoneUtils.Entity.Skeleton;
public class SkeletonEntityOps {
	// (!) DFS and BFS limits have been set high to accomodate HelixDemo
	// For any real world integration, adjust to your usecase.
	public int DFSLimit = 10000;
	public int BFSLimit = 10000;

	public delegate bool SkeletonEntityMutator(ref SkeletonEntity skeleton);

	public void PreProcessSkeleton(ref SkeletonEntity skeleton, List<SkeletonEntityMutator> preProcessors) {
		foreach (var preProcessor in preProcessors) {
				// Raise exception if mutate fails, for the time being.
			if(!preProcessor(ref skeleton)) 
				throw new Exception($"SkeletonEntityMutator failed! {preProcessor.Method.Name}");
			else
				skeleton.Mutators.Add(preProcessor.Method.Name);
	}
	}

	// Mutators

	/// <summary>
	/// Checks node tree in Skeleton for circularity. 
	/// Run this early to validate the skeleton!
	/// </summary>
	/// <param name="sken">Skeleton containing nodes</param>
	/// <returns>false if skeleton has no nodes, or contains circular links in the node tree, or DFS depth is reached (n > 100)</returns>
	public bool ValidateBoneNodeTree(ref SkeletonEntity sken) {
		if(sken.Bones.Count == 0) return false;

		// DFS validation of skeleton
		var seen = new HashSet<BoneNode>();

		return Recurse(sken.RootNode, seen, 0, DFSLimit);

		bool Recurse(BoneNode bn, HashSet<BoneNode> seen, int depth, int maxDepth) {
			if(depth > maxDepth) return false;

			if(!seen.Add(bn))
				return false;

			foreach(var bone in bn.Children) {
				if(!Recurse(bone.Value, seen, depth+1, maxDepth)) 
					return false;
			}
			return true;
		}
	}

	/// <summary>
	/// Sets the BoneNode.TreeDepth property for each node in the skeleton.
	/// </summary>
	/// <param name="skeleton">Skeleton containing nodes</param>
	/// <returns>false if the skeleton has no nodes or DFS depth is reached (n > 100)</returns>
	public bool LabelDepthBoneNodeTree(ref SkeletonEntity skeleton) {
		if(!skeleton.Mutators.Contains(nameof(ValidateBoneNodeTree))) return false;

		// BFS for setting BoneNode depth
		var queue = new Queue<(BoneNode node, int depth)>();
		int depthLimit = BFSLimit;

		queue.Enqueue((skeleton.RootNode, 0));
		int maxDepth = 0;

		while(queue.Count > 0) {
			var (node, depth) = queue.Dequeue();
			node.TreeDepth = depth;

			maxDepth = int.Max(maxDepth, depth);
			if(maxDepth > depthLimit) 
				return false;

			foreach(var bone in node.Children.Values) {
				queue.Enqueue((bone, depth+1));
			}
		}

		skeleton.BoneDepth = maxDepth;
		return true;
	}

	/// <summary>
	/// Builds a list of Bones to supplement dictionary access. 
	/// Places a complete list of node references in SkeletonEntity.RenderBones, 
	/// and the count in SkeletonEntity.RenderBoneCount.
	/// Also builds local lists for each BoneNode and their direct leaf nodes, placed
	/// in BoneNode.RenderChildren and RenderChildrenCount.
	/// </summary>
	/// <param name="skeleton">Skeleton containing nodes</param>
	/// <returns>false if skeleton has no nodes or DFS depth is reached (n > 100)</returns>
	public bool BoneNodeTreeBuildRenderLists(ref SkeletonEntity skeleton) {
		if(!skeleton.Mutators.Contains(nameof(ValidateBoneNodeTree))) return false;

		skeleton.RenderBoneCount = skeleton.Bones.Count;
		skeleton.RenderBones = [.. skeleton.Bones.Values];
		return Recurse(skeleton.RootNode, 0, DFSLimit);

		bool Recurse(BoneNode node, int depth = 0, int maxDepth = 100) {
			if(depth > maxDepth) return false;

			node.RenderChildrenCount = node.Children.Count;
			node.RenderChildren = [.. node.Children.Values];
			
			foreach(var child in node.Children.Values) {
				Recurse(child, depth+1, maxDepth);
			}
			return true;
		}
	}

	/// <summary>
	/// Calculates the R3 parent-child distances between nodes in the tree,
	/// storing them in Bonenode.ParentRelativePosition.
	/// </summary>
	/// <param name="skeleton">Skeleton containing nodes.</param>
	/// <returns>false if skeleton has no nodes or BFS run limit is reached (n > 500).</returns>
	public bool BoneNodeTreeCalculateConstraints(ref SkeletonEntity skeleton) {
		if(!skeleton.Mutators.Contains(nameof(ValidateBoneNodeTree))) return false;

		var queue = new Queue<BoneNode>();
		int runLimit = BFSLimit, runs = 0;

		queue.Enqueue(skeleton.RootNode);
		Vector3 parentPosition = skeleton.RootNode.Transform.Position;
		Quat parentOrientation = skeleton.RootNode.Transform.Rotation;

		while(queue.Count > 0) {
			if(runs > runLimit)
				return false;
			var node = queue.Dequeue();

			Vector3 relativePosition = node.Transform.Position - parentPosition;
			node.ParentRelativePosition = new ParentRelativePosition {
				ParentOrientation = parentOrientation,
				NodePosition = relativePosition,
				Distance = relativePosition.Length()
			};

			// Update parentOrientation before queueing children
			parentOrientation = node.Transform.Rotation;
			parentPosition = node.Transform.Position;

			foreach(var child in node.Children.Values) {
				queue.Enqueue(child);
			}
			runs++;
		}
		return true;
	}

	/// <summary>
	/// Links each BoneNode to its SkeletonEntity owner.
	/// </summary>
	/// <param name="skeleton">Skeleton containing nodes to link.</param>
	/// <returns>false if there are no bones to link.</returns>
	public bool BoneNodeTreeSetParentEntity(ref SkeletonEntity skeleton) {
		if(skeleton.Bones.Count == 0) return false;

		foreach(BoneNode node in skeleton.Bones.Values) 
			node.ParentEntity = skeleton;
		return true;
	}

	// Construction helpers

	/// <summary>
	/// Takes a list of tuples and constructrs a BoneNode tree for SkeletonEntity.
	/// </summary>
	/// <param name="nodeTemplate">List of tuples where the strings are names of nodes to be created.
	/// 1st string: Name of parent node.
	/// 2nd string. Unique name of node to be created.
	/// Transform: Transform of node to be created.
	/// </param>
	/// <param name="addRootNode">Adds a RootNode without a parent.</param>
	/// <returns>Dictionary ready to use for SkeletonEntity.Bones</returns>
	public Dictionary<string, BoneNode> ConstructBoneNodeTreeFromList(List<(string, string, Transform)> nodeTemplate, bool addRootNode = true) {
		Dictionary<string, BoneNode> nodes = [];

		if(addRootNode) {
			BoneNode root = new("Root", new Transform());
			nodes.Add("Root", root);
		}

		foreach (var (parent, child, transform) in nodeTemplate) {
			BoneNode bnode = new(child, transform);
			nodes.Add(child, bnode);
		}

		foreach (var (parent, child, transform) in nodeTemplate) {
			nodes[parent].Children.Add(child, nodes[child]);
			nodes[child].ParentBone = nodes[parent];
		}

		return nodes;
	}
}
