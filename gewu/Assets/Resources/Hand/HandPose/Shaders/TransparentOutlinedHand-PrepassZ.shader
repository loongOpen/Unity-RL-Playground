Shader "HandEditor/Transparent Outlined Hand (PrepassZ)"
{
    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "RenderType"="Transparent"
        }
        LOD 200

        // Pre-pass Zwrite. Makes the semitransparent hands
        // sort correctly (i.e., the fingers blend onto the
        // background, but not each other!)
        Pass
        {
            ZWrite On
            ColorMask A // Prevents Z prepass from actually drawing anything.

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            uniform float _HandThickness;

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag() : SV_Target { return 0; } // nop!
            ENDCG
        }
    }
    FallBack "Diffuse"
}