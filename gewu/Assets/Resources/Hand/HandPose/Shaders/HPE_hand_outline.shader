
Shader "HPE/Outline"
{
	Properties
	{
		[Header(Outline)]
		_OutlineWidth("Width", Range(0 , 0.005)) = 0.002
		_OutlineColor("Color", Color) = (1.0,1.0,1.0,1)
		_OutlineOpacity("Opacity", Range(0 , 1)) = 0.4
		_FadeIntensity("Intensity", Range(-5 , 5)) = 0.0
		_FadeOffset("Offset",Range(-5 , 5)) = 1.0
		_FadeOffset2("Offset2",Range(-5 , 5)) = 1.0

		[MaterialToggle] _HandFade_Toggle ("HandFade_Toggle", Float ) = 0.5
		
	}

	CGINCLUDE
	#include "Lighting.cginc"
	#pragma target 3.0

	// Outline
	uniform float4 _OutlineColor;
	uniform float _OutlineWidth;
	uniform float _OutlineOpacity;

	// fade 
	uniform half _FadeIntensity;
	uniform half _FadeOffset;
	uniform half _FadeOffset2;
	// fade mask
	uniform sampler2D _FingerGlowMask;
	
	 uniform fixed _HandFade_Toggle;
	
	ENDCG

	SubShader
	{
		LOD 100
		Tags
		{
			"Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True""RenderPipeline" = "UniversalPipeline"
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
			Name "Outline"
			Tags
			{
				"RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True""LightMode" = "UniversalForward"
			}
			Cull Front
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex outlineVertex
			#pragma fragment outlineFragment
			

			struct OutlineVertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct OutlineVertexOutput
			{
				float4 vertex : SV_POSITION;
				float3 normal: NORMAL;
				half4 glowColor : TEXCOORD1;
				half3 fadeMask : TEXCOORD2;
				float4 localPos : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			OutlineVertexOutput outlineVertex(OutlineVertexInput v)
			{
				OutlineVertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				v.vertex.xyz += v.normal * _OutlineWidth;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.localPos = v.vertex;
				half3 fadeMask = v.vertex;
				half  fadeMaskOffset = o.localPos.y*_FadeOffset+_FadeOffset2;
				half  fadeMaskOffset2 = o.localPos.z*_FadeOffset+_FadeOffset2;
				half _HandFade_Toggle_var = lerp( fadeMaskOffset,fadeMaskOffset2,_HandFade_Toggle );
				
				half4 glow = _OutlineColor;

				o.glowColor.rgb = glow.rgb;
				o.glowColor.a = clamp((_HandFade_Toggle_var*_FadeIntensity),0,1) * glow.a * _OutlineOpacity;

				return o;
			}

			half4 outlineFragment(OutlineVertexOutput i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				return i.glowColor;
			}
			ENDCG
		 }
		
	}

	SubShader
	{
		LOD 200
		Tags
		{
			"Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True"
		}

		Pass
		{
			Name "Depth"
			
			ZWrite On
			ColorMask 0
		}

		Pass
		{	
			Name "Outline"
			Tags
			{
				"RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True"
			}
			Cull Front
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex outlineVertex
			#pragma fragment outlineFragment
			

			struct OutlineVertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct OutlineVertexOutput
			{
				float4 vertex : SV_POSITION;
				float3 normal: NORMAL;
				half4 glowColor : TEXCOORD1;
				half3 fadeMask : TEXCOORD2;
				float4 localPos : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			OutlineVertexOutput outlineVertex(OutlineVertexInput v)
			{
				OutlineVertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				v.vertex.xyz += v.normal * _OutlineWidth;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.localPos = v.vertex;
				half3 fadeMask = v.vertex;
				half  fadeMaskOffset = o.localPos.y*_FadeOffset+_FadeOffset2;
				half  fadeMaskOffset2 = o.localPos.z*_FadeOffset+_FadeOffset2;
				half _HandFade_Toggle_var = lerp( fadeMaskOffset,fadeMaskOffset2,_HandFade_Toggle );
				
				half4 glow = _OutlineColor;

				o.glowColor.rgb = glow.rgb;
				o.glowColor.a = clamp((_HandFade_Toggle_var*_FadeIntensity),0,1) * glow.a * _OutlineOpacity;

				return o;
			}

			half4 outlineFragment(OutlineVertexOutput i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				return i.glowColor;
			}
			ENDCG
		 }
		
	}
}
