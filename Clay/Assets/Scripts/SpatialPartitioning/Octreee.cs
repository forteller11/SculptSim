
using System;
using Collision;
using Unity.Collections;
using UnityEngine;

namespace SpatialPartitioning
{
    public struct Octree : IDisposable
    {
        
        //todo use lists instead, .Clear() doesn't impose any real perf decreases
        public NativeArray<OctNode> Nodes;
        public int NodesCount;
        
        public NativeArray<OctValue> Values;
        public int ValuesCount;
        
        public OctSettings Settings;

        public Octree(OctSettings settings, int maxParticles)
        {
            Nodes  = new NativeArray<OctNode>(maxParticles/2 + 40, Allocator.Persistent);
            Values = new NativeArray<OctValue>(maxParticles+1, Allocator.Persistent);
            
            NodesCount  = 0;
            ValuesCount = 0;
            
            Settings = settings;
        }

        #region construction
        public void CleanAndPrepareForInsertion(AABB aabb)
        {
            NodesCount  = 1;
            ValuesCount = 1; //todo make values start at 0 not 1???
            
            Nodes[0] = new OctNode(aabb);
        }

        public void Insert(Vector3 point)
        {
            var rootIndex = new IndexToOctNode(0);
            var root = rootIndex.GetElement(Nodes);
            if (root.PointOverlaps(point))
            {
                IndexToOctValue valueIndex = IndexToOctValue.NewElement(Values, ref ValuesCount, OctValue.CreateTail(point));
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
            node.FirstChildIndex = NodesCount;
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
            
            Nodes[NodesCount] = childNode;
            NodesCount++;
        }

        #endregion

        #region querying normal
        public int QueryNonAlloc(Sphere sphere, NativeArray<Vector3> results)
        {
            var root = Nodes[0];
            int resultsCount = 0;
            
            if (root.SphereOverlaps(sphere))
                GetOverlappingChildrenOrAddToResultsDepthFirst(sphere, root, results, ref resultsCount);
            
            return resultsCount;
        }

        private void GetOverlappingChildrenOrAddToResultsDepthFirst(in Sphere sphere, in OctNode node, NativeArray<Vector3> results, ref int resultsCount)
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
                IndexToOctValue currentValueIndex = node.LastValue;
                while (currentValueIndex.HasValue())
                {
                    var currentElement = currentValueIndex.GetElement(Values); 
                    currentValueIndex = currentElement.PreviousValue;
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

            IndexToOctValue currentValueIndex = node.LastValue;
            while (currentValueIndex.HasValue())
            {
                var currentElement = currentValueIndex.GetElement(Values); 
                currentValueIndex = currentElement.PreviousValue;
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