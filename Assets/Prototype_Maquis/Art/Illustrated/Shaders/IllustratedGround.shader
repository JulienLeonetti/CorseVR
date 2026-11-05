Shader "CorseVR/Illustrated Ground"
{
 Properties { _Earth("Ochre earth",Color)=(0.37,0.31,0.16,1) _Sage("Sage ground cover",Color)=(0.19,0.26,0.105,1) _Path("Lit footpath",Color)=(0.66,0.43,0.22,1) }
 SubShader{
  Tags {"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
  Pass{
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
   half4 _Earth;half4 _Sage;half4 _Path;
   CBUFFER_END
   struct A{float4 p:POSITION;float3 n:NORMAL;half4 c:COLOR;};
   struct V{float4 p:SV_POSITION;float3 world:TEXCOORD0;half3 n:TEXCOORD1;half4 c:COLOR;half fog:TEXCOORD2;};
   float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float noise(float2 p){float2 a=floor(p),b=frac(p);b=b*b*(3-2*b);return lerp(lerp(hash(a),hash(a+float2(1,0)),b.x),lerp(hash(a+float2(0,1)),hash(a+1),b.x),b.y);}
   V vert(A i){V o;VertexPositionInputs p=GetVertexPositionInputs(i.p.xyz);o.p=p.positionCS;o.world=p.positionWS;o.n=TransformObjectToWorldNormal(i.n);o.c=i.c;o.fog=ComputeFogFactor(p.positionCS.z);return o;}
   half4 frag(V i):SV_Target{
    float2 uv=i.world.xz;
    half broad=noise(uv*0.65);half brush=noise(uv*float2(7,3));half grain=noise(uv*24);
    half3 soil=lerp(_Earth.rgb,_Sage.rgb,smoothstep(0.20,0.77,broad)*0.88);
    soil*=lerp(0.87,1.08,brush);
    half3 path=_Path.rgb*lerp(0.93,1.08,broad)*lerp(0.97,1.03,grain);
    half3 color=lerp(soil,path,i.c.r);
    Light sun=GetMainLight(TransformWorldToShadowCoord(i.world));
    half lit=smoothstep(-0.3,0.9,dot(normalize(i.n),sun.direction))*lerp(0.2,1,sun.shadowAttenuation);
    color*=lerp(half3(0.53,0.67,0.65),sun.color*1.03,lit);
    return half4(MixFog(color,i.fog),1);
   }
   ENDHLSL
  }
 }
}
