Shader "CorseVR/Mer Littorale Opaque" {
 Properties{_Near("Near teal",Color)=(.10,.39,.43,1) _Horizon("Marine haze",Color)=(.65,.76,.78,1) _Origin("Coast origin",Vector)=(0,0,0,0) _Right("Coast right",Vector)=(1,0,0,0) _Forward("Coast forward",Vector)=(0,0,1,0)}
 SubShader{Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"} Cull Off
 Pass{HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _Near,_Horizon;float4 _Origin,_Right,_Forward;
 CBUFFER_END
 struct A{float4 p:POSITION;UNITY_VERTEX_INPUT_INSTANCE_ID};struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;UNITY_VERTEX_OUTPUT_STEREO};
 V vert(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.w=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.w);return o;}
 half4 frag(V i):SV_Target{UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);float2 p=i.w.xz;float d=distance(i.w,_WorldSpaceCameraPos);half waves=sin(p.x*.43+p.y*.31+_Time.y*.7)*.5+sin(p.x*.79-p.y*.26-_Time.y*.43)*.25;half3 col=lerp(_Near.rgb,_Horizon.rgb,1-exp(-d/110));col*=.96+.04*waves;
 float3 delta=i.w-_Origin.xyz;float cx=dot(delta,_Right.xyz),cz=dot(delta,_Forward.xyz);float shore=6.8+1.6*sin(cz*.11)+4.3;half foam=exp(-abs(cx-shore-sin(cz*2.3+_Time.y)*.15)*3)*(.55+.45*sin(cz*6-_Time.y*.8))*step(2.4,cz)*step(cz,44);
 col=lerp(col,half3(.76,.83,.78),foam*.5);Light sun=GetMainLight();float3 n=normalize(float3(.022*cos(p.x*.43+_Time.y*.7),1,.027*cos(p.y*.31-_Time.y*.4)));float3 view=SafeNormalize(GetWorldSpaceViewDir(i.w));half glint=pow(saturate(dot(n,SafeNormalize(view+sun.direction))),96)*.15;return half4(col+glint*sun.color,1);}
 ENDHLSL}
 }
}
