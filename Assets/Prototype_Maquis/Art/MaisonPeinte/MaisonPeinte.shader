Shader "CorseVR/Maison Peinte" {
 Properties {
  _BaseMap("Painted surface",2D)="white"{} _BaseColor("Tint",Color)=(1,1,1,1)
  _Mode("0 Stone 1 Wood 2 Ceramic 3 Fabric 4 Metal 5 Foliage 6 Lime",Float)=0
  _Smoothness("Smoothness",Range(0,1))=.08 _Metallic("Metallic",Range(0,1))=0
  _AmbientScale("Ambient",Range(0,1))=.8 _TextureScale("Texture scale",Float)=.75
  _Relief("Relief",Range(0,.03))=.0005 _TextureWeight("Texture weight",Range(0,1))=.7
 }
 SubShader {
  Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}
  Cull Off ZWrite On
  HLSLINCLUDE
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
  CBUFFER_START(UnityPerMaterial)
  half4 _BaseColor;float4 _BaseMap_ST;float _Mode,_Smoothness,_Metallic,_AmbientScale,_TextureScale,_Relief,_TextureWeight;
  CBUFFER_END
  TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
  ENDHLSL
  Pass {
   Name "ForwardPaint" Tags{"LightMode"="UniversalForward"}
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
   half3 diffusePaint(Light light,half3 n){
    half facing=smoothstep(-.13,.92,dot(n,light.direction));
    return light.color*light.distanceAttenuation*light.shadowAttenuation*facing;
   }
   half4 frag(V i,FRONT_FACE_TYPE face:FRONT_FACE_SEMANTIC):SV_Target {
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    half3 n=normalize(i.n)*IS_FRONT_VFACE(face,1,-1);
    half3 base=SRGBToLinear(i.c.rgb)*_BaseColor.rgb;
    float2 plane=i.world.xz+i.world.y*.77;half wash=noise(plane*1.6);half structure=.5;
    if(_Mode<.5){
     half3 weights=pow(abs(n),6);weights/=max(dot(weights,1),.001);
     half3 paint=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.world.zy*_TextureScale).rgb*weights.x
      +SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.world.xz*_TextureScale).rgb*weights.y
      +SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.world.xy*_TextureScale).rgb*weights.z;
     base*=lerp(1,paint*2.45,_TextureWeight);
     base*=lerp(half3(.90,.97,1.08),half3(1.10,1.04,.91),wash);
     structure=dot(paint,half3(.333,.333,.333));
    }else if(_Mode<1.5){
     half3 paint=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv*_TextureScale).rgb;
     base*=lerp(1,paint*3.45,_TextureWeight);
     base*=lerp(half3(.88,.91,1.03),half3(1.10,1.03,.93),noise(i.uv*float2(7,1.4)));
     structure=dot(paint,half3(.333,.333,.333));
    }else if(_Mode<2.5){
     half brush=noise(i.uv*float2(5,13));
     base*=lerp(half3(.88,.92,1.05),half3(1.16,1.04,.88),brush);
    }else if(_Mode<3.5){
     half attenuation=saturate(1-max(fwidth(i.uv.x),fwidth(i.uv.y))*500);
     half weave=sin(i.uv.x*750)*sin(i.uv.y*750)*attenuation;
     base*=.97+.035*weave+.05*noise(plane*14);
    }else if(_Mode<4.5){base*=.94+.10*noise(plane*8);}
    else if(_Mode<5.5){base*=.95+.10*noise(i.uv*7);}
    else {
     half3 paint=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,plane*_TextureScale).rgb;
     base*=lerp(1,paint*2.45,_TextureWeight);
     base*=lerp(half3(.91,.96,1.06),half3(1.06,1.03,.92),noise(plane*2.5));
    }
    float3 dx=ddx(i.world),dy=ddy(i.world);float3 r1=cross(dy,n),r2=cross(n,dx);float det=dot(dx,r1);
    float3 gradient=(ddx(structure)*r1+ddy(structure)*r2)*sign(det)/max(abs(det),.00001);n=normalize(n-gradient*_Relief);
    InputData inputData=(InputData)0;inputData.positionWS=i.world;inputData.normalWS=n;
    inputData.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.world);
    inputData.shadowCoord=TransformWorldToShadowCoord(i.world);inputData.fogCoord=i.fog;
    inputData.vertexLighting=VertexLighting(i.world,n);
    inputData.bakedGI=SampleSH(n)*.45+half3(.035,.065,.085);
    inputData.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.p);inputData.shadowMask=half4(1,1,1,1);
    SurfaceData surface=(SurfaceData)0;surface.albedo=base;surface.alpha=1;
    surface.metallic=min(_Metallic,.35);surface.smoothness=min(_Smoothness,.23);surface.normalTS=half3(0,0,1);
    surface.occlusion=clamp(i.c.a,.35,1);surface.specular=half3(.025,.025,.025);
    half3 raw=UniversalFragmentPBR(inputData,surface).rgb;
    // Compress bright window highlights while retaining the real cast shadows and volume.
    half3 color=raw*1.28/(1+raw*.52);
    half lit=saturate(dot(raw,half3(.2126,.7152,.0722))*1.3);
    color*=lerp(half3(.83,.97,1.11),half3(1.05,1.025,.93),lit);
    color=MixFog(color,i.fog);return half4(color,1);
   }
   ENDHLSL
  }
  UsePass "Universal Render Pipeline/Lit/ShadowCaster"
  UsePass "Universal Render Pipeline/Lit/DepthOnly"
 }
}
