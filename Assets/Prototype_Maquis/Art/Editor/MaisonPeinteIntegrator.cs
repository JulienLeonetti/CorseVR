using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace CorseVR.Editor {
 public static class MaisonPeinteIntegrator {
  const string Root="Assets/Prototype_Maquis/Art/MaisonPeinte";
  const string Original="Assets/Prototype_Maquis/Art/MaisonArtistique";
  static Dictionary<string,Material> mats;
  static Material Make(string key,int mode,string tint,float smooth=.16f,float metal=0){
   var path=Root+"/Materials/"+key+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(m==null){m=new Material(Shader.Find("CorseVR/Maison Peinte"));AssetDatabase.CreateAsset(m,path);}
   m.shader=Shader.Find("CorseVR/Maison Peinte");m.name=key;Color c;ColorUtility.TryParseHtmlString("#"+tint,out c);m.SetColor("_BaseColor",c);m.SetFloat("_Mode",mode);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metal);m.SetFloat("_AmbientScale",.82f);m.SetFloat("_Relief",mode==0?.0006f:mode==1?.0004f:.0002f);m.SetFloat("_TextureScale",mode==1?.72f:.65f);m.SetFloat("_TextureWeight",mode==0?.82f:mode==1?.78f:.1f);
   m.SetFloat("_TextureWeight",key=="MaisonFloor"?.33f:mode==0?.65f:mode==1?.78f:mode==6?.28f:.1f);
   if(key=="MaisonFloor")m.SetFloat("_TextureScale",.4f);
   var image=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/"+(mode==1?"Bois_Chataignier_Peint.png":"Granit_Gris_Peint.png"));m.SetTexture("_BaseMap",image);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;
  }
  static Material Glass(){
   var path=Root+"/Materials/MaisonGlass.mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(m==null){m=new Material(AssetDatabase.LoadAssetAtPath<Material>(Original+"/Materials/MaisonGlass.mat"));AssetDatabase.CreateAsset(m,path);}
   m.SetColor("_BaseColor",new Color(.67f,.80f,.83f,.035f));m.SetShaderPassEnabled("ShadowCaster",false);m.renderQueue=3000;EditorUtility.SetDirty(m);return m;
  }
  static void Textures(){
   foreach(var n in new[]{"Bois_Chataignier_Peint.png","Granit_Gris_Peint.png"}){
    var importer=(TextureImporter)AssetImporter.GetAtPath(Root+"/Textures/"+n);importer.maxTextureSize=1024;importer.sRGBTexture=true;importer.mipmapEnabled=true;importer.wrapMode=TextureWrapMode.Repeat;importer.anisoLevel=4;importer.textureCompression=TextureImporterCompression.Compressed;
    var web=importer.GetPlatformTextureSettings("WebGL");web.overridden=true;web.maxTextureSize=1024;web.format=TextureImporterFormat.ASTC_6x6;importer.SetPlatformTextureSettings(web);importer.SaveAndReimport();
   }
  }
  static GameObject Model(string name,string file,Transform parent){
   var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/"+file+".fbx");if(asset==null)throw new Exception("Missing "+file);
   var go=(GameObject)PrefabUtility.InstantiatePrefab(asset);go.name=name;go.transform.SetParent(parent,false);go.transform.localScale=Vector3.Scale(go.transform.localScale,new Vector3(-1,1,1));
   foreach(var rr in go.GetComponentsInChildren<Renderer>()){
    var mm=rr.sharedMaterials;for(int i=0;i<mm.Length;i++){string key=mats.Keys.FirstOrDefault(k=>mm[i]!=null&&mm[i].name.Contains(k));if(key!=null)mm[i]=mats[key];}
    rr.sharedMaterials=mm;rr.lightProbeUsage=LightProbeUsage.BlendProbes;rr.reflectionProbeUsage=ReflectionProbeUsage.Off;
    bool glass=rr.name.StartsWith("Vitres");rr.shadowCastingMode=glass?ShadowCastingMode.Off:ShadowCastingMode.On;rr.receiveShadows=!glass;
    PrefabUtility.RecordPrefabInstancePropertyModifications(rr);
   }
   foreach(var t in go.GetComponentsInChildren<Transform>())t.gameObject.isStatic=true;
   PrefabUtility.RecordPrefabInstancePropertyModifications(go.transform);return go;
  }
  static Light SetupLight(Transform house,string name,Vector3 local,Vector3 direction,Color color,float intensity,float range,bool shadow,bool spot){
   var t=house.Find(name);if(t==null){t=new GameObject(name).transform;t.SetParent(house,false);}
   t.localPosition=local;t.localRotation=Quaternion.LookRotation(direction);var l=t.GetComponent<Light>();if(!l)l=t.gameObject.AddComponent<Light>();l.type=spot?LightType.Spot:LightType.Point;l.color=color;l.intensity=intensity;l.range=range;l.shadows=shadow?LightShadows.Soft:LightShadows.None;l.shadowBias=.02f;l.shadowNormalBias=.05f;
   if(spot){l.spotAngle=115;l.innerSpotAngle=88;}return l;
  }
  public static string Apply(string stage){
   var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(Application.isPlaying||scene.path!="Assets/Scenes/Prototype_Maquis.unity")throw new Exception("Edit Prototype_Maquis required");
   var house=GameObject.Find("Maison_Accueil_Corse").transform;if(!house.Find("Porte_Accueil_Interactive"))throw new Exception("Existing door required");
   Directory.CreateDirectory(Root+"/Materials");AssetDatabase.Refresh();Textures();
   mats=new Dictionary<string,Material>{
    {"MaisonStone",Make("MaisonStone",0,"FFF9EE",.06f)},{"MaisonMortar",Make("MaisonMortar",6,"E9E5D7",.07f)},{"MaisonWood",Make("MaisonWood",1,"FFF2E6",.08f)},
    {"MaisonFloor",Make("MaisonFloor",0,"E9E5D9",.16f)},{"MaisonTile",Make("MaisonTile",2,"F0E8D9",.27f)},{"MaisonIron",Make("MaisonIron",4,"C4C9C6",.28f,.65f)},
    {"MaisonLinen",Make("MaisonLinen",3,"F6F2E7",.06f)},{"MaisonWicker",Make("MaisonWicker",1,"DDD0B3",.08f)},{"MaisonCopper",Make("MaisonCopper",4,"E3C0A0",.24f,.55f)},
    {"MaisonLeaves",Make("MaisonLeaves",5,"DCE6D8",.08f)},{"MaisonSoot",Make("MaisonSoot",6,"766E63",.02f)},{"MaisonGlass",Glass()}
   };
   var obsolete=new List<GameObject>();
   foreach(Transform t in house){
    if(t.name=="Maison_Artistique_3D"||t.name=="Maison_Corse_Accueil")t.gameObject.SetActive(false);
    if(t.name=="Maison_Finition_Prototype"||t.name=="Maison_Finition_3D"||t.name=="Maison_Peinte_3D")obsolete.Add(t.gameObject);
   }
   foreach(var old in obsolete)UnityEngine.Object.DestroyImmediate(old);
   var model=Model(stage=="prototype"?"Maison_Finition_Prototype":"Maison_Peinte_3D","Maison_Finition_"+stage,house);
   // The corrected outline remains the existing Unity object and mesh.
   var map=house.Find("Carte_Corse_Contour_Fidele");if(map!=null)map.GetComponent<MeshRenderer>().sharedMaterial=mats["MaisonLeaves"];
   if(stage=="final"){
    var hinge=house.Find("Porte_Accueil_Interactive");
    obsolete.Clear();foreach(Transform t in hinge){if(t.name=="Porte_Artistique_3D")t.gameObject.SetActive(false);if(t.name=="Porte_Finition_3D"||t.name=="Porte_Peinte_3D")obsolete.Add(t.gameObject);}
    foreach(var old in obsolete)UnityEngine.Object.DestroyImmediate(old);
    var movingDoor=Model("Porte_Peinte_3D","Porte_Finition_final",hinge);
    foreach(var t in movingDoor.GetComponentsInChildren<Transform>())t.gameObject.isStatic=false;
   }
   SetupLight(house,"Lumiere_Fenetre_Maquis",new Vector3(-3.85f,2.10f,-3.53f),new Vector3(1,-.12f,0),new Color(.80f,.92f,1),8,8,true,true);
   SetupLight(house,"Lumiere_Fenetre_Facade",new Vector3(-2.05f,2.10f,.83f),new Vector3(0,-.22f,-1),new Color(1,.91f,.76f),7,8,true,true);
   SetupLight(house,"Rebond_Interieur",new Vector3(0,2.25f,-3.0f),Vector3.forward,new Color(1,.85f,.70f),1.2f,6,false,false);
   var threshold=house.Find("Lumiere_Seuil_Ouvrable");if(threshold)threshold.GetComponent<Light>().shadows=LightShadows.None;
   Physics.SyncTransforms();AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
   return stage+" house integrated; original model archived inactive, corrected Corsica and all functional objects preserved.";
  }
 }
}
