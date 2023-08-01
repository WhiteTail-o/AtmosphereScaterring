Shader "Skybox/RayDir"
{
    Properties
    {
        
    }
    SubShader
    {
        Tags { "Queue"="Background"
        // "LightMode" = "CopyRayDir"
        }

        Cull Off ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex VertexAtmosphereScattering
            #pragma fragment FragmentAtmosphereScattering

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "ShaderLibrary/Scattering.hlsl"

            struct Attribute {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varying {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            Varying VertexAtmosphereScattering(Attribute input) {
                Varying output;
                output.positionHCS = TransformObjectToHClip(input.vertex.xyz);
                output.positionWS = TransformObjectToWorld(input.vertex.xyz);
                output.uv = input.uv;

                return output;
            }

            float4 FragmentAtmosphereScattering(Varying input) : SV_TARGET {
                float2 screenUV = input.positionHCS.xy / _ScaledScreenParams.xy;
                real depth = SampleSceneDepth(screenUV);

                float3 rayDir = normalize(input.positionWS - _WorldSpaceCameraPos);
                
                return float4(screenUV, 0, depth);
            }

            ENDHLSL
        }
    }
}
