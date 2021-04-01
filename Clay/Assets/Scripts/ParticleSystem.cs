using Collision;
using DefaultNamespace;
using SpatialPartitioning;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct ParticleSystem : IJobParallelFor
{
    [ReadOnly] public ClaySimSettings Settings;
    [ReadOnly] public CurveNormalized ForceMultCurve;
    [ReadOnly] public Octree Octree;
    [ReadOnly] public float DeltaTime;
    
    public NativeArray<Vector3> QueryResults;
    public NativeArray<Vector3> ToMove;

    public void Execute(int index)
    {
        float deltaTime = DeltaTime;
        var particles = Settings.Particles;
        float minRadius = Settings.MinMaxRadius.x;
        float maxRadius = Settings.MinMaxRadius.y;
        float desiredPercentBetweenMinMax = Settings.DesiredPercentBetweenMinMax;
        int maxParticlesToSimulate = Settings.MaxParticlesToSimulate;
        float constantMult = Settings.ConstMult;
        
        var p1Pos = particles[index];
        var querySphere = new Sphere(p1Pos, maxRadius);
        var queryResults = QueryFiniteByMinDist(querySphere, maxParticlesToSimulate);

        for (int j = 0; j < queryResults.Length; j++)
        {
            var p2Pos = queryResults[j];

            Vector3 p1ToP2 = p2Pos - p1Pos;
            float p1P2Dist = p1ToP2.magnitude;
            Vector3 p1ToP2Dir = p1ToP2 / p1P2Dist;
                

            if (p1P2Dist < maxRadius)
            {
                float percentageBetweenMinMax = Mathf.InverseLerp(minRadius, maxRadius, p1P2Dist);
                float currentToDesiredPercentage = percentageBetweenMinMax - desiredPercentBetweenMinMax;
                float desiredDist = Mathf.Lerp(minRadius, maxRadius, desiredPercentBetweenMinMax);
                    
                float indexInCurve;
                if (p1P2Dist < desiredDist)
                    indexInCurve = -Mathf.InverseLerp(desiredDist, minRadius, p1P2Dist); //0, -1
                else 
                    indexInCurve = Mathf.InverseLerp(desiredDist, maxRadius, p1P2Dist); //0, 1
                    
                float scale = currentToDesiredPercentage * constantMult * ForceMultCurve.EvaluateDiscrete(indexInCurve) * deltaTime;
                Vector3 posToAddScaled = p1ToP2Dir * scale;
                    
                ToMove[index] += posToAddScaled;
            }
        }
    }
    
    
        
        //Query for all points
        //then filter out to get the [maxQuery] closest points to the sphere
        NativeList<Vector3> QueryFiniteByMinDist(Sphere sphere, int maxQuery)
        {
            var finiteResults = new NativeList<Vector3>(maxQuery, Allocator.Temp);
            var finiteResultsDistSqr = new NativeList<float>(maxQuery, Allocator.Temp);
            
            float currentMaxDistSqr = float.MinValue;
            int currentMaxIndex = -1;
            
            float maxDistOfSphereSqrd = sphere.Radius * sphere.Radius;
            
            var query = new NativeArray<Vector3>(Settings.Particles.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
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
}
