Shader "FlatDepth/DungeonCard"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Ambient ("Paint visibility", Range(0,2)) = 0.82
        _Emission ("Self illumination", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" "CanUseSpriteAtlas"="True" }
        Cull Off ZWrite On
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; float3 world:TEXCOORD1; UNITY_FOG_COORDS(2) };
            sampler2D _MainTex; fixed4 _Color; float _Ambient, _Emission;
            v2f vert(appdata v) { v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color*_Color; o.world=mul(unity_ObjectToWorld,v.vertex).xyz; UNITY_TRANSFER_FOG(o,o.pos); return o; }
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 c=tex2D(_MainTex,i.uv)*i.color;
                clip(c.a-.22);
                float k=floor((i.world.z-6)/9+.5);
                float side=fmod(abs(k),2)<.5?-1:1;
                float3 lightPosition=float3(side*2.65,3.0,6+k*9);
                float3 delta=i.world-lightPosition;
                float pool=exp(-dot(delta,delta)*.105);
                float flicker=.93+.045*sin(_Time.y*5.4+k*3)+.025*sin(_Time.y*12.7+k);
                float3 illumination=_Ambient*float3(.86,.92,1.0)+float3(.78,.41,.16)*pool*flicker;
                c.rgb*=lerp(illumination,float3(1.24,1.12,.95),_Emission);
                UNITY_APPLY_FOG(i.fogCoord,c);
                return c;
            }
            ENDCG
        }
    }
}
