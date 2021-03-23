Shader "Unlit/Clay04"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MinDistance  ("Min Distance Until Considered Hit", Float) = .1
        _ParticleRadius  ("Particle Visual Radius", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
            int _ParticlesLength;
            float4 _Particles[5]; //where xyz == pos, and w == radius
            float _MinDistance;
            float _ParticleRadius;

            float signedDistToSphere(float3 origin, float3 spherePosition, float sphereRadius)
            {
                const float signedDistance = distance(origin, spherePosition) - sphereRadius;
                return signedDistance;
            }
            
            float minDistToScene(float3 position)
            {
                float minDist = 4096; //a big number
                for (int i = 0; i < _ParticlesLength; i++)
                {
                    float currentDist = signedDistToSphere(position, _Particles[i].xyz, _ParticleRadius);
                    minDist = min(minDist, currentDist);
                }

                return minDist;
            }
            
            fixed4 frag (v2f input) : SV_Target
            {
                fixed4 color;
                
                float3 rayPos = input.worldPos;
                float3 rayDir = normalize(rayPos - _WorldSpaceCameraPos);

                for (int i = 0; i < 100; i++)
                {
                    const float minDistanceToScene = minDistToScene(rayPos);
                    
                    if (minDistanceToScene < _MinDistance)
                    {
                        color = fixed4(0,1,0,1);
                        return color;
                    }

                    const float3 amountToMarch = minDistanceToScene * rayDir;
                    rayPos += amountToMarch;
                }

                color = fixed4(1,0,0, 1);
                
                return color;
            }
            

            
            ENDCG
        }
    }
}
