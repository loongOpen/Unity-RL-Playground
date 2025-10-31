Shader "PXR/PICO4_Power"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("Texture", 2D) = "white" {}
        [KeywordEnum(CutOff,Transparent)]_RenderMode("RenderMode",float) = 0
    }
    CGINCLUDE
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
    uniform sampler2D _MainTex;
    uniform float4 _MainTex_ST;
    uniform float _AlphaScale;

    v2f vert(appdata v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        return o;
    }

    fixed4 frag(v2f i) : SV_Target
    {
        fixed4 col = tex2D(_MainTex, i.uv);

        #if defined(_RENDERMODE_CUTOFF)
        clip(col.a-0.1);
        #elif defined(_RENDERMODE_TRANSPARENT)

        #endif
        return col;
    }
    ENDCG
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
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
            #pragma shader_feature  _RENDERMODE_CUTOFF _RENDERMODE_TRANSPARENT
            ENDCG
        }
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "IgnoreProjector"="True"
        }
        LOD 100
        ZWrite On
        ZTest On
        Pass
        {
            Tags
            {
                "Queue"="Geometry"
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature  _RENDERMODE_CUTOFF _RENDERMODE_TRANSPARENT
            ENDCG
        }
    }
}