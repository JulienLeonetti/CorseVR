using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using CorseVR;

namespace CorseVR.Editor {
public static class MaquisZigzagBuilder {
 const string Root="Assets/Prototype_Maquis/Art/SentierCorseV3";
 const string V1="Assets/Prototype_Maquis/Art/SentierCorse";
 const string V2="Assets/Prototype_Maquis/Art/SentierCorseV2";
 static System.Random rng;
 static Transform vegetation,groundRoot,coast,portalRoot;
 static Material wood,leaf,flower,grass,stone,archStone,soil,sand,water;
 static Vector2 finish;
 static float R(float a,float b){return a+(float)rng.NextDouble()*(b-a);}
 static Color C(string h){Color c;ColorUtility.TryParseHtmlString("#"+h,out c);return c.linear;}
 static Transform Group(string name,Transform parent){var t=new GameObject(name).transform;t.SetParent(parent,false);return t;}
 static Material Mat(string name,Material source){string p=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(m==null){m=new Material(source);AssetDatabase.CreateAsset(m,p);}else EditorUtility.CopySerialized(source,m);m.name=name;m.enableInstancing=true;EditorUtility.SetDirty(m);return m;}
 static Material Plain(string name,string shader,Color color){string p=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(m==null){m=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(m,p);}m.SetColor("_BaseColor",color);EditorUtility.SetDirty(m);return m;}
 static Mesh SaveMesh(Mesh mesh,string name){mesh.name=name;string p=Root+"/Meshes/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(p);if(old==null){AssetDatabase.CreateAsset(mesh,p);return mesh;}EditorUtility.CopySerialized(mesh,old);EditorUtility.SetDirty(old);UnityEngine.Object.DestroyImmediate(mesh);return old;}
 static GameObject MeshGo(string name,Mesh mesh,Material material,Transform parent,bool shadows=false,bool collider=false){var go=new GameObject(name);go.transform.SetParent(parent,false);var saved=SaveMesh(mesh,name);go.AddComponent<MeshFilter>().sharedMesh=saved;var rr=go.AddComponent<MeshRenderer>();rr.sharedMaterial=material;rr.shadowCastingMode=shadows?ShadowCastingMode.On:ShadowCastingMode.Off;rr.lightProbeUsage=LightProbeUsage.Off;if(collider)go.AddComponent<MeshCollider>().sharedMesh=saved;go.isStatic=true;return go;}
 static void Ready(){finish=MaquisRouteDefinition.Point(MaquisRouteDefinition.Length);}
 public static float Height(float x,float z){
  Ready();float d=MaquisRouteDefinition.Distance(x,z);float bank=2.8f*Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.1f,6.5f,d));bank*=.84f+.30f*Mathf.PerlinNoise(x*.31f+38,z*.30f+71);
  float y=MaquisRouteDefinition.Grade(x,z)+bank;
  float endDistance=Vector2.Distance(new Vector2(x,z),finish);float clearing=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(4.5f,10.0f,endDistance));
  y=Mathf.Lerp(y,MaquisRouteDefinition.Grade(finish.x,finish.y),clearing);
  float edge=Mathf.Max(Mathf.Abs(x),Mathf.Abs(z));return Mathf.Lerp(y,SentierCorseFinition.Height(x,z),Mathf.SmoothStep(0,1,Mathf.InverseLerp(21,26,edge)));
 }
 static bool Clear(Vector2 p,float radius){Ready();return Vector2.Distance(p,finish)<4.7f+radius;}
 static Mesh NearTerrain(){
  int n=208;float step=.25f;var vs=new List<Vector3>();var ns=new List<Vector3>();var uv=new List<Vector2>();var cs=new List<Color>();var ts=new List<int>();
  for(int j=0;j<=n;j++)for(int i=0;i<=n;i++){
   float x=-26+i*step,z=-26+j*step,y=Height(x,z),e=.10f;
   // Match the 2m grid of the outer landscape at the seam.
   if(i==0||i==n){float lo=Mathf.Floor(z/2)*2;y=Mathf.Lerp(Height(x,lo),Height(x,lo+2),(z-lo)/2);}else if(j==0||j==n){float lo=Mathf.Floor(x/2)*2;y=Mathf.Lerp(Height(lo,z),Height(lo+2,z),(x-lo)/2);}
   vs.Add(new Vector3(x,y,z));ns.Add(new Vector3(Height(x-e,z)-Height(x+e,z),2*e,Height(x,z-e)-Height(x,z+e)).normalized);uv.Add(new Vector2(x,z));
   float distance=MaquisRouteDefinition.Distance(x,z)+(Mathf.PerlinNoise(x*2.2f,z*2.2f)-.5f)*.15f;
   float path=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.82f,1.20f,distance));float clearing=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(3.6f,4.9f,Vector2.Distance(new Vector2(x,z),finish)));cs.Add(new Color(Mathf.Max(path,clearing),0,0,1));
  }
  for(int j=0;j<n;j++)for(int i=0;i<n;i++){int a=j*(n+1)+i,b=a+1,c=a+n+1,d=c+1;ts.AddRange(new[]{a,c,b,b,c,d});}
  var mesh=new Mesh();mesh.indexFormat=IndexFormat.UInt32;mesh.SetVertices(vs);mesh.SetNormals(ns);mesh.SetUVs(0,uv);mesh.SetColors(cs);mesh.SetTriangles(ts,0);mesh.RecalculateBounds();return mesh;
 }
 static void FarTerrain(){
  var source=AssetDatabase.LoadAssetAtPath<Mesh>(V2+"/Meshes/Versants_Corses_360m.asset");var mesh=UnityEngine.Object.Instantiate(source);var vertices=mesh.vertices;var tris=mesh.triangles;var kept=new List<int>();
  for(int i=0;i<tris.Length;i+=3){var p=(vertices[tris[i]]+vertices[tris[i+1]]+vertices[tris[i+2]])/3;if(Mathf.Abs(p.x)<26&&Mathf.Abs(p.z)<26)continue;kept.Add(tris[i]);kept.Add(tris[i+1]);kept.Add(tris[i+2]);}mesh.SetTriangles(kept,0);mesh.RecalculateBounds();
  MeshGo("Paysage_Exterieur",mesh,AssetDatabase.LoadAssetAtPath<Material>(V2+"/Materials/Versants_Verts_Granit.mat"),groundRoot);
 }
 class Chunk {public Dictionary<Material,List<CombineInstance>> high=new Dictionary<Material,List<CombineInstance>>();public Dictionary<Material,List<CombineInstance>> low=new Dictionary<Material,List<CombineInstance>>();}
 static Dictionary<string,Chunk> chunks;
 static Material Resolve(Material m){string n=m==null?"":m.name;return n.Contains("Wood")?wood:n.Contains("Flower")?flower:leaf;}
 static void AddModel(Dictionary<Material,List<CombineInstance>> target,string path,Matrix4x4 matrix){
  var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(asset==null)throw new Exception("Missing "+path);
  foreach(var mf in asset.GetComponentsInChildren<MeshFilter>()){
   var mats=mf.GetComponent<Renderer>().sharedMaterials;
   for(int sub=0;sub<mf.sharedMesh.subMeshCount;sub++){var mat=Resolve(mats[Mathf.Min(sub,mats.Length-1)]);if(!target.ContainsKey(mat))target[mat]=new List<CombineInstance>();target[mat].Add(new CombineInstance{mesh=mf.sharedMesh,subMeshIndex=sub,transform=matrix*mf.transform.localToWorldMatrix});}
  }
 }
 static int shrubCount;
 static void TrimWalkway(Mesh mesh){
  var line=MaquisRouteDefinition.Corridor();var vertices=mesh.vertices;var cut=new bool[vertices.Length];
  for(int i=0;i<vertices.Length;i++){
   var p=vertices[i];float grade=MaquisRouteDefinition.Grade(p.x,p.z);if(p.y<grade+.85f||p.y>grade+2.9f)continue;
   int lo=0,hi=line.Length-1;while(lo<hi){int mid=(lo+hi)/2;if(line[mid].z<p.z-1.5f)lo=mid+1;else hi=mid;}
   for(int j=Mathf.Max(1,lo);j<line.Length&&line[j-1].z<p.z+1.5f;j++){
    var a=new Vector2(line[j-1].x,line[j-1].z);var b=new Vector2(line[j].x,line[j].z);var ab=b-a;var q=a+ab*Mathf.Clamp01(Vector2.Dot(new Vector2(p.x,p.z)-a,ab)/Mathf.Max(ab.sqrMagnitude,.000001f));
    if((new Vector2(p.x,p.z)-q).sqrMagnitude<1.45f*1.45f){cut[i]=true;break;}
   }
  }
  var old=mesh.triangles;var kept=new List<int>();for(int i=0;i<old.Length;i+=3)if(!cut[old[i]]&&!cut[old[i+1]]&&!cut[old[i+2]]){kept.Add(old[i]);kept.Add(old[i+1]);kept.Add(old[i+2]);}mesh.SetTriangles(kept,0);mesh.RecalculateBounds();
 }
 static void Shrub(string species,int variant,Vector2 p,float scale,bool detailed){
  float radius=(species=="LentisqueRampant"?1.65f:species=="ArbousierBuisson"?1.25f:species=="CisteSauge"?.65f:.8f)*scale;
  float minDistance=species=="ArbousierBuisson"||species=="BruyereFine"?1.15f+radius:species=="LentisqueRampant"?1.0f+radius*.65f:1.25f;
  if(Clear(p,radius)||MaquisRouteDefinition.Distance(p.x,p.y)<minDistance)return;
  string key=Mathf.FloorToInt(p.y/4).ToString();if(!chunks.ContainsKey(key))chunks[key]=new Chunk();var chunk=chunks[key];
  var matrix=Matrix4x4.TRS(new Vector3(p.x,Height(p.x,p.y)-.04f,p.y),Quaternion.Euler(R(-4,4),R(0,360),R(-4,4)),new Vector3(scale*R(.9f,1.12f),scale,scale*R(.9f,1.12f)));
  string model=V2+"/Models/"+species+"_"+variant;
  AddModel(chunk.high,model+(detailed?"_High.fbx":"_Low.fbx"),matrix);AddModel(chunk.low,model+"_Low.fbx",matrix);shrubCount++;
 }
 static void BakeChunks(){
  foreach(var item in chunks){var root=Group("Massif_Dense_"+item.Key,vegetation);var hi=Group("Detail_Proche",root);var lo=Group("Detail_Lointain",root);
   foreach(var entry in item.Value.high){var mesh=new Mesh();mesh.indexFormat=IndexFormat.UInt32;mesh.CombineMeshes(entry.Value.ToArray(),true,true);if(entry.Key!=wood)TrimWalkway(mesh);MeshGo("Maquis_High_"+item.Key+"_"+entry.Key.name,mesh,entry.Key,hi,true);}
   foreach(var entry in item.Value.low){var mesh=new Mesh();mesh.indexFormat=IndexFormat.UInt32;mesh.CombineMeshes(entry.Value.ToArray(),true,true);if(entry.Key!=wood)TrimWalkway(mesh);MeshGo("Maquis_Low_"+item.Key+"_"+entry.Key.name,mesh,entry.Key,lo,false);}
   var lod=root.gameObject.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.25f,hi.GetComponentsInChildren<Renderer>()),new LOD(.012f,lo.GetComponentsInChildren<Renderer>())});lod.RecalculateBounds();
  }
 }
 static void Vegetation(){
  chunks=new Dictionary<string,Chunk>();shrubCount=0;float length=MaquisRouteDefinition.Length;
  for(float s=-2.5f;s<length+3;s+=.72f)foreach(int sign in new[]{-1,1}){
   int i=Mathf.RoundToInt((s+3)*10);Shrub("LentisqueRampant",i%4,MaquisRouteDefinition.Side(s+R(-.15f,.15f),sign*R(2.3f,2.6f)),R(.85f,1.12f),true);
   Shrub("CisteSauge",i%4,MaquisRouteDefinition.Side(s+.3f,sign*R(1.5f,1.8f)),R(.8f,1.2f),true);
   Shrub("ArbousierBuisson",i%3,MaquisRouteDefinition.Side(s-.20f,sign*R(3.15f,3.60f)),R(1.1f,1.35f),true);
   Shrub(i%2==0?"ArbousierBuisson":"BruyereFine",i%3,MaquisRouteDefinition.Side(s+.20f,sign*R(3.8f,4.4f)),R(1.15f,1.5f),false);
   Shrub("ArbousierBuisson",i%3,MaquisRouteDefinition.Side(s+R(-.3f,.3f),sign*R(5.1f,6.2f)),R(1.1f,1.55f),false);
  }
  // Fill inside corners where a simple offset ribbon would leave a wedge-shaped hole.
  for(int i=0;i<210;i++){Vector2 p=new Vector2(R(-12,12),R(-18,24));float d=MaquisRouteDefinition.Distance(p.x,p.y);if(d<3.2f||d>10)continue;Shrub(i%3==0?"BruyereFine":"ArbousierBuisson",i%3,p,R(1,1.5f),false);}
  BakeChunks();
  int trees=0;for(float s=-2;s<length+2;s+=1.6f){int sign=trees%2==0?-1:1;Vector2 p=MaquisRouteDefinition.Side(s,sign*R(4.4f,6.5f));if(Clear(p,4)||MaquisRouteDefinition.Distance(p.x,p.y)<3.6f)continue;var asset=AssetDatabase.LoadAssetAtPath<GameObject>(V1+"/Prefabs/Chene_"+(trees%3)+".prefab");var go=(GameObject)PrefabUtility.InstantiatePrefab(asset);go.name="Chene_Couvert_"+trees;go.transform.SetParent(vegetation,false);go.transform.position=new Vector3(p.x,Height(p.x,p.y)-.06f,p.y);go.transform.rotation=Quaternion.Euler(0,R(0,360),0);go.transform.localScale=Vector3.one*R(.85f,1.10f);trees++;}
  // Close the rear of the departure. The foreground pathway remains open.
  for(int i=0;i<5;i++){var p=MaquisRouteDefinition.Side(-3.0f,-2.8f+i*1.35f);if(Clear(p,2))continue;var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(V2+"/Prefabs/ArbousierBuisson_"+(i%3)+".prefab"));go.name="Arriere_Depart_"+i;go.transform.SetParent(vegetation,false);go.transform.position=new Vector3(p.x,Height(p.x,p.y)-.04f,p.y);go.transform.rotation=Quaternion.Euler(0,i*112,0);go.transform.localScale=Vector3.one*1.4f;}
 }
 static void GrassAndPebbles(){
  var vs=new List<Vector3>();var cs=new List<Color>();var ts=new List<int>();
  for(int i=0;i<720;i++){
   float s=R(-1,MaquisRouteDefinition.Length);Vector2 p=MaquisRouteDefinition.Side(s,(i%2==0?-1:1)*R(1.12f,2.3f));if(Clear(p,.6f))continue;
   for(int j=0;j<6;j++){Vector3 b=new Vector3(p.x+R(-.1f,.1f),0,p.y+R(-.1f,.1f));b.y=Height(b.x,b.z);float a=R(0,Mathf.PI*2),h=R(.15f,.42f),w=R(.015f,.027f);var d=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));var side=new Vector3(d.z,0,-d.x)*w;int k=vs.Count;vs.AddRange(new[]{b-side,b+side,b+Vector3.up*h*.6f+d*h*.2f+side*.5f,b+Vector3.up*h+d*h*.45f,b+Vector3.up*h*.6f+d*h*.2f-side*.5f});for(int q=0;q<5;q++)cs.Add(C(i%4==0?"ACA272":i%4==1?"617C43":"819452")*(.7f+q*.07f));ts.AddRange(new[]{k,k+2,k+1,k,k+4,k+2,k+4,k+3,k+2});}
  }
  var mesh=new Mesh();mesh.SetVertices(vs);mesh.SetColors(cs);mesh.SetTriangles(ts,0);mesh.RecalculateNormals();mesh.RecalculateBounds();MeshGo("Herbes_Bordures_Hors_Clairiere",mesh,grass,vegetation);
  for(int i=0;i<65;i++){
   float s=R(0,MaquisRouteDefinition.Length);Vector2 p=MaquisRouteDefinition.Side(s,(i%2==0?-1:1)*R(1.0f,1.6f));if(Clear(p,.4f))continue;
   Rock("Pierre_Bordure_"+i,p,R(.12f,.34f),groundRoot,false);
  }
 }
 static GameObject Rock(string name,Vector2 p,float scale,Transform parent,bool coastal){
  var go=new GameObject(name);go.transform.SetParent(parent,false);var imported=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(V1+"/Models/Granit_Anguleux_"+(rng.Next(4))+".fbx"));imported.transform.SetParent(go.transform,false);go.transform.position=new Vector3(p.x,(coastal?CoastHeight(p.x,p.y):Height(p.x,p.y))-scale*.20f,p.y);go.transform.rotation=Quaternion.Euler(R(-8,8),R(0,360),R(-8,8));go.transform.localScale=new Vector3(scale*R(.8f,1.2f),scale*.6f,scale);foreach(var rr in go.GetComponentsInChildren<Renderer>())rr.sharedMaterial=stone;return go;
 }
 public static float CoastHeight(float x,float z){return Mathf.Max(.015f,(225-x)*.023f)+.012f*Mathf.Sin(z*.45f);}
 static Vector3[] CoastRoute(){return new[]{new Vector3(220,0,-4.35f),new Vector3(220,0,1),new Vector3(219.4f,0,6),new Vector3(220,0,12)};}
 static void BuildCoast(){
  var vs=new List<Vector3>();var cs=new List<Color>();var ts=new List<int>();int nx=52,nz=84;
  for(int j=0;j<=nz;j++)for(int i=0;i<=nx;i++){float x=200+i*.5f,z=-18+j*.5f;vs.Add(new Vector3(x,CoastHeight(x,z),z));float grain=Mathf.PerlinNoise(x*1.8f,z*1.8f);cs.Add(Color.Lerp(C("C7B48B"),C("E4D0A1"),grain));}
  for(int j=0;j<nz;j++)for(int i=0;i<nx;i++){int a=j*(nx+1)+i,b=a+1,c=a+nx+1,d=c+1;ts.AddRange(new[]{a,c,b,b,c,d});}var mesh=new Mesh();mesh.SetVertices(vs);mesh.SetColors(cs);mesh.SetTriangles(ts,0);mesh.RecalculateNormals();mesh.RecalculateBounds();MeshGo("Plage_Arrivee_Littoral",mesh,sand,coast,false,true);
  vs=new List<Vector3>();ts=new List<int>();int n=48;for(int j=0;j<=n;j++)for(int i=0;i<=n;i++)vs.Add(new Vector3(225+i*10,-.055f,-258+j*10));for(int j=0;j<n;j++)for(int i=0;i<n;i++){int a=j*(n+1)+i,b=a+1,c=a+n+1,d=c+1;ts.AddRange(new[]{a,c,b,b,c,d});}mesh=new Mesh();mesh.SetVertices(vs);mesh.SetTriangles(ts,0);mesh.RecalculateNormals();mesh.RecalculateBounds();MeshGo("Mer_Turquoise",mesh,water,coast);
  for(int i=0;i<22;i++){float x=i<10?R(207,214):R(224.5f,230),z=R(-12,23);Rock("Granit_Littoral_"+i,new Vector2(x,z),R(.5f,1.8f),coast,true);}
  // The coastal arrival is a small playable starting area; its vegetation and shore will be developed later.
 }
 static Mesh ArchMesh(){
  var vs=new List<Vector3>();var colors=new List<Color>();var ts=new List<int>();
  Action<Vector3[],Color> box=(points,color)=>{int k=vs.Count;vs.AddRange(points);for(int v=0;v<8;v++)colors.Add(color);int[] t={0,2,1,0,3,2,4,5,6,4,6,7,0,1,5,0,5,4,3,7,6,3,6,2,0,4,7,0,7,3,1,2,6,1,6,5};ts.AddRange(t.Select(i=>i+k));};
  for(int sign=-1;sign<=1;sign+=2)for(int row=0;row<5;row++){
   float a=sign<0?-1.52f:1.08f,b=a+.44f,y=row*.4f+.015f;box(new[]{new Vector3(a,y,-.24f),new Vector3(b,y,-.24f),new Vector3(b,y+.37f,-.24f),new Vector3(a,y+.37f,-.24f),new Vector3(a,y,.24f),new Vector3(b,y,.24f),new Vector3(b,y+.37f,.24f),new Vector3(a,y+.37f,.24f)},C(row%2==0?"B0A99B":"BDB09B"));
  }
  for(int i=0;i<11;i++){
   float a=i*Mathf.PI/11+.009f,b=(i+1)*Mathf.PI/11-.009f;
   Func<float,float,Vector3> pt=(angle,r)=>new Vector3(Mathf.Cos(angle)*r,2+Mathf.Sin(angle)*r,0);
   Vector3 a0=pt(a,1.07f),a1=pt(b,1.07f),b0=pt(a,1.50f),b1=pt(b,1.50f);var front=new Vector3(0,0,-.24f);var back=-front;
   box(new[]{a0+front,b0+front,b1+front,a1+front,a0+back,b0+back,b1+back,a1+back},C(i%2==0?"B8AD97":"A5A596"));
  }
  var mesh=new Mesh();mesh.SetVertices(vs);mesh.SetColors(colors);mesh.SetTriangles(ts,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
 }
 static Mesh Aperture(){
  var points=new List<Vector3>{new Vector3(-1.035f,.08f,-.01f),new Vector3(1.035f,.08f,-.01f)};
  for(int i=0;i<=24;i++){float a=i*Mathf.PI/24;points.Add(new Vector3(Mathf.Cos(a)*1.035f,2+Mathf.Sin(a)*1.035f,-.01f));}
  var verts=new List<Vector3>{new Vector3(0,1.5f,-.01f)};verts.AddRange(points);var uv=verts.Select(p=>new Vector2((p.x+1.035f)/2.07f,p.y/3.05f)).ToList();var tris=new List<int>();for(int i=0;i<points.Count;i++){tris.Add(0);tris.Add(1+(i+1)%points.Count);tris.Add(i+1);}var mesh=new Mesh();mesh.SetVertices(verts);mesh.SetUVs(0,uv);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
 }
 static BiomePortal Gate(string name,Vector3 pos,Vector3 forward){
  var t=Group(name,portalRoot);t.position=pos;t.rotation=Quaternion.LookRotation(forward);MeshGo(name+"_Arche_Granit",ArchMesh(),archStone,t,true);
  foreach(int sign in new[]{-1,1}){var post=Group("Collision_Montant_"+sign,t);post.localPosition=new Vector3(sign*1.30f,1,0);var box=post.gameObject.AddComponent<BoxCollider>();box.size=new Vector3(.44f,2,.48f);}
  return t.gameObject.AddComponent<BiomePortal>();
 }
 static Camera Window(BiomePortal gate,string name,Vector3 eye,Quaternion rotation,Transform player){
  string path=Root+"/Materials/"+name+".renderTexture";var rt=AssetDatabase.LoadAssetAtPath<RenderTexture>(path);if(rt==null){rt=new RenderTexture(512,768,24,RenderTextureFormat.ARGB32);rt.name=name;rt.antiAliasing=2;AssetDatabase.CreateAsset(rt,path);}rt.Create();
  var t=Group(name+"_Camera",portalRoot);t.position=eye;t.rotation=rotation;var cam=t.gameObject.AddComponent<Camera>();cam.enabled=false;cam.fieldOfView=65;cam.nearClipPlane=.05f;cam.farClipPlane=380;cam.targetTexture=rt;cam.cullingMask=~(1<<31);cam.clearFlags=CameraClearFlags.Skybox;
  var mat=Plain(name+"_Vue","Universal Render Pipeline/Unlit",Color.white);mat.SetTexture("_BaseMap",rt);var surface=MeshGo(name+"_Ouverture",Aperture(),mat,gate.transform);surface.layer=31;var preview=gate.gameObject.AddComponent<PortalPreview>();preview.Configure(cam,player);return cam;
 }
 public static string Build(){
  var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(Application.isPlaying||scene.path!="Assets/Scenes/Prototype_Maquis.unity")throw new Exception("Open Prototype_Maquis in Edit mode");
  Ready();rng=new System.Random(987451);var env=GameObject.Find("Maquis_Environment").transform;var anchors=Resources.FindObjectsOfTypeAll<Transform>().Where(t=>t.gameObject.scene==scene&&(t.name.StartsWith("Emplacement_")||t.name.StartsWith("Son_Directionnel_"))).ToArray();foreach(var t in anchors)t.SetParent(env,true);
  var old=env.Find("Sentier_Zigzag_Portail_Littoral");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
  foreach(Transform t in env)t.gameObject.SetActive(false);
  var root=Group("Sentier_Zigzag_Portail_Littoral",env);groundRoot=Group("01_Terrain_Zigzag",root);vegetation=Group("02_Maquis_Occultant",root);portalRoot=Group("03_Portails",root);coast=Group("04_Arrivee_Littoral",root);
  soil=Mat("Terre_Sentier",AssetDatabase.LoadAssetAtPath<Material>(V1+"/Materials/Sol_Sous_Bois.mat"));wood=Mat("Bois_Maquis",AssetDatabase.LoadAssetAtPath<Material>(V2+"/Materials/Ecorces_Chaudes.mat"));leaf=Mat("Feuilles_Maquis",AssetDatabase.LoadAssetAtPath<Material>(V1+"/Materials/Feuillages_Mats.mat"));flower=Mat("Fleurs_Ivoire",AssetDatabase.LoadAssetAtPath<Material>(V2+"/Materials/Fleurs_Ivoire.mat"));grass=Mat("Herbes_Bordures",AssetDatabase.LoadAssetAtPath<Material>(V2+"/Materials/Herbes_Vertes_et_Seches.mat"));stone=Mat("Granit",AssetDatabase.LoadAssetAtPath<Material>(V2+"/Materials/Granit_Chauds_Lichens.mat"));archStone=Mat("Granit_Arches",stone);archStone.SetFloat("_ColorsAreSRGB",0);
  sand=Mat("Sable_Littoral",grass);sand.SetFloat("_Mode",4);sand.SetFloat("_Cull",2);sand.SetColor("_BaseColor",Color.white);sand.SetColor("_ShadowTint",new Color(.67f,.74f,.73f));water=Plain("Mer_Littoral","CorseVR/Coast Water",Color.white);
  MeshGo("Sol_Zigzag_52m",NearTerrain(),soil,groundRoot,false,true);FarTerrain();Vegetation();GrassAndPebbles();BuildCoast();
  var interactionRoot=Group("05_Reperes_Interactions",root);foreach(var t in anchors){t.SetParent(interactionRoot,true);t.gameObject.SetActive(true);float s=t.name.Contains("Nepita")?7:MaquisRouteDefinition.Length*.58f;var p=MaquisRouteDefinition.Side(s,t.name.Contains("Nepita")?-1.0f:1.3f);t.position=new Vector3(p.x,Height(p.x,p.y),p.y);}
  var player=GameObject.Find("Joueur_Desktop");var start=MaquisRouteDefinition.Point(0);var cc=player.GetComponent<CharacterController>();cc.enabled=false;player.transform.position=new Vector3(start.x,Height(start.x,start.y)+.035f,start.y);player.transform.rotation=Quaternion.LookRotation(MaquisRouteDefinition.Forward(0));player.GetComponent<SentierCorridor>().Configure(MaquisRouteDefinition.Corridor(),.76f);cc.enabled=true;
  var main=player.GetComponentInChildren<Camera>();main.transform.localPosition=new Vector3(0,1.65f,0);main.transform.localRotation=Quaternion.identity;main.farClipPlane=380;
  var departure=GameObject.Find("Depart_WebXR");departure.transform.position=player.transform.position-Vector3.up*.035f;departure.transform.rotation=player.transform.rotation;
  Vector3 forward=MaquisRouteDefinition.Forward(MaquisRouteDefinition.Length-.2f);var gate=Gate("Portail_Vers_Littoral",new Vector3(finish.x,Height(finish.x,finish.y),finish.y)+forward*.05f,forward);var back=Gate("Portail_Retour_Maquis",new Vector3(220,CoastHeight(220,-4.1f),-4.1f),Vector3.back);
  var coastArrival=Group("Arrivee_Depuis_Maquis",portalRoot);coastArrival.position=new Vector3(220,CoastHeight(220,-1.8f)+.035f,-1.8f);coastArrival.rotation=Quaternion.Euler(0,15,0);
  var pReturn=MaquisRouteDefinition.Point(MaquisRouteDefinition.Length-1.8f);var maquisArrival=Group("Arrivee_Retour_Maquis",portalRoot);maquisArrival.position=new Vector3(pReturn.x,Height(pReturn.x,pReturn.y)+.035f,pReturn.y);maquisArrival.rotation=Quaternion.LookRotation(-MaquisRouteDefinition.Forward(MaquisRouteDefinition.Length-1.8f));
  gate.Configure(player.transform,coastArrival,back,CoastRoute(),1.1f,"Littoral");back.Configure(player.transform,maquisArrival,gate,MaquisRouteDefinition.Corridor(),.76f,"Maquis");
  var previewCoast=Window(gate,"Fenetre_Littoral",coastArrival.position+Vector3.up*1.65f,Quaternion.Euler(0,43,0),player.transform);var previewMaquis=Window(back,"Fenetre_Maquis",maquisArrival.position+Vector3.up*1.65f,maquisArrival.rotation,player.transform);
  RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.007f;RenderSettings.fogColor=new Color(.59f,.73f,.77f);
  Physics.SyncTransforms();AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);previewCoast.Render();previewMaquis.Render();
  return "Saved zigzag route "+MaquisRouteDefinition.Length.ToString("F2")+"m; "+shrubCount+" combined shrubs; clearing radius 4.7m; two-way littoral portals; playable coastal arrival in same scene.";
 }
 public static string Capture(string directory){
  Directory.CreateDirectory(directory);foreach(var camera in GameObject.Find("Sentier_Zigzag_Portail_Littoral").GetComponentsInChildren<Camera>())if(camera.targetTexture!=null)camera.Render();
  var cam=Camera.main;var pos=cam.transform.position;var rot=cam.transform.rotation;float aspect=cam.aspect,fov=cam.fieldOfView;var target=cam.targetTexture;var active=RenderTexture.active;
  string[] names={"01_Depart","02_Premier_Virage","03_Cote_Gauche","04_Cote_Droit","05_Clairiere_Portail","06_Littoral","07_Vue_Parcours"};float[] distances={0,7,13,19,MaquisRouteDefinition.Length-4,0,0};
  try{for(int i=0;i<names.Length;i++){
   Vector2 p=MaquisRouteDefinition.Point(distances[i]);cam.transform.position=new Vector3(p.x,Height(p.x,p.y)+1.65f,p.y);var forward=MaquisRouteDefinition.Forward(distances[i]);
   if(i==2)forward=Quaternion.Euler(0,-90,0)*forward;if(i==3)forward=Quaternion.Euler(0,90,0)*forward;
   if(i==4)forward=(new Vector3(finish.x,Height(finish.x,finish.y)+1.6f,finish.y)-cam.transform.position).normalized;
   if(i==5){cam.transform.position=new Vector3(220,CoastHeight(220,-1.8f)+1.65f,-1.8f);forward=Quaternion.Euler(0,43,0)*Vector3.forward;}
   if(i==6){cam.transform.position=new Vector3(17,28,-19);forward=(new Vector3(0,0,2)-cam.transform.position).normalized;}
   cam.transform.rotation=Quaternion.LookRotation(forward);cam.aspect=1920f/1080;cam.fieldOfView=65;var rt=RenderTexture.GetTemporary(1920,1080,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB,4);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1920,1080,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1920,1080),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(directory,names[i]+".png"),tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);cam.targetTexture=null;RenderTexture.active=null;RenderTexture.ReleaseTemporary(rt);
  }}finally{cam.transform.position=pos;cam.transform.rotation=rot;cam.aspect=aspect;cam.fieldOfView=fov;cam.targetTexture=target;RenderTexture.active=active;}
  return "7 native 1920x1080 captures, 6 eye-level plus overhead route check";
 }
}
}
