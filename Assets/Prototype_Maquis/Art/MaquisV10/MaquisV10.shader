Shader "CorseVR/Maquis V10"
{
Properties{
 _BaseMap("Texture peinte",2D)="white"{} _LitterMap("Litiere",2D)="white"{} _NormalMap("Relief peint",2D)="bump"{}
 _BaseColor("Teinte",Color)=(1,1,1,1) _Cutoff("Decoupe",Range(0,1))=.42 _Mode("0 Feuilles 1 Sol 2 Litiere 3 Billboard",Float)=0
 _Roughness("Rugosite",Range(0,1))=.78 _NormalStrength("Force relief",Float)=.20 _ShadowTint("Ombres fraiches",Color)=(.65,.76,.68,1)
}
SubShader{
 Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest"}
 Cull Off ZWrite On
 HLSLINCLUDE
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 CBUFFER_START(UnityPerMaterial)
 float4 _BaseMap_ST;half4 _BaseColor,_ShadowTint;half _Cutoff,_Mode,_Roughness,_NormalStrength;
 CBUFFER_END
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);TEXTURE2D(_LitterMap);SAMPLER(sampler_LitterMap);TEXTURE2D(_NormalMap);SAMPLER(sampler_NormalMap);
 struct A{float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;half4 c:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V{float4 p:SV_POSITION;float3 world:TEXCOORD0;half3 n:TEXCOORD1;float2 uv:TEXCOORD2;half4 c:COLOR;half fog:TEXCOORD3;UNITY_VERTEX_INPUT_INSTANCE_ID UNITY_VERTEX_OUTPUT_STEREO};
 float3 PositionWS(A i){
  float3 w=TransformObjectToWorld(i.p.xyz);
  if(_Mode>2.5){float3 origin=TransformObjectToWorld(float3(0,0,0));float3 offset=w-origin;float3 view=SafeNormalize(GetCameraPositionWS()-origin);float3 right=SafeNormalize(cross(view,float3(0,1,0)));float horizontal=dot(offset,normalize(TransformObjectToWorldDir(float3(1,0,0))));w=origin+right*horizontal+float3(0,offset.y,0);}
  return w;
 }
 V vert(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_TRANSFER_INSTANCE_ID(i,o);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.world=PositionWS(i);o.p=TransformWorldToHClip(o.world);o.n=TransformObjectToWorldNormal(i.n);o.uv=i.uv;o.c=i.c;o.fog=ComputeFogFactor(o.p.z);return o;}
 half4 Albedo(V i){
  if(_Mode<.5 || _Mode>2.5){half4 t=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);clip(t.a-_Cutoff);return half4(t.rgb*_BaseColor.rgb,t.a);}
  if(_Mode>1.5)return half4(SRGBToLinear(i.c.rgb)*_BaseColor.rgb,1);
  half mask=SRGBToLinear(i.c.rgb).r;half3 earth=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb;
  half3 litter=SAMPLE_TEXTURE2D(_LitterMap,sampler_LitterMap,i.uv).rgb;
  earth*=half3(1.02,.97,.92);litter*=half3(.66,.75,.70);return half4(lerp(litter,earth,smoothstep(.10,.90,mask))*_BaseColor.rgb*lerp(.80,1,saturate(i.c.a)),1);
 }
 ENDHLSL
 Pass{
  Name "ForwardLit" Tags{"LightMode"="UniversalForward"} AlphaToMask On
  HLSLPROGRAM
  #pragma vertex vert
  #pragma fragment frag
  #pragma multi_compile_instancing
  #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
  #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
  #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
  #pragma multi_compile_fog
  half4 frag(V i,FRONT_FACE_TYPE face:FRONT_FACE_SEMANTIC):SV_Target{
   UNITY_SETUP_INSTANCE_ID(i);UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
   half4 paint=Albedo(i);half3 n=normalize(i.n)*IS_FRONT_VFACE(face,1,-1);
   if(_Mode<1.5){
    half3 bump=SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,i.uv).rgb*2-1;
    float3 dp1=ddx(i.world),dp2=ddy(i.world);float2 du1=ddx(i.uv),du2=ddy(i.uv);
    float3 t=cross(dp2,n)*du1.x+cross(n,dp1)*du2.x;float3 b=cross(dp2,n)*du1.y+cross(n,dp1)*du2.y;
    float inv=rsqrt(max(max(dot(t,t),dot(b,b)),.000001));n=normalize(n+_NormalStrength*(bump.x*t+bump.y*b)*inv);
   }
   Light sun=GetMainLight(TransformWorldToShadowCoord(i.world));AmbientOcclusionFactor ao=GetScreenSpaceAmbientOcclusion(GetNormalizedScreenSpaceUV(i.p));
   half leaf=_Mode<.5?1:0;half facing=dot(n,sun.direction);half lambert=lerp(saturate(facing),saturate((facing+.35)/1.35),leaf);
   half3 ambient=(SampleSH(n)*.78+_ShadowTint.rgb*.13)*ao.indirectAmbientOcclusion;
   half3 lighting=ambient+sun.color*(lambert*sun.shadowAttenuation*ao.directAmbientOcclusion*.92+leaf*.10)+leaf*half3(.045,.070,.038);
   half3 view=SafeNormalize(GetWorldSpaceViewDir(i.world));half spec=pow(saturate(dot(n,SafeNormalize(view+sun.direction))),24)*(1-_Roughness)*.085*sun.shadowAttenuation;
   if(leaf>.5)paint.rgb=pow(max(paint.rgb,.001),.86)*1.03;half3 color=paint.rgb*lighting+spec*sun.color;
   if(_Mode>2.5)color=paint.rgb;
   return half4(MixFog(color,i.fog),1);
  }
  ENDHLSL
 }
 Pass{
  Name "ShadowCaster" Tags{"LightMode"="ShadowCaster"} ColorMask 0
  HLSLPROGRAM
  #pragma vertex shadowVert
  #pragma fragment sf
  #pragma multi_compile_instancing
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
  float3 _LightDirection;
  V shadowVert(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_TRANSFER_INSTANCE_ID(i,o);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.world=PositionWS(i);o.p=ApplyShadowClamping(TransformWorldToHClip(ApplyShadowBias(o.world,TransformObjectToWorldNormal(i.n),_LightDirection)));o.uv=i.uv;o.c=i.c;o.n=0;o.fog=0;return o;}
  half4 sf(V i):SV_Target{UNITY_SETUP_INSTANCE_ID(i);if(_Mode<.5||_Mode>2.5)clip(SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).a-_Cutoff);return 0;}
  ENDHLSL
 }
 Pass{
  Name "DepthOnly" Tags{"LightMode"="DepthOnly"} ColorMask R
  HLSLPROGRAM
  #pragma vertex vert
  #pragma fragment df
  #pragma multi_compile_instancing
  half4 df(V i):SV_Target{UNITY_SETUP_INSTANCE_ID(i);Albedo(i);return 0;}
  ENDHLSL
 }
 Pass{
  Name "DepthNormals" Tags{"LightMode"="DepthNormals"}
  HLSLPROGRAM
  #pragma vertex vert
  #pragma fragment nf
  #pragma multi_compile_instancing
  #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
  half4 nf(V i,FRONT_FACE_TYPE face:FRONT_FACE_SEMANTIC):SV_Target{UNITY_SETUP_INSTANCE_ID(i);Albedo(i);float3 n=normalize(i.n)*IS_FRONT_VFACE(face,1,-1);
  #if defined(_GBUFFER_NORMALS_OCT)
   float2 oct=PackNormalOctQuadEncode(n);return half4(PackFloat2To888(saturate(oct*.5+.5)),0);
  #else
   return half4(n,0);
  #endif
  }
  ENDHLSL
 }
}
}

