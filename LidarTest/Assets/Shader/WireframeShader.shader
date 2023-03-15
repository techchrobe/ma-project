Shader "Unlit/WireframeShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        lineColor("Line colour", Color) = (1.0, 1.0, 1.0, 1.0)
        surfaceColor("Surface colour", Color) = (0.5, 0.5, 0.5, 1.0)
        lineWidth("Line Width", Float) = 1
    }
    SubShader
    {
            Tags { "RenderType" = "Opaque" "Queue" = "Transparent"}
            LOD 100
            Blend SrcAlpha OneMinusSrcAlpha
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fog
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
                    UNITY_FOG_COORDS(1)
                    float4 vertex : POSITION;
                    float4 color : COLOR0;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    UNITY_TRANSFER_FOG(o, o.vertex);
                    o.color = v.color;
                    return o;
                }

                fixed4 lineColor;
                fixed4 surfaceColor;
                fixed lineWidth;
                fixed4 frag(v2f i) : Color
                {
                    // Find the barycentric coordinate closest to the edge.
                    float closest = min(i.color.x, min(i.color.y, i.color.z));
                    // Set alpha to 1 if within the threshold, else 0.
                    float alpha = step(closest, lineWidth);

                    //return i.color;

                    if (closest <= lineWidth) {
                        return lineColor;
                    }
                    else {
                        return surfaceColor;
                    }

                }
                ENDCG
            }
    }
}
