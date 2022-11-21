Shader "kinect/PointCloud"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        squareSize          ("Square size", float)      = 0.01
        removeBackground    ("Remove background", int)  = 0
    }

    SubShader {
        Tags { "RenderType"="Opaque" }

        LOD 100
        Lighting Off

        Pass {

            CGPROGRAM 
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom

            #include "UnityCG.cginc"

            uniform float squareSize;
            uniform int   removeBackground;

            // StructuredBuffer<float>  bodyIndexBuffer;
            // StructuredBuffer<float2> depthSpaceCoordinates;
            // StructuredBuffer<float2> colorSpaceCoordinates;
            // StructuredBuffer<float3> cameraSpacePositions;

            //sampler2D colorTexture;
            sampler2D _MainTex;


            sampler2D _bakedPositions;
            sampler2D _bakedUVs;
            sampler2D _bakedBodyIndexes;
            
            // Texture2D _bakedPositions;
            // SamplerState sampler_bakedPositions;

            // Texture2D _bakedUVs;
            // SamplerState sampler_bakedUVs;
            
            // Texture2D _bakedBodyIndexes;
            // SamplerState sampler_bakedBodyIndexes;

            uint colorFrameWidth, colorFrameHeight;
            //uint depthFrameWidth, depthFrameHeight;

            uniform uint nPointsHorizontal;
            uniform uint nPointsVertical;

            uniform float4 positionOffset;

            float4 _MainTex_ST;

            struct appdata {
                uint   id  : SV_VertexID;
            };

            struct v2g {
                uint   id  : VERTEXID;
            };

            struct g2f {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2g vert(appdata v) { 

                v2g o;
                o.id = v.id;
                return o;
            } 

            [maxvertexcount(6)] 
            void geom(point v2g v[1], inout TriangleStream<g2f> stream) {
                uint id = v[0].id;
                
                // reject extra points
                // if(id >= nPointsHorizontal * nPointsVertical) return;
                


                float2 sampleCoords = {
                    ((float)id % nPointsHorizontal) / nPointsHorizontal,
                    ((float)id / nPointsHorizontal) / nPointsVertical
                };

                float4 pos = tex2Dlod(_bakedPositions, float4(sampleCoords,0,0));
                pos += positionOffset;

                float4 uv  = tex2Dlod(_bakedUVs,       float4(sampleCoords,0,0));
                uv.x = uv.x / colorFrameWidth;
                uv.y = uv.y / colorFrameHeight;
        
                if(removeBackground) {
                    if(tex2Dlod(_bakedBodyIndexes, float4(sampleCoords,0,0)).r == 0.0f)
                        return;
                }


                g2f topLeft, topRight, botLeft, botRight;

                // offset in camera space // square will always face camera
                topLeft.pos  = mul(UNITY_MATRIX_MV, float4(pos.xyz, 1.0)) + float4(-squareSize,  squareSize, 0, 0);
                topRight.pos = mul(UNITY_MATRIX_MV, float4(pos.xyz, 1.0)) + float4( squareSize,  squareSize, 0, 0);
                botLeft.pos  = mul(UNITY_MATRIX_MV, float4(pos.xyz, 1.0)) + float4(-squareSize, -squareSize, 0, 0);
                botRight.pos = mul(UNITY_MATRIX_MV, float4(pos.xyz, 1.0)) + float4( squareSize, -squareSize, 0, 0);

                topLeft.pos  = mul(UNITY_MATRIX_P, float4(topLeft.pos .xyz, 1.0));
                topRight.pos = mul(UNITY_MATRIX_P, float4(topRight.pos.xyz, 1.0));
                botLeft.pos  = mul(UNITY_MATRIX_P, float4(botLeft.pos .xyz, 1.0));
                botRight.pos = mul(UNITY_MATRIX_P, float4(botRight.pos.xyz, 1.0));

                topLeft.uv  = uv.xy;
                topRight.uv = uv.xy;
                botLeft.uv  = uv.xy;
                botRight.uv = uv.xy;

                // first triangle
                    stream.Append(topLeft);
                    stream.Append(topRight);
                    stream.Append(botLeft);
                stream.RestartStrip();

                // second tiangle
                    stream.Append(botLeft);
                    stream.Append(topRight);
                    stream.Append(botRight);
                stream.RestartStrip();
            }


            fixed4 frag (g2f f) : SV_Target
            {
                
                fixed4 col = fixed4(255,255,255,255); //
                col = tex2D(_MainTex, f.uv);
                //UNITY_APPLY_FOG(i.fogCoord, col);

                return col;
            }
            ENDCG
        }
    }
}
