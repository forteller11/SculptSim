Shader "Unlit/Clay01"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
#pragma exclude_renderers d3d11 gles
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float4 _ParticlePositions [];
            int _ParticleLength;
            fixed4 frag (v2f input) : SV_Target 
            {
                fixed4 col = (0,0,0,0);
                
                for (int i = 0; i < 20; i++)
                {
                    //float3 pos = _ParticlePositions[i].xyz;
                    
                    col += 0.01f;
                }
                
                UNITY_APPLY_FOG(input.fogCoord, col);
                return col;
            }

            // float minDistToScene(float3 from)
            // {
            //     
            // }

            //get nearest dist to scene
            //move ray tht way (along cam)
            //repeat until min dist is reached
            //calc norms
            ENDCG
        }
    }
}
