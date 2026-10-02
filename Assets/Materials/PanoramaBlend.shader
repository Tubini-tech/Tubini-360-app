Shader "Custom/PanoramaBlend"
{
    Properties
    {
        _MainTex("Current Panorama", 2D) = "white" {}
        _NextTex("Next Panorama", 2D) = "black" {}
        _Blend("Blend", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _NextTex;
            float4 _MainTex_ST;  // tiling y offset
            float4 _NextTex_ST;  // tiling y offset
            float _Blend;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 colA = tex2D(_MainTex, i.uv);
                fixed4 colB = tex2D(_NextTex, i.uv);
                return lerp(colA, colB, _Blend);
            }
            ENDCG
        }
    }
}
