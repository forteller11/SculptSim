
using System;
using Collision;
using Unity.Assertions;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SpatialPartitioning
{
    public struct Octree : IDisposable
    {
        public NativeArray<OctNode> Nodes;
        public NativeReference<int> NodesCount;
        
        public NativeArray<OctValue> Values;
        public NativeReference<int> ValuesCount;
        
        public OctSettings Settings;

        public Octree(OctSettings settings, int maxParticles)
        {
            Nodes  = new NativeArray<OctNode>(maxParticles/2 + 40, Allocator.Persistent);
            Values = new NativeArray<OctValue>(maxParticles, Allocator.Persistent);
            
            NodesCount = new NativeReference<int>(0, Allocator.Persistent);
            ValuesCount = new NativeReference<int>(0, Allocator.Persistent);

            Settings = settings;
        }
        
        #region construction
        [BurstCompile]
        public OctreeConstructJob CreateConstructJob(NativeArray<Vector3> toInsert, AABB aabb)
        {
            return new OctreeConstructJob()
            {
                //input
                ToInsert = toInsert,
                AABB = aabb,
                
                //output
                Nodes = Nodes,
                NodesCount = NodesCount,
                Values = Values,
                ValuesCount = ValuesCount,
                Settings = Settings
            }; 
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