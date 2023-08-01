Shader "Skybox/SingleAtmosphereScattering_ComputeShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AtmosphereHeight ("Atmosphere Height", Float) = 100000
        _PlanetRadius ("Planet Radius", Float) = 6357000
    }
    SubShader
    {
        Tags { "Queue"="Background"
        "RenderType"="Background"
        "RenderPipeline"="UniversalPipeline"
        "PreviewType" = "Skybox"
        }
        Cull Off ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex VertexAtmosphereScattering
            #pragma fragment FragmentAtmosphereScattering

            #define USE_COMPUTE_SHADER

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
                float3 positionOS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            Varying VertexAtmosphereScattering(Attribute input) {
                Varying output;
                output.positionHCS = TransformObjectToHClip(input.vertex.xyz);
                output.positionOS = input.vertex.xyz;
                output.positionWS = TransformObjectToWorld(input.vertex.xyz);
                output.uv = input.uv;

                return output;
            }

            float4 FragmentAtmosphereScattering(Varying input) : SV_TARGET {
                // //Get Depth
                float2 screenUV = input.positionHCS.xy / _ScaledScreenParams.xy;

                // float4 result = SAMPLE_TEXTURE2D(_RayDirAndZTexture, sampler_RayDirAndZTexture, screenUV);

                // // 从摄像机深度纹理中采样深度。
                // real depth = result.w;
                
                // float zBuffer = LinearEyeDepth(depth, _ZBufferParams);

                // float3 rayStart = _WorldSpaceCameraPos.xyz;

                // //未归一化的光线方向
                // float3 rayDir = normalize(result.xyz);
                // float3 planetCenter = float3(0, -_PlanetRadius - _OriginHeight, 0);
                // float2 intersection = RaySphereInterection(rayStart, rayDir, planetCenter, _PlanetRadius + _AtmosphereHeight);
                
                // float rayLength = intersection.y;

                // intersection = RaySphereInterection(rayStart, rayDir, planetCenter, _PlanetRadius);
                // rayLength = lerp(intersection.x, rayLength, step(intersection.x, 0));
                
                // if (zBuffer < _ProjectionParams.z-200) {
                //     rayLength = min(rayLength, zBuffer);
                // }
                
                
                // float4 extinction;
                // float4 inScattering = IntegrateInscatteringRealtime(rayStart, normalize(rayDir), rayLength, planetCenter, 1, -normalize(_MainLightPosition.xyz), extinction);
                // float4 opaqueColor = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV);
                // inScattering = opaqueColor * extinction + inScattering;
                // inScattering.a = 1;
                float4 inScattering = SAMPLE_TEXTURE2D(_SkyboxTexture, sampler_SkyboxTexture, screenUV);
                return inScattering;
            }

            ENDHLSL
        }
    }
}
