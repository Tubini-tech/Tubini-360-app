Shader "Custom/PanoramaStretchVR"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _TexA("Texture A", 2D) = "white" {}
        _TexB("Texture B", 2D) = "black" {}
        _Fade("Fade", Range(0,1)) = 0
        _StretchFactor("Stretch Factor", Range(0,3)) = 0
        _CameraForwardWS("Camera Forward", Vector) = (0,0,1,0)
        _FlipHorizontal("Flip Horizontal", Float) = 1  // ⭐ NUEVO: Control de inversión
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Off
        ZWrite On
        ZTest LEqual

        Pass
        {
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                float3 normalOS     : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 worldPos     : TEXCOORD1;
                float stretchAmount : TEXCOORD2;
                float3 viewDir      : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_TexA);
            SAMPLER(sampler_TexA);
            TEXTURE2D(_TexB);
            SAMPLER(sampler_TexB);

            float _Fade;
            float _StretchFactor;
            float3 _CameraForwardWS;
            float _FlipHorizontal;  // ⭐ NUEVO

            Varyings vert (Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 posWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 radialDir = normalize(posWS);
                float3 camForward = normalize(_CameraForwardWS);
                float forwardAlignment = saturate(dot(camForward, radialDir));
                float stretchAmount = _StretchFactor * pow(forwardAlignment, 2.0);

                posWS += radialDir * stretchAmount * 2.5;
                float3 lateralOffset = posWS - dot(posWS, camForward) * camForward;
                posWS -= lateralOffset * stretchAmount * 0.3;

                output.positionHCS = TransformWorldToHClip(posWS);
                output.worldPos = posWS;
                output.stretchAmount = stretchAmount;
                output.viewDir = normalize(posWS - _WorldSpaceCameraPos);

                // ⭐ INVERTIR UV HORIZONTALMENTE si _FlipHorizontal = 1
                float u = lerp(input.uv.x, 1.0 - input.uv.x, _FlipHorizontal);
                output.uv = float2(u, input.uv.y);

                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.uv;
                half4 color = half4(0,0,0,1);

                if (_Fade < 0.01)
                {
                    if (input.stretchAmount > 0.1)
                    {
                        // Motion blur radial
                        int samples = 10;
                        float3 camForward = normalize(_CameraForwardWS);
                        float3 viewDir = normalize(input.viewDir);
                        float alignment = saturate(dot(viewDir, camForward));

                        float2 uvCenter = float2(0.5, 0.5);
                        float2 uvDir = normalize(uv - uvCenter);
                        float blurStrength = input.stretchAmount * alignment * 0.04;

                        color = half4(0, 0, 0, 0);
                        for (int i = 0; i < samples; i++)
                        {
                            float t = (float)i / (float)(samples - 1);
                            float2 offset = uvDir * blurStrength * (t - 0.5);
                            float2 sampleUV = uv - offset;
                            color += SAMPLE_TEXTURE2D(_TexA, sampler_TexA, sampleUV);
                        }
                        color /= (float)samples;
                    }
                    else
                    {
                        color = SAMPLE_TEXTURE2D(_TexA, sampler_TexA, uv);
                    }
                }
                else
                {
                    half4 colA = SAMPLE_TEXTURE2D(_TexA, sampler_TexA, uv);
                    half4 colB = SAMPLE_TEXTURE2D(_TexB, sampler_TexB, uv);
                    color = lerp(colA, colB, _Fade);
                }

                // Vignette en espacio de vista
                if (input.stretchAmount > 0.15)
                {
                    float3 camForward = normalize(_CameraForwardWS);
                    float3 viewDir = normalize(input.viewDir);
                    float angleAlignment = dot(viewDir, camForward);
                    float vignette = lerp(0.3, 1.0, pow(saturate(angleAlignment), 2.0));
                    vignette = lerp(1.0, vignette, input.stretchAmount);
                    color.rgb *= vignette;
                }

                return color;
            }
            ENDHLSL
        }
    }
}
