Shader "UI/Outline2D"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        _OutlineThickness ("Outline Thickness", Range(0,10)) = 2
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Lighting Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _OutlineThickness;
            float4 _OutlineColor;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float alpha = tex2D(_MainTex, uv).a;

                // 주변 픽셀 체크해서 외곽선 생성
                float outline = 0;
                float step = _OutlineThickness / 100.0; // 두께 조절
                for(float x = -step; x <= step; x += step)
                {
                    for(float y = -step; y <= step; y += step)
                    {
                        if(tex2D(_MainTex, uv + float2(x,y)).a > 0.01)
                        {
                            outline = 1;
                        }
                    }
                }

                if(alpha > 0.01)
                    return tex2D(_MainTex, uv); // 원본 픽셀
                else if(outline > 0)
                    return _OutlineColor;       // 외곽선
                else
                    return float4(0,0,0,0);    // 투명
            }
            ENDCG
        }
    }
}
