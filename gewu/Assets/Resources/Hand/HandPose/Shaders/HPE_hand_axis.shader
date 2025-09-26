
Shader "HPE/hand_axis"
{
	Properties
	{
		[Hand(FadeColor)]
		_Scale("Scale", Float) = 20
        _Power("Power", Float) = 3
        _ColorX("ColorX", Color) = (206,71, 38, 1)
        _ColorY("ColorY", Color) = (156,204,85, 1)
        _ColorZ("ColorZ", Color) = (66, 118, 184, 1)
			
	}

	CGINCLUDE
	#include "Lighting.cginc"
	#pragma target 3.0

	// FadeColor
	uniform half _Scale;
	uniform half _Power;
	uniform half4 _ColorX;
	uniform half4 _ColorY;
	uniform half4 _ColorZ;

	ENDCG

	SubShader
	{
		LOD 100
		Tags
		{
			"Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True""RenderPipeline" = "UniversalPipeline"
		}

		//Pass
		//{
		//	Name "Depth"
		//	 Tags
  //          {
		//	     "LightMode" = "SRPDefaultUnlit"
  //          }
		//	ZWrite On
		//	ColorMask 0
		//}

		Pass
		{	
			Name "Fade"
			Tags
			{
				"RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True""LightMode" = "UniversalForward"
			}
			//Cull Front
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
				float2 uv : TEXCOORD1;
				float4 localPos : TEXCOORD2;
			
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			OutlineVertexOutput outlineVertex(OutlineVertexInput v)
			{
				OutlineVertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
			    o.localPos = v.vertex;
			
				return o;
			}

			half4 outlineFragment(OutlineVertexOutput i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				half3   axis = pow((clamp((i.localPos*_Scale) , 0.01, 1)),_Power)-0.01;
				half3   axiscolor = _ColorX*axis.r+_ColorY*axis.g+_ColorZ*axis.b;
				half    fade = length(axis);
	            half4  result = half4(axiscolor.rgb, fade);
	            return result;

				
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

		//Pass
		//{
		//	Name "Depth"
		//	 Tags
  //          {
		//	     "LightMode" = "SRPDefaultUnlit"
  //          }
		//	ZWrite On
		//	ColorMask 0
		//}

		Pass
		{	
			Name "Fade"
			Tags
			{
				"RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True"
			}
			//Cull Front
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
				float2 uv : TEXCOORD1;
				float4 localPos : TEXCOORD2;
			
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			OutlineVertexOutput outlineVertex(OutlineVertexInput v)
			{
				OutlineVertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
			    o.localPos = v.vertex;
			
				return o;
			}

			half4 outlineFragment(OutlineVertexOutput i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				half3   axis = pow((clamp((i.localPos*_Scale) , 0.01, 1)),_Power)-0.01;
				half3   axiscolor = _ColorX*axis.r+_ColorY*axis.g+_ColorZ*axis.b;
				half    fade = length(axis);
	            half4  result = half4(axiscolor.rgb, fade);
	            return result;

				
			}
			ENDCG
		 }

		
		
	}

	
}
