Shader "CorseVR/Maquis Ground Mobile"
{
 Properties {
  _BaseMap("Rocky soil", 2D)="white" {}
  _DirtMap("Path soil", 2D)="white" {}
  _NormalMap("Rocky normal", 2D)="bump" {}
  _DirtNormal("Path normal", 2D)="bump" {}
  _BaseColor("Soil tint",Color)=(0.85,0.78,0.62,1)
  _PathColor("Path tint",Color)=(1,0.87,0.67,1)
 }
 SubShader {
  Tags {"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry"}
  Pass {
   Name "ForwardLit"
   Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile_fragment _ _SHADOWS_SOFT
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
   TEXTURE2D(_DirtMap); SAMPLER(sampler_DirtMap);
   TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);
   TEXTURE2D(_DirtNormal); SAMPLER(sampler_DirtNormal);
   CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor; half4 _PathColor;
   CBUFFER_END
   struct A {float4 positionOS:POSITION; float3 normalOS:NORMAL; half4 color:COLOR;};
   struct V {float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1; half4 color:COLOR; half fog:TEXCOORD2;};
   V vert(A i) {V o; VertexPositionInputs p=GetVertexPositionInputs(i.positionOS.xyz); o.positionCS=p.positionCS; o.positionWS=p.positionWS; o.normalWS=TransformObjectToWorldNormal(i.normalOS); o.color=i.color; o.fog=ComputeFogFactor(p.positionCS.z); return o;}
   half4 frag(V i):SV_Target {
    float2 uv=i.positionWS.xz*0.5;
    half w=i.color.r;
    half3 a=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv).rgb*_BaseColor.rgb;
    half3 b=SAMPLE_TEXTURE2D(_DirtMap,sampler_DirtMap,uv*1.25).rgb*_PathColor.rgb;
    half3 n1=UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,uv));
    half3 n2=UnpackNormal(SAMPLE_TEXTURE2D(_DirtNormal,sampler_DirtNormal,uv*1.25));
    half3 bump=lerp(n1,n2,w);
    half3 n=normalize(i.normalWS+half3(bump.x,0,bump.y)*0.45);
    Light light=GetMainLight(TransformWorldToShadowCoord(i.positionWS));
    half3 illumination=SampleSH(n)+light.color*saturate(dot(n,light.direction))*light.shadowAttenuation;
    half3 c=lerp(a,b,w)*lerp(0.83,1.12,i.color.g)*illumination;
    return half4(MixFog(c,i.fog),1);
   }
   ENDHLSL
  }
 }
}
