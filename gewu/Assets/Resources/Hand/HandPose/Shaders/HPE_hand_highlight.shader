
Shader "HPE/Highlight"
{
	Properties
	{
	[Header(Finger)]
	_FingerIndex("FingerIndex", Vector) = (0.28, 0.875, 0.03, 0.3)
    _FingerMiddle("FingerMiddle", Vector) = (0.38, 0.825, 0.035, 0.3)
    _FingerRing("FingerRing", Vector) = (0.47, 0.86, 0.04, 0.3)
    _FingerThumb("FingerThumb", Vector) = (0.178, 0.72, 0.05, 0.25)
    _LightColor("LightColor", Color) = (1, 0, 0, 0)
    _BlendPower("BlendPower", Vector) = (1, 1, 1, 1)
	[HideInInspector]_Head("Head", Float) = 1
	[HideInInspector]_Smooth("Smooth", Float) = 1
		
	}


	CGINCLUDE
	#include "Lighting.cginc"
	#pragma target 3.0

	// Finger
	uniform float4 _FingerIndex;
	uniform float4 _FingerMiddle;
	uniform float4 _FingerRing;
	uniform float4 _FingerThumb;
	uniform float4 _LightColor;
	uniform float4 _BlendPower;
	//Test
	uniform float  _Head;
	uniform float  _Smooth;

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
			Name "Highlight"
			Tags
			{
				"RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True""LightMode" = "UniversalForward"
			}
			Cull off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex highlightVertex
			#pragma fragment highlightFragment
			
			struct highlightVertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};


			struct highlightVertexOutput
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

	    void  Remap(float In, float2 InMinMax, float2 OutMinMax, out float Out)
           {
             Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
           }
    
        void SplitQuad(float4 _QuadRange, float2 _UV, out float2 _UV1, out float _Range2)
          {
		    // 从输入中提取UV
		    half2 uv = _UV;
		    half uv_R = uv[0];
		    half uv_G = uv[1];
		    half4 quadRange = _QuadRange;
		    half quad_R = quadRange[0];
		    half quad_G = quadRange[1];
		    half quad_B = quadRange[2];
		    half quad_A = quadRange[3];
		
		    //shape_Range
		    half subtract_1 = quad_R - quad_B;
		    half subtract_4 = quad_G - quad_A;
		
		    half add_1 = quad_R + quad_B;
		    half add_2 = quad_G + quad_A;
		
		    half2 pos = half2(subtract_1, subtract_4);
		
		    half2 pos2 = half2(add_1, add_2);
		      
		    half2  shape01 = float2(subtract_1, subtract_4)-uv;
		    half2  shape02 = uv-float2(add_1, add_2);
		    float2 _Step_1 = step(shape01, float2(0, 0));
		    float2 _Step_2 = step(shape02, float2(0, 0));
		     
		    float2 _Blend_Out = min(_Step_1, _Step_2);
		    float _Final_Blend_Out = min(_Blend_Out.x, _Blend_Out.y);
		
		    //outR
		    half subtract_3 = add_1 - subtract_1;
		    half subtract_2 = uv_R - subtract_1;
		    half divide_1 = subtract_2 / subtract_3;
		
		    //outG
		    half subtract_6 = add_2 - subtract_4;
		    half subtract_5 = uv_G - subtract_4;
		    half divide_2 = subtract_5 / subtract_6;
		
		    //outUV
		   
		    half4 combine_RGBA;
		    combine_RGBA.x = divide_1;
		    combine_RGBA.y = divide_2;
		    combine_RGBA.z = 0;
		    combine_RGBA.w = 0;
		
		 
		    _UV1 = float2(combine_RGBA.x, combine_RGBA.y);
		    _Range2 = _Final_Blend_Out;
			   
          }

        void CapsuleArea(float _Head, float2 _UV,  out float shape2)
         {        
             float u = _UV.r;
             float v = _UV.g;  
             float _Head_Processed = 1-_Head;

 
             float part1;
			 
             Remap(v,float2(_Head_Processed, 1), float2(0,1), part1);
             part1 = max(part1,0);

             float part2;
             Remap(v,float2(0,_Head), float2(1,0), part2);
             part2 = max(part2,0);

             float v2 = part1 + part2;
		   

             float dis = distance(float2(u,v2),float2(0.5, 0))*2;
			 
			 
             float result = clamp(1-dis,0,1);
      
             shape2 = result;
     
         }
        void processFinger(float4 _Finger, float2 _UV, out float result)
         {
           float2 _UV1;
           float _Range2;
           SplitQuad(_Finger, _UV, _UV1, _Range2);
           float2 _UV2 =_UV1*_Range2;

           float _shape2;
           CapsuleArea(_Head,_UV2,_shape2);  
           float shape3 = smoothstep(_Smooth,0,_UV.g)*_shape2;
           result = shape3; 		
         }     

	    highlightVertexOutput highlightVertex(highlightVertexInput v)
			{
				highlightVertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
		
				o.vertex = UnityObjectToClipPos(v.vertex);
			    o.uv = v.texcoord.xy;

				return o;
			 }

	    float4 highlightFragment(highlightVertexOutput i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);


				float2 _UV2 = i.uv;
					
                float shape3_index;
                processFinger(_FingerIndex,_UV2,shape3_index);
                shape3_index *= _BlendPower.x;

                float shape3_middle;
                processFinger(_FingerMiddle,_UV2,shape3_middle);
                shape3_middle *= _BlendPower.y;

                float shape3_ring;
                processFinger(_FingerRing,_UV2,shape3_ring);
                shape3_ring *= _BlendPower.z;

                float shape3_thumb;
                processFinger(_FingerThumb,_UV2,shape3_thumb);
                shape3_thumb *= _BlendPower.w;

				float4 glow = clamp(_LightColor,0,1);
		        glow.rgb *= shape3_index + shape3_middle + shape3_ring + shape3_thumb;
		        glow.a   *=   shape3_index + shape3_middle + shape3_ring + shape3_thumb;

			
		        return glow*3.8;		

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
			Name "Highlight"
			Tags
			{
				"RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True"
			}
			Cull off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex highlightVertex
			#pragma fragment highlightFragment
			
			struct highlightVertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};


			struct highlightVertexOutput
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

        void  Remap(float In, float2 InMinMax, float2 OutMinMax, out float Out)
           {
             Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
           }
    
        void SplitQuad(float4 _QuadRange, float2 _UV, out float2 _UV1, out float _Range2)
          {
		    // 从输入中提取UV
		    half2 uv = _UV;
		    half uv_R = uv[0];
		    half uv_G = uv[1];
		    half4 quadRange = _QuadRange;
		    half quad_R = quadRange[0];
		    half quad_G = quadRange[1];
		    half quad_B = quadRange[2];
		    half quad_A = quadRange[3];
		
		    //shape_Range
		    half subtract_1 = quad_R - quad_B;
		    half subtract_4 = quad_G - quad_A;
		
		    half add_1 = quad_R + quad_B;
		    half add_2 = quad_G + quad_A;
		
		    half2 pos = half2(subtract_1, subtract_4);
		
		    half2 pos2 = half2(add_1, add_2);
		      
		    half2  shape01 = float2(subtract_1, subtract_4)-uv;
		    half2  shape02 = uv-float2(add_1, add_2);
		    float2 _Step_1 = step(shape01, float2(0, 0));
		    float2 _Step_2 = step(shape02, float2(0, 0));
		     
		    float2 _Blend_Out = min(_Step_1, _Step_2);
		    float _Final_Blend_Out = min(_Blend_Out.x, _Blend_Out.y);
		
		    //outR
		    half subtract_3 = add_1 - subtract_1;
		    half subtract_2 = uv_R - subtract_1;
		    half divide_1 = subtract_2 / subtract_3;
		
		    //outG
		    half subtract_6 = add_2 - subtract_4;
		    half subtract_5 = uv_G - subtract_4;
		    half divide_2 = subtract_5 / subtract_6;
		
		    //outUV
		   
		    half4 combine_RGBA;
		    combine_RGBA.x = divide_1;
		    combine_RGBA.y = divide_2;
		    combine_RGBA.z = 0;
		    combine_RGBA.w = 0;
		
		 
		    _UV1 = float2(combine_RGBA.x, combine_RGBA.y);
		    _Range2 = _Final_Blend_Out;
			   
          }

        void CapsuleArea(float _Head, float2 _UV,  out float shape2)
         {        
             float u = _UV.r;
             float v = _UV.g;  
             float _Head_Processed = 1-_Head;

 
             float part1;
			 
             Remap(v,float2(_Head_Processed, 1), float2(0,1), part1);
             part1 = max(part1,0);

             float part2;
             Remap(v,float2(0,_Head), float2(1,0), part2);
             part2 = max(part2,0);

             float v2 = part1 + part2;
		   

             float dis = distance(float2(u,v2),float2(0.5, 0))*2;
			 
			 
             float result = clamp(1-dis,0,1);
      
             shape2 = result;
     
         }
        void processFinger(float4 _Finger, float2 _UV, out float result)
         {
           float2 _UV1;
           float _Range2;
           SplitQuad(_Finger, _UV, _UV1, _Range2);
           float2 _UV2 =_UV1*_Range2;

           float _shape2;
           CapsuleArea(_Head,_UV2,_shape2);  
           float shape3 = smoothstep(_Smooth,0,_UV.g)*_shape2;
           result = shape3; 		
         }     



	    highlightVertexOutput highlightVertex(highlightVertexInput v)
			{
				highlightVertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
		
				o.vertex = UnityObjectToClipPos(v.vertex);
			    o.uv = v.texcoord.xy;

				return o;
			 }

	    float4 highlightFragment(highlightVertexOutput i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);


				float2 _UV2 = i.uv;
					
                float shape3_index;
                processFinger(_FingerIndex,_UV2,shape3_index);
                shape3_index *= _BlendPower.x;

                float shape3_middle;
                processFinger(_FingerMiddle,_UV2,shape3_middle);
                shape3_middle *= _BlendPower.y;

                float shape3_ring;
                processFinger(_FingerRing,_UV2,shape3_ring);
                shape3_ring *= _BlendPower.z;

                float shape3_thumb;
                processFinger(_FingerThumb,_UV2,shape3_thumb);
                shape3_thumb *= _BlendPower.w;

				float4 glow = clamp(_LightColor,0,1);
		        glow.rgb *= shape3_index + shape3_middle + shape3_ring + shape3_thumb;
		        glow.a   *=   shape3_index + shape3_middle + shape3_ring + shape3_thumb;

			
		        return glow*3.8;		

			}
			ENDCG
		 }
		
	}
}
