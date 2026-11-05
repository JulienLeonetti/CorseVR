using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;
namespace CorseVR.Editor {
 public static class MaquisV10Builder {
  const string Art="Assets/Prototype_Maquis/Art/MaquisV10";
  const string Work=@"C:/Users/jeanj/Documents/Codex/2026-10-22/le-serveur-unitymcp-est-d-j/work/maquis_v10";
  [Serializable]public class Point {public float x,z,s,y;}
  [Serializable]public class Plant {public int variante;public float x,z,y,scale,yaw,station;}
  [Serializable]public class Plan {public float longueur_prevue_m,longueur_prototype_m;public Point[] axe_35m;public Plant[] arbousiers;}
  static Plan Load(){return JsonUtility.FromJson<Plan>(System.IO.File.ReadAllText(Work+"/Docs/Implantation_V10.json"));}
  static void Guard(){if(EditorApplication.isPlaying)throw new Exception("Edition requise");}
  static Material Material(string name,Shader shader){
   string path=Art+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(shader);AssetDatabase.CreateAsset(m,path);}m.enableInstancing=true;return m;
  }
  static Texture2D Texture(string name){return AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"/Textures/"+name+".png");}
  static GameObject Model(string name,Transform parent){
   var a=AssetDatabase.LoadAssetAtPath<GameObject>(Art+"/Models/"+name+".fbx");if(!a)throw new Exception("Absent "+name);var g=(GameObject)PrefabUtility.InstantiatePrefab(a,parent);g.name=name;return g;
  }
  static void Save(){var s=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(s);AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(s);}
  public static string Prepare(){
   Guard();foreach(string f in new[]{"Materials","Prefabs"})if(!AssetDatabase.IsValidFolder(Art+"/"+f))AssetDatabase.CreateFolder(Art,f);
   foreach(var guid in AssetDatabase.FindAssets("t:Texture2D",new[]{Art+"/Textures"})){
    var path=AssetDatabase.GUIDToAssetPath(guid);var im=AssetImporter.GetAtPath(path) as TextureImporter;
    im.maxTextureSize=path.Contains("Atlas_Arbousier")?2048:1024;im.mipmapEnabled=true;im.anisoLevel=4;im.alphaIsTransparency=path.Contains("Atlas_Arbousier_V10")||path.Contains("Billboard");im.wrapMode=path.Contains("Atlas_")||path.Contains("Billboard")?TextureWrapMode.Clamp:TextureWrapMode.Repeat;
    bool normal=path.Contains("_N_");im.textureType=normal&&path.Contains("Ecorce_")?TextureImporterType.NormalMap:TextureImporterType.Default;im.sRGBTexture=!normal;im.textureCompression=TextureImporterCompression.CompressedHQ;im.SaveAndReimport();
   }
   var lit=Shader.Find("Universal Render Pipeline/Lit");var painted=Shader.Find("CorseVR/Maquis V10");if(!painted||!painted.isSupported)throw new Exception("Shader indisponible");
   var wood=Material("Ecorce_Arbousier",lit);wood.SetTexture("_BaseMap",Texture("Ecorce_Arbousier_V10"));wood.SetTexture("_BumpMap",Texture("Ecorce_Arbousier_N_V10"));wood.SetColor("_BaseColor",Color.white);wood.SetFloat("_Smoothness",.14f);wood.SetFloat("_Metallic",0);wood.SetFloat("_BumpScale",.30f);wood.EnableKeyword("_NORMALMAP");
   var leaf=Material("Feuillage_Arbousier",painted);leaf.SetTexture("_BaseMap",Texture("Atlas_Arbousier_V10"));leaf.SetTexture("_NormalMap",Texture("Atlas_Arbousier_N_V10"));leaf.SetFloat("_Mode",0);leaf.SetFloat("_Cutoff",.42f);leaf.SetFloat("_NormalStrength",.18f);leaf.SetFloat("_Roughness",.75f);leaf.SetColor("_BaseColor",Color.white);
   var soil=Material("Terre_et_litiere",painted);soil.SetFloat("_Mode",1);soil.SetTexture("_BaseMap",Texture("Terre_Tassee_V10"));soil.SetTexture("_LitterMap",Texture("Litiere_Arbousier_V10"));soil.SetTexture("_NormalMap",Texture("Terre_N_V10"));soil.SetFloat("_NormalStrength",.08f);soil.SetColor("_BaseColor",new Color(.78f,.84f,.87f));
   var stone=Material("Granit",lit);stone.SetTexture("_BaseMap",Texture("Granit_Peint_V10"));stone.SetColor("_BaseColor",Color.white);stone.SetFloat("_Smoothness",.09f);stone.SetFloat("_Metallic",0);
   var litter=Material("Litiere_geometrique",painted);litter.SetFloat("_Mode",2);litter.SetColor("_BaseColor",Color.white);litter.SetFloat("_Roughness",.97f);
   for(int v=1;v<=4;v++){
    var root=new GameObject("Arbousier_V10_"+v);try{
     var levels=new LOD[4];for(int level=0;level<4;level++){
      var model=Model("Arbousier_V10_"+v+(level==3?"_Billboard":"_LOD"+level),root.transform);model.name="LOD"+level;var rs=model.GetComponentsInChildren<Renderer>();
      foreach(var r in rs){
       if(level==3){var billboard=Material("Billboard_Arbousier_"+v,painted);billboard.SetFloat("_Mode",3);billboard.SetTexture("_BaseMap",Texture("Arbousier_Billboard_"+v+"_V10"));billboard.SetFloat("_Cutoff",.42f);r.sharedMaterial=billboard;}
       else{var before=r.sharedMaterials;r.sharedMaterials=before.Select(m=>m&&m.name.Contains("Feuillage")?leaf:wood).ToArray();}
       r.shadowCastingMode=level<2?ShadowCastingMode.TwoSided:ShadowCastingMode.Off;
      }
      levels[level]=new LOD(new[]{.36f,.14f,.055f,.006f}[level],rs);
     }
     var group=root.AddComponent<LODGroup>();group.SetLODs(levels);group.RecalculateBounds();var bounds=levels[0].renderers[0].bounds;foreach(var renderer in levels[0].renderers)bounds.Encapsulate(renderer.bounds);group.localReferencePoint=root.transform.InverseTransformPoint(bounds.center);group.size=Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z));
     PrefabUtility.SaveAsPrefabAsset(root,Art+"/Prefabs/Arbousier_V10_"+v+".prefab");
    }finally{Object.DestroyImmediate(root);}
   }
   Save();return "Textures, 4 prefabs et 4 niveaux de detail prepares.";
  }
  public static string Ground(){
   Guard();if(GameObject.Find("Maquis_Prototype_V10"))throw new Exception("Prototype deja present");
   var h=GameObject.Find("Maison_Accueil_Corse").transform;var root=new GameObject("Maquis_Prototype_V10");root.transform.SetPositionAndRotation(h.position,h.rotation);
   var terrain=Model("Sol_5m_V10",root.transform);terrain.GetComponentInChildren<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Art+"/Materials/Terre_et_litiere.mat");var f=terrain.GetComponentInChildren<MeshFilter>();f.gameObject.AddComponent<MeshCollider>().sharedMesh=f.sharedMesh;
   var litter=Model("Litiere_5m_V10",root.transform);litter.GetComponentInChildren<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Art+"/Materials/Litiere_geometrique.mat");litter.GetComponentInChildren<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
   var plan=Load();var rocks=new[]{new Vector3(-1.45f,0,2.10f),new Vector3(2.15f,0,3.75f),new Vector3(-1.7f,0,5.75f),new Vector3(2.6f,0,6.3f)};
   Physics.SyncTransforms();for(int i=0;i<rocks.Length;i++){var world=h.TransformPoint(rocks[i]);RaycastHit hit;if(!f.GetComponent<MeshCollider>().Raycast(new Ray(world+Vector3.up*20,Vector3.down),out hit,40))continue;var g=Model("Granit_V10_"+(i%2+1),root.transform);g.transform.position=hit.point-Vector3.up*.20f;g.transform.rotation=h.rotation*Quaternion.Euler(0,i*83+16,0);g.GetComponentInChildren<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Art+"/Materials/Granit.mat");var mesh=g.GetComponentInChildren<MeshFilter>();mesh.gameObject.AddComponent<MeshCollider>().sharedMesh=mesh.sharedMesh;}
   var points=new List<Vector3>{h.TransformPoint(new Vector3(0,.08f,-.66f)),h.TransformPoint(new Vector3(0,.08f,.54f))};points.AddRange(plan.axe_35m.Where(p=>p.s<=5.001f).Select(p=>h.TransformPoint(new Vector3(p.x,p.y,p.z))));var corridor=GameObject.Find("Joueur_Desktop").GetComponent<SentierCorridor>();corridor.Configure(points.ToArray(),.64f);corridor.ConfigureStartRoom(h,new Vector2(-1.85f,-2.5f),new Vector2(2.67f,2.17f));corridor.ConfigureWidths(points.Select((p,i)=>i<5?.44f:.64f).ToArray());EditorUtility.SetDirty(corridor);Physics.SyncTransforms();Save();return "Sol Blender et raccord au palier installes sur le prototype de 5 m.";
  }
  public static string Trees(){
   Guard();var h=GameObject.Find("Maison_Accueil_Corse").transform;var root=GameObject.Find("Maquis_Prototype_V10").transform;if(root.Find("Arbousiers"))throw new Exception("Plantation deja presente");var folder=new GameObject("Arbousiers").transform;folder.SetParent(root,false);
   var plan=Load();foreach(var p in plan.arbousiers){var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Art+"/Prefabs/Arbousier_V10_"+p.variante+".prefab");var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab,folder);g.name="Arbousier_"+p.variante+"_S"+p.station.ToString("F2",System.Globalization.CultureInfo.InvariantCulture);g.transform.SetPositionAndRotation(h.TransformPoint(new Vector3(p.x,p.y,p.z)),h.rotation*Quaternion.Euler(0,p.yaw,0));g.transform.localScale=Vector3.one*p.scale;var collider=g.AddComponent<CapsuleCollider>();collider.radius=p.variante==3?.23f:.15f;collider.height=1.1f;collider.center=new Vector3(0,.48f,0);}
   Physics.SyncTransforms();Save();return plan.arbousiers.Length+" arbousiers installes ; aucune autre espece.";
  }
  public static string Capture(string label){
   var h=GameObject.Find("Maison_Accueil_Corse").transform;var plan=Load();var point=plan.axe_35m[12];var look=plan.axe_35m[47];var cam=Camera.main;var op=cam.transform.position;var oq=cam.transform.rotation;var ot=cam.targetTexture;var oa=RenderTexture.active;var aspect=cam.aspect;var fov=cam.fieldOfView;var rt=new RenderTexture(1920,1080,24){antiAliasing=4};rt.Create();var tex=new Texture2D(1920,1080,TextureFormat.RGB24,false);
   try{foreach(float eye in new[]{.3f,1.7f}){cam.transform.position=h.TransformPoint(new Vector3(point.x,point.y+eye,point.z));cam.transform.LookAt(h.TransformPoint(new Vector3(look.x,look.y+(eye<1?.55f:1.5f),look.z)));cam.fieldOfView=65;cam.aspect=16f/9;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1920,1080),0,0);tex.Apply();System.IO.File.WriteAllBytes(Work+"/Captures/"+label+"_"+eye.ToString("F1",System.Globalization.CultureInfo.InvariantCulture)+"m.png",tex.EncodeToPNG());}return label;}
   finally{cam.transform.SetPositionAndRotation(op,oq);cam.targetTexture=ot;cam.aspect=aspect;cam.fieldOfView=fov;RenderTexture.active=oa;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);}
  }
 }
}

