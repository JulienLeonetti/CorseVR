Shader "CorseVR/Illustrated Matte"
{
 Properties { _BaseMap("Shadow pass compatibility",2D)="white"{} _BaseColor("Palette multiplier",Color)=(1,1,1,1) _ShadowTint("Cool open shadows",Color)=(0.51,0.66,0.62,1) _Cull("Cull",Float)=0 _Cutoff("Cutoff",Float)=0.5 }
 SubShader {
  Tags {"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
  Cull Off
  Pass {
   Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   CBUFFER_START(UnityPerMaterial)
   float4 _BaseMap_ST; half4 _BaseColor; half4 _ShadowTint; half _Cutoff; half _Cull;
   CBUFFER_END
   struct A {float4 p:POSITION;float3 n:NORMAL;half4 c:COLOR;};
   struct V {float4 p:SV_POSITION;float3 world:TEXCOORD0;half3 n:TEXCOORD1;half4 c:COLOR;half fog:TEXCOORD2;};
   V vert(A i){V o;VertexPositionInputs p=GetVertexPositionInputs(i.p.xyz);o.p=p.positionCS;o.world=p.positionWS;o.n=TransformObjectToWorldNormal(i.n);o.c=i.c;o.fog=ComputeFogFactor(p.positionCS.z);return o;}
   half4 frag(V i,FRONT_FACE_TYPE face:FRONT_FACE_SEMANTIC):SV_Target{
    half3 n=normalize(i.n)*IS_FRONT_VFACE(face,1,-1);
    Light sun=GetMainLight(TransformWorldToShadowCoord(i.world));
    half shape=smoothstep(-0.38,0.90,dot(n,sun.direction));
    half band=floor(shape*4)/4; shape=lerp(shape,band,0.18);
    half lit=shape*lerp(0.18,1,sun.shadowAttenuation);
    half3 illumination=lerp(_ShadowTint.rgb,sun.color*1.03,lit);
    half brushed=0.97+0.025*sin(i.world.x*8.7+i.world.y*15+sin(i.world.z*4))*sin(i.world.z*13+i.world.y*7);
    half3 col=i.c.rgb*_BaseColor.rgb*illumination*brushed;
    return half4(MixFog(col,i.fog),1);
   }
   ENDHLSL
  }
  Pass {
   Name "ShadowCaster"
   Tags {"LightMode"="ShadowCaster"}
   ZWrite On ZTest LEqual ColorMask 0
   HLSLPROGRAM
   #pragma vertex shadowVert
   #pragma fragment shadowFrag
   #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
   CBUFFER_START(UnityPerMaterial)
   float4 _BaseMap_ST; half4 _BaseColor; half4 _ShadowTint; half _Cutoff; half _Cull;
   CBUFFER_END
   float3 _LightDirection;float3 _LightPosition;
   struct SA {float4 p:POSITION;float3 n:NORMAL;};
   float4 shadowVert(SA i):SV_POSITION {
    float3 p=TransformObjectToWorld(i.p.xyz),n=TransformObjectToWorldNormal(i.n);
    #if _CASTING_PUNCTUAL_LIGHT_SHADOW
    float3 lightDir=normalize(_LightPosition-p);
    #else
    float3 lightDir=_LightDirection;
    #endif
    return ApplyShadowClamping(TransformWorldToHClip(ApplyShadowBias(p,n,lightDir)));
   }
   half4 shadowFrag():SV_Target{return 0;}
   ENDHLSL
  }
 }
}
