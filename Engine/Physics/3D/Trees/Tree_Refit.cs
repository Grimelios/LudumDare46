﻿using Engine.Physics._3D.Utility;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace Engine.Physics._3D.Trees
{
    partial struct Tree
    {
        /// <summary>
        /// Refits the bounding box of every parent of the node recursively to the root.
        /// </summary>
        /// <param name="nodeIndex">Node to propagate a node change for.</param>
        public unsafe void RefitForNodeBoundsChange(int nodeIndex)
        {
            //Note that no attempt is made to refit the root node. Note that the root node is the only node that can have a number of children less than 2.
            var node = nodes + nodeIndex;
            var metanode = metanodes + nodeIndex;
            while (metanode->Parent >= 0)
            {
                //Compute the new bounding box for this node.
                var parent = nodes + metanode->Parent;
                ref var childInParent = ref (&parent->A)[metanode->IndexInParent];
                BoundingBox.CreateMerged(node->A.Min, node->A.Max, node->B.Min, node->B.Max, out childInParent.Min, out childInParent.Max);
                node = parent;
                metanode = metanodes + metanode->Parent;
            }
        }

        //TODO: Recursive approach is a bit silly. Our earlier nonrecursive implementations weren't great, but we could do better.
        //This is especially true if we end up changing the memory layout. If we go back to a contiguous array per level, refit becomes trivial.
        //That would only happen if it turns out useful for other parts of the execution, though- optimizing refits at the cost of self-tests would be a terrible idea.
        unsafe void Refit(int nodeIndex, out Vector3 min, out Vector3 max)
        {
            Debug.Assert(leafCount >= 2);
            var node = nodes + nodeIndex;
            ref var a = ref node->A;
            if (node->A.Index >= 0)
            {
                Refit(a.Index, out a.Min, out a.Max);
            }
            ref var b = ref node->B;
            if (b.Index >= 0)
            {
                Refit(b.Index, out b.Min, out b.Max);
            }
            BoundingBox.CreateMerged(a.Min, a.Max, b.Min, b.Max, out min, out max);
        }
        
        public unsafe void Refit()
        {
            //No point in refitting a tree with no internal nodes!
            if (leafCount <= 2)
                return;
            Refit(0, out var rootMin, out var rootMax);
        }       
        


    }
}
