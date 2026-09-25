Shader "FlatDepth/CountryGround"
{
    Properties { _Color("Base",Color)=(.4,.6,.2,1) _Dirt("Dirt",Float)=0 }
    SubShader
    {
        Tags { "RenderType"="Opaque" } Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; UNITY_FOG_COORDS(1) };
            fixed4 _Color; float _Dirt;
            float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            v2f vert(appdata v) { v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.uv; UNITY_TRANSFER_FOG(o,o.pos); return o; }
            fixed4 frag(v2f i):SV_Target
            {
                float n=hash(floor(i.uv*float2(_Dirt>.5?38:8,9)));
                float value=n>.94?1.21:n<.07?.76:1;
                if(_Dirt>.5)
                {
                    float rut=abs(abs(i.uv.x-.5)-.24);
                    value*=rut<.055?.84:1;
                    if(abs(i.uv.x-.5)<.055 && n>.55) return fixed4(.43,.5,.24,1);
                }
                fixed4 c=fixed4(_Color.rgb*value,1); UNITY_APPLY_FOG(i.fogCoord,c); return c;
            }
            ENDCG
        }
    }
}
