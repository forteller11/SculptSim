﻿
using System;
using Collision;
using Unity.Collections;
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
            Nodes = new NativeList<OctNode>(200, Allocator.Persistent);
            Values = new NativeList<OctValue>(1024, Allocator.Persistent);
            Settings = settings;
            
        }

        #region construction
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

        
        private void InsertPointInNodeOrChildren(IndexToOctNode nodeIndex, IndexToOctValue valueToInsertIndex)
        {
            var node = nodeIndex.GetElement(Nodes);

            //if node is a leaf
            if (node.ValueCount >= 0)
            {
                #region insert into self

                //if no values currently in node
                if (!node.FirstValue.HasValue())
                {
                    node.FirstValue = valueToInsertIndex;
                    node.LastValue  = valueToInsertIndex;
                }
                
                //otherwise find last element and link to new element
                else
                {
                    var lastValueIndex = node.LastValue;
                    
                    var lastValue = lastValueIndex.GetElement(Values);
                    lastValue.NextValue = valueToInsertIndex; //connect previous last value to insert
                    lastValueIndex.SetElement(Values, lastValue); //persist last element.NextValue changes to global array
                    
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
                    node.ValueCount = -1; //makes node non-insertable (a leaf)
                    #endregion
                }
            }

            //if node is not a leaf
            else
            {
                InsertValueInChildren(valueToInsertIndex);
            }

            //persist state of node index by copying it to global array
            nodeIndex.SetElement(Nodes, node);

            // <remarks> creates new children as necessary</remarks>
            void InsertValueInChildren(IndexToOctValue valueToInsert)
            {
                if (!node.HasChildren())
                {
                    CreateAllChildrenAndPersist(ref node);
                }
                
                var octant = node.OctantAtPosition(valueToInsert.GetElement(Values).Position);
                var childNodeIndex = node.GetChildNodeFromOctant(octant);

                InsertPointInNodeOrChildren(childNodeIndex, valueToInsert);
            }
        }
        
        void CreateAllChildrenAndPersist(ref OctNode node)
        {
            //THE ORDER MATTERS
            //as the order in the array implicit tells the program what octant the child is
            
            //todo set first child to appropriate index here
            //todo remove indextooctnode, is confusing now
            node.FirstChildIndex = Nodes.Length;
            CreateChildAtOctant(in node, Octant.___);
            CreateChildAtOctant(in node, Octant.X__);
            CreateChildAtOctant(in node, Octant._Y_);
            CreateChildAtOctant(in node, Octant.__Z);
            CreateChildAtOctant(in node, Octant.XY_);
            CreateChildAtOctant(in node, Octant.X_Z);
            CreateChildAtOctant(in node, Octant._YZ);
            CreateChildAtOctant(in node, Octant.XYZ);
        }
        
        void CreateChildAtOctant(in OctNode node, Octant octant)
        {
            var octantPosition = OctHelpers.OctantToVector3Int(octant);
            var quarterWidth = node.AABB.HalfWidth / 2;
            var childOffset = (Vector3) octantPosition * quarterWidth;
            var childPos = node.AABB.Center + childOffset;

            var childNode = new OctNode(new AABB(childPos, quarterWidth));
            
            Nodes.Add(childNode);
        }

        #endregion
        
        #region querying
        public int QueryNonAlloc(Sphere sphere, NativeArray<Vector3> results)
        {
            var root = Nodes[0];
            int resultsCount = 0;
            
            if (root.SphereOverlaps(sphere))
                GetOverlappingChildrenOrAddToResultsDepthFirst(sphere, root, results, ref resultsCount);
        
            // Debug.Log(resultsCount);
            return resultsCount;
        }

        void GetOverlappingChildrenOrAddToResultsDepthFirst(in Sphere sphere, in OctNode node, NativeArray<Vector3> results, ref int resultsCount)
        {
            //if a parent, recursively call function on children
            if (node.ValueCount < 0)
            {
                var children = node.GetChildren(Nodes);
                for (int i = 0; i < children.Length; i++)
                {
                    var child = children[i];
                    if (child.SphereOverlaps(sphere))
                        GetOverlappingChildrenOrAddToResultsDepthFirst(in sphere, in child, results, ref resultsCount);
                }
            }
            //otherwise get values and add to results
            else
            {
                IndexToOctValue currentValueIndex = node.FirstValue;
                while (currentValueIndex.HasValue())
                {
                    var currentElement = currentValueIndex.GetElement(Values); 
                    currentValueIndex = currentElement.NextValue;
                    results[resultsCount] = currentElement.Position;
                    resultsCount++;
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