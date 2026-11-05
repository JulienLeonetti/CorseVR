Shader "CorseVR/Maquis Opaque Foliage"
{
 Properties { _BaseMap("Unused shadow compatibility",2D)="white"{} _BaseColor("Tint",Color)=(1,1,1,1) _Cull("Cull",Float)=0 _Cutoff("Cutoff",Float)=0.5 }
 SubShader {
  Tags {"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
  Cull Off
  Pass {
   Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile_fragment _ _SHADOWS_SOFT
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   CBUFFER_START(UnityPerMaterial)
   float4 _BaseMap_ST; half4 _BaseColor; half _Cutoff; half _Cull;
   CBUFFER_END
   struct A {float4 positionOS:POSITION; float3 normalOS:NORMAL; half4 color:COLOR;};
   struct V {float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1; half4 color:COLOR; half fog:TEXCOORD2;};
   V vert(A i){V o; VertexPositionInputs p=GetVertexPositionInputs(i.positionOS.xyz);o.positionCS=p.positionCS;o.positionWS=p.positionWS;o.normalWS=TransformObjectToWorldNormal(i.normalOS);o.color=i.color;o.fog=ComputeFogFactor(p.positionCS.z);return o;}
   half4 frag(V i):SV_Target {
    half3 n=normalize(i.normalWS); Light light=GetMainLight(TransformWorldToShadowCoord(i.positionWS));
    half diffuse=0.28+0.72*saturate(abs(dot(n,light.direction)));
    half3 illumination=SampleSH(half3(0,1,0))*0.85+light.color*diffuse*lerp(0.3,1,light.shadowAttenuation);
    return half4(MixFog(i.color.rgb*_BaseColor.rgb*illumination,i.fog),1);
   }
   ENDHLSL
  }
  UsePass "Universal Render Pipeline/Lit/ShadowCaster"
 }
}
