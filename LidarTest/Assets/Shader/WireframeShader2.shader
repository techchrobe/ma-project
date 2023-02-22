Shader "Unlit/WireframeShader2"
{
    Properties
    {
        lineColor("Line colour", Color) = (1.0, 1.0, 1.0, 1.0)
        surfaceColor("Surface colour", Color) = (0.5, 0.5, 0.5, 1.0)
        lineWidth("Line Width", Float) = 1
    }
    SubShader
    {
            Lighting Off
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
                    float4 color : COLOR0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : POSITION;
                    float4 color : COLOR0;
                };

                fixed4 lineColor;
                fixed4 surfaceColor;
                fixed lineWidth;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    o.color = v.color;
                    return o;
                }

                fixed4 frag(v2f i) : Color
                {
                    float2 d = fwidth(i.uv);
                    float lineY = smoothstep(float(0), d.y * lineWidth, i.uv.y);
                    float lineX = smoothstep(float(0), d.x * lineWidth, 1 - i.uv.x);

                    float diagonal = smoothstep(float(0), fwidth(i.uv.x - i.uv.y) * lineWidth, (i.uv.x - i.uv.y));
                    float color = lerp(lineColor, surfaceColor, diagonal * lineX * lineY);
                    return color;
                }
                ENDCG
            }
    }
}
