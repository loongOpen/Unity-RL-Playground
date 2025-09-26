
Shader "HPE/hand_Fade"
{
	Properties
	{
		[Hand(Fade)]
		_Length("Length", Float) = 0
        _Color("Color", Color) = (0, 0, 0, 0)
        _Scale("Scale", Float) = 1
			
	}

	CGINCLUDE
	#include "Lighting.cginc"
	#pragma target 3.0

	// fade 
	uniform half _Length;
	uniform half4 _Color;
	uniform half _Scale;

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
			
			
				return o;
			}

			half4 outlineFragment(OutlineVertexOutput i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				half   fade = clamp((i.uv.y + _Length) * _Scale, 0, 1) * _Color.a;
	            half4  result = half4(_Color.rgb, fade);
	            return result;

				
			}
			ENDCG
		 }

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
			
			
				return o;
			}

			half4 outlineFragment(OutlineVertexOutput i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				half   fade = clamp((i.uv.y + _Length) * _Scale, 0, 1) * _Color.a;
	            half4  result = half4(_Color.rgb, fade);
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
			
			
				return o;
			}

			half4 outlineFragment(OutlineVertexOutput i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				half   fade = clamp((i.uv.y + _Length) * _Scale, 0, 1) * _Color.a;
	            half4  result = half4(_Color.rgb, fade);
	            return result;

				
			}
			ENDCG
		 }

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
			
			
				return o;
			}

			half4 outlineFragment(OutlineVertexOutput i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				half   fade = clamp((i.uv.y + _Length) * _Scale, 0, 1) * _Color.a;
	            half4  result = half4(_Color.rgb, fade);
	            return result;

				
			}
			ENDCG
		 }
		
	}

	
}
