
Shader "HPE/hand_FloorShadow"
{
	Properties
	{
		[Hand(Fade)]
		_MainTex ("Texture",2D) = "white" {}
        _Color("Color", Color) = (0, 0, 0, 0)
       
			
	}

	CGINCLUDE
	#include "Lighting.cginc"
	#pragma target 3.0


	sampler2D _MainTex;
	uniform float4 _MainTex_ST;
	uniform half4 _Color;


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
			Name "Fade"
			Tags
			{
				"RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True""LightMode" = "UniversalForward"
			}
			//Cull Front
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex Vertex
			#pragma fragment Fragment
			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD1;
				
			
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			VertexOutput Vertex(VertexInput v)
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
			
			
				return o;
			}

			half4 Fragment(VertexOutput i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				 fixed4 col = tex2D(_MainTex,i.uv);
				 fixed3 colRGB = col.rgb*_Color.rgb;
				 fixed  colAlpha = col.a*_Color.a;
				
	         
	             return half4(colRGB,colAlpha);

				
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
			Name "Fade"
			Tags
			{
				"RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True"
			}
			//Cull Front
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex Vertex
			#pragma fragment Fragment
			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD1;
				
			
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			VertexOutput Vertex(VertexInput v)
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
			
			
				return o;
			}

			half4 Fragment(VertexOutput i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				 fixed4 col = tex2D(_MainTex,i.uv);
				 fixed3 colRGB = col.rgb*_Color.rgb;
				 fixed  colAlpha = col.a*_Color.a;
				
	         
	             return half4(colRGB,colAlpha);

				
			}
			ENDCG
		 }

	
		
	}
	
}
