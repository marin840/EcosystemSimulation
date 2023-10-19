Shader "Unlit/MapShader"
{
    Properties
    {
        _NoiseTex ("Noise", 2D) = "white" {}
        _WaterColor ("Water Color", Color) = (0, 0, 1, 1)
        _GrassColor ("Grass Color", Color) = (0, 1, 0, 1)
        _SandColor ("Sand Color", Color) = (1, 1,  0, 1)
        _GrassMin ("Grass Min Value", Range(0, 1)) = 0.50
        _WaterMax ("Water Max Value", Range(0, 1)) = 0.35
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
                float4 vertex : SV_POSITION;
            };

            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            float4 _WaterColor;
            float4 _GrassColor;
            float4 _SandColor;
            float _WaterMax;
            float _GrassMin;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _NoiseTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_NoiseTex, i.uv);
                float val = col.r;
                
                int is_grass = step(_GrassMin, val);
                int is_sand =step(_WaterMax, val) * step(val, _GrassMin);
                int is_water = step(val, _WaterMax);

                _GrassColor = vector(0, 1 - (val-0.4), 0, 1);
                _WaterColor = vector(0, 2*val, 0.33 + 2*val, 1);
                _SandColor = vector(1 - (val - 0.33) * 2, 1 - (val - 0.33) * 2, 0, 0);

                float4 color = is_water * _WaterColor + is_sand * _SandColor + is_grass * _GrassColor;
                return color;
            }
            ENDCG
        }
    }
}