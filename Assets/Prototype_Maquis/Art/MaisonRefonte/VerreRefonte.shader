Shader "CorseVR/Verre Pictural" {
 Properties {_Tint("Tint",Color)=(.72,.82,.83,1) _Reflection("Interior reflection",Cube)="black"{} _ReflectionHDR("HDR decode",Vector)=(1,1,0,0)}
 SubShader {
  Tags{"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
  Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
  Pass {
   Tags{"LightMode"="SRPDefaultUnlit"}
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_instancing
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   CBUFFER_START(UnityPerMaterial)
   half4 _Tint;half4 _ReflectionHDR;
   CBUFFER_END
   TEXTURECUBE(_Reflection);SAMPLER(sampler_Reflection);
   struct A{float4 p:POSITION;float3 n:NORMAL;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct V{float4 p:SV_POSITION;float3 world:TEXCOORD0;float3 n:TEXCOORD1;UNITY_VERTEX_OUTPUT_STEREO};
   V vert(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);VertexPositionInputs p=GetVertexPositionInputs(i.p.xyz);o.p=p.positionCS;o.world=p.positionWS;o.n=TransformObjectToWorldNormal(i.n);return o;}
   half4 frag(V i):SV_Target{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);half3 n=normalize(i.n),v=GetWorldSpaceNormalizeViewDir(i.world);half f=pow(1-abs(dot(n,v)),4);
    half4 encoded=SAMPLE_TEXTURECUBE_LOD(_Reflection,sampler_Reflection,reflect(-v,n),1.3);half3 reflection=DecodeHDREnvironment(encoded,_ReflectionHDR);
    half3 col=lerp(_Tint.rgb,reflection/(1+reflection*.25),.65);return half4(col,.025+f*.18);
   }
   ENDHLSL
  }
 }
}
