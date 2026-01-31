Shader "Custom/SpriteOutlineElectric"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _OutlineColor("Outline Color", Color) = (0,1,1,1)
        _Thickness("Outline Thickness", Range(0.0, 0.5)) = 0.01
        _Speed("Electric Speed", Range(0, 10)) = 3.0
        _Intensity("Electric Intensity", Range(0, 2)) = 1.0
        _NoiseScale("Noise Scale", Range(1, 30)) = 10.0
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _OutlineColor;
            float _Thickness;
            float _Speed;
            float _Intensity;
            float _NoiseScale;

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

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) +
                       (c - a) * u.y * (1.0 - u.x) +
                       (d - b) * u.x * u.y;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 baseColor = tex2D(_MainTex, i.uv);

                // Animate electric noise offset
                float time = _Time.y * _Speed;
                float n = noise(i.uv * _NoiseScale + time);

                // Slight random offset per pixel
                float2 jitter = (n - 0.5) * _MainTex_TexelSize.xy * _Thickness * 100 * _Intensity;

                // Multi-directional outline sampling
                float2 step = _MainTex_TexelSize.xy * _Thickness * 100;
                float alpha = 0.0;

                // 8 surrounding samples (N, S, E, W, NE, NW, SE, SW)
                alpha += tex2D(_MainTex, i.uv + float2(step.x, 0) + jitter).a;
                alpha += tex2D(_MainTex, i.uv - float2(step.x, 0) + jitter).a;
                alpha += tex2D(_MainTex, i.uv + float2(0, step.y) + jitter).a;
                alpha += tex2D(_MainTex, i.uv - float2(0, step.y) + jitter).a;
                alpha += tex2D(_MainTex, i.uv + float2(step.x, step.y) + jitter).a;
                alpha += tex2D(_MainTex, i.uv + float2(-step.x, step.y) + jitter).a;
                alpha += tex2D(_MainTex, i.uv + float2(step.x, -step.y) + jitter).a;
                alpha += tex2D(_MainTex, i.uv + float2(-step.x, -step.y) + jitter).a;

                // Pulse and flicker effect
                float pulse = 0.5 + 0.5 * sin(_Time.y * _Speed * 2);
                float4 electricColor = _OutlineColor * (1 + pulse * _Intensity);

                // Draw electric outline where alpha edge exists
                if (baseColor.a < 0.1 && alpha > 0)
                {
                    return electricColor;
                }

                return baseColor;
            }
            ENDCG
        }
    }
}
