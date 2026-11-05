using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace CorseVR.Editor {
public static class SentierCorseFinition {
 const string Root="Assets/Prototype_Maquis/Art/SentierCorseV2";
 const string Kit="Assets/Prototype_Maquis/Art/SentierCorse";
 static System.Random rng;
 static Transform plants,scenery,stoneRoot;
 static Material wood,flower,granite,landMat,grass;
 static Dictionary<string,Material> leaves=new Dictionary<string,Material>();
 static Dictionary<string,GameObject> prefabs=new Dictionary<string,GameObject>();
 static float R(float a,float b){return a+(float)rng.NextDouble()*(b-a);}
 static Color C(string h){Color c;ColorUtility.TryParseHtmlString("#"+h,out c);return c.linear;}
 static Transform Group(string name,Transform parent){var go=new GameObject(name);go.transform.SetParent(parent,false);return go.transform;}
 static Material Mat(string name,Material source){string path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(source);AssetDatabase.CreateAsset(m,path);}else EditorUtility.CopySerialized(source,m);m.name=name;m.enableInstancing=true;EditorUtility.SetDirty(m);return m;}
 static Mesh SaveMesh(Mesh m,string name){m.name=name;string p=Root+"/Meshes/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(p);if(old==null){AssetDatabase.CreateAsset(m,p);return m;}EditorUtility.CopySerialized(m,old);EditorUtility.SetDirty(old);UnityEngine.Object.DestroyImmediate(m);return old;}
 static GameObject MeshGo(string n,Mesh m,Material mat,Transform parent,bool shadows){var go=new GameObject(n);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=SaveMesh(m,n);var rr=go.AddComponent<MeshRenderer>();rr.sharedMaterial=mat;rr.shadowCastingMode=shadows?ShadowCastingMode.On:ShadowCastingMode.Off;go.isStatic=true;return go;}
 static float Gaussian(float x,float z,float a,float b,float wx,float wz){return Mathf.Exp(-((x-a)*(x-a)/(wx*wx)+(z-b)*(z-b)/(wz*wz)));}
 static bool patchesReady;
 static Vector2 start,end,entryDir,endDir;
 static float startY,endY;
 static void PatchPoints(){if(patchesReady)return;start=BavellaJourneyBuilder.Point(0);end=BavellaJourneyBuilder.Point(100);var f=BavellaJourneyBuilder.Forward(.3f);entryDir=new Vector2(f.x,f.z);f=BavellaJourneyBuilder.Forward(22.5f);endDir=new Vector2(f.x,f.z);startY=BavellaJourneyBuilder.Height(start.x,start.y);endY=BavellaJourneyBuilder.Height(end.x,end.y);patchesReady=true;}
 static float EntryDistance(Vector2 p){PatchPoints();float along=Mathf.Clamp(Vector2.Dot(p-start,entryDir),-5,0);return Vector2.Distance(p,start+entryDir*along);}
 static float GroundHeight(float x,float z,float original){
  PatchPoints();Vector2 p=new Vector2(x,z);float before=Vector2.Dot(p-start,entryDir),after=Vector2.Dot(p-end,endDir);
  if(before<0&&before>-8){float w=(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.8f,1.9f,EntryDistance(p))))*Mathf.SmoothStep(0,1,Mathf.InverseLerp(-7,-3,before));original=Mathf.Lerp(original,startY+before*.012f,w);}
  if(after>0&&after<3){float w=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.0f,2.0f,Vector2.Distance(p,end)));original=Mathf.Lerp(original,endY+after*.012f,w);}
  return original;
 }
 public static float Height(float x,float z){
  float old=BavellaJourneyBuilder.Height(x,z),edge=Mathf.Max(Mathf.Abs(x),Mathf.Abs(z));
  if(edge<=18)return GroundHeight(x,z,old);
  float mass=Mathf.Max(32*Gaussian(x,z,-65,72,35,31),46*Gaussian(x,z,-22,94,36,32),57*Gaussian(x,z,24,116,40,37),37*Gaussian(x,z,76,78,34,29));
  mass=Mathf.Max(mass,24*Gaussian(x,z,-66,-52,32,28),27*Gaussian(x,z,66,-54,33,31));
  float ridge=1-Mathf.Abs(Mathf.PerlinNoise(x*.043f+63,z*.036f+39)*2-1);
  float gullies=Mathf.Abs(Mathf.Sin(x*.16f+z*.10f+Mathf.PerlinNoise(x*.03f,z*.03f)*4));
  float detail=1-Mathf.Abs(2*Mathf.PerlinNoise(x*.11f+12,z*.09f+52)-1);
  float h=.5f+3.5f*Mathf.PerlinNoise(x*.025f+10,z*.025f+46)+mass*(.65f+.30f*ridge+.17f*detail)-mass*.16f*gullies;
  return Mathf.Lerp(old,h,Mathf.SmoothStep(0,1,Mathf.InverseLerp(18,44,edge)));
 }
 static Vector2 Side(float s,float offset){var f=BavellaJourneyBuilder.Forward(Mathf.Clamp(s,.3f,22.5f));Vector2 p=BavellaJourneyBuilder.Point(Mathf.Clamp(s,0,22.9f));if(s<0)p+=new Vector2(f.x,f.z)*s;if(s>22.9f)p+=new Vector2(f.x,f.z)*(s-22.9f);return p+new Vector2(f.z,-f.x)*offset;}
 static bool InTrail(Vector2 p,float margin){
  if(BavellaJourneyBuilder.PathDistance(p.x,p.y)<margin)return true;
  Vector2 start=Side(-4,0),end=Side(0,0),d=end-start;float t=Mathf.Clamp01(Vector2.Dot(p-start,d)/d.sqrMagnitude);
  return Vector2.Distance(p,start+d*t)<margin;
 }
 static Material Resolve(Material original,string species){string n=original==null?"":original.name;return n.Contains("Wood")?wood:n.Contains("Flower")?flower:n.Contains("Granite")?granite:leaves[species];}
 static GameObject Model(string file,string species,Transform parent){var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/"+file+".fbx");if(asset==null)throw new Exception("Missing "+file);var go=(GameObject)PrefabUtility.InstantiatePrefab(asset);go.transform.SetParent(parent,false);foreach(var r in go.GetComponentsInChildren<Renderer>()){r.sharedMaterials=r.sharedMaterials.Select(m=>Resolve(m,species)).ToArray();r.lightProbeUsage=LightProbeUsage.Off;r.reflectionProbeUsage=ReflectionProbeUsage.Off;}return go;}
 static GameObject Prefab(string species,int variant){string key=species+"_"+variant;if(prefabs.ContainsKey(key))return prefabs[key];var g=new GameObject(key);var hi=Model(key+"_High",species,g.transform);var lo=Model(key+"_Low",species,g.transform);var lod=g.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.13f,hi.GetComponentsInChildren<Renderer>()),new LOD(.012f,lo.GetComponentsInChildren<Renderer>())});lod.RecalculateBounds();var prefab=PrefabUtility.SaveAsPrefabAsset(g,Root+"/Prefabs/"+key+".prefab");UnityEngine.Object.DestroyImmediate(g);prefabs[key]=prefab;return prefab;}
 static GameObject Plant(string species,int variant,Vector2 p,float scale){var go=(GameObject)PrefabUtility.InstantiatePrefab(Prefab(species,variant));go.transform.SetParent(plants,false);go.transform.position=new Vector3(p.x,Height(p.x,p.y)-.035f,p.y);go.transform.rotation=Quaternion.Euler(R(-5,5),R(0,360),R(-4,4));go.transform.localScale=new Vector3(scale*R(.85f,1.2f),scale,scale*R(.8f,1.2f));foreach(var t in go.GetComponentsInChildren<Transform>())t.gameObject.isStatic=true;return go;}
 static void Rock(Vector2 p,Vector3 size,int variant,string label){
  var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Kit+"/Models/Granit_Anguleux_"+(variant%4)+".fbx");var go=(GameObject)PrefabUtility.InstantiatePrefab(asset);go.name=label;go.transform.SetParent(stoneRoot,false);go.transform.position=new Vector3(p.x,Height(p.x,p.y)-size.y*.35f,p.y);go.transform.rotation=Quaternion.Euler(R(-13,13),R(0,360),R(-9,9));go.transform.localScale=size;
  foreach(var rr in go.GetComponentsInChildren<Renderer>())rr.sharedMaterial=granite;
  if(BavellaJourneyBuilder.PathDistance(p.x,p.y)<5)foreach(var mf in go.GetComponentsInChildren<MeshFilter>())mf.gameObject.AddComponent<MeshCollider>().sharedMesh=mf.sharedMesh;
 }
 static void Landscape(){
  int count=180;float step=2;var vs=new List<Vector3>();var ns=new List<Vector3>();var uv=new List<Vector2>();var colors=new List<Color>();var indices=new List<int>();
  for(int j=0;j<=count;j++)for(int i=0;i<=count;i++){
   float x=-180+i*step,z=-180+j*step,y=Height(x,z),e=.3f;Vector3 n=new Vector3(Height(x-e,z)-Height(x+e,z),2*e,Height(x,z-e)-Height(x,z+e)).normalized;
   vs.Add(new Vector3(x,y,z));ns.Add(n);uv.Add(new Vector2(x,z));
   float patch=Mathf.PerlinNoise(x*.16f+4,z*.16f+38);float stone=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.40f,.78f,1-n.y))*.65f+Mathf.SmoothStep(0,1,Mathf.InverseLerp(18,37,y))*.7f;
   Color foliage=Color.Lerp(C("4B633D"),C("7F8955"),patch);Color rock=Color.Lerp(C("8C8D80"),C("B8AC97"),patch);colors.Add(Color.Lerp(foliage,rock,Mathf.Clamp01(stone)));
  }
  for(int j=0;j<count;j++)for(int i=0;i<count;i++){float x=-180+(i+.5f)*step,z=-180+(j+.5f)*step;if(Mathf.Abs(x)<18&&Mathf.Abs(z)<18)continue;int a=j*(count+1)+i,b=a+1,c=a+count+1,d=c+1;indices.AddRange(new[]{a,c,b,b,c,d});}
  var mesh=new Mesh();mesh.indexFormat=IndexFormat.UInt32;mesh.SetVertices(vs);mesh.SetNormals(ns);mesh.SetUVs(0,uv);mesh.SetColors(colors);mesh.SetTriangles(indices,0);mesh.RecalculateBounds();MeshGo("Versants_Corses_360m",mesh,landMat,scenery,false);
 }
 static void NearTerrain(Transform old){
  var mesh=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Prototype_Maquis/Art/Bavella/Meshes/Terrain_Proche_36m.asset"));
  var vertices=mesh.vertices;var colors=mesh.colors;
  for(int i=0;i<vertices.Length;i++){
   Vector2 p=new Vector2(vertices[i].x,vertices[i].z);vertices[i].y=Height(p.x,p.y);
   float entrance=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.8f,1.3f,EntryDistance(p)));
   float landing=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.1f,1.7f,Vector2.Distance(p,end)));
   colors[i].r=Mathf.Max(colors[i].r,entrance,landing);
  }
  mesh.vertices=vertices;mesh.colors=colors;mesh.RecalculateNormals();mesh.RecalculateBounds();var saved=SaveMesh(mesh,"Sol_Entree_et_Fin_Soignes");
  var go=old.Find("01_Terrain_et_Enveloppe/Sol_Corse_Proche");go.GetComponent<MeshFilter>().sharedMesh=saved;go.GetComponent<MeshCollider>().sharedMesh=saved;
 }
 class Batch {public Material material;public List<CombineInstance> instances=new List<CombineInstance>();}
 static Dictionary<string,Batch> batches;
 static void Scatter(string model,string species,Vector2 p,float scale,bool pine=false){
  var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/"+model+".fbx");var matrix=Matrix4x4.TRS(new Vector3(p.x,Height(p.x,p.y)-.05f,p.y),Quaternion.Euler(0,R(0,360),0),new Vector3(scale*R(.85f,1.15f),scale,scale*R(.85f,1.15f)));
  foreach(var mf in asset.GetComponentsInChildren<MeshFilter>()){
   var renderer=mf.GetComponent<Renderer>();var mats=renderer.sharedMaterials;
   for(int sub=0;sub<mf.sharedMesh.subMeshCount;sub++){
    Material mat=Resolve(mats[Mathf.Min(sub,mats.Length-1)],species);if(mat!=wood&&mat!=flower&&mat!=granite)mat=leaves["PinFond"];string key="Fond_"+Mathf.FloorToInt(p.x/32)+"_"+Mathf.FloorToInt(p.y/32)+"_"+mat.name;
    if(!batches.ContainsKey(key))batches[key]=new Batch{material=mat};batches[key].instances.Add(new CombineInstance{mesh=mf.sharedMesh,subMeshIndex=sub,transform=matrix*mf.transform.localToWorldMatrix});
   }
  }
 }
 static void Background(){
  batches=new Dictionary<string,Batch>();
  for(int i=0;i<560;i++){
   Vector2 p=new Vector2(R(-52,52),R(-47,75));float d=BavellaJourneyBuilder.PathDistance(p.x,p.y);if(d<6||InTrail(p,3)||Height(p.x,p.y)>17)continue;
   string species=i%3==0?"ArbousierBuisson":i%3==1?"LentisqueRampant":"BruyereFine";int v=i%(species=="LentisqueRampant"?4:3);
   Scatter(species+"_"+v+"_Low",species,p,R(1.05f,1.85f));
  }
  for(int i=0;i<110;i++){
   Vector2 p=new Vector2(R(-70,70),R(-45,98));if(InTrail(p,32)||Height(p.x,p.y)>20)continue;Scatter("PinFond_"+(i%3),"PinFond",p,R(.78f,1.3f),true);
  }
  foreach(var entry in batches){var m=new Mesh();m.indexFormat=IndexFormat.UInt32;m.CombineMeshes(entry.Value.instances.ToArray(),true,true);MeshGo(entry.Key,m,entry.Value.material,scenery,false);}
 }
 static void NearGrass(){
  for(int section=0;section<7;section++){
   var vs=new List<Vector3>();var cs=new List<Color>();var ts=new List<int>();
   for(int t=0;t<185;t++){
    float s=-3+section*4.5f+R(0,4.5f),off=(t%2==0?-1:1)*R(1.05f,3.8f);Vector2 p=Side(s,off);
    for(int j=0;j<5;j++){
     Vector3 b=new Vector3(p.x+R(-.17f,.17f),0,p.y+R(-.17f,.17f));b.y=Height(b.x,b.z);float a=R(0,Mathf.PI*2),h=R(.13f,.38f),w=R(.012f,.028f);Vector3 dir=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)),side=new Vector3(dir.z,0,-dir.x)*w;int k=vs.Count;
     vs.AddRange(new[]{b-side,b+side,b+Vector3.up*h*.6f+dir*h*.24f+side*.5f,b+Vector3.up*h+dir*h*.46f,b+Vector3.up*h*.6f+dir*h*.24f-side*.5f});
     Color c=C(t%5==0?"B1A47A":t%5==1?"83915B":t%5==2?"5E7D46":"768C51");for(int q=0;q<5;q++)cs.Add(c*(.65f+q*.085f));ts.AddRange(new[]{k,k+2,k+1,k,k+4,k+2,k+4,k+3,k+2});
    }
   }
   var mesh=new Mesh();mesh.SetVertices(vs);mesh.SetColors(cs);mesh.SetTriangles(ts,0);mesh.RecalculateNormals();mesh.RecalculateBounds();MeshGo("Herbes_Variees_"+section,mesh,grass,plants,false);
  }
 }
 public static string Build(){
  var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(Application.isPlaying||scene.path!="Assets/Scenes/Prototype_Maquis.unity")throw new Exception("Requires Prototype_Maquis Edit mode");
  BavellaJourneyBuilder.Route();patchesReady=false;PatchPoints();rng=new System.Random(143891);prefabs.Clear();leaves.Clear();
  var env=GameObject.Find("Maquis_Environment").transform;var baseRoot=env.Find("Sentier_Corse_Dense");if(baseRoot==null)throw new Exception("V1 missing");
  foreach(Transform t in baseRoot.Find("Vegetation_en_strates"))if(t.name.StartsWith("Lentisque")||t.name.StartsWith("Ciste")||t.name.StartsWith("Herbes_"))t.gameObject.SetActive(false);
  var original=env.Find("Parcours_Corse_Bavella");original.Find("01_Terrain_et_Enveloppe/Enveloppe_Paysage_240m").gameObject.SetActive(false);
  var old=env.Find("Sentier_Corse_Finition");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
  var root=Group("Sentier_Corse_Finition",env);plants=Group("01_Arbustes_Differencies",root);scenery=Group("03_Versants_Boises_et_Reliefs",root);stoneRoot=Group("02_Entree_et_Affleurements",root);
  var leafSource=AssetDatabase.LoadAssetAtPath<Material>(Kit+"/Materials/Feuillages_Mats.mat");
  string[] species={"LentisqueRampant","ArbousierBuisson","CisteSauge","BruyereFine","PinFond"};
  foreach(string s in species){var mat=Mat(s,leafSource);mat.SetColor("_BaseColor",s=="CisteSauge"?new Color(.98f,1.08f,1.02f):s=="BruyereFine"?new Color(.95f,1.13f,1.05f):new Color(1.02f,1.17f,1.08f));leaves[s]=mat;}
  wood=Mat("Ecorces_Chaudes",AssetDatabase.LoadAssetAtPath<Material>(Kit+"/Materials/Ecorce_Chene_Arbousier.mat"));granite=Mat("Granit_Chauds_Lichens",AssetDatabase.LoadAssetAtPath<Material>(Kit+"/Materials/Granit_Gris_Chaud.mat"));granite.SetColor("_BaseColor",new Color(.88f,.87f,.82f));
  grass=Mat("Herbes_Vertes_et_Seches",AssetDatabase.LoadAssetAtPath<Material>(Kit+"/Materials/Herbes_Ombre_Sauge.mat"));flower=Mat("Fleurs_Ivoire",grass);flower.SetFloat("_ColorsAreSRGB",1);flower.SetFloat("_Cull",0);
  landMat=Mat("Versants_Verts_Granit",granite);landMat.shader=Shader.Find("CorseVR/Landscape Varie");landMat.SetFloat("_Mode",5);landMat.SetFloat("_ColorsAreSRGB",0);landMat.SetFloat("_Cull",2);landMat.SetColor("_BaseColor",Color.white);landMat.SetColor("_ShadowTint",new Color(.45f,.57f,.56f));
  // The shapes, not just rotations, alternate: compact, upright, airy and low flowering.
  int nearCount=0;
  for(int i=0;i<24;i++)foreach(int sign in new[]{-1,1}){
   float s=-3+i*1.4f+R(-.3f,.3f);
   Plant("LentisqueRampant",i%4,Side(s,sign*R(1.9f,2.65f)),R(.74f,1.14f));nearCount++;
   if(i%3!=1){string type=i%2==0?"ArbousierBuisson":"BruyereFine";Plant(type,i%3,Side(s+R(-.4f,.4f),sign*R(3,4.0f)),R(.8f,1.2f));nearCount++;}
   Plant("CisteSauge",i%4,Side(s+R(-.5f,.5f),sign*R(1.35f,1.8f)),R(.75f,1.25f));nearCount++;
  }
  // Make the ending and back of the entrance as furnished as the middle.
  for(int i=0;i<54;i++){
   float s=i<28?R(17,33):R(-8,1);float off=(i%2==0?-1:1)*R(3.6f,8.5f);string type=species[i%4];Plant(type,i%(type=="LentisqueRampant"||type=="CisteSauge"?4:3),Side(s,off),R(.9f,1.45f));nearCount++;
  }
  for(int i=0;i<58;i++){
   float s=R(14,31),off=(i%2==0?-1:1)*R(2.1f,5.0f);string type=i%3==0?"ArbousierBuisson":i%3==1?"LentisqueRampant":"BruyereFine";
   Plant(type,i%(type=="LentisqueRampant"?4:3),Side(s,off),R(.95f,1.4f));nearCount++;
  }
  // Layered banks beyond both sides, not a thin line of planting.
  for(int i=0;i<56;i++){
   Vector2 p=new Vector2(R(-13,14),R(-22,30));if(InTrail(p,4.0f))continue;string type=i%2==0?"ArbousierBuisson":"BruyereFine";Plant(type,i%3,p,R(1.0f,1.45f));nearCount++;
  }
  for(int i=0;i<13;i++){
   float s=i<7?R(19,30):R(-7,0);Rock(Side(s,(i%2==0?-1:1)*R(3.4f,5.4f)),new Vector3(R(.9f,1.7f),R(.7f,1.4f),R(.8f,1.6f)),i,"Banquette_Granit_"+i);
  }
  // Quiet dry-stone entrance edge, staggered and softened by cistus.
  for(int side=-1;side<=1;side+=2)for(int j=0;j<4;j++)Rock(Side(-2.4f+j*.48f,side*1.7f),new Vector3(.30f,.28f,.38f),j,"Pierre_Entree_"+side+"_"+j);
  for(int i=0;i<36;i++){
   Vector2 p=new Vector2(R(-46,47),R(-38,82));if(InTrail(p,8))continue;float sz=R(1.2f,3.0f);Rock(p,new Vector3(sz*R(1,1.5f),sz*.8f,sz*R(.8f,1.3f)),i,"Affleurement_Versant_"+i);
  }
  NearTerrain(original);Landscape();Background();NearGrass();
  // A few lower branches and taller screens beyond the end prevent a visible scenery cutoff.
  for(int i=0;i<5;i++){
   string prefabPath=Kit+"/Prefabs/Chene_"+(i%3)+".prefab";var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);go.name="Chene_Fin_"+i;go.transform.SetParent(plants,false);Vector2 p=Side(24+i*1.5f,(i%2==0?-1:1)*R(4.5f,7));go.transform.position=new Vector3(p.x,Height(p.x,p.y)-.06f,p.y);go.transform.rotation=Quaternion.Euler(0,R(0,360),0);go.transform.localScale=Vector3.one*R(.8f,1.15f);
  }
  for(int i=0;i<4;i++){
   var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Kit+"/Prefabs/Chene_"+(i%3)+".prefab"));go.name="Chene_Encadrement_Fin_"+i;go.transform.SetParent(plants,false);Vector2 p=Side(18+i*2.1f,(i%2==0?-1:1)*3.2f);go.transform.position=new Vector3(p.x,Height(p.x,p.y)-.05f,p.y);go.transform.rotation=Quaternion.Euler(0,R(0,360),0);go.transform.localScale=Vector3.one*R(.90f,1.15f);
  }
  for(int i=0;i<8;i++){
   string name="ArbousierBuisson_"+(i%3);var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/"+name+".prefab"));go.name="Fond_Entree_"+i;go.transform.SetParent(plants,false);Vector2 p=Side(-4.6f-(i%2)*.35f,-2.2f+i*.62f);go.transform.position=new Vector3(p.x,Height(p.x,p.y)-.04f,p.y);go.transform.rotation=Quaternion.Euler(0,i*119,0);go.transform.localScale=Vector3.one*(1.12f+(i%3)*.06f);
  }
  for(int i=0;i<5;i++){
   var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/LentisqueRampant_"+(i%4)+".prefab"));go.name="Bordure_Fond_Entree_"+i;go.transform.SetParent(plants,false);Vector2 p=Side(-3.8f,-2.1f+i);go.transform.position=new Vector3(p.x,Height(p.x,p.y)-.04f,p.y);go.transform.rotation=Quaternion.Euler(0,i*113,0);go.transform.localScale=Vector3.one*1.1f;
  }
  RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.0105f;RenderSettings.fogColor=new Color(.58f,.71f,.73f);
  Physics.SyncTransforms();AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
  return "V2 saved; "+(nearCount+13)+" differentiated shrubs; 9 end trees; layered middle-distance scrub/pines; rocky 360m landscape; 57 stone groups; corridor unchanged.";
 }
 public static string Capture(string directory){
  Directory.CreateDirectory(directory);var cam=Camera.main;var oldp=cam.transform.position;var oldr=cam.transform.rotation;var oldtarget=cam.targetTexture;float oldAspect=cam.aspect,oldFov=cam.fieldOfView;var oldactive=RenderTexture.active;
  string[] names={"01_Entree","02_Milieu","03_Approche_Fin","04_Fin_Panorama","05_Cote_Fin","06_Retour_Entree"};float[] s={0,6,18,22.4f,21,0};
  try{for(int i=0;i<s.Length;i++){
   Vector2 p=BavellaJourneyBuilder.Point(s[i]);cam.transform.position=new Vector3(p.x,Height(p.x,p.y)+1.65f,p.y);Vector3 dir=BavellaJourneyBuilder.Forward(s[i]);if(i==4)dir=new Vector3(-1,0,.25f);if(i==5)dir=-dir;
   cam.transform.rotation=Quaternion.LookRotation(dir);cam.aspect=1920f/1080;cam.fieldOfView=65;
   var rt=RenderTexture.GetTemporary(1920,1080,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB,4);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1920,1080,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1920,1080),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(directory,names[i]+".png"),tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);cam.targetTexture=null;RenderTexture.active=null;RenderTexture.ReleaseTemporary(rt);
  }}finally{cam.transform.position=oldp;cam.transform.rotation=oldr;cam.targetTexture=oldtarget;cam.aspect=oldAspect;cam.fieldOfView=oldFov;RenderTexture.active=oldactive;}
  return "6 native captures, eye 1.65m, 1920x1080, FOV65, MSAA4";
 }
}
}
