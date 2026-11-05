Shader "CorseVR/Painted Plant"
{
 Properties { _BaseMap("Painted albedo",2D)="white"{} _Tint("Tint",Color)=(1,1,1,1) _ShadowTint("Open cool shadow",Color)=(0.52,0.68,0.65,1) _Cutoff("Leaf cutoff",Range(0,1))=0.32 _Cull("Cull",Float)=0 _AlphaToMask("MSAA alpha coverage",Float)=1 }
 SubShader {
  Tags {"RenderType"="TransparentCutout" "Queue"="AlphaTest" "RenderPipeline"="UniversalPipeline"}
  Cull [_Cull] ZWrite On
  HLSLINCLUDE
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
  CBUFFER_START(UnityPerMaterial)
  float4 _BaseMap_ST;half4 _Tint;half4 _ShadowTint;half _Cutoff;half _Cull;half _AlphaToMask;
  CBUFFER_END
  TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
  ENDHLSL
  Pass {
   Name "ForwardLit" Tags {"LightMode"="UniversalForward"}
   AlphaToMask [_AlphaToMask]
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma shader_feature_local_fragment _ALPHATEST_ON
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   struct A {float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;half4 c:COLOR;};
   struct V {float4 p:SV_POSITION;float3 world:TEXCOORD0;half3 n:TEXCOORD1;float2 uv:TEXCOORD2;half4 c:COLOR;half fog:TEXCOORD3;};
   V vert(A i){V o;VertexPositionInputs p=GetVertexPositionInputs(i.p.xyz);o.p=p.positionCS;o.world=p.positionWS;o.n=TransformObjectToWorldNormal(i.n);o.uv=TRANSFORM_TEX(i.uv,_BaseMap);o.c=i.c;o.fog=ComputeFogFactor(p.positionCS.z);return o;}
   half4 frag(V i,FRONT_FACE_TYPE face:FRONT_FACE_SEMANTIC):SV_Target {
    half4 tex=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);half alpha=1;
    #if defined(_ALPHATEST_ON)
    clip(tex.a-_Cutoff);
    alpha=saturate((tex.a-_Cutoff)/max(fwidth(tex.a),0.06)+0.5);
    #endif
    half3 n=normalize(i.n)*IS_FRONT_VFACE(face,1,-1);Light sun=GetMainLight(TransformWorldToShadowCoord(i.world));
    half shade=smoothstep(-0.4,0.9,dot(n,sun.direction));
    half lit=shade*lerp(0.2,1,sun.shadowAttenuation);
    half3 lightColor=lerp(_ShadowTint.rgb,sun.color*1.08,lit);
    half3 rgb=tex.rgb*_Tint.rgb*i.c.rgb*lightColor;
    return half4(MixFog(rgb,i.fog),alpha);
   }
   ENDHLSL
  }
  Pass {
   Name "ShadowCaster" Tags {"LightMode"="ShadowCaster"}
   ColorMask 0 ZWrite On
   HLSLPROGRAM
   #pragma vertex shadowVert
   #pragma fragment shadowFrag
   #pragma shader_feature_local_fragment _ALPHATEST_ON
   #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
   float3 _LightDirection;float3 _LightPosition;
   struct A {float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;};
   struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
   V shadowVert(A i){V o;float3 p=TransformObjectToWorld(i.p.xyz),n=TransformObjectToWorldNormal(i.n);
    #if _CASTING_PUNCTUAL_LIGHT_SHADOW
    float3 d=normalize(_LightPosition-p);
    #else
    float3 d=_LightDirection;
    #endif
    o.p=ApplyShadowClamping(TransformWorldToHClip(ApplyShadowBias(p,n,d)));o.uv=TRANSFORM_TEX(i.uv,_BaseMap);return o;}
   half4 shadowFrag(V i):SV_Target{
    #if defined(_ALPHATEST_ON)
    clip(SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).a-_Cutoff);
    #endif
    return 0;}
   ENDHLSL
  }
  Pass {
   Name "DepthOnly" Tags {"LightMode"="DepthOnly"}
   ZWrite On ColorMask R
   HLSLPROGRAM
   #pragma vertex depthVert
   #pragma fragment depthFrag
   #pragma shader_feature_local_fragment _ALPHATEST_ON
   struct A {float4 p:POSITION;float2 uv:TEXCOORD0;};struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
   V depthVert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=TRANSFORM_TEX(i.uv,_BaseMap);return o;}
   half4 depthFrag(V i):SV_Target{
    #if defined(_ALPHATEST_ON)
    clip(SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).a-_Cutoff);
    #endif
    return 0;}
   ENDHLSL
  }
 }
}
