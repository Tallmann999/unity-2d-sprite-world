Shader "FlatDepth/DungeonGround"
{
    Properties
    {
        _MainTex ("Painted flagstones", 2D) = "white" {}
        _Color ("Stone tint", Color) = (.85,.83,.79,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; };
            struct v2f { float4 pos:SV_POSITION; float3 world:TEXCOORD0; UNITY_FOG_COORDS(1) };
            fixed4 _Color;
            sampler2D _MainTex;
            v2f vert(appdata v) { v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.world=mul(unity_ObjectToWorld,v.vertex).xyz; UNITY_TRANSFER_FOG(o,o.pos); return o; }
            fixed4 frag(v2f i):SV_Target
            {
                fixed3 stone=tex2D(_MainTex,i.world.xz*.19).rgb;
                float k=floor((i.world.z-6)/9+.5);
                float side=fmod(abs(k),2)<.5?-1:1;
                float2 delta=i.world.xz-float2(side*2.65,6+k*9);
                float pool=exp(-dot(delta,delta)*.095)*(.96+.04*sin(_Time.y*5.4+k*3));
                fixed4 c=fixed4(_Color.rgb*stone*(float3(.74,.80,.88)+pool*float3(.85,.44,.17)),1);
                // Evaluate distance per pixel: the long floor is one large mesh face.
                float fog=saturate((distance(i.world,_WorldSpaceCameraPos)-8)/27);
                c.rgb=lerp(c.rgb,unity_FogColor.rgb,fog); return c;
            }
            ENDCG
        }
    }
}
