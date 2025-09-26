Shader "PXR_SDK/PXR_CubemapBlit" {
	Properties{
		_MainTex("MainTex", CUBE) = "white" {}
		_d("Direction", Int) = 0
		_Gamma("Gamma", Range(0.1, 5)) = 1.0
	}
		SubShader{
			Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

			Pass{
				ZWrite Off
				ColorMask RGBA

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
						float4 vertex : POSITION;
						half3 cubedir : TEXCOORD0;
					};

					samplerCUBE _MainTex;
					int _d;
					float _Gamma;

					v2f vert(appdata v)
					{
						v2f o;
						o.vertex = UnityObjectToClipPos(v.vertex);
						float3 of[6] = { {1.0, -1.0,  1.0}, {-1.0, -1.0, -1.0}, {-1.0, 1.0,  1.0}, {-1.0, -1.0, -1.0}, {-1.0, -1.0, 1.0}, { 1.0, -1.0, -1.0} };
						float3 uf[6] = { {0.0,  0.0, -1.0}, { 0.0,  0.0,  1.0}, { 1.0, 0.0,  0.0}, { 1.0,  0.0,  0.0}, { 1.0,  0.0, 0.0}, {-1.0,  0.0,  0.0} };
						float3 vf[6] = { {0.0,  1.0,  0.0}, { 0.0,  1.0,  0.0}, { 0.0, 0.0, -1.0}, { 0.0,  0.0,  1.0}, { 0.0,  1.0, 0.0}, { 0.0,  1.0,  0.0} };
						o.cubedir = of[_d] + 2.0 * v.texcoord.x * uf[_d] + 2.0 * (1.0 - v.texcoord.y) * vf[_d];
						return o;
					}

					fixed4 frag(v2f v) : COLOR
					{
						fixed4 col = texCUBE(_MainTex, v.cubedir);

						if (_Gamma != 1)
							col.rgb = pow(col.rgb, _Gamma);
						return col;
					}
				ENDCG
			}
		}
}
