namespace BoneUtils.Entity.Skeleton;
public class SkeletonEntityOps {
	public bool ValidateBoneNodeTree(SkeletonEntity sken) {
		// DFS validation of skeleton
		var seen = new Dictionary<string, BoneNode>();
		int depth = 0;
		int maxDepth = 100;
		seen.Add("Root", sken.RootNode);

		return RecurseBoneNode(sken.RootNode, seen, depth, maxDepth);
	}
	private bool RecurseBoneNode(BoneNode bn, Dictionary<string, BoneNode> seen, int depth, int maxDepth) {
		if(depth+1 > maxDepth) return false;

		depth++;

		foreach(var bone in bn.Children) {
			if(seen.ContainsKey(bone.Key))
				return false;
			else
				seen.Add(bone.Key, bone.Value);

			if(!RecurseBoneNode(bone.Value, seen, depth, maxDepth))
				return false;
		}
		return true;
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
