Shader "Unlit/Clay04"
{
    Properties
    {
        _MinDistance  ("Min Distance Until Considered Hit", Float) = .1
        _ParticleRadius  ("Particle Visual Radius", Float) = 2
        _SmoothingFactor  ("Particle Smoothing", Float) = .1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        //todo AlphaBlending 
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 worldPos : POSITION1;
            };

            v2f vert (appdata input)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(input.vertex);
                o.worldPos = mul(unity_ObjectToWorld, input.vertex);
                return o;
            }
      

            // polynomial smooth min (k = 0.1);
            float smin( float a, float b, float k )
            {
                float h = clamp( 0.5+0.5*(b-a)/k, 0.0, 1.0 );
                return lerp( b, a, h ) - k*h*(1.0-h);
            }

            float signedDistToSphere(float3 position, float3 spherePosition, float sphereRadius)
            {
                const float signedDistance = distance(position, spherePosition) - sphereRadius;
                return signedDistance;
            }
           
            
            int _ParticlesLength;
            float4 _Particles[5]; //where xyz == pos
            float _MinDistance;
            float _ParticleRadius;
            float _SmoothingFactor;


            float signedDistToScene(float3 position)
            {
                float minDist = 4096; //a big number
                for (int i = 0; i < _ParticlesLength; i++)
                {
                    float currentDist = signedDistToSphere(position, _Particles[i].xyz, _ParticleRadius);
                    minDist = smin(minDist, currentDist, _SmoothingFactor);
                }

                return minDist;
            }

            //translated from: http://jamie-wong.com/2016/07/15/ray-marching-signed-distance-functions/
            float3 estimateNormal(float3 p) {
                const float EPISILON = 0.0001;
                return normalize(float3(
                    signedDistToScene(float3(p.x + EPISILON, p.y, p.z)) - signedDistToScene(float3(p.x - EPISILON, p.y, p.z)),
                    signedDistToScene(float3(p.x, p.y + EPISILON, p.z)) - signedDistToScene(float3(p.x, p.y - EPISILON, p.z)),
                    signedDistToScene(float3(p.x, p.y, p.z + EPISILON)) - signedDistToScene(float3(p.x, p.y, p.z - EPISILON))
                ));
            }

            
            
            fixed4 frag (v2f input) : SV_Target
            {
                fixed4 color;
                
                float3 rayPos = input.worldPos;
                float3 rayDir = normalize(rayPos - _WorldSpaceCameraPos);

                for (int i = 0; i < 500; i++)
                {
                    const float minDistanceToScene = signedDistToScene(rayPos);
                    
                    if (minDistanceToScene < _MinDistance)
                    {
                        float3 normal = estimateNormal(rayPos);
                        color = fixed4(normal.xyz, 1);
                        return color;
                    }

                    const float3 amountToMarch = minDistanceToScene * rayDir;
                    rayPos +=  amountToMarch;
                }

                return fixed4(0,0,0,0);
             
            }
            

            
            ENDCG
        }
    }
}
