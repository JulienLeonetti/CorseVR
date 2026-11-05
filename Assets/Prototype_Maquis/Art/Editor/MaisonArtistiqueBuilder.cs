using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using CorseVR;
namespace CorseVR.Editor {
 public static class MaisonArtistiqueBuilder {
  const string Root="Assets/Prototype_Maquis/Art/MaisonArtistique";
  static Dictionary<string,Material> mats;
  static Material Mat(string key,int mode,string tint,float smooth=.12f,float metal=0){
   string path=Root+"/Materials/"+key+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find("CorseVR/Maison Painterly Lit"));AssetDatabase.CreateAsset(m,path);}m.shader=Shader.Find("CorseVR/Maison Painterly Lit");m.SetFloat("_Mode",mode);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metal);m.SetFloat("_AmbientScale",.42f);m.SetFloat("_Relief",mode==0?.008f:mode==1?.004f:.0015f);Color c;ColorUtility.TryParseHtmlString("#"+tint,out c);m.SetColor("_BaseColor",c);m.SetFloat("_TextureScale",1.6f);m.SetFloat("_TextureWeight",.22f);
   var old=AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype_Maquis/Art/MaisonAccueil/Materials/Pierre_Granit.mat");m.SetTexture("_BaseMap",old.GetTexture("_BaseMap"));m.enableInstancing=true;EditorUtility.SetDirty(m);return m;
  }
  static GameObject Model(string name,string path,Transform parent){
   var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);var go=(GameObject)PrefabUtility.InstantiatePrefab(source);go.name=name;go.transform.SetParent(parent,false);go.transform.localScale=Vector3.Scale(go.transform.localScale,new Vector3(-1,1,1));
   foreach(var rr in go.GetComponentsInChildren<Renderer>()){var materials=rr.sharedMaterials;for(int i=0;i<materials.Length;i++){string key=mats.Keys.FirstOrDefault(k=>materials[i].name.Contains(k));if(key!=null)materials[i]=mats[key];}rr.sharedMaterials=materials;rr.shadowCastingMode=rr.name.Contains("Vitres_Fenetres")?UnityEngine.Rendering.ShadowCastingMode.Off:UnityEngine.Rendering.ShadowCastingMode.TwoSided;if(rr.name.Contains("Vitres_Fenetres"))rr.receiveShadows=false;PrefabUtility.RecordPrefabInstancePropertyModifications(rr);}PrefabUtility.RecordPrefabInstancePropertyModifications(go.transform);return go;
  }
  static Material Glass(){
   string path=Root+"/Materials/MaisonGlass.mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find("CorseVR/Window Clear Glass"));AssetDatabase.CreateAsset(m,path);}m.shader=Shader.Find("CorseVR/Window Clear Glass");m.shaderKeywords=new string[0];m.SetColor("_BaseColor",new Color(.70f,.83f,.86f,.065f));m.SetShaderPassEnabled("ShadowCaster",false);m.renderQueue=3000;EditorUtility.SetDirty(m);return m;
  }
  static Light LightAt(Transform house,string name,Vector3 position,Color color,float intensity,float range,bool shadows){var go=new GameObject(name);go.transform.SetParent(house,false);go.transform.localPosition=position;var l=go.AddComponent<Light>();l.type=LightType.Point;l.color=color;l.intensity=intensity;l.range=range;l.shadows=shadows?LightShadows.Soft:LightShadows.None;l.shadowBias=.02f;l.shadowNormalBias=.08f;return l;}
  public static string Build(){
   if(Application.isPlaying)throw new Exception("Edit mode required");MaisonAccueilBuilder.Build();var house=GameObject.Find("Maison_Accueil_Corse").transform;UnityEngine.Object.DestroyImmediate(house.Find("Maison_Corse_Accueil").gameObject);var door=house.Find("Porte_Accueil_Interactive");UnityEngine.Object.DestroyImmediate(door.Find("Porte_Accueil").gameObject);
   mats=new Dictionary<string,Material>{
    {"MaisonStone",Mat("MaisonStone",0,"E7E8D7")},{"MaisonMortar",Mat("MaisonMortar",6,"D1D0BB")},{"MaisonWood",Mat("MaisonWood",1,"E3D7BB")},{"MaisonFloor",Mat("MaisonFloor",0,"DDD7C0")},{"MaisonTile",Mat("MaisonTile",2,"EAD5B9",.21f)},
    {"MaisonIron",Mat("MaisonIron",4,"B5B5A5",.25f,.65f)},{"MaisonLinen",Mat("MaisonLinen",3,"F1ECD9")},{"MaisonWicker",Mat("MaisonWicker",1,"EAD4AA")},{"MaisonCopper",Mat("MaisonCopper",4,"E2BD93",.15f,.10f)},
    {"MaisonLeaves",Mat("MaisonLeaves",5,"D9E7CE")},{"MaisonSoot",Mat("MaisonSoot",6,"E8D8C1",.07f,0)},{"MaisonGlass",Glass()}
   };
   Model("Maison_Artistique_3D",Root+"/Models/Maison_Corse_Accueil.fbx",house);Model("Porte_Artistique_3D",Root+"/Models/Porte_Accueil.fbx",door);
   var oldFill=house.Find("Lumiere_Interieure_Douce");if(oldFill!=null)UnityEngine.Object.DestroyImmediate(oldFill.gameObject);
   LightAt(house,"Lumiere_Fenetre_Maquis",new Vector3(-4.30f,2.10f,-3.50f),new Color(.86f,.94f,1),3.8f,8,true);
   LightAt(house,"Lumiere_Fenetre_Facade",new Vector3(-2.08f,2.10f,1.25f),new Color(1,.89f,.68f),2.7f,7,true);
   LightAt(house,"Rebond_Interieur",new Vector3(.4f,2.1f,-3.8f),new Color(1,.88f,.70f),1.1f,5,false);
   var threshold=LightAt(house,"Lumiere_Seuil_Ouvrable",new Vector3(.15f,2.1f,.85f),new Color(1,.91f,.74f),.15f,8,true);house.gameObject.AddComponent<AccueilArtLighting>().Configure(door,threshold);
   // The new chest remains against the back wall, outside the clear central route.
   var cc=new GameObject("Collision_Coffre");cc.transform.SetParent(house,false);cc.transform.localPosition=new Vector3(1.95f,.40f,-5.29f);cc.AddComponent<BoxCollider>().size=new Vector3(1.42f,.80f,.75f);
   CadreCorseBuilder.Apply(false);Physics.SyncTransforms();AssetDatabase.SaveAssets();var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);return "Maison artistique saved: 11 material families, baked geometric contact AO, new decoration, window and doorway lighting. Door and paths retained.";
  }
  public static string Capture(string dir){
   Directory.CreateDirectory(dir);var cam=Camera.main;var originalP=cam.transform.position;var originalQ=cam.transform.rotation;float aspect=cam.aspect,fov=cam.fieldOfView;var target=cam.targetTexture;var active=RenderTexture.active;
   var house=GameObject.Find("Maison_Accueil_Corse").transform;var door=house.Find("Porte_Accueil_Interactive");var saved=door.localRotation;var light=house.GetComponent<AccueilArtLighting>();
   var positions=new[]{new Vector3(.18f,1.73f,-4.65f),new Vector3(.18f,1.73f,-4.65f),new Vector3(.6f,1.55f,-4.6f),new Vector3(-.25f,1.6f,-3.9f),new Vector3(.25f,1.7f,-2.6f)};
   var look=new[]{new Vector3(0,1.7f,0),new Vector3(0,1.7f,0),new Vector3(-2.2f,1.05f,-3.6f),new Vector3(2.6f,1.2f,-1.65f),new Vector3(0,1.6f,-5.9f)};
   string[] names={"01_Accueil_Porte_Fermee","02_Accueil_Porte_Ouverte","03_Table_Vannerie_Poteries","04_Cheminee_Cuivres","05_Coffre_Carte"};
   try{for(int i=0;i<positions.Length;i++){door.localRotation=Quaternion.Euler(0,i==0?0:-105,0);light.UpdateLight();cam.transform.position=house.TransformPoint(positions[i]);cam.transform.rotation=Quaternion.LookRotation(house.TransformPoint(look[i])-cam.transform.position);cam.fieldOfView=i<2?76:68;cam.aspect=1920f/1080;Physics.SyncTransforms();var rt=RenderTexture.GetTemporary(1920,1080,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB,4);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1920,1080,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1920,1080),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(dir,names[i]+".png"),tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);cam.targetTexture=null;RenderTexture.active=null;RenderTexture.ReleaseTemporary(rt);}}
   finally{cam.transform.position=originalP;cam.transform.rotation=originalQ;cam.aspect=aspect;cam.fieldOfView=fov;cam.targetTexture=target;RenderTexture.active=active;door.localRotation=saved;light.UpdateLight();Physics.SyncTransforms();}return "5 native Unity captures";
  }
 }
}
