using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace CorseVR.Editor
{
 public static class SentierCorseBuilder
 {
  const string Root="Assets/Prototype_Maquis/Art/SentierCorse";
  static System.Random rng;
  static Transform vegetation,rocks;
  static Material leaf,wood,stone,grass;
  static readonly Dictionary<string,GameObject> prefabs=new Dictionary<string,GameObject>();
  static float R(float a,float b){return a+(float)rng.NextDouble()*(b-a);}
  static Color C(string hex){Color c;ColorUtility.TryParseHtmlString("#"+hex,out c);return c.linear;}
  static Transform Group(string name,Transform parent){var t=new GameObject(name).transform;t.SetParent(parent,false);return t;}
  static Material Mat(string name,Material source){
   string path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(m==null){m=new Material(source);AssetDatabase.CreateAsset(m,path);}else EditorUtility.CopySerialized(source,m);
   m.name=name;m.enableInstancing=true;EditorUtility.SetDirty(m);return m;
  }
  static GameObject Model(string file,Transform parent){
   var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/"+file+".fbx");
   if(asset==null)throw new Exception("Missing model "+file);
   var go=(GameObject)PrefabUtility.InstantiatePrefab(asset);go.transform.SetParent(parent,false);
   foreach(var r in go.GetComponentsInChildren<Renderer>()){
    var mats=r.sharedMaterials;
    for(int i=0;i<mats.Length;i++)mats[i]=mats[i]!=null&&mats[i].name.Contains("Wood")?wood:mats[i]!=null&&mats[i].name.Contains("Granite")?stone:mats[i]!=null&&mats[i].name.Contains("Flower")?grass:leaf;
    r.sharedMaterials=mats;r.lightProbeUsage=LightProbeUsage.Off;r.reflectionProbeUsage=ReflectionProbeUsage.Off;
   }
   return go;
  }
  static GameObject Prefab(string species,int variant){
   string key=species+"_"+variant;
   if(prefabs.ContainsKey(key))return prefabs[key];
   var go=new GameObject(key);var hi=Model(key+"_High",go.transform);var lo=Model(key+"_Low",go.transform);
   var lod=go.AddComponent<LODGroup>();bool tree=species=="Chene"||species=="Arbousier";
   lod.SetLODs(new[]{new LOD(tree?.20f:.23f,hi.GetComponentsInChildren<Renderer>()),new LOD(tree?.012f:.018f,lo.GetComponentsInChildren<Renderer>())});lod.RecalculateBounds();
   var saved=PrefabUtility.SaveAsPrefabAsset(go,Root+"/Prefabs/"+key+".prefab");UnityEngine.Object.DestroyImmediate(go);prefabs[key]=saved;return saved;
  }
  static GameObject Plant(string species,int variant,Vector2 p,float scale){
   var go=(GameObject)PrefabUtility.InstantiatePrefab(Prefab(species,variant));go.transform.SetParent(vegetation,false);
   go.transform.position=new Vector3(p.x,BavellaJourneyBuilder.Height(p.x,p.y)-.065f,p.y);
   go.transform.rotation=Quaternion.Euler(R(-2,2),R(0,360),R(-2,2));go.transform.localScale=new Vector3(scale*R(.88f,1.12f),scale,scale*R(.88f,1.12f));
   foreach(var t in go.GetComponentsInChildren<Transform>())t.gameObject.isStatic=true;
   return go;
  }
  static Vector2 SidePoint(float s,float offset){
   Vector3 f=BavellaJourneyBuilder.Forward(Mathf.Clamp(s,.3f,22.5f));Vector2 dir=new Vector2(f.x,f.z);
   Vector2 p=BavellaJourneyBuilder.Point(Mathf.Clamp(s,0,22.9f));
   if(s<0)p+=dir*s;if(s>22.9f)p+=dir*(s-22.9f);
   return p+new Vector2(f.z,-f.x)*offset;
  }
  static Mesh SaveMesh(Mesh m,string name){
   string p=Root+"/Meshes/"+name+".asset";m.name=name;var old=AssetDatabase.LoadAssetAtPath<Mesh>(p);
   if(old==null){AssetDatabase.CreateAsset(m,p);return m;}EditorUtility.CopySerialized(m,old);EditorUtility.SetDirty(old);UnityEngine.Object.DestroyImmediate(m);return old;
  }
  static void Bordures(){
   for(int section=0;section<6;section++){
    var vs=new List<Vector3>();var ns=new List<Vector3>();var cs=new List<Color>();var ts=new List<int>();
    for(int t=0;t<190;t++){
     float s=Mathf.Min(23.3f,section*4+R(0,4));int side=t%2==0?-1:1;
     float offset=side*R(1.04f,2.8f);Vector2 p=SidePoint(s,offset);
     for(int b=0;b<5;b++){
      float angle=R(0,Mathf.PI*2),height=R(.15f,.46f),width=R(.014f,.032f);
      Vector3 basep=new Vector3(p.x+R(-.12f,.12f),0,p.y+R(-.12f,.12f));basep.y=BavellaJourneyBuilder.Height(basep.x,basep.z)-.01f;
      Vector3 dir=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));Vector3 sidev=new Vector3(dir.z,0,-dir.x)*width;
      Color color=C(t%4==0?"9A9B61":t%4==1?"647E42":t%4==2?"819451":"42673D");
      int k=vs.Count;
      vs.Add(basep-sidev);vs.Add(basep+sidev);vs.Add(basep+Vector3.up*height*.62f+dir*height*.18f+sidev*.65f);vs.Add(basep+Vector3.up*height+dir*height*.45f);vs.Add(basep+Vector3.up*height*.62f+dir*height*.18f-sidev*.65f);
      for(int q=0;q<5;q++){ns.Add((Vector3.up*.65f-dir*.35f).normalized);cs.Add(color*(.72f+q*.07f));}
      ts.AddRange(new[]{k,k+2,k+1,k,k+4,k+2,k+4,k+3,k+2});
     }
    }
    var mesh=new Mesh();mesh.SetVertices(vs);mesh.SetNormals(ns);mesh.SetColors(cs);mesh.SetUVs(0,new List<Vector2>(new Vector2[vs.Count]));mesh.SetTriangles(ts,0);mesh.RecalculateBounds();
    var go=new GameObject("Herbes_Lisiere_"+section);go.transform.SetParent(vegetation,false);go.AddComponent<MeshFilter>().sharedMesh=SaveMesh(mesh,go.name);go.AddComponent<MeshRenderer>().sharedMaterial=grass;go.isStatic=true;
   }
  }
  public static string Build(){
   var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
   if(Application.isPlaying||scene.path!="Assets/Scenes/Prototype_Maquis.unity")throw new Exception("Requires Prototype_Maquis in Edit mode.");
   BavellaJourneyBuilder.Route();rng=new System.Random(238124);prefabs.Clear();
   var env=GameObject.Find("Maquis_Environment").transform;var old=env.Find("Parcours_Corse_Bavella");if(old==null)throw new Exception("Existing route missing.");
   foreach(string name in new[]{"02_Maquis_Botanique_et_Pins","04_Bavella_Plans_de_Profondeur"}){var t=old.Find(name);if(t!=null)t.gameObject.SetActive(false);}
   var originalRocks=old.Find("03_Granit_Proche_et_Passage");
   foreach(Transform child in originalRocks)child.gameObject.SetActive(child.name.StartsWith("Pierres_Enfouies"));
   var existing=env.Find("Sentier_Corse_Dense");if(existing!=null)UnityEngine.Object.DestroyImmediate(existing.gameObject);
   var root=Group("Sentier_Corse_Dense",env);vegetation=Group("Vegetation_en_strates",root);rocks=Group("Granit_Enfoui",root);
   var source=AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype_Maquis/Art/Bavella/Materials/Lentisque_Cire_Olive.mat");
   leaf=Mat("Feuillages_Mats",source);leaf.SetColor("_BaseColor",new Color(1.08f,1.30f,1.18f));leaf.SetColor("_ShadowTint",new Color(.33f,.47f,.46f));leaf.SetFloat("_Roughness",1);
   wood=Mat("Ecorce_Chene_Arbousier",AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype_Maquis/Art/Bavella/Materials/Bois_Rameaux_Peints.mat"));wood.SetColor("_ShadowTint",new Color(.40f,.47f,.43f));
   stone=Mat("Granit_Gris_Chaud",AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype_Maquis/Art/Bavella/Materials/Granit_Rose_Peint.mat"));stone.SetColor("_BaseColor",new Color(.80f,.86f,.85f));stone.SetFloat("_TextureScale",1.1f);
   grass=Mat("Herbes_Ombre_Sauge",AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype_Maquis/Art/Bavella/Materials/Herbes_Sauge_Seches.mat"));grass.SetColor("_ShadowTint",new Color(.34f,.48f,.43f));
   var ground=Mat("Sol_Sous_Bois",AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype_Maquis/Art/Bavella/Materials/Arene_Granitique_Sentier.mat"));ground.shader=Shader.Find("CorseVR/Sentier Painted");ground.SetColor("_ShadowTint",new Color(.38f,.49f,.44f));
   foreach(var rr in old.Find("01_Terrain_et_Enveloppe").GetComponentsInChildren<Renderer>())rr.sharedMaterial=ground;
   int count=0;
   // Tall trunks frame a winding opening; canopy covers both sides of the trail.
   for(int i=0;i<19;i++){
    float s=-2f+i*1.42f,offset=(i%2==0?-1:1)*R(3.0f,4.1f);
    Plant("Chene",i%3,SidePoint(s,offset),R(.86f,1.1f));count++;
   }
   for(int i=0;i<15;i++){
    float s=R(-3,25),offset=(i%2==0?-1:1)*R(5,9);
    Plant("Chene",i%3,SidePoint(s,offset),R(.85f,1.24f));count++;
   }
   for(int i=0;i<17;i++){
    float s=-1+i*1.5f,offset=(i%2==0?-1:1)*R(2.6f,3.4f);
    Plant("Arbousier",i%2,SidePoint(s,offset),R(.72f,1.0f));count++;
   }
   // Interlocking low and medium masses hide empty terrain and root bases.
   for(int i=0;i<66;i++){
    float s=-1.5f+i*.40f;float side=i%2==0?-1:1;
    Plant("Lentisque",i%3,SidePoint(s,side*R(1.85f,2.5f)),R(.70f,1.05f));count++;
   }
   for(int i=0;i<50;i++){
    float s=R(-3,26),offset=(i%2==0?-1:1)*R(3.0f,6.5f);
    Plant("Lentisque",i%3,SidePoint(s,offset),R(.95f,1.45f));count++;
   }
   for(int i=0;i<46;i++){
    float s=-.4f+i*.53f,offset=(i%2==0?-1:1)*R(1.35f,1.85f);
    Plant("Ciste",i%2,SidePoint(s,offset),R(.55f,.85f));count++;
   }
   // Scattered stone shoulders, partially buried and never across the walking strip.
   for(int i=0;i<32;i++){
    float s=R(-2,26);Vector2 p=SidePoint(s,(i%2==0?-1:1)*R(1.85f,4.3f));
    var go=Model("Granit_Anguleux_"+(i%4),rocks);float scale=R(.32f,.85f);
    go.transform.position=new Vector3(p.x,BavellaJourneyBuilder.Height(p.x,p.y)-scale*R(.23f,.45f),p.y);go.transform.rotation=Quaternion.Euler(R(-14,14),R(0,360),R(-10,10));go.transform.localScale=new Vector3(scale*R(.8f,1.3f),scale,scale*R(.7f,1.2f));
    foreach(var mf in go.GetComponentsInChildren<MeshFilter>())mf.gameObject.AddComponent<MeshCollider>().sharedMesh=mf.sharedMesh;
   }
   Bordures();
   var player=GameObject.Find("Joueur_Desktop");var corridor=player.GetComponent<CorseVR.SentierCorridor>();if(corridor==null)corridor=player.AddComponent<CorseVR.SentierCorridor>();
   var line=new List<Vector3>();for(float s=0;s<23.4f;s+=.2f){Vector2 p=BavellaJourneyBuilder.Point(s);line.Add(new Vector3(p.x,0,p.y));}Vector2 end=BavellaJourneyBuilder.Point(100);line.Add(new Vector3(end.x,0,end.y));corridor.Configure(line.ToArray(),.68f);
   var depart=BavellaJourneyBuilder.Point(0);player.transform.position=new Vector3(depart.x,BavellaJourneyBuilder.Height(depart.x,depart.y)+.035f,depart.y);player.transform.rotation=Quaternion.LookRotation(BavellaJourneyBuilder.Forward(0));
   var cam=player.GetComponentInChildren<Camera>();cam.transform.localPosition=new Vector3(0,1.65f,0);cam.transform.localRotation=Quaternion.identity;
   var sun=GameObject.Find("Directional Light").GetComponent<Light>();sun.transform.rotation=Quaternion.Euler(51,-38,0);sun.color=new Color(1,.95f,.85f);sun.intensity=1.18f;sun.shadowStrength=.92f;
   RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.012f;RenderSettings.fogColor=new Color(.55f,.70f,.70f);
   Physics.SyncTransforms();AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
   return count+" new plants; 32 integrated rocks; 6 grass meshes; Bavella formations inactive; path corridor +/-.68m. Scene saved.";
  }
 }
}
