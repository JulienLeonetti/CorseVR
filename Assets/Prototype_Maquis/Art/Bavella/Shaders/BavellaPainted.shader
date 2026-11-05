Shader "CorseVR/Bavella Painted"
{
 Properties {
 _BaseMap("Painted source",2D)="white"{} _BaseColor("Paint tint",Color)=(1,1,1,1)
 _Mode("0 Leaf 1 Bark 2 Granite 3 Ground 4 Detail",Float)=0
 _Roughness("Roughness",Range(0,1))=.9 _TextureScale("World paint scale",Float)=.45
 _ShadowTint("Cool open shadow",Color)=(.52,.64,.61,1) _Cull("Cull",Float)=0 _ColorsAreSRGB("FBX vertex color encoding",Float)=1
 }
 SubShader{
 Tags{"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry"}
 Cull [_Cull] ZWrite On
 HLSLINCLUDE
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 float4 _BaseMap_ST;half4 _BaseColor;half4 _ShadowTint;half _Mode;half _Roughness;float _TextureScale;half _Cull;half _ColorsAreSRGB;
 CBUFFER_END
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
 ENDHLSL
 Pass{
 Name "ForwardLit" Tags{"LightMode"="UniversalForward"}
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 struct A{float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;half4 c:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V{float4 p:SV_POSITION;float3 world:TEXCOORD0;half3 n:TEXCOORD1;float2 uv:TEXCOORD2;half4 c:COLOR;half fog:TEXCOORD3;UNITY_VERTEX_INPUT_INSTANCE_ID UNITY_VERTEX_OUTPUT_STEREO};
 V vert(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_TRANSFER_INSTANCE_ID(i,o);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);VertexPositionInputs p=GetVertexPositionInputs(i.p.xyz);o.p=p.positionCS;o.world=p.positionWS;o.n=TransformObjectToWorldNormal(i.n);o.uv=i.uv;o.c=i.c;o.fog=ComputeFogFactor(p.positionCS.z);return o;}
 half hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 half noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
 half4 frag(V i,FRONT_FACE_TYPE face:FRONT_FACE_SEMANTIC):SV_Target{
 UNITY_SETUP_INSTANCE_ID(i);UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
 half3 n=normalize(i.n)*IS_FRONT_VFACE(face,1,-1);
 half3 vertexPaint=lerp(i.c.rgb,SRGBToLinear(i.c.rgb),_ColorsAreSRGB);half3 base=vertexPaint*_BaseColor.rgb;
 if(_Mode<.5){
  half vein=1-smoothstep(.012,.065,abs(i.uv.x-.5));
  half sideveins=pow(saturate(cos((i.uv.y*7-abs(i.uv.x-.5)*2.5)*6.283)),14)*.08;
  base*=.88+.16*i.uv.y+.13*vein+sideveins+.08*noise(i.uv*9);
 }else if(_Mode<1.5){
  half3 paint=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv*float2(1,2)).rgb;
  base*=lerp(half3(.7,.7,.7),paint*1.7,.40);
 }else if(_Mode<2.5){
  half3 blend=pow(abs(n),4);blend/=max(dot(blend,1),.001);
  half3 tx=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.world.zy*_TextureScale).rgb;
  half3 ty=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.world.xz*_TextureScale).rgb;
  half3 tz=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.world.xy*_TextureScale).rgb;
  half3 granite=tx*blend.x+ty*blend.y+tz*blend.z;
  base=lerp(base,granite*_BaseColor.rgb,.52);
  base*=.94+.10*noise(i.world.xz*.6+i.world.y*.3);
 }else if(_Mode<3.5){
  half path=i.c.r;half broad=noise(i.world.xz*.48);half fine=noise(i.world.xz*12);
  half3 earth=lerp(half3(.22,.25,.12),half3(.36,.32,.20),broad);
  half3 trail=lerp(half3(.48,.37,.24),half3(.66,.53,.36),broad);
  base=lerp(earth,trail,path);
  half3 grit=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.world.xz*1.7).rgb;
  base*=.91+.11*fine;base=lerp(base,base*grit*2,.15);
 }
 Light sun=GetMainLight(TransformWorldToShadowCoord(i.world));
 half nd=smoothstep(-.2,.9,dot(n,sun.direction));half lit=nd*lerp(.22,1,sun.shadowAttenuation);
 half3 lightColor=lerp(_ShadowTint.rgb,sun.color*1.05,lit);
 half3 view=SafeNormalize(GetWorldSpaceViewDir(i.world));
 half spec=pow(saturate(dot(n,SafeNormalize(view+sun.direction))),lerp(64,8,_Roughness))*(1-_Roughness)*.08*sun.shadowAttenuation;
 return half4(MixFog(base*lightColor+spec*sun.color,i.fog),1);
 }
 ENDHLSL
 }
 Pass{
 Name "ShadowCaster" Tags{"LightMode"="ShadowCaster"} ColorMask 0
 HLSLPROGRAM
 #pragma vertex sv
 #pragma fragment sf
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
 float3 _LightDirection;
 struct A{float4 p:POSITION;float3 n:NORMAL;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V{float4 p:SV_POSITION;UNITY_VERTEX_OUTPUT_STEREO};
 V sv(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);float3 w=TransformObjectToWorld(i.p.xyz);o.p=ApplyShadowClamping(TransformWorldToHClip(ApplyShadowBias(w,TransformObjectToWorldNormal(i.n),_LightDirection)));return o;}
 half4 sf(V i):SV_Target{return 0;}
 ENDHLSL
 }
 Pass{
 Name "DepthOnly" Tags{"LightMode"="DepthOnly"} ColorMask R
 HLSLPROGRAM
 #pragma vertex dv
 #pragma fragment df
 #pragma multi_compile_instancing
 struct A{float4 p:POSITION;UNITY_VERTEX_INPUT_INSTANCE_ID};struct V{float4 p:SV_POSITION;UNITY_VERTEX_OUTPUT_STEREO};
 V dv(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.p=TransformObjectToHClip(i.p.xyz);return o;}half4 df(V i):SV_Target{return 0;}
 ENDHLSL
 }
 }
}
