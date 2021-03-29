
using System;
using System.Collections.Generic;
using Collision;
using Unity.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace SpatialPartitioning
{
    public class Octree : IDisposable
    {
        public NativeList<OctNode> Nodes;
        public NativeList<OctValue> Values;
        public OctSettings Settings;

        public Octree(OctSettings settings)
        {
            Nodes = new NativeList<OctNode>(128, Allocator.Persistent);
            Values = new NativeList<OctValue>(1024, Allocator.Persistent);
            Settings = settings;
        }

        public void CleanAndPrepareForInsertion(AABB aabb)
        {
            Nodes.Clear();
            Values.Clear();
            Nodes.Add(new OctNode(new IndexToOctNode(Nodes.Length), aabb, Settings));
        }

        public void Insert(Vector3 point)
        {
            var rootIndex = new IndexToOctNode(0);
            var root = rootIndex.GetElement(Nodes);
            if (root.PointOverlaps(point))
            {
                InsertPointNodeOrChildren(rootIndex, point);
            }
            else
            {
                throw new ArgumentException("Cannot insert point outside of octree!");
            }

        }

        private void InsertPointNodeOrChildren(IndexToOctNode nodeIndex, Vector3 point)
        {
            var node = nodeIndex.GetElement(Nodes);

            if (node.IsLeaf != 0)
            {
                #region insert into self

                //if no values currently in node
                var pointValue = OctValue.CreateTail(point);

                if (!node.FirstValue.HasValue())
                {
                    node.FirstValue.AddElement(Values, pointValue);
                }
                //otherwise find last element and link to new element
                else
                {
                    var lastElementIndex = GetTail(node.FirstValue);
                    var lastElement = lastElementIndex.GetElement(Values);
                    lastElement.NextValue.AddElement(Values, pointValue);
                    lastElementIndex.SetElement(Values, lastElement);
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
                    IndexToOctValue currentValueIndex = node.FirstValue;
                    while (currentValueIndex.HasValue())
                    {
                        var currentValue = currentValueIndex.GetElement(Values);
                        var nextValueIndexCache = currentValue.NextValue;

                        //break up linked list (child will reconstruct it appropriately)
                        currentValue.NextValue = IndexToOctValue.Empty();
                        currentValueIndex.SetElement(Values, currentValue);
                        
                        InsertValueInChildren(nodes, values, currentValue);

                        currentValueIndex = nextValueIndexCache;
                    }

                    //this node is no longer a leaf, revoke ownership of values
                    node.FirstValue = IndexToOctValue.Empty();
                    node.IsLeaf = 0; //todo: remove use of IsLeaf by just using if (valueCount > SpecialReallyBigNumber)
                    node.ValueCount = 0;
                    #endregion
                }
            }

            else
            {
                InsertValueInChildren(nodes, values, currentValue);
            }

            nodeIndex.SetElement(Nodes, node);

        }

        // <remarks> creates new children as necessary</remarks>
        void InsertValueInChildren(ref OctNode node, Vector3 point)
        {
            var octant = node.OctantAtPosition(point);
            var childIndex = node.GetChildNodeFromOctant(octant);
            
            //if it doesn't exist, create new child and set to appropriate octNode child member
            if (!childIndex.HasValue())
            {
                childIndex = CreateChildNodeAtOctant(nodes, octant);
            }
            
            childIndex.GetElement(nodes).InsertValueInSelfOrChildren(nodes, values, value);
        }
        
        IndexToOctNode CreateChildNodeAtOctant(NativeList<OctNode> nodes, Octant octant)
        {
            var octantPosition = OctantToVector3Int(octant);
            var quarterWidth = AABB.HalfWidth / 2;
            var childOffset = (Vector3) octantPosition * quarterWidth;
            var childPos = AABB.Center + childOffset;

            var childNode = new OctNode(new IndexToOctNode(nodes.Length), new AABB(childPos, quarterWidth), Settings);
            var childNodeIndex = IndexToOctNode.Empty();
            childNodeIndex.AddElement(nodes, childNode);
            
            SetChildNodeFromOctant(nodes, octant, childNode);
            SelfIndex.SetElement(nodes, this);

            return childNodeIndex;
        }
        
        public IndexToOctValue GetTail(IndexToOctValue octValueIndex)
        {
            IndexToOctValue currentValue = octValueIndex;
            IndexToOctValue previousValue = IndexToOctValue.Empty();
            
            while (currentValue.HasValue())
            {
                previousValue = currentValue;
                currentValue  = currentValue.GetElement(Values).NextValue;
            }

            if (!previousValue.HasValue())
                throw new Exception("Cant call last value if there isn't a first value!");
            
            return previousValue;
        }



        public bool QueryNonAlloc(Sphere sphere, NativeList<Vector3> results)
        {
            var root = Nodes[0];
            if (root.SphereOverlaps(sphere))
                GetOverlappingChildrenOrAddToResultsDepthFirst(sphere, root, results);
        
            return results.Length > 0;
        }

        void GetOverlappingChildrenOrAddToResultsDepthFirst(Sphere sphere, OctNode node, NativeList<Vector3> results)
        {
            if (node.IsLeaf != 0)
            {
                //todo remove closure allocation...
                node.ForEachChild(Nodes, (child) =>
                {
                    if (child.SphereOverlaps(sphere))
                        GetOverlappingChildrenOrAddToResultsDepthFirst(sphere, child, results);
                });
            }
            else
            {
                node.GetValues(Values, out var values);
                for (int i = 0; i < values.Length; i++)
                    results.Add(values[i].Position);
                values.Dispose();
            }
        }

        public void Dispose()
        {
            Nodes.Dispose();
            Values.Dispose();
        }
    }
}