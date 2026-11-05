Shader "CorseVR/Decoration Surface" {
 Properties {
  _BaseMap("Albedo",2D)="white"{} _BaseColor("Tint",Color)=(1,1,1,1)
  _Kind("0 Horn 1 Polished 2 Forged 3 Brass 4 Blued steel",Float)=0
  _Metallic("Metallic",Range(0,1))=0 _Smoothness("Smoothness",Range(0,1))=.4
 }
 SubShader {
  Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"} Cull Back ZWrite On
  HLSLINCLUDE
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
  CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor;float4 _BaseMap_ST;float _Kind,_Metallic,_Smoothness;
  CBUFFER_END
  TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
  ENDHLSL
  Pass {
   Name "ForwardLit" Tags{"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma target 3.5
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_instancing
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
   #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
   #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
   #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"
   struct A{float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;half4 c:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct V{float4 p:SV_POSITION;float3 world:TEXCOORD0;half3 n:TEXCOORD1;float2 uv:TEXCOORD2;half4 c:COLOR;half fog:TEXCOORD3;UNITY_VERTEX_OUTPUT_STEREO};
   V vert(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);VertexPositionInputs p=GetVertexPositionInputs(i.p.xyz);o.p=p.positionCS;o.world=p.positionWS;o.n=TransformObjectToWorldNormal(i.n);o.uv=i.uv;o.c=i.c;o.fog=ComputeFogFactor(p.positionCS.z);return o;}
   half hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   half noise(float2 p){float2 k=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(k),hash(k+float2(1,0)),f.x),lerp(hash(k+float2(0,1)),hash(k+1),f.x),f.y);}
   half4 frag(V i):SV_Target{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    half3 n=normalize(i.n);half3 base=SRGBToLinear(i.c.rgb)*_BaseColor.rgb;half smooth=_Smoothness;
    if(_Kind<.5){half3 horn=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb;base*=lerp(half3(.30,.19,.105),horn,.72);smooth*=.94+.06*noise(i.uv*float2(12,4));}
    else if(_Kind<1.5){base*=.98+.015*noise(i.uv*float2(4,28));}
    else if(_Kind<2.5){half hammer=noise(i.uv*12);base*=.82+.25*hammer;smooth*=.65+.22*hammer;}
    else if(_Kind>3.5){base*=.96+.045*noise(i.uv*float2(6,34));}
    InputData d=(InputData)0;d.positionWS=i.world;d.normalWS=n;d.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.world);
    d.shadowCoord=TransformWorldToShadowCoord(i.world);d.fogCoord=i.fog;d.vertexLighting=VertexLighting(i.world,n);
    d.bakedGI=SampleSH(n)*.75+half3(.018,.026,.032);d.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.p);d.shadowMask=half4(1,1,1,1);
    SurfaceData s=(SurfaceData)0;s.albedo=base;s.alpha=1;s.metallic=_Metallic;s.smoothness=smooth;s.normalTS=half3(0,0,1);s.occlusion=1;s.specular=half3(.04,.04,.04);
    half4 c=UniversalFragmentPBR(d,s);c.rgb=c.rgb/(1+c.rgb*.14);c.rgb=MixFog(c.rgb,i.fog);return c;
   }
   ENDHLSL
  }
  UsePass "Universal Render Pipeline/Lit/ShadowCaster"
  UsePass "Universal Render Pipeline/Lit/DepthOnly"
 }
}
