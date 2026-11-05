Shader "CorseVR/Finition V7 Peinte"
{
 Properties {
 _SlopeMap("Rocky banks",2D)="white"{} _BaseMap("Painted source",2D)="white"{} _FoliageAtlas("Botanical sprays",2D)="white"{} _BarkMap("Bark grain",2D)="white"{} _NormalMap("Surface normal",2D)="bump"{} _BarkNormal("Bark normal",2D)="bump"{} _BaseColor("Paint tint",Color)=(1,1,1,1)
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
 TEXTURE2D(_SlopeMap);SAMPLER(sampler_SlopeMap); TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);TEXTURE2D(_FoliageAtlas);SAMPLER(sampler_FoliageAtlas); TEXTURE2D(_BarkMap);SAMPLER(sampler_BarkMap);TEXTURE2D(_NormalMap);SAMPLER(sampler_NormalMap);TEXTURE2D(_BarkNormal);SAMPLER(sampler_BarkNormal);
 ENDHLSL
 Pass{
 Name "ForwardLit" Tags{"LightMode"="UniversalForward"} AlphaToMask On
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
 half coverage=1;half3 n=normalize(i.n)*IS_FRONT_VFACE(face,1,-1);
 half3 vertexPaint=lerp(i.c.rgb,SRGBToLinear(i.c.rgb),_ColorsAreSRGB);half3 base=vertexPaint*_BaseColor.rgb;
 bool card=_Mode<.5 && i.c.a>.28 && i.c.a<.42;
 if(card){half4 spray=SAMPLE_TEXTURE2D(_FoliageAtlas,sampler_FoliageAtlas,i.uv);clip(spray.a-.40);coverage=saturate((spray.a-.35)*4);base=spray.rgb*vertexPaint*_BaseColor.rgb*1.40;n=normalize(n+half3(0,.22,0));}
 else if(_Mode<.5){
  if(i.c.a>.75){
   half vein=1-smoothstep(.015,.060,abs(i.uv.x-.5));
   half sideveins=pow(saturate(cos((i.uv.y*6-abs(i.uv.x-.5)*2.2)*6.283)),12)*.035;
   base*=lerp(1,SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb*2.0,.45); base*=.95+.11*vein+sideveins+.035*noise(i.uv*6);
   base*=IS_FRONT_VFACE(face,1,1.14);
  }else if(i.c.a<.25){
   half grain=noise(i.uv*float2(17,1.3));base*=.92+.16*grain;
   half3 paint=SAMPLE_TEXTURE2D(_BarkMap,sampler_BarkMap,i.uv*float2(1,.32)).rgb;
   base*=lerp(1,clamp(paint*2.1,.65,1.35),.60);
  }
 }else if(_Mode<1.5){
  half3 paint=SAMPLE_TEXTURE2D(_BarkMap,sampler_BarkMap,i.uv*float2(1,.32)).rgb;
  base*=lerp(half3(.7,.7,.7),paint*1.7,.40);
 }else if(_Mode<2.5){
  half3 blend=pow(abs(n),4);blend/=max(dot(blend,1),.001);
  half3 tx=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.world.zy*_TextureScale).rgb;
  half3 ty=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.world.xz*_TextureScale).rgb;
  half3 tz=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.world.xy*_TextureScale).rgb;
  half3 granite=tx*blend.x+ty*blend.y+tz*blend.z;
  base*=lerp(1,clamp(lerp(dot(granite,half3(.333,.333,.333)),granite,.25)*3.0,.55,1.35),.78);
  base*=.94+.10*noise(i.world.xz*.6+i.world.y*.3);
 }else if(_Mode<3.5){
  half broad=noise(i.world.xz*.23);half patches=noise(i.world.xz*.84+13);
  half path=smoothstep(.12,.88,i.c.r+(patches-.5)*.18);
  half3 earth=lerp(half3(.15,.205,.095),half3(.28,.275,.16),broad);
  half3 trail=lerp(half3(.43,.345,.235),half3(.59,.47,.30),broad);
  half3 grit=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.world.xz*.93).rgb;
  half lum=dot(grit,half3(.25,.60,.15));half3 pigment=lerp(lum.xxx,grit,.45)*1.24;
  trail=lerp(trail,pigment,.43);earth=lerp(earth,pigment,.34)*lerp(.86,1.12,saturate(lum*1.6));
  half3 wbank=pow(abs(n),4);wbank/=max(dot(wbank,1),.001);half3 bank=SAMPLE_TEXTURE2D(_SlopeMap,sampler_SlopeMap,i.world.zy*.44).rgb*wbank.x+SAMPLE_TEXTURE2D(_SlopeMap,sampler_SlopeMap,i.world.xz*.44).rgb*wbank.y+SAMPLE_TEXTURE2D(_SlopeMap,sampler_SlopeMap,i.world.xy*.44).rgb*wbank.z;
  half steep=saturate((.92-abs(n.y))/.32)*(1-path);earth=lerp(earth,bank*.94,steep*.87);base=lerp(earth,trail,path)*_BaseColor.rgb;
  base*=.965+.07*patches;
 }
 // UV derivatives give a stable tangent basis without adding tangents to every leaf.
 if((_Mode<.5 && !card) || _Mode>2.5){
  float2 uvn=_Mode>2.5?i.world.xz*1.7:i.uv;
  half3 bump=(_Mode<.5 && i.c.a<.25)?SAMPLE_TEXTURE2D(_BarkNormal,sampler_BarkNormal,uvn).xyz*2-1:SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,uvn).xyz*2-1;
  float3 dp1=ddx(i.world),dp2=ddy(i.world);float2 duv1=ddx(uvn),duv2=ddy(uvn);
  float3 t=cross(dp2,n)*duv1.x+cross(n,dp1)*duv2.x;
  float3 b=cross(dp2,n)*duv1.y+cross(n,dp1)*duv2.y;
  float inv=rsqrt(max(max(dot(t,t),dot(b,b)),.000001));
  half normalStrength=_Mode>2.5?.075:.16;n=normalize(n+(normalStrength*bump.x*t+normalStrength*bump.y*b)*inv);
 }else if(_Mode>1.5 && _Mode<2.5){
  half3 nx=SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,i.world.zy*_TextureScale).xyz*2-1;
  half3 ny=SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,i.world.xz*_TextureScale).xyz*2-1;
  half3 nz=SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,i.world.xy*_TextureScale).xyz*2-1;
  half3 w=pow(abs(n),4);w/=max(dot(w,1),.001);
  n=normalize(n+.065*(half3(0,nx.y,nx.x)*w.x+half3(ny.x,0,ny.y)*w.y+half3(nz.x,nz.y,0)*w.z));
 }
 Light sun=GetMainLight(TransformWorldToShadowCoord(i.world));
 half3 view=SafeNormalize(GetWorldSpaceViewDir(i.world));
 half foliage=_Mode<.5?1:0;half facing=dot(n,sun.direction);
 half nd=lerp(saturate(facing),saturate((facing+.30)/1.30),foliage);
 half hemisphere=saturate(n.y*.5+.5);
 half3 ambient=_ShadowTint.rgb*(.32+.20*hemisphere)+SampleSH(n)*.36;
 half transmission=foliage*.12*pow(saturate(dot(view,-sun.direction)),3)*sun.shadowAttenuation;
 half3 lighting=ambient+sun.color*(nd*sun.shadowAttenuation*1.08+transmission);
 half spec=pow(saturate(dot(n,SafeNormalize(view+sun.direction))),lerp(72,12,_Roughness))*(1-_Roughness)*.12*sun.shadowAttenuation;
 half3 origin=TransformObjectToWorld(float3(0,0,0));half individuality=noise(origin.xz*.71);
 if(foliage>.5)base*=lerp(half3(.88,.98,.91),half3(1.10,1.04,.91),individuality);
 if(foliage>.5){base=pow(max(base,half3(.001,.001,.001)),.76)*1.06;base=lerp(dot(base,half3(.22,.68,.1)).xxx,base,.88);}
 if(_Mode>2.5 && _Mode<3.5)base*=lerp(.65,1,saturate(i.c.a));
 half3 color=base*lighting+spec*sun.color;
 return half4(MixFog(color,i.fog),coverage);
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
 struct A{float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;half4 c:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;half a:TEXCOORD1;UNITY_VERTEX_OUTPUT_STEREO};
 V sv(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.uv=i.uv;o.a=i.c.a;float3 w=TransformObjectToWorld(i.p.xyz);o.p=ApplyShadowClamping(TransformWorldToHClip(ApplyShadowBias(w,TransformObjectToWorldNormal(i.n),_LightDirection)));return o;}
 half4 sf(V i):SV_Target{if(_Mode<.5 && i.a>.28 && i.a<.42)clip(SAMPLE_TEXTURE2D(_FoliageAtlas,sampler_FoliageAtlas,i.uv).a-.45);return 0;}
 ENDHLSL
 }
 Pass{
 Name "DepthOnly" Tags{"LightMode"="DepthOnly"} ColorMask R
 HLSLPROGRAM
 #pragma vertex dv
 #pragma fragment df
 #pragma multi_compile_instancing
 struct A{float4 p:POSITION;float2 uv:TEXCOORD0;half4 c:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;half a:TEXCOORD1;UNITY_VERTEX_OUTPUT_STEREO};
 V dv(A i){V o;UNITY_SETUP_INSTANCE_ID(i);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;o.a=i.c.a;return o;}half4 df(V i):SV_Target{if(_Mode<.5 && i.a>.28 && i.a<.42)clip(SAMPLE_TEXTURE2D(_FoliageAtlas,sampler_FoliageAtlas,i.uv).a-.45);return 0;}
 ENDHLSL
 }
 }
}
