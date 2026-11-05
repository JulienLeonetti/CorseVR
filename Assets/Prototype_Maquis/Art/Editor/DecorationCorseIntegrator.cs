using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace CorseVR.Editor {
 public static class DecorationCorseIntegrator {
  const string Root="Assets/Prototype_Maquis/Art/DecorationCorse";
  static Transform Parent(){
   if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!="Assets/Scenes/Prototype_Maquis.unity")throw new Exception("Edit Prototype_Maquis required");
   var house=GameObject.Find("Maison_Accueil_Corse").transform;var t=house.Find("Decoration_Corse");
   if(!t){t=new GameObject("Decoration_Corse").transform;t.SetParent(house,false);}return t;
  }
  static Material Mat(string name,int kind,float metallic,float smooth){
   string path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(!m){m=new Material(Shader.Find("CorseVR/Decoration Surface"));AssetDatabase.CreateAsset(m,path);}
   m.SetColor("_BaseColor",Color.white);m.SetFloat("_Kind",kind);m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",smooth);m.enableInstancing=true;
   if(kind==0)m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/Corne_Ambree_Peinte.png"));EditorUtility.SetDirty(m);return m;
  }
  static Dictionary<string,Material> Materials(){
   Directory.CreateDirectory(Root+"/Materials");AssetDatabase.Refresh();
   var im=(TextureImporter)AssetImporter.GetAtPath(Root+"/Textures/Corne_Ambree_Peinte.png");
   if(im!=null){im.maxTextureSize=1024;im.mipmapEnabled=true;im.anisoLevel=4;im.wrapMode=TextureWrapMode.Repeat;var web=im.GetPlatformTextureSettings("WebGL");web.overridden=true;web.maxTextureSize=1024;web.format=TextureImporterFormat.ASTC_6x6;im.SetPlatformTextureSettings(web);im.SaveAndReimport();}
   return new Dictionary<string,Material>{
    {"DecoHorn",Mat("Corne_Ambree",0,0,.40f)},{"DecoBlade",Mat("Acier_Poli",1,.68f,.64f)},
    {"DecoForge",Mat("Acier_Forge",2,.46f,.30f)},{"DecoRivet",Mat("Rivets_Laiton",3,.68f,.48f)},
    {"DecoGunMetal",Mat("Acier_Bleui",4,.55f,.42f)},
    {"DecoWood",AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype_Maquis/Art/MaisonPeinte/Materials/MaisonWood.mat")}
   };
  }
  static GameObject Model(GameObject outer,string file,Dictionary<string,Material> mats){
   var src=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/"+file+".fbx");if(!src)throw new Exception("Missing "+file);
   var go=(GameObject)PrefabUtility.InstantiatePrefab(src);go.transform.SetParent(outer.transform,false);go.transform.localScale=Vector3.Scale(go.transform.localScale,new Vector3(-1,1,1));PrefabUtility.RecordPrefabInstancePropertyModifications(go.transform);
   foreach(var r in go.GetComponentsInChildren<Renderer>()){
    var mm=r.sharedMaterials;for(int i=0;i<mm.Length;i++){var key=mats.Keys.FirstOrDefault(k=>mm[i]!=null&&mm[i].name.Contains(k));if(key!=null)mm[i]=mats[key];}r.sharedMaterials=mm;
    r.shadowCastingMode=ShadowCastingMode.On;r.receiveShadows=true;r.reflectionProbeUsage=ReflectionProbeUsage.BlendProbes;PrefabUtility.RecordPrefabInstancePropertyModifications(r);
   }return go;
  }
  static string Finish(GameObject outer,string name,Vector3 pos,Quaternion rot,Transform parent){
   Directory.CreateDirectory(Root+"/Prefabs");foreach(var t in outer.GetComponentsInChildren<Transform>())t.gameObject.isStatic=true;
   var prefab=PrefabUtility.SaveAsPrefabAsset(outer,Root+"/Prefabs/"+name+".prefab");UnityEngine.Object.DestroyImmediate(outer);
   var old=parent.Find(name);if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
   var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);instance.transform.localPosition=pos;instance.transform.localRotation=rot;PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
   AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(parent.gameObject.scene);EditorSceneManager.SaveScene(parent.gameObject.scene);
   return Root+"/Prefabs/"+name+".prefab";
  }
  public static string InstallKnife(){
   var parent=Parent();var mats=Materials();var outer=new GameObject("Couteau_Corse_Presentation");Model(outer,"Couteau_Corse",mats);
   return Finish(outer,"Couteau_Corse_Presentation",new Vector3(-2.25f,.8771f,-3.34f),Quaternion.Euler(0,22,0),parent);
  }
  public static string InstallRack(){
   var parent=Parent();var mats=Materials();var outer=new GameObject("Ratelier_Chasse");Model(outer,"Ratelier_Chasse",mats);
   var c=outer.AddComponent<BoxCollider>();c.center=new Vector3(0,1.96f,-.11f);c.size=new Vector3(1.42f,1.06f,.23f);
   return Finish(outer,"Ratelier_Chasse",new Vector3(-2.77f,0,-1.65f),Quaternion.Euler(0,-90,0),parent);
  }
 }
}
