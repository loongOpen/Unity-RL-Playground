Shader "PXR/HandRay"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
        _AlphaIntensity("AlphaIntensity",Range(0,1))=0.5

        [Header(Fresnel)]
        _FresnelColor("FresnelColor",Color) = (1,1,1,1)
        _FresnelSize("FresnelSize",Range(0,5))=1
        _FresnelPower("FresnelPower",Range(0,10))=1

        [Space(10)]
        _NonFresnelColor("NonFresnelColor",Color)=(1,1,1,1)
    }


    CGINCLUDE
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
        float3 wsPos :TEXCOORD1;
        float3 nDir :TEXCOORD2;
    };

    uniform sampler2D _MainTex;
    uniform float4 _MainTex_ST;

    uniform fixed _FresnelSize;
    uniform fixed _FresnelPower;
    uniform fixed4 _FresnelColor;

    uniform fixed _AlphaIntensity;
    uniform fixed4 _NonFresnelColor;


    v2f vert(appdata v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        o.wsPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        o.nDir = UnityObjectToWorldNormal(v.normal);
        return o;
    }

    fixed4 frag(v2f i) : SV_Target
    {
        float3 wsPos = i.wsPos;
        float3 nDir = i.nDir;

        float3 vDir = normalize(_WorldSpaceCameraPos - wsPos);

        //菲涅尔区域
        float fresnelMask = 1 - saturate(dot(vDir, nDir));

        //菲涅尔值
        float fresnel = pow(fresnelMask, _FresnelSize) * _FresnelPower;

        //菲涅尔区域颜色值
        fixed3 fresnelColor = _FresnelColor.rgb * fresnel;

        //非菲涅尔区域颜色值
        float nonFresnelMask = 1 - fresnel;
        _NonFresnelColor *= nonFresnelMask;

        //透明度贡献值
        float circleAlpha = saturate(fresnel + _AlphaIntensity);

        fixed4 finalColor = fixed4(saturate(fresnelColor + _NonFresnelColor), circleAlpha);
        return finalColor;
    }
    ENDCG

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }
        Pass
        {
            Name "Depth"
            Tags
            {
                "LightMode" = "SRPDefaultUnlit"
            }
            ZWrite On
            ColorMask 0
        }
        Pass
        {
            Name "Fresnel"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            ENDCG
        }
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }
        Pass
        {
            Name "Depth"
            ZWrite On
            ColorMask 0
        }
        Pass
        {
            Name "Fresnel"
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            ENDCG
        }
    }
}