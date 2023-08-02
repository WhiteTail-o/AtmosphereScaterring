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

                // float3 worldPos = ComputeWorldSpacePosition(screenUV, depth, UNITY_MATRIX_I_VP);
                float4 positionHCS = float4(screenUV*2.0 - 1.0, 1, 1) * _ProjectionParams.z;
            #if UNITY_UV_STARTS_AT_TOP
                // Our world space, view space, screen space and NDC space are Y-up.
                // Our clip space is flipped upside-down due to poor legacy Unity design.
                // The flip is baked into the projection matrix, so we only have to flip
                // manually when going from CS to NDC and back.
                positionHCS.y = -positionHCS.y;
            #endif
                float3 rayDir = normalize((mul(UNITY_MATRIX_I_VP, positionHCS) - _WorldSpaceCameraPos).xyz);
                // float3 rayDir = normalize(input.positionWS - _WorldSpaceCameraPos);
                
                return float4(rayDir, depth);
            }

            ENDHLSL
        }
    }
}
