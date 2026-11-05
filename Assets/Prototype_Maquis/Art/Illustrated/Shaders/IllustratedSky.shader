Shader "CorseVR/Illustrated Sky"
{
 Properties { _Zenith("Soft blue",Color)=(0.28,0.52,0.70,1) _Horizon("Warm horizon",Color)=(0.79,0.83,0.77,1) _Cloud("Painted clouds",Color)=(1,0.96,0.83,1) }
 SubShader {
  Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline"}
  Cull Off ZWrite Off
  Pass{
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
   half4 _Zenith;half4 _Horizon;half4 _Cloud;
   CBUFFER_END
   struct A{float4 p:POSITION;};struct V{float4 p:SV_POSITION;float3 dir:TEXCOORD0;};
   V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.dir=i.p.xyz;return o;}
   float h(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float noise(float2 p){float2 a=floor(p),b=frac(p);b=b*b*(3-2*b);return lerp(lerp(h(a),h(a+float2(1,0)),b.x),lerp(h(a+float2(0,1)),h(a+1),b.x),b.y);}
   half4 frag(V i):SV_Target{
    float3 d=normalize(i.dir);half3 col=lerp(_Horizon.rgb,_Zenith.rgb,pow(saturate(d.y*1.5),0.4));
    float2 uv=d.xz/(max(0.05,d.y)+0.55)*float2(2.8,4.3)+float2(17,11);
    float cloud=noise(uv)*0.72+noise(uv*2.1)*0.28;
    cloud=smoothstep(0.60,0.69,cloud)*smoothstep(0.06,0.22,d.y)*(1-smoothstep(0.65,0.9,d.y));
    col=lerp(col,_Cloud.rgb,cloud*0.85);return half4(col,1);
   }
   ENDHLSL
  }
 }
}
