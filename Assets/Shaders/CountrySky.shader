Shader "FlatDepth/CountrySky"
{
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" } Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct v2f { float4 pos:SV_POSITION; float3 dir:TEXCOORD0; };
            v2f vert(float4 vertex:POSITION) { v2f o; o.pos=UnityObjectToClipPos(vertex); o.dir=vertex.xyz; return o; }
            fixed4 frag(v2f i):SV_Target
            {
                float3 d=normalize(i.dir); d=floor(d*90)/90;
                float t=floor(saturate(d.y)*5)/5;
                float3 c=lerp(float3(.70,.82,.70),float3(.35,.64,.79),t);
                float cloud=sin(d.x*18+d.z*9)+.55*sin(d.x*37-d.z*16);
                if(d.y>.18 && d.y<.28+cloud*.025 && cloud>.1) c=float3(.93,.93,.79);
                float sun=distance(d.xy,float2(-.38,.48));
                if(sun<.042 && d.z>0) c=float3(1,.91,.59);
                return fixed4(c,1);
            }
            ENDCG
        }
    }
}
