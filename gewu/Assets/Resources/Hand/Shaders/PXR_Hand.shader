Shader "PXR/Hand"
{
    Properties
    {
        [Header(BaseColor)]
        _InnerColor("内部颜色",Color) = (1,1,1,1)
        _OutColor("外部颜色",Color) = (1,1,1,1)
        _FresnelPower("菲涅尔强度",float) = 1

        [Header(Light)][Space(5)]
        _PressLight("按下光照",Color) = (1,1,1,1)
        _ClickLight("点击光照",Color) = (1,1,1,1)
        _PressRange("按压范围",Range(0,1)) = 0.015
        _ClickRange("点击范围",Range(0,1)) = 0.015
        _ClickPosition("点击位置",Vector)=(1,1,1,1)
        _PressIntensity("按压强度",Range(0,1)) = 1

        [Header(Wrist)][Space(10)]
        _WristFadeRange("腕部渐变范围",Range(0,1)) = 1
        _MainAlpha("总透明度",Range(0,1)) = 1

        _FadeIn("透明消隐",Range(0,1))=0
    }

    CGINCLUDE
    #include "Lighting.cginc"
    #pragma target 3.0

    float4 _InnerColor;
    float4 _OutColor;
    float _FresnelPower;

    float4 _PressLight;
    float4 _ClickLight;
    half _PressIntensity;
    float4 _ClickPosition;

    float _PressRange;
    float _ClickRange;

    float _WristFadeRange;
    float _FadeIn;
    float _MainAlpha;

    //-----------------描边参数----------------
    struct OutlineVertexInput
    {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
        float2 texcoord : TEXCOORD0;
    };

    struct OutlineVertexOutput
    {
        float4 vertex : SV_POSITION;
        float2 uv:TEXCOORD3;
    };

    //-------------------------------------------

    //---------------------正常绘制参数-------------------
    struct VertexInput
    {
        float4 vertex : POSITION;
        half3 normal : NORMAL;
        half4 vertexColor : COLOR;
        float2 texcoord : TEXCOORD0;
    };

    struct VertexOutput
    {
        float4 vertex : SV_POSITION;
        float3 worldPos : TEXCOORD1;
        float3 worldNormal : TEXCOORD2;
        float2 uv:TEXCOORD3;
    };

    void CustomRemap(in float4 inValue, float2 inMinMax, float2 outMinMax, out float4 outValue)
    {
        outValue = outMinMax.x + (inValue - inMinMax.x) * (outMinMax.y - outMinMax.x) / (inMinMax.y - inMinMax.x);
    }

    float GetAlpha(float2 uv)
    {
        float dis = distance(float2(0.5, 0), uv * float2(0.9, 1) + float2(0.05, 0));
        float4 s1;
        CustomRemap(_WristFadeRange, float2(0, 1), float2(0.12, 1), s1);
        const float s2 = 0.12;
        float alpha = smoothstep(s2, s1, dis);

        float s3 = 1 - _FadeIn;
        float4 s4;
        CustomRemap(s3, float2(0, 0.5), float2(0, 1), s4);
        s4 = 1.1 * saturate(s4);

        return alpha * smoothstep(s3, s4, dis);
    }
    float GetFresnel(float3 viewDir, float3 normal, float power)
    {
        return pow(1 - dot(viewDir, normal), power);
    }

    //-----------------正常绘制------------------------
    VertexOutput baseVertex(VertexInput v)
    {
        VertexOutput o;
        o.worldPos = mul(unity_ObjectToWorld, v.vertex);
        o.worldNormal = UnityObjectToWorldNormal(v.normal);
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.texcoord;
        return o;
    }

    fixed4 baseFragment(VertexOutput v) : SV_Target
    {
        float3 normalWS = normalize(v.worldNormal);
        float3 viewWS = normalize(UnityWorldSpaceViewDir(v.worldPos));
        float fresnel = saturate(GetFresnel(viewWS, normalWS, _FresnelPower));

        float4 baseColor = lerp(_InnerColor, _OutColor, fresnel);
        float4 clickColor = lerp(_PressLight, _ClickLight, step(0.99, _PressIntensity));


        float3 localClickPos = mul((float3x3)unity_WorldToObject, _ClickPosition);
        float3 vertexPos = mul((float3x3)unity_WorldToObject, v.worldPos);
        float dis = distance(localClickPos, vertexPos);

        float2 inMinMax = float2(0, lerp(_PressRange, _ClickRange, _PressIntensity));
        float2 outMinMax = float2(1, 0);
        float4 s;
        CustomRemap(dis, inMinMax, outMinMax, s);
        float4 r = smoothstep(0, 1, clamp(s, 0, 1));
        r.a *= _PressIntensity;

        fixed4 finalCol = lerp(baseColor, clickColor, r.a);
        finalCol.a *= saturate(GetAlpha(v.uv));
        finalCol.a *= _MainAlpha;
        return finalCol;
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
            Name "BaseColor"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            CGPROGRAM
            #pragma vertex baseVertex
            #pragma fragment baseFragment
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
        LOD 200
        Pass
        {
            Name "Depth"
            ZWrite On
            ColorMask 0
        }
        Pass
        {
            Name "Interior"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            CGPROGRAM
            #pragma vertex baseVertex
            #pragma fragment baseFragment
            ENDCG
        }
    }
}