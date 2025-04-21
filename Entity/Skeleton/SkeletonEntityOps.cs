using System.Diagnostics;

namespace BoneUtils.Entity.Skeleton;
public class SkeletonEntityOps {
	public delegate bool SkeletonEntityMutator(ref SkeletonEntity skeleton);

	public void PreProcessSkeleton(ref SkeletonEntity skeleton, List<SkeletonEntityMutator> preProcessors) {
		foreach (var preProcessor in preProcessors) 
			if(!preProcessor(ref skeleton)) 
				throw new Exception($"SkeletonEntityMutator failed! {preProcessor.ToString}");
		// Raise exception if mutate fails, for the time being.
	}

	// Mutators

	public bool ValidateBoneNodeTree(ref SkeletonEntity sken) {
		// DFS validation of skeleton
		var seen = new HashSet<BoneNode>();

		return Recurse(sken.RootNode, seen, 0, 100);

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
	public bool LabelDepthBoneNodeTree(ref SkeletonEntity sken) {
		// BFS for setting BoneNode depth
		var queue = new Queue<(BoneNode node, int depth)>();
		int depthLimit = 100;

		queue.Enqueue((sken.RootNode, 0));
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

		sken.BoneDepth = maxDepth;
		return true;
	}
	public bool BoneNodeTreeBuildRenderLists(ref SkeletonEntity skeleton) {
		skeleton.RenderBoneCount = skeleton.Bones.Count;
		skeleton.RenderBones = [.. skeleton.Bones.Values];
		return Recurse(skeleton.RootNode);

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

	// Construction helpers

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
