Shader "Unlit/Water"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color",Color)=(1,1,1,1)
        _WaveAmp("Wave Value",Range(0,0.2))=0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define TAU 6.28318530
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _WaveAmp;

        
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

           

            float4 frag(v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
