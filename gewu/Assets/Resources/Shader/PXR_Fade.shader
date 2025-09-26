Shader "PXR_SDK/PXR_Fade" {
	Properties{
		 _Color("Color", Color) = (0,0,0,1)
	}
		SubShader{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100

		ZWrite Off
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha
		Color[_Color]

		Pass{}
	}
}