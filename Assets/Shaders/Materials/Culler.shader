// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Deferred Renderer/Culler"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        //ZWrite Off
        
        LOD 100

        Pass
        {
            CGPROGRAM
            
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 4.0

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    float3 normal : NORMAL;
                };

                struct v2f
                {
                    float depth : DEPTH;
                    float4 vertex : SV_POSITION;
                    float3 normal : NORMAL;
                    float3 worldPos : TEXCOORD0;
                };

                struct FragmentOutput
                {
                    float4 aColor;
                    float4 bColor;
                };

                v2f vert (appdata v)
                { 
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                    o.depth = 1 + UnityObjectToViewPos(v.vertex).z * 200 * _ProjectionParams.w;
                    o.normal = UnityObjectToWorldNormal(v.normal);
                    return o;
                }

                FragmentOutput frag (v2f i) : COLOR
                {
                    FragmentOutput output;
                    output.aColor = float4(i.normal, 1);
                    output.bColor = float4(i.worldPos, i.depth);
                    return output;
                }

            ENDCG
        }
    }
}
