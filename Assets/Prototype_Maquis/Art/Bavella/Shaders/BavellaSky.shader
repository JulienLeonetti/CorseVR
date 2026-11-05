Shader "CorseVR/Bavella Sky" {
 Properties{_Zenith("Zenith",Color)=(.24,.48,.68,1) _Horizon("Horizon",Color)=(.70,.78,.77,1)}
 SubShader{Tags{"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"} Cull Off ZWrite Off
 Pass{HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 half4 _Zenith,_Horizon;
 struct A{float4 p:POSITION;};struct V{float4 p:SV_POSITION;float3 d:TEXCOORD0;};
 V vert(A i){V o;o.p=UnityObjectToClipPos(i.p);o.d=i.p.xyz;return o;}
 half h(float2 p){return frac(sin(dot(p,float2(12.989,78.233)))*43758.54);}
 half noise(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(h(a),h(a+float2(1,0)),f.x),lerp(h(a+float2(0,1)),h(a+1),f.x),f.y);}
 half4 frag(V i):SV_Target{float3 d=normalize(i.d);half t=pow(saturate(d.y),.55);half3 col=lerp(_Horizon.rgb,_Zenith.rgb,t);float2 p=d.xz/max(d.y,.12);half streak=noise(p*float2(1.2,4))*.6+noise(p*float2(2.4,8))*.4;half cloud=smoothstep(.72,.88,streak)*smoothstep(.12,.40,d.y)*.32;return half4(lerp(col,half3(.86,.85,.77),cloud),1);}
 ENDHLSL}}
}
