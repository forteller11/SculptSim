
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
            Nodes.Add(new OctNode(aabb));
        }

        public void Insert(Vector3 point)
        {
            var rootIndex = new IndexToOctNode(0);
            var root = rootIndex.GetElement(Nodes);
            if (root.PointOverlaps(point))
            {
                IndexToOctValue valueIndex = IndexToOctValue.NewElement(Values, OctValue.CreateTail(point));
                InsertPointInNodeOrChildren(rootIndex, valueIndex);
            }
            else
            {
                throw new ArgumentException("Cannot insert point outside of octree!");
            }

        }

        
        private void InsertPointInNodeOrChildren(IndexToOctNode nodeIndex, IndexToOctValue valueIndex)
        {
            var node = nodeIndex.GetElement(Nodes);

            if (node.IsLeaf != 0)
            {
                #region insert into self

                //if no values currently in node
                if (!node.FirstValue.HasValue())
                {
                    node.FirstValue = valueIndex;
                }
                
                //otherwise find last element and link to new element
                else
                {
                    var lastValueIndex = GetTail(node.FirstValue);
                    
                    var lastValue = lastValueIndex.GetElement(Values);
                    lastValue.NextValue = valueIndex;
                    
                    lastValueIndex.SetElement(Values, lastValue); //persist last element.NextValue changes to global array
                    
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
                    IndexToOctValue currentRedistributeValueIndex = node.FirstValue;
                    while (currentRedistributeValueIndex.HasValue())
                    {
                        var currentRedistributeValue = currentRedistributeValueIndex.GetElement(Values);
                        var nextRedistributeValueCache = currentRedistributeValue.NextValue;

                        //break up linked list (child will reconstruct it appropriately)
                        currentRedistributeValue.NextValue = IndexToOctValue.Empty();
                        currentRedistributeValueIndex.SetElement(Values, currentRedistributeValue);
                        
                        InsertValueInChildren(currentRedistributeValueIndex);

                        currentRedistributeValueIndex = nextRedistributeValueCache;
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
                InsertValueInChildren(valueIndex);
            }

            //persist state of node index by copying it to global array
            nodeIndex.SetElement(Nodes, node);

            // <remarks> creates new children as necessary</remarks>
            void InsertValueInChildren(IndexToOctValue valueToInsert)
            {
                var octant = node.OctantAtPosition(valueToInsert.GetElement(Values).Position);
                var childNodeIndex = node.GetChildNodeFromOctant(octant);
            
                //if it doesn't exist, create new child and set to appropriate octNode child member
                if (!childNodeIndex.HasValue())
                {
                    #region create child node at octant
                    var octantPosition = OctHelpers.OctantToVector3Int(octant);
                    var quarterWidth = node.AABB.HalfWidth / 2;
                    var childOffset = (Vector3) octantPosition * quarterWidth;
                    var childPos = node.AABB.Center + childOffset;

                    var childNode = new OctNode(new AABB(childPos, quarterWidth));
                    childNodeIndex = IndexToOctNode.NewElement(Nodes, childNode);

                    node.SetChildNodeIndexFromOctant(octant, childNodeIndex);
                    #endregion
                }

                InsertPointInNodeOrChildren(childNodeIndex, valueToInsert);
            }
        }

        public IndexToOctValue GetTail(IndexToOctValue octValueIndex)
        {
            IndexToOctValue currentValue = octValueIndex;
            IndexToOctValue previousValue = IndexToOctValue.Empty();

            int upperLimit = 0;
            while (currentValue.HasValue())
            {
                previousValue = currentValue;
                currentValue  = currentValue.GetElement(Values).NextValue;

                upperLimit++;
                if (upperLimit > Values.Length)
                    throw new Exception("Ifinite loop!");
            }    

            if (!previousValue.HasValue())
                throw new Exception("Cant call last value if there isn't a first value!");
            
            return previousValue;
        }

        #region querying
        public bool QueryNonAlloc(Sphere sphere, NativeList<Vector3> results)
        {
            var root = Nodes[0];
            if (root.SphereOverlaps(sphere))
                GetOverlappingChildrenOrAddToResultsDepthFirst(sphere, root, results);
        
            return results.Length > 0;
        }

        void GetOverlappingChildrenOrAddToResultsDepthFirst(in Sphere sphere, in OctNode node, NativeList<Vector3> results)
        {
            if (node.IsLeaf == 0)
            {
                //todo remove closure allocation...
                var children = node.GetChildren(Nodes);
                for (int i = 0; i < children.Length; i++)
                {
                    var child = children[i];
                    if (child.SphereOverlaps(sphere))
                        GetOverlappingChildrenOrAddToResultsDepthFirst(sphere, child, results);
                }
            }
            else
            {
                IndexToOctValue currentValueIndex = node.FirstValue;
                while (currentValueIndex.HasValue())
                {
                    var currentElement = currentValueIndex.GetElement(Values); 
                    currentValueIndex = currentElement.NextValue;
                    results.Add(currentElement.Position);
                }
            }
        }
        
        /// <summary>
        /// converts linked list of values of a node to a contiguous nativelist
        /// </summary> 
        public int GetValuesAsArray(in OctNode node, out NativeList<OctValue> results, Allocator allocator = Allocator.Temp)
        {
            results = new NativeList<OctValue>(Settings.MaxValuesPerNode, allocator);

            IndexToOctValue currentValueIndex = node.FirstValue;
            while (currentValueIndex.HasValue())
            {
                var currentElement = currentValueIndex.GetElement(Values); 
                currentValueIndex = currentElement.NextValue;
                results.Add(currentElement);
            }

            return results.Length;
        }
        #endregion

        public void Dispose()
        {
            Nodes.Dispose();
            Values.Dispose();
        }
    }
}