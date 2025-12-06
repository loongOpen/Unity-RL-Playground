
Shader "Custom/Arrow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MSpeed("MoveSpeed", Range(1, 3)) = 2 //移动速度
    }
    SubShader
    {
        //贴图带透明通道 ，半透明效果设置如下：
        tags{"Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True"}
        LOD 100
        Blend  SrcAlpha OneMinusSrcAlpha           //Blend选值为：SrcAlpha 和1-SrcAlpha  //也可测试为 DstColor SrcColor    //one one    
        Pass
        {
            Name "Simple"
            Cull off //双面
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _MSpeed;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            half4 frag(v2f i) : SV_Target
            {
                float2 uv = float2(i.uv.x - _MSpeed * _Time.y,i.uv.y); //箭头移动的计算
                // sample the texture
                fixed4 col = tex2D(_MainTex, uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}