Shader "PXR/PICO4"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("Texture", 2D) = "white" {}
        _DiffuseColor("DiffuseColor",Color) = (1,1,1,1)
        [HDR]_FresnelCol("FresnelColor",Color) =(1,1,1,1)
        _FresnelPower("FresnelPower",Range(0,99))=1
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
            "IgnoreProjector"="True"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100
        ZWrite On
        ZTest On
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
                float3 normal:NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 nDir:TEXCOORD1;
                float3 wsPos:TEXCOORD2;
            };

            uniform sampler2D _MainTex;
            uniform float4 _MainTex_ST;
            uniform float4 _FresnelCol;
            uniform float _FresnelPower;
            uniform float4 _DiffuseColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.nDir = UnityObjectToWorldNormal(v.normal);
                o.wsPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 vDir = normalize(_WorldSpaceCameraPos.xyz - i.wsPos);
                float fresnelRate = 1 - saturate(dot(i.nDir, vDir));
                float3 fresnelCol = pow(fresnelRate, _FresnelPower) * _FresnelCol;

                //非菲尼尔区域的颜色
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb *= (1 - fresnelCol) * _DiffuseColor;
                col.rgb += fresnelCol;
                return col;
            }
            ENDCG
        }
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "Queue"="Geometry" "IgnoreProjector"="True"
        }
        LOD 100
        ZWrite On
        ZTest On

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
                float3 normal:NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 nDir:TEXCOORD1;
                float3 wsPos:TEXCOORD2;
            };

            uniform sampler2D _MainTex;
            uniform float4 _MainTex_ST;
            uniform float4 _FresnelCol;
            uniform float _FresnelPower;
            uniform float4 _DiffuseColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.nDir = UnityObjectToWorldNormal(v.normal);
                o.wsPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 vDir = normalize(_WorldSpaceCameraPos.xyz - i.wsPos);
                float fresnelRate = 1 - saturate(dot(i.nDir, vDir));
                float3 fresnelCol = pow(fresnelRate, _FresnelPower) * _FresnelCol;

                //非菲尼尔区域的颜色
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb *= (1 - fresnelCol) * _DiffuseColor;
                col.rgb += fresnelCol;
                return col;
            }
            ENDCG
        }
    }
}