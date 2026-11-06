Shader "Kaguya/Mobile Toon"
{
    Properties
    {
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)

        _ShadowColor("Shadow Color", Color) = (0.55, 0.5, 0.7, 1)
        _ShadowThreshold("Shadow Threshold", Range(0, 1)) = 0.5
        _ShadowSmoothness("Shadow Smoothness", Range(0.001, 0.2)) = 0.04

        _AmbientStrength("Ambient Strength", Range(0, 1)) = 0.2

        _RimColor("Rim Color", Color) = (1, 0.85, 0.9, 1)
        _RimPower("Rim Power", Range(1, 8)) = 3
        _RimStrength("Rim Strength", Range(0, 1)) = 0.15
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM

            #pragma target 2.0
            #pragma vertex Vertex
            #pragma fragment Fragment

            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _ShadowColor;
                half _ShadowThreshold;
                half _ShadowSmoothness;
                half _AmbientStrength;
                half4 _RimColor;
                half _RimPower;
                half _RimStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                half3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                half fogFactor : TEXCOORD3;
            };

            Varyings Vertex(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(input.positionOS.xyz);

                VertexNormalInputs normalInputs =
                    GetVertexNormalInputs(input.normalOS);

                output.positionHCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);

                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                half4 textureColor =
                    SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);

                half3 albedo = textureColor.rgb * _BaseColor.rgb;
                half3 normalWS = normalize(input.normalWS);

                float4 shadowCoordinates =
                    TransformWorldToShadowCoord(input.positionWS);

                Light mainLight = GetMainLight(shadowCoordinates);

                half normalLight =
                    saturate(dot(normalWS, mainLight.direction));

                half lightAmount =
                    normalLight *
                    mainLight.shadowAttenuation *
                    mainLight.distanceAttenuation;

                half toonLight = smoothstep(
                    _ShadowThreshold - _ShadowSmoothness,
                    _ShadowThreshold + _ShadowSmoothness,
                    lightAmount
                );

                half3 shadowColor = albedo * _ShadowColor.rgb;
                half3 litColor = albedo * mainLight.color;

                half3 finalColor =
                    lerp(shadowColor, litColor, toonLight);

                // Невелике освітлення, щоб тіні не були чорними
                half3 ambientLight =
                    SampleSH(normalWS) * albedo * _AmbientStrength;

                finalColor += ambientLight;

                // М'яке підсвічування країв моделі
                half3 viewDirection =
                    SafeNormalize(GetWorldSpaceViewDir(input.positionWS));

                half rim =
                    pow(1.0h - saturate(dot(normalWS, viewDirection)),
                        _RimPower);

                finalColor += rim * _RimStrength * _RimColor.rgb;
                finalColor = MixFog(finalColor, input.fogFactor);

                return half4(finalColor, textureColor.a * _BaseColor.a);
            }

            ENDHLSL
        }

        // Дозволяє моделі відкидати тінь
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM

            #pragma target 2.0
            #pragma vertex ShadowVertex
            #pragma fragment ShadowFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;

            struct ShadowAttributes
            {
                float4 positionOS : POSITION;
                half3 normalOS : NORMAL;
            };

            struct ShadowVaryings
            {
                float4 positionHCS : SV_POSITION;
            };

            ShadowVaryings ShadowVertex(ShadowAttributes input)
            {
                ShadowVaryings output;

                float3 positionWS =
                    TransformObjectToWorld(input.positionOS.xyz);

                float3 normalWS =
                    TransformObjectToWorldNormal(input.normalOS);

                positionWS = ApplyShadowBias(
                    positionWS,
                    normalWS,
                    _LightDirection
                );

                output.positionHCS = TransformWorldToHClip(positionWS);

                #if UNITY_REVERSED_Z
                    output.positionHCS.z =
                        min(output.positionHCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    output.positionHCS.z =
                        max(output.positionHCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                return output;
            }

            half4 ShadowFragment() : SV_Target
            {
                return 0;
            }

            ENDHLSL
        }
    }
}