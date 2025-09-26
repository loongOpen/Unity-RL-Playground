Shader "o_com_power_code"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    
     CGINCLUDE
     #include "UnityCG.cginc"
     #pragma target 3.0
     
     sampler2D _MainTex;
     float4 _MainTex_ST;
     
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




            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
    	SubShader
	{
		Tags 
		{ 
			"RenderPipeline"="UniversalPipeline" 
            "Queue" = "Transparent"
            "RenderType" = "Opaque"
		}

		Pass
		{
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0,0
			ColorMask RGBA

	
            CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            ENDCG
		}
		

	}
    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent"
            "RenderType" = "Opaque"
        	"IgnoreProjector" = "True"
        }
        Pass
        {   
            Name "Interior"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}

