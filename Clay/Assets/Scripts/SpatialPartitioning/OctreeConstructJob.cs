using System;
using Collision;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SpatialPartitioning
{
    [BurstCompile]
    public struct OctreeConstructJob : IJob
    {
        [ReadOnly] public NativeArray<Vector3> ToInsert;
        [ReadOnly] public AABB AABB;
        
        public NativeArray<OctNode> Nodes;
        public NativeReference<int> NodesCount;
        
        public NativeArray<OctValue> Values;
        public NativeReference<int> ValuesCount;
        
        public OctSettings Settings;


        public void Execute()
        {
            NodesCount.Value = 1;
            ValuesCount.Value = 0;
            Nodes[0] = new OctNode(AABB);
            
            var rootIndex = new IndexToOctNode(0);
            var root = rootIndex.GetElement(Nodes);
            
            for (int i = 0; i < ToInsert.Length; i++)
            {
                var point = ToInsert[i];
                if (root.PointOverlaps(point))
                {
                    IndexToOctValue valueIndex = IndexToOctValue.NewElement(Values, ValuesCount, OctValue.CreateTail(point));
                    InsertPointInNodeOrChildren(rootIndex, valueIndex);
                }
                else
                {
                    Debug.LogError("Cannot insert point outside of octree!");
                }
            }
        }
        
        private void InsertPointInNodeOrChildren(IndexToOctNode nodeIndex, IndexToOctValue valueToInsertIndex)
        {
            var node = nodeIndex.GetElement(Nodes);

            //if node is a leaf
            if (node.ValueCount >= 0)
            {
                #region insert into self

                //if no values currently in node
                if (!node.LastValue.HasValue())
                {
                    node.LastValue = valueToInsertIndex;
                }
                
                //otherwise find last element and link to new element
                else
                {
                    //connect new last value to previous
                    var newLastValue = valueToInsertIndex.GetElement(Values);
                    newLastValue.PreviousValue = node.LastValue;
                    valueToInsertIndex.SetElement(Values, newLastValue);
                    
                    node.LastValue = valueToInsertIndex; //set last value to new inserted value
                }

                node.ValueCount++;

                #endregion
                
                float theoreticalChildHalfWidth = node.AABB.HalfWidth / 2f;
                //if exceeded maxium allowed values,
                //and child would not be less than min half width
                if (node.ValueCount > Settings.MaxValuesPerNode &&
                    theoreticalChildHalfWidth > Settings.MinHalfSize)
                {
                    #region redistributie values into children if max values exceeded and can still subdivide
                    IndexToOctValue currentRedistributeValueIndex = node.LastValue;
                    while (currentRedistributeValueIndex.HasValue())
                    {
                        var currentRedistributeValue = currentRedistributeValueIndex.GetElement(Values);
                        var nextRedistributeValueCache = currentRedistributeValue.PreviousValue;

                        //break up linked list (child will reconstruct it appropriately)
                        currentRedistributeValue.PreviousValue = IndexToOctValue.Empty();
                        currentRedistributeValueIndex.SetElement(Values, currentRedistributeValue);
                        
                        InsertValueInChildren(ref node, currentRedistributeValueIndex);

                        currentRedistributeValueIndex = nextRedistributeValueCache;
                    }

                    //this node is no longer a leaf, revoke ownership of values
                    node.LastValue = IndexToOctValue.Empty();
                    node.ValueCount = -1; //makes node non-insertable (a leaf)
                    #endregion
                }
            }

            //if node is not a leaf
            else
            {
                InsertValueInChildren(ref node, valueToInsertIndex);
            }
            
            nodeIndex.SetElement(Nodes, node);
        }
        
        private void InsertValueInChildren(ref OctNode node, IndexToOctValue valueToInsert)
        {
            if (!node.HasChildren())
            {
                CreateAllChildrenAndPersist(ref node);
            }
                
            var octant = node.OctantAtPosition(valueToInsert.GetElement(Values).Position);
            var childNodeIndex = node.GetChildNodeFromOctant(octant);

            InsertPointInNodeOrChildren(childNodeIndex, valueToInsert);
        }
        
        private void CreateAllChildrenAndPersist(ref OctNode node)
        {
            //THE ORDER MATTERS
            //as the order in the array implicit tells the program what octant the child is
            node.FirstChildIndex = NodesCount.Value;
            CreateChildAtOctant(in node, Octant.___);
            CreateChildAtOctant(in node, Octant.X__);
            CreateChildAtOctant(in node, Octant._Y_);
            CreateChildAtOctant(in node, Octant.__Z);
            CreateChildAtOctant(in node, Octant.XY_);
            CreateChildAtOctant(in node, Octant.X_Z);
            CreateChildAtOctant(in node, Octant._YZ);
            CreateChildAtOctant(in node, Octant.XYZ);
        }
        
        private void CreateChildAtOctant(in OctNode node, Octant octant)
        {
            var octantPosition = OctHelpers.OctantToVector3Int(octant);
            var quarterWidth = node.AABB.HalfWidth / 2;
            var childOffset = (Vector3) octantPosition * quarterWidth;
            var childPos = node.AABB.Center + childOffset;

            var childNode = new OctNode(new AABB(childPos, quarterWidth));
            
            Nodes[NodesCount.Value] = childNode;
            NodesCount.Value++;
        }
    }
}