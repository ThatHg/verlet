// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/PositionedColors"
{
    Properties
    {
        _Color1("First Color", Color) = (0,0,0,1)
        _Color2("Second Color", Color) = (1,1,1,1)
        _Color3("Third Color", Color) = (1,1,1,1)
        _Color4("Fourth Color", Color) = (1,1,1,1)
        _WorldPos("World Position", Vector) = (0,0,0)
        _Scale("Scale", Float) = 1
        _Middle("Middle", Range(0.001, 0.999)) = 1
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

            float4 _Color1;
            float4 _Color2;
            float4 _Color3;
            float4 _Color4;
            float3 _WorldPos;
            float _Scale;
            float _Middle;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.uv.x = distance(mul(unity_ObjectToWorld, v.vertex), _WorldPos);
                o.uv.y = 1 / lerp(0.001, _Scale, step(0, _Scale));
                return o;
            }

            
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                //fixed4 col = lerp(_Color1, _Color2, clamp(i.uv.x * i.uv.y,0,1));
                fixed dist = clamp(i.uv.x * i.uv.y,0.001,1);
                fixed4 col = lerp(_Color1, _Color2, dist / _Middle) * step(dist, _Middle);
                col += lerp(_Color2, _Color3, (dist - _Middle) / (1 - _Middle)) * step(_Middle, dist);
                col.a = 1;
                return col;
            }
            ENDCG
        }
    }
}
