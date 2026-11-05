using System;using System.Linq;using System.IO;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEngine.Rendering;using UnityEditor.SceneManagement;
namespace CorseVR.Editor {
 public static class TourV3Installer {
 const string Root="Assets/Prototype_Maquis/Art/TourGenoiseV3",Old="Assets/Prototype_Maquis/Art/TourLittoral";
 static Material stone,inside,joint,wood,iron,floor,clay,rope,wick;
 static Material Lit(string name,Color tint,Texture texture=null,float metallic=0) {
  string path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
  m.SetColor("_BaseColor",tint);m.SetTexture("_BaseMap",texture);m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",metallic>.1f?.28f:.14f);m.SetFloat("_Cull",2);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;
 }
 static Material Clone(string name,Material source) {
  string path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(!m){m=new Material(source);AssetDatabase.CreateAsset(m,path);}return m;
 }
 static GameObject Model(string file,Transform parent) {
  var a=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/"+file+".fbx");if(!a)throw new Exception("Missing "+file);
  var o=(GameObject)PrefabUtility.InstantiatePrefab(a,parent);o.transform.localScale=Vector3.Scale(o.transform.localScale,new Vector3(-1,1,1));PrefabUtility.RecordPrefabInstancePropertyModifications(o.transform);return o;
 }
 static void Record(GameObject o) {foreach(var c in o.GetComponentsInChildren<Component>(true))if(c)PrefabUtility.RecordPrefabInstancePropertyModifications(c);}
 static void BodyMaterials(GameObject o) {
  foreach(var r in o.GetComponentsInChildren<Renderer>()){
   string n=r.name;Material m=stone;
   if(n.Contains("Chaux")||n.Contains("Joints"))m=joint;
   else if(n.Contains("Dallage")||n.Contains("Dalles"))m=floor;
   else if(n.Contains("Parement")||n.Contains("Retours")||n.Contains("Voute"))m=inside;
   r.sharedMaterial=m;r.shadowCastingMode=ShadowCastingMode.On;
  }Record(o);
 }
 public static string PrepareAssets() {
  foreach(var n in new[]{"Materials","Prefabs"})Directory.CreateDirectory(Root+"/"+n);AssetDatabase.Refresh();
  foreach(string file in Directory.GetFiles(Root+"/Models","*.fbx")){var imp=AssetImporter.GetAtPath(file.Replace('\\','/')) as ModelImporter;if(imp){imp.isReadable=true;imp.importCameras=false;imp.importLights=false;imp.SaveAndReimport();}}
  stone=AssetDatabase.LoadAssetAtPath<Material>(Old+"/Materials/Pierre_Tour_Peinte.mat");
  inside=Clone("Pierre_Interieure_Abri",stone);inside.SetColor("_BaseColor",new Color(.87f,.84f,.78f));inside.SetColor("_ShadowTint",new Color(.40f,.38f,.32f));EditorUtility.SetDirty(inside);
  var grain=stone.GetTexture("_BaseMap");var timber=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Prototype_Maquis/Art/MaisonRefonte/Textures/Bois_Miel_Peint.png");
  joint=Lit("Chaux_Granulee",new Color(.56f,.52f,.44f),grain);joint.SetTextureScale("_BaseMap",Vector2.one*.8f);
  floor=Lit("Dallage_Use",new Color(.64f,.59f,.49f),grain);
  wood=Lit("Chataignier_Peint",new Color(1.03f,.94f,.78f),timber);
  iron=Lit("Fer_Forge_Satine",new Color(.29f,.31f,.29f),null,.65f);
  clay=Lit("Terre_Cuite_Mate",new Color(.95f,.53f,.27f),grain);
  rope=Lit("Chanvre",new Color(.78f,.65f,.44f),grain);
  wick=Lit("Meche_Huile",new Color(.30f,.18f,.06f));wick.EnableKeyword("_EMISSION");wick.SetColor("_EmissionColor",new Color(.44f,.20f,.035f));
  var body=new GameObject("Tour_Genoise_V3");var lods=new List<LOD>();
  for(int i=0;i<3;i++){var ob=Model("Tour_V3_LOD"+i,body.transform);BodyMaterials(ob);lods.Add(new LOD(i==0?.30f:i==1?.12f:.006f,ob.GetComponentsInChildren<Renderer>()));}
  var lg=body.AddComponent<LODGroup>();lg.SetLODs(lods.ToArray());lg.RecalculateBounds();
  var collision=Model("Collision_V3",body.transform);foreach(var f in collision.GetComponentsInChildren<MeshFilter>()){var c=f.gameObject.AddComponent<MeshCollider>();c.sharedMesh=f.sharedMesh;}foreach(var r in collision.GetComponentsInChildren<Renderer>()){r.sharedMaterial=joint;r.enabled=false;}Record(collision);
  var fc=body.AddComponent<BoxCollider>();fc.center=new Vector3(0,.05f,0);fc.size=new Vector3(6.2f,.10f,6.2f);
  var strip=body.AddComponent<BoxCollider>();strip.center=new Vector3(0,.05f,0);strip.size=new Vector3(1.96f,.10f,8.8f);
  foreach(int sign in new[]{-1,1}) {
   var sill=body.AddComponent<BoxCollider>();sill.center=new Vector3(0,.06f,sign*3.5f);sill.size=new Vector3(2.4f,.12f,2.00f);
   var turn=body.AddComponent<BoxCollider>();turn.center=new Vector3(sign*1.225f,1.85f,sign*.90f);turn.size=new Vector3(3.65f,3.5f,.36f);
  }
  PrefabUtility.SaveAsPrefabAsset(body,Root+"/Prefabs/Tour_Genoise_V3.prefab");UnityEngine.Object.DestroyImmediate(body);
  var leaf=new GameObject("Vantail_Tour_V3");var leafModel=Model("Porte_V3",leaf.transform);
  foreach(var r in leafModel.GetComponentsInChildren<Renderer>())r.sharedMaterial=r.name.Contains("Ferrures")?iron:wood;
  Record(leafModel);var box=leaf.AddComponent<BoxCollider>();box.center=new Vector3(.4875f,1.22f,.035f);box.size=new Vector3(.935f,2.40f,.25f);
  PrefabUtility.SaveAsPrefabAsset(leaf,Root+"/Prefabs/Vantail_Tour_V3.prefab");UnityEngine.Object.DestroyImmediate(leaf);
  var pins=new GameObject("Gonds_Mur_V3");var pinModel=Model("Gonds_V3",pins.transform);foreach(var r in pinModel.GetComponentsInChildren<Renderer>())r.sharedMaterial=iron;Record(pinModel);PrefabUtility.SaveAsPrefabAsset(pins,Root+"/Prefabs/Gonds_Mur_V3.prefab");UnityEngine.Object.DestroyImmediate(pins);
  var props=new GameObject("Poste_Guet_1600");var model=Model("Garniture_V3",props.transform);
  foreach(var r in model.GetComponentsInChildren<Renderer>()){string n=r.name;r.sharedMaterial=n.Contains("Terres")?clay:n.Contains("Cordage")?rope:n.Contains("Meche")?wick:n.Contains("Margelle")?inside:n.Contains("Fermeture")?iron:wood;}Record(model);
  Action<string,Vector3,Vector3> solid=(name,centre,size)=>{var ob=new GameObject(name);ob.transform.SetParent(props.transform,false);var c=ob.AddComponent<BoxCollider>();c.center=centre;c.size=size;};
  solid("Collision_Table",new Vector3(-2.27f,.48f,-.11f),new Vector3(.46f,.76f,.73f));
  solid("Collision_Tabouret",new Vector3(-2.11f,.31f,.57f),new Vector3(.31f,.42f,.30f));
  solid("Collision_Coffre",new Vector3(2.25f,.34f,-.24f),new Vector3(.46f,.48f,.70f));
  solid("Collision_Citerne_Couverte",new Vector3(-1.91f,.30f,-1.37f),new Vector3(.58f,.40f,.58f));
  solid("Collision_Echelle",new Vector3(2.17f,1.55f,.40f),new Vector3(.45f,2.9f,.63f));
  PrefabUtility.SaveAsPrefabAsset(props,Root+"/Prefabs/Poste_Guet_1600.prefab");UnityEngine.Object.DestroyImmediate(props);
  AssetDatabase.SaveAssets();return "V3 reusable assets prepared";
 }
 static GameObject Instance(string prefab,Transform parent) {return (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/"+prefab+".prefab"),parent);}
 static TourDoubleDoor Portal(Transform parent,string name,bool rear,Transform camera) {
  var portal=new GameObject(name);portal.transform.SetParent(parent,false);portal.transform.localRotation=Quaternion.Euler(0,rear?180:0,0);
  var pair=portal.AddComponent<TourDoubleDoor>();
  var left=Instance("Vantail_Tour_V3",portal.transform);left.name="Vantail_Gauche";left.transform.localPosition=new Vector3(-.955f,.13f,-2.63f);
  var door=left.AddComponent<AccueilDoor>();var settings=new SerializedObject(door);settings.FindProperty("openAngle").floatValue=-155;settings.FindProperty("handleLocalPosition").vector3Value=new Vector3(.79f,1.12f,-.175f);settings.ApplyModifiedPropertiesWithoutUndo();
  var mirror=new GameObject("Cote_Droit");mirror.transform.SetParent(portal.transform,false);mirror.transform.localScale=Vector3.one;
  var right=Instance("Vantail_Tour_V3",mirror.transform);right.name="Vantail_Droit";right.transform.localPosition=new Vector3(.955f,.13f,-2.63f);foreach(Transform visual in right.transform){visual.localScale=Vector3.Scale(visual.localScale,new Vector3(-1,1,1));PrefabUtility.RecordPrefabInstancePropertyModifications(visual);}var rightBox=right.GetComponent<BoxCollider>();var rc=rightBox.center;rc.x=-rc.x;rightBox.center=rc;PrefabUtility.RecordPrefabInstancePropertyModifications(rightBox);
  foreach(var pr in new[]{portal.transform,mirror.transform}){var pins=Instance("Gonds_Mur_V3",pr);pins.transform.localPosition=pr==portal.transform?left.transform.localPosition:right.transform.localPosition;if(pr==mirror.transform)foreach(Transform visual in pins.transform){visual.localScale=Vector3.Scale(visual.localScale,new Vector3(-1,1,1));PrefabUtility.RecordPrefabInstancePropertyModifications(visual);}PrefabUtility.RecordPrefabInstancePropertyModifications(pins.transform);}
  pair.left=door;pair.right=right.transform;pair.Configure(camera);Record(left);Record(right);return pair;
 }
 public static object Install() {
  if(EditorApplication.isPlaying)throw new Exception("Edit mode required");
  var root=GameObject.Find("Extension_Tour_Littoral").transform;
  if(root.GetComponent<TourInteriorRoute>())throw new Exception("Already installed: do not rerun installation");
  PrepareAssets();
  var old=root.Find("01_Tour_Genoise");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
  var architecture=new GameObject("01_Tour_Genoise");architecture.transform.SetParent(root,false);
  var body=Instance("Tour_Genoise_V3",architecture.transform);var props=Instance("Poste_Guet_1600",architecture.transform);
  var interior=root.gameObject.AddComponent<TourInteriorRoute>();interior.maquisDoor=Portal(architecture.transform,"Porte_Maquis",false,Camera.main.transform);interior.littoralDoor=Portal(architecture.transform,"Porte_Littoral",true,Camera.main.transform);
  root.GetComponent<TourLittoralRoute>().entranceDoor=interior.maquisDoor.left.transform;
  var keys=new[]{new Vector3(0,.10f,-3.65f),new Vector3(0,.10f,-2.75f),new Vector3(0,.10f,-1.85f),new Vector3(1.55f,.10f,-1.80f),new Vector3(1.55f,.10f,-.15f),new Vector3(0,.10f,0),new Vector3(-1.55f,.10f,.15f),new Vector3(-1.55f,.10f,1.80f),new Vector3(0,.10f,1.85f),new Vector3(0,.10f,2.75f),new Vector3(0,.10f,3.65f)};
  var points=new List<Vector3>();for(int i=1;i<keys.Length;i++){int n=Mathf.CeilToInt(Vector3.Distance(keys[i-1],keys[i])/.10f);for(int j=0;j<n;j++)points.Add(Vector3.Lerp(keys[i-1],keys[i],j/(float)n));}points.Add(keys[keys.Length-1]);interior.points=points.ToArray();interior.distances=new float[points.Count];for(int i=1;i<points.Count;i++)interior.distances[i]=interior.distances[i-1]+Vector3.Distance(points[i-1],points[i]);
  var player=GameObject.Find("Joueur_Desktop");var corridor=player.GetComponent<SentierCorridor>();var main=GameObject.Find("Maquis_35m").GetComponent<Maquis35Route>();var coast=root.GetComponent<TourLittoralRoute>();var oldPoints=corridor.CentreLine;int end=Enumerable.Range(0,oldPoints.Length).OrderBy(i=>(oldPoints[i]-main.WorldAt(main.Length)).sqrMagnitude).First();
  var widthSettings=new SerializedObject(corridor).FindProperty("sectionHalfWidths");var line=oldPoints.Take(end+1).ToList();var widths=Enumerable.Range(0,end+1).Select(i=>widthSettings.arraySize==oldPoints.Length?widthSettings.GetArrayElementAtIndex(i).floatValue:corridor.HalfWidth).ToList();
  foreach(var p in points){line.Add(root.TransformPoint(p));widths.Add(Mathf.Abs(p.z)<.6f?.28f:Mathf.Abs(p.z)<2.4f?.43f:.56f);}
  foreach(var p in coast.coastPoints){line.Add(root.TransformPoint(p));widths.Add(.58f);}corridor.Configure(line.ToArray(),.58f);corridor.ConfigureWidths(widths.ToArray());EditorUtility.SetDirty(corridor);
  var zones=root.gameObject.AddComponent<TourZoneSeparation>();zones.player=player.transform;
  zones.maquisRenderers=GameObject.Find("Maquis_35m").GetComponentsInChildren<Renderer>(true).Concat(GameObject.Find("Maison_Accueil_Corse").GetComponentsInChildren<Renderer>(true)).Distinct().ToArray();
  zones.littoralRenderers=new[]{"02_Terrain_Littoral","03_Vegetation_Littorale","04_Rochers_Littoral","05_Mer_Horizon"}.SelectMany(n=>root.Find(n).GetComponentsInChildren<Renderer>(true)).ToArray();
  zones.maquisInitial=zones.maquisRenderers.Select(r=>r.enabled).ToArray();zones.littoralInitial=zones.littoralRenderers.Select(r=>r.enabled).ToArray();zones.RestoreAll();
  EditorUtility.SetDirty(interior);EditorUtility.SetDirty(zones);EditorUtility.SetDirty(coast);
  Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
  return new{interiorLength=interior.Length,interiorPoints=interior.points.Length,maquisLength=main.Length,coastLength=coast.Length,maquisRenderers=zones.maquisRenderers.Length,coastRenderers=zones.littoralRenderers.Length};
 }
 }
}