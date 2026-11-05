Shader "CorseVR/Coast Water" {
 Properties { _ShoreX("Shore world X",Float)=225 _Shallow("Shallows",Color)=(.15,.69,.65,1) _Deep("Open sea",Color)=(.035,.22,.40,1) }
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}
  Cull Off ZWrite On
  Pass {
   Name "ForwardLit" Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_instancing
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
   half4 _Shallow,_Deep; float _ShoreX;
   CBUFFER_END
   struct A {float4 p:POSITION;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct V {float4 p:SV_POSITION;float3 world:TEXCOORD0;half fog:TEXCOORD1;UNITY_VERTEX_OUTPUT_STEREO};
   V vert(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);float3 w=TransformObjectToWorld(i.p.xyz);w.y+=sin(w.x*.8+w.z*.28+_Time.y*.9)*.035;o.world=w;o.p=TransformWorldToHClip(w);o.fog=ComputeFogFactor(o.p.z);return o;}
   half4 frag(V i):SV_Target{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    float distance=i.world.x-_ShoreX;
    half deep=smoothstep(0,24,distance);
    half ripple=sin(i.world.x*2.1+i.world.z*.73+_Time.y*1.1)*.035+sin(i.world.z*3-i.world.x*.6+_Time.y*.7)*.018;
    half3 color=lerp(_Shallow.rgb,_Deep.rgb,deep)+ripple;
    half shore=1-smoothstep(.2,1.15,abs(distance-.35-sin(i.world.z*.65+_Time.y*.7)*.18));
    half crest=pow(saturate(sin(i.world.x*2+i.world.z*.3+_Time.y)),28)*.06;
    color=lerp(color,half3(.82,.92,.85),shore*.72)+crest;
    return half4(MixFog(color,i.fog),1);
   }
   ENDHLSL
  }
 }
}
