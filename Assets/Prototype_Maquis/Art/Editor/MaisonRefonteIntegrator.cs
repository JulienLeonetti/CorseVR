using System;using System.IO;using System.Linq;using System.Collections.Generic;
using UnityEngine;using UnityEngine.Rendering;using UnityEditor;using UnityEditor.SceneManagement;
namespace CorseVR.Editor {
 public static class MaisonRefonteIntegrator {
  public const string Root="Assets/Prototype_Maquis/Art/MaisonRefonte";
  static Dictionary<string,Material> Materials(){
   Directory.CreateDirectory(Root+"/Materials");AssetDatabase.Refresh();var map=new Dictionary<string,Material>();
   foreach(string key in new[]{"MaisonStone","MaisonWood","MaisonFloor","MaisonMortar","MaisonTile","MaisonIron","MaisonCopper","MaisonLinen","MaisonWicker","MaisonSoot"}){
    string path=Root+"/Materials/"+key+"_Refonte.mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
    if(!m){m=new Material(Shader.Find("CorseVR/Maison Refonte"));AssetDatabase.CreateAsset(m,path);}
    int mode=key=="MaisonWood"||key=="MaisonWicker"?1:key=="MaisonTile"?2:key=="MaisonLinen"?3:key=="MaisonIron"||key=="MaisonCopper"?4:key=="MaisonMortar"||key=="MaisonSoot"?6:0;
    m.SetFloat("_Mode",mode);m.SetFloat("_Metallic",mode==4?.6f:0);m.SetFloat("_Smoothness",mode==4?.40f:mode==2?.30f:mode==1?.15f:.06f);
    m.SetFloat("_AmbientScale",.78f);m.SetFloat("_Relief",mode==0?.0007f:mode==1?.00035f:0);m.SetFloat("_TextureScale",mode==1?1f:1.1f);
    m.SetFloat("_TextureWeight",mode==1?.62f:mode==0?.65f:mode==6?.18f:0);
    m.SetColor("_BaseColor",mode==1?new Color(.86f,.88f,.92f):key=="MaisonSoot"?new Color(.40f,.38f,.33f):Color.white);
    if(mode==1||mode==0||mode==6)m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/"+(mode==1?"Chataignier_Pictural":"Granit_Pictural")+".png"));
    if(key=="MaisonSoot")m.SetFloat("_TextureWeight",0);
    if(key=="MaisonFloor"){m.SetFloat("_TextureWeight",.28f);m.SetFloat("_Relief",.0002f);}
    if(key=="MaisonStone")m.SetColor("_BaseColor",new Color(.95f,1f,1.06f));
    m.enableInstancing=true;EditorUtility.SetDirty(m);map[key]=m;
   }
   string glassPath=Root+"/Materials/MaisonGlass_Refonte.mat";var glass=AssetDatabase.LoadAssetAtPath<Material>(glassPath);
   if(!glass){glass=new Material(Shader.Find("CorseVR/Verre Pictural"));AssetDatabase.CreateAsset(glass,glassPath);}
   glass.SetTexture("_Reflection",AssetDatabase.LoadAssetAtPath<Cubemap>(Root+"/Reflections/Maison_Refonte_Interieur.exr"));glass.SetVector("_ReflectionHDR",new Vector4(1,1,0,0));EditorUtility.SetDirty(glass);map["MaisonGlass"]=glass;return map;
  }
  static GameObject Visual(string file,string name,Dictionary<string,Material> mats){
   var outer=new GameObject(name);var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/"+file+".fbx");if(!asset)throw new Exception(file);
   var go=(GameObject)PrefabUtility.InstantiatePrefab(asset);go.transform.SetParent(outer.transform,false);go.transform.localScale=Vector3.Scale(go.transform.localScale,new Vector3(-1,1,1));PrefabUtility.RecordPrefabInstancePropertyModifications(go.transform);
   foreach(var r in go.GetComponentsInChildren<Renderer>()){
    r.sharedMaterials=r.sharedMaterials.Select(m=>{string k=mats.Keys.FirstOrDefault(x=>m.name.StartsWith(x+"_"));return k==null?m:mats[k];}).ToArray();r.shadowCastingMode=r.name.StartsWith("Vitres")?ShadowCastingMode.Off:ShadowCastingMode.On;r.receiveShadows=true;r.reflectionProbeUsage=ReflectionProbeUsage.BlendProbes;PrefabUtility.RecordPrefabInstancePropertyModifications(r);
   }
   foreach(var t in outer.GetComponentsInChildren<Transform>())t.gameObject.isStatic=file!="Porte_Refonte";
   string path=Root+"/Prefabs/"+name+".prefab";var prefab=PrefabUtility.SaveAsPrefabAsset(outer,path);UnityEngine.Object.DestroyImmediate(outer);return prefab;
  }
  static void Place(GameObject prefab,Transform parent,string name){
   var old=parent.Find(name);if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);instance.name=name;instance.transform.localPosition=Vector3.zero;instance.transform.localRotation=Quaternion.identity;PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
  }
  static void SetLight(Transform h,string name,Vector3 p,Vector3 direction,float intensity,Color color){
   var t=h.Find(name);var l=t.GetComponent<Light>();t.localPosition=p;t.localRotation=Quaternion.LookRotation(direction);l.intensity=intensity;l.color=color;l.range=8;l.spotAngle=105;l.innerSpotAngle=65;l.shadows=LightShadows.Soft;l.shadowBias=.025f;l.shadowNormalBias=.12f;l.shadowNearPlane=.05f;
  }
  public static string Install(){
   if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="Prototype_Maquis")throw new Exception("Edit Prototype_Maquis required");
   Directory.CreateDirectory(Root+"/Prefabs");var h=GameObject.Find("Maison_Accueil_Corse").transform;var mats=Materials();
   var body=Visual("Maison_Refonte","Maison_Refonte_Visuels",mats);var door=Visual("Porte_Refonte","Porte_Refonte_Visuels",mats);
   foreach(string path in new[]{"Maison_Peinte_3D","Maison_Artistique_3D","Porte_Accueil_Interactive/Porte_Peinte_3D","Porte_Accueil_Interactive/Porte_Artistique_3D"}){var t=h.Find(path);if(t&&t.GetComponentsInChildren<Collider>(true).Length==0&&t.GetComponentsInChildren<MonoBehaviour>(true).Length==0)UnityEngine.Object.DestroyImmediate(t.gameObject);}
   Place(body,h,"Maison_Refonte_Visuels");
   var hinge=h.Find("Porte_Accueil_Interactive");Place(door,hinge,"Porte_Refonte_Visuels");
   var deco=h.Find("Decoration_Corse");foreach(var r in deco.GetComponentsInChildren<Renderer>()){
    var mm=r.sharedMaterials;for(int i=0;i<mm.Length;i++)if(mm[i].name.StartsWith("MaisonWood"))mm[i]=mats["MaisonWood"];r.sharedMaterials=mm;PrefabUtility.RecordPrefabInstancePropertyModifications(r);
   }
   foreach(string name in new[]{"Couteau_Corse_Presentation","Ratelier_Chasse"}){
    string path="Assets/Prototype_Maquis/Art/DecorationCorse/Prefabs/"+name+".prefab";var contents=PrefabUtility.LoadPrefabContents(path);
    foreach(var r in contents.GetComponentsInChildren<Renderer>()){var mm=r.sharedMaterials;for(int i=0;i<mm.Length;i++)if(mm[i].name.StartsWith("MaisonWood"))mm[i]=mats["MaisonWood"];r.sharedMaterials=mm;}
    PrefabUtility.SaveAsPrefabAsset(contents,path);PrefabUtility.UnloadPrefabContents(contents);
   }
   SetLight(h,"Lumiere_Fenetre_Maquis",new Vector3(-3.10f,2.32f,-3.53f),new Vector3(1,-.31f,.18f),8,new Color(1f,.93f,.80f));
   SetLight(h,"Lumiere_Fenetre_Facade",new Vector3(-2.05f,2.32f,.08f),new Vector3(.22f,-.30f,-1),6,new Color(1f,.91f,.76f));
   var fill=h.Find("Rebond_Interieur");fill.localPosition=new Vector3(-.25f,2.80f,-3.80f);fill.GetComponent<Light>().intensity=.38f;fill.GetComponent<Light>().color=new Color(1,.85f,.70f);
   RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.34f,.39f,.44f);RenderSettings.ambientEquatorColor=new Color(.23f,.22f,.19f);RenderSettings.ambientGroundColor=new Color(.12f,.105f,.085f);RenderSettings.ambientIntensity=1;
   var sun=GameObject.Find("Directional Light").GetComponent<Light>();sun.transform.rotation=Quaternion.Euler(40,140,0);sun.intensity=.95f;sun.color=new Color(1,.94f,.84f);DynamicGI.UpdateEnvironment();
   AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(h.gameObject.scene);EditorSceneManager.SaveScene(h.gameObject.scene);return "Maison et porte intégrées; collisions et scripts conservés";
  }
 }
}
