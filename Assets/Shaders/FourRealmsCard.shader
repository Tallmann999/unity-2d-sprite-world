Shader "FlatDepth/FourRealmsCard"
{
    Properties
    {
        [PerRendererData] _MainTex ("Painting", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Haze ("Atmosphere", Color) = (.55,.65,.72,1)
        _HazeStrength ("Distant haze", Range(0,1)) = .28
    }
    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
        Cull Off ZWrite On
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata {float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR;};
            struct v2f {float4 pos:SV_POSITION; float2 uv:TEXCOORD0; float3 world:TEXCOORD1; fixed4 color:COLOR;};
            sampler2D _MainTex; fixed4 _Color, _Haze; float _HazeStrength;
            v2f vert(appdata v) {v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.color=v.color*_Color;return o;}
            fixed4 frag(v2f i):SV_Target {fixed4 c=tex2D(_MainTex,i.uv)*i.color;clip(c.a-.25);float fog=saturate((distance(i.world,_WorldSpaceCameraPos)-9)/30)*_HazeStrength;c.rgb=lerp(c.rgb,_Haze.rgb,fog);return c;}
            ENDCG
        }
    }
}
