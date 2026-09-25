Shader "FlatDepth/FourRealmsGround"
{
    Properties { _MainTex ("Painted ground", 2D) = "white" {} _Color ("Tint", Color) = (1,1,1,1) _Tile ("Atlas quarter XY", Vector) = (0,0,0,0) }
    SubShader
    {
        Tags {"RenderType"="Opaque"} Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; fixed4 _Color;float4 _Tile;
            struct appdata {float4 vertex:POSITION;};struct v2f {float4 pos:SV_POSITION;float3 world:TEXCOORD0;};
            v2f vert(appdata v) {v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;return o;}
            fixed4 frag(v2f i):SV_Target {float2 repeat=1-abs(frac(i.world.xz*.11)*2-1);float2 uv=(_Tile.xy+.018+repeat*.964)*.5;fixed4 c=tex2D(_MainTex,uv)*_Color;return c;}
            ENDCG
        }
    }
}
