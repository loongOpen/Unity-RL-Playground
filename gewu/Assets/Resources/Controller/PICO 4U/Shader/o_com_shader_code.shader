Shader "o_com_shader_code"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		_LightColor_01("Light Color_01", Color) = (0.3921569,0.3921569,0.3921569,1)
		_LightColor_02("Light Color_02", Color) = (0.3921569,0.3921569,0.3921569,1)
		_MainTex("MainTex", 2D) = "white" {}
		_Tint("Tint", Color) = (1,1,1,0)
		_Matcap_01("Matcap_01", 2D) = "white" {}
		_Matcap_02("Matcap_02", 2D) = "white" {}
		_MatcapIntensity("MatcapIntensity", Range( 0 , 2)) = 0
		_Light("Light", Vector) = (0,0,0,0)
		_Light_Power("Light_Power", Range( 0 , 4)) = 1
		[Toggle(_LIGHT_O_ON)] _Light_O("Light_O", Float) = 0
		_Mask("Mask", 2D) = "white" {}
	}
	
	
    CGINCLUDE
    #include "Lighting.cginc"
    #pragma target 3.0

	float4 _Tint;
	float4 _LightColor_01;
	float4 _LightColor_02;
	float3 _Light;
	float _MatcapIntensity;
	float _Light_Power;
    
	sampler2D _MainTex;
	sampler2D _Matcap_01;
	sampler2D _Matcap_02;
	sampler2D _Mask;
    

	struct VertexInput
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float4 texcoord : TEXCOORD0;
	};

	struct VertexOutput
	{
		float4 clipPos : SV_POSITION;
		float4 uv : TEXCOORD3;
		float4 vertex : TEXCOORD4;
		float4 worldNormal : TEXCOORD5;
	};
    
 
    VertexOutput vert ( VertexInput v )
	{
		VertexOutput o = (VertexOutput)0;
		o.worldNormal.xyz = UnityObjectToWorldNormal(v.normal);
		o.uv.xy = v.texcoord.xy;
		o.vertex = v.vertex;
		//float3 positionWS = mul(unity_ObjectToWorld, v.vertex.xyz);
		float4 positionCS = UnityObjectToClipPos(v.vertex);
		o.clipPos = positionCS;
		return o;
	}

	half4 frag ( VertexOutput IN  ) : SV_Target
	{
		float2 uv = IN.uv.xy * float2( 1,1 ) + float2( 0,0 );
		float4 maintex = tex2D( _MainTex, uv );
		float3 ViewPos = mul( UNITY_MATRIX_MV, float4( IN.vertex.xyz, 1 ) ).xyz;
			   ViewPos = normalize( ViewPos );
		float3 worldNormal = normalize(IN.worldNormal.xyz);
		float3 break80 = cross( ViewPos , mul( UNITY_MATRIX_V, float4( worldNormal , 0.0 ) ).xyz );
		float2 MatCapUV = (float2(-break80.y , break80.x));
		       MatCapUV = (MatCapUV*0.5 + 0.5);
		float4 mask = tex2D( _Mask, uv );
		float4 lerpMatCap = lerp( tex2D( _Matcap_01, MatCapUV ) , tex2D( _Matcap_02, MatCapUV ) , mask.r);
		float4 mainColor = ( ( maintex * _Tint ) * ( lerpMatCap * _MatcapIntensity ) );
		       mainColor+=mainColor;
		float4 lerpLightColor = lerp( _LightColor_01 , _LightColor_02 , mask.r);
		float  lambert = dot( worldNormal , _Light );
		float4 Color = ( mainColor + ( ( ( lerpLightColor * max( lambert , 0.0 ) ) / 20.0 ) * _Light_Power ) );
		float4 finalCol = float4(Color.rgb, 1);
		
		return finalCol;
	}

    ENDCG
	
	SubShader
	{
		Tags 
		{ 
			"RenderPipeline"="UniversalPipeline" 
            "Queue" = "Geometry"
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
        LOD 200
        Pass
        {
            Name "Depth"
            ZWrite On
            ColorMask 0
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