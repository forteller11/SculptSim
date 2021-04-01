using System;
using Collision;
using DefaultNamespace;
using SpatialPartitioning;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct ParticleSimJob : IJobParallelFor, IDisposable
{
    #region inputs
    [ReadOnly] public Octree Octree;
    [ReadOnly] public NativeArray<Vector3> Positions;
    [ReadOnly] public CurveNormalized ForceMultCurve;
    public float ConstMult;
    public float MinRadius;
    public float MaxRadius;
    public float DesiredPercentBetweenMinMax;
    public int MaxParticlesToSimulate;
    public float DeltaTime;
    #endregion
    
    
    public NativeArray<Vector3> ToMove;

    public void Execute(int index)
    {
        float deltaTime = DeltaTime;

        var p1Pos = Positions[index];
        var querySphere = new Sphere(p1Pos, MaxRadius);
        var queryResults = QueryFiniteByMinDist(querySphere, MaxParticlesToSimulate);

        for (int j = 0; j < queryResults.Length; j++)
        {
            var p2Pos = queryResults[j];

            Vector3 p1ToP2 = p2Pos - p1Pos;
            float p1P2Dist = p1ToP2.magnitude;
            Vector3 p1ToP2Dir = p1ToP2 / p1P2Dist;
                

            if (p1P2Dist < MaxRadius)
            {
                float percentageBetweenMinMax = Mathf.InverseLerp(MinRadius, MaxRadius, p1P2Dist);
                float currentToDesiredPercentage = percentageBetweenMinMax - DesiredPercentBetweenMinMax;
                float desiredDist = Mathf.Lerp(MinRadius, MaxRadius, DesiredPercentBetweenMinMax);
                    
                float indexInCurve;
                if (p1P2Dist < desiredDist)
                    indexInCurve = -Mathf.InverseLerp(desiredDist, MinRadius, p1P2Dist); //0, -1
                else 
                    indexInCurve = Mathf.InverseLerp(desiredDist, MaxRadius, p1P2Dist); //0, 1
                    
                float scale = currentToDesiredPercentage * ConstMult * ForceMultCurve.EvaluateDiscrete(indexInCurve) * deltaTime;
                Vector3 posToAddScaled = p1ToP2Dir * scale;
                    
                ToMove[index] += posToAddScaled;
            }
        }
    }
    
    
        
        //Query for all points
        //then filter out to get the [maxQuery] closest points to the sphere
        NativeList<Vector3> QueryFiniteByMinDist(Sphere sphere, int maxQuery)
        {
            var finiteResults = new NativeList<Vector3>(maxQuery, Allocator.TempJob);
            var finiteResultsDistSqr = new NativeList<float>(maxQuery, Allocator.TempJob);
            
            float currentMaxDistSqr = float.MinValue;
            int currentMaxIndex = -1;
            
            float maxDistOfSphereSqrd = sphere.Radius * sphere.Radius;
            
            var query = new NativeArray<Vector3>(Positions.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            int resultsCount = Octree.QueryNonAlloc(sphere, query);
            
            
            for (int i = 0; i < resultsCount; i++)
            {
                
                var currentQuery = query[i];
                var distSqr = Vector3.SqrMagnitude(currentQuery - sphere.Position);

                //if the same particle (the same pos), or if outside max range, continue
                if (distSqr == 0 || 
                    distSqr > maxDistOfSphereSqrd)
                {
                    continue;
                }
                
                //if haven't filled up _maxParticlesToSimulate, just add the query to the finite buffers
                if (finiteResults.Length < maxQuery)
                {
                    finiteResults.Add(currentQuery);
                    finiteResultsDistSqr.Add(distSqr);
                    
                    if (distSqr > currentMaxDistSqr)
                    {
                        currentMaxDistSqr = distSqr;
                        currentMaxIndex = finiteResults.Length - 1;
                    }
                }
                
                else
                {
                    //otherwise replace the current max index, only if the current query has a larger distSqrd
                    //then go through the finite buffers to find the new largest dist sqrd from the sphere
                    if (distSqr < currentMaxDistSqr)
                    {
                        finiteResults[currentMaxIndex] = currentQuery;
                        finiteResultsDistSqr[currentMaxIndex] = distSqr;

                        currentMaxDistSqr = float.MinValue;
                        currentMaxIndex = -1;
                        
                        for (int j = 0; j < finiteResults.Length; j++)
                        {
                            if (finiteResultsDistSqr[j] > currentMaxDistSqr)
                            {
                                currentMaxDistSqr = finiteResultsDistSqr[j];
                                currentMaxIndex = j;
                            }
                        }
                    }
                } //end-else
            } //end-forloop
            
            return finiteResults;
        }

        public void Dispose()
        {
            Octree.Dispose();
            Positions.Dispose();
            ForceMultCurve.Dispose();
            ToMove.Dispose();
        }
}
