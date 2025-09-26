Shader "PXR_SDK/PXR_UnderlayHole"
{
	Properties
	{
	   _MainTex("Texture(A)", 2D) = "black" {}
	}

		SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100
				ZWrite Off
				Blend Zero OneMinusSrcAlpha,Zero Zero
				ColorMask RGBA

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.texcoord);
								col.r = 0;
								col.g = 0;
								col.b = 0;
								col.a = col.a;
								return col;
			}
			ENDCG
		}
	}
}