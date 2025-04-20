using System.Diagnostics;

namespace BoneUtils.Entity.Skeleton;
public class SkeletonEntityOps {
	public bool ValidateBoneNodeTree(SkeletonEntity sken) {
		// DFS validation of skeleton
		var seen = new Dictionary<string, BoneNode> {
			{ "Root", sken.RootNode }
		};

		return Recurse(sken.RootNode, seen, 0, 100);

		bool Recurse(BoneNode bn, Dictionary<string, BoneNode> seen, int depth, int maxDepth) {
			if(depth+1 > maxDepth) return false;

			depth++;

			foreach(var bone in bn.Children) {
				if(seen.ContainsKey(bone.Key))
					return false;
				else
					seen.Add(bone.Key, bone.Value);

				if(!Recurse(bone.Value, seen, depth, maxDepth))
					return false;
			}
			return true;
		}
	}
	public int? LabelDepthBoneNodeTree(SkeletonEntity sken, int depthLimit = 100) {
		// BFS for setting BoneNode depth
		var queue = new Queue<(BoneNode node, int depth)>();

		queue.Enqueue((sken.RootNode, 0));
		int maxDepth = 0;

		while(queue.Count > 0) {
			var (node, depth) = queue.Dequeue();
			node.TreeDepth = depth;

			maxDepth = int.Max(maxDepth, depth);
			if(maxDepth > depthLimit) 
				return null;


			foreach(var bone in node.Children.Values) {
				queue.Enqueue((bone, depth+1));
			}
		}

		sken.BoneDepth = maxDepth;
		return maxDepth;
	}
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
