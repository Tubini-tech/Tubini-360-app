Shader "Custom/PanoramaTest_SIMPLE"
{
    Properties
    {
        _TexA("Texture A", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_TexA);
            SAMPLER(sampler_TexA);

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(TransformObjectToWorld(input.positionOS.xyz));
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Lo más simple posible: solo leer la textura
                return SAMPLE_TEXTURE2D(_TexA, sampler_TexA, input.uv);
            }
            ENDHLSL
        }
    }
}
