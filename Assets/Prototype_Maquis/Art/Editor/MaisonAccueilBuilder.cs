using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using CorseVR;
namespace CorseVR.Editor {
public static class MaisonAccueilBuilder {
 const string Root="Assets/Prototype_Maquis/Art/MaisonAccueil";
 const string V4="Assets/Prototype_Maquis/Art/SentierCorseV4";
 static Transform house;
 static GameObject Group(string name,Transform parent){var go=new GameObject(name);go.transform.SetParent(parent,false);return go;}
 static Mesh Save(Mesh m,string n){m.name=n;var path=Root+"/Meshes/"+n+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old==null){AssetDatabase.CreateAsset(m,path);return m;}old.Clear();old.indexFormat=m.indexFormat;old.vertices=m.vertices;old.normals=m.normals;old.colors=m.colors;if(m.uv.Length>0)old.uv=m.uv;old.triangles=m.triangles;old.RecalculateBounds();old.UploadMeshData(false);EditorUtility.SetDirty(old);UnityEngine.Object.DestroyImmediate(m);return old;}
 static Material Mat(string name,string source,float mode){var path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);var src=AssetDatabase.LoadAssetAtPath<Material>(source);if(src==null)throw new Exception(source);if(m==null){m=new Material(src);AssetDatabase.CreateAsset(m,path);}else EditorUtility.CopySerialized(src,m);m.name=name;m.SetFloat("_ColorsAreSRGB",1);m.SetFloat("_Mode",mode);m.SetFloat("_Cull",0);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;}
 static void Collider(string name,Vector3 pos,Vector3 size){var go=Group(name,house);go.transform.localPosition=pos;go.AddComponent<BoxCollider>().size=size;}
 static GameObject Model(string path,Transform parent,Dictionary<string,Material> mats){
  var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(asset==null)throw new Exception("Missing "+path);var go=(GameObject)PrefabUtility.InstantiatePrefab(asset);go.transform.SetParent(parent,false);go.transform.localScale=Vector3.Scale(go.transform.localScale,new Vector3(-1,1,1));
  foreach(var rr in go.GetComponentsInChildren<Renderer>()){var old=rr.sharedMaterials;for(int i=0;i<old.Length;i++){string key=mats.Keys.FirstOrDefault(k=>old[i].name.Contains(k));if(key!=null)old[i]=mats[key];}rr.sharedMaterials=old;rr.lightProbeUsage=LightProbeUsage.Off;PrefabUtility.RecordPrefabInstancePropertyModifications(rr);}PrefabUtility.RecordPrefabInstancePropertyModifications(go.transform);return go;
 }
 static float FootprintDistance(Vector3 local){return Mathf.Max(Mathf.Abs(local.x)-3.6f,Mathf.Max(local.z-1.0f,-6.35f-local.z));}
 static void TerrainAndPlants(Transform root){
  // Flatten only the house footprint and its immediate apron; preserve the winding path.
  var near=root.Find("01_Terrain_Zigzag/Sol_Zigzag_52m");var source=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Prototype_Maquis/Art/SentierCorseV3/Meshes/Sol_Zigzag_52m.asset");var terrain=UnityEngine.Object.Instantiate(source);var v=terrain.vertices;
  for(int i=0;i<v.Length;i++){var world=near.TransformPoint(v[i]);var local=house.InverseTransformPoint(world);float d=FootprintDistance(local);float blend=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(0,1.8f,d));world.y=Mathf.Lerp(world.y,house.position.y+.025f,blend);v[i]=near.InverseTransformPoint(world);}terrain.vertices=v;terrain.RecalculateNormals();terrain.RecalculateBounds();var saved=Save(terrain,"Terrain_Raccord_Maison");near.GetComponent<MeshFilter>().sharedMesh=saved;near.GetComponent<MeshCollider>().sharedMesh=saved;
  // Clear foliage faces that intersect the room, including combined shrub LOD chunks.
  foreach(Transform child in root.Find("02_Maquis_Occultant"))if(child.name.StartsWith("Arriere_Depart_"))child.gameObject.SetActive(false);
  foreach(var mf in root.Find("02_Maquis_Occultant").GetComponentsInChildren<MeshFilter>(true)){
   if(!mf.gameObject.activeInHierarchy)continue;
   var original=mf.sharedMesh;if(original==null)continue;var points=original.vertices;var remove=new bool[points.Length];bool touched=false;
   for(int i=0;i<points.Length;i++){var p=house.InverseTransformPoint(mf.transform.TransformPoint(points[i]));if(Mathf.Abs(p.x)<3.8f&&p.z> -6.55f&&p.z<.9f&&p.y<4.7f){remove[i]=true;touched=true;}}
   if(!touched)continue;var clone=UnityEngine.Object.Instantiate(original);var old=original.triangles;var tris=new List<int>();for(int i=0;i<old.Length;i+=3)if(!remove[old[i]]&&!remove[old[i+1]]&&!remove[old[i+2]])tris.AddRange(new[]{old[i],old[i+1],old[i+2]});clone.SetTriangles(tris,0);clone.RecalculateBounds();mf.sharedMesh=Save(clone,"Hors_Maison_"+mf.name);
  }
  // Remove complete large trees only where their trunks would enter the house.
  var treeRoot=root.Find("06_Grands_Arbres_Maisons_Cote_V4/01_Grands_Arbres");foreach(Transform t in treeRoot){var p=house.InverseTransformPoint(t.position);if(Mathf.Abs(p.x)<3.9f&&p.z>-6.8f&&p.z<1.0f)t.gameObject.SetActive(false);}
 }
 public static string Build(){
  var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(Application.isPlaying||scene.path!="Assets/Scenes/Prototype_Maquis.unity")throw new Exception("Edit Prototype_Maquis required");
  var env=GameObject.Find("Maquis_Environment").transform;var pathRoot=env.Find("Sentier_Zigzag_Portail_Littoral");var old=env.Find("Maison_Accueil_Corse");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
  house=Group("Maison_Accueil_Corse",env).transform;var p=MaquisRouteDefinition.Point(0);house.position=new Vector3(p.x,MaquisZigzagBuilder.Height(p.x,p.y),p.y);house.rotation=Quaternion.LookRotation(MaquisRouteDefinition.Forward(0));
  var mats=new Dictionary<string,Material>();mats["MaisonStone"]=Mat("Pierre_Granit",V4+"/Materials/Granit_Maison.mat",2);mats["MaisonMortar"]=Mat("Joints_Chaux",V4+"/Materials/Joints_Recesses.mat",4);mats["MaisonWood"]=Mat("Bois_Chataignier",V4+"/Materials/Bois_Maison.mat",1);mats["MaisonTile"]=Mat("Terre_Cuite",V4+"/Materials/Tuiles_Vieillies.mat",4);mats["MaisonFloor"]=Mat("Dalles_Usees",V4+"/Materials/Granit_Maison.mat",2);mats["MaisonIron"]=Mat("Ferrures",V4+"/Materials/Details_Mats.mat",4);
  foreach(var mat in mats.Values){mat.SetColor("_ShadowTint",new Color(.73f,.68f,.58f));mat.SetColor("_BaseColor",Color.white);EditorUtility.SetDirty(mat);}
  Model(Root+"/Models/Maison_Corse_Accueil.fbx",house,mats);
  Collider("Sol_Interieur",new Vector3(0,-.03f,-2.8f),new Vector3(6.25f,.14f,6.5f));
  Collider("Mur_Arriere",new Vector3(0,1.5f,-5.9f),new Vector3(6.5f,3,.5f));
  Collider("Mur_Droit",new Vector3(3.15f,1.5f,-2.95f),new Vector3(.5f,3,5.9f));Collider("Mur_Gauche",new Vector3(-3.15f,1.5f,-2.95f),new Vector3(.5f,3,5.9f));
  Collider("Facade_Gauche",new Vector3(-2.06f,1.5f,0),new Vector3(2.30f,3,.5f));Collider("Facade_Droite",new Vector3(2.06f,1.5f,0),new Vector3(2.30f,3,.5f));Collider("Linteau_Porte",new Vector3(0,2.82f,0),new Vector3(1.82f,.50f,.5f));
  Collider("Table",new Vector3(-2.25f,.42f,-3.55f),new Vector3(.90f,.84f,2.12f));Collider("Banc",new Vector3(-1.50f,.23f,-3.55f),new Vector3(.37f,.46f,2.06f));Collider("Cheminee",new Vector3(2.60f,.8f,-1.65f),new Vector3(1.2f,1.6f,1.76f));
  var hinge=Group("Porte_Accueil_Interactive",house).transform;hinge.localPosition=new Vector3(-.91f,.055f,0);Model(Root+"/Models/Porte_Accueil.fbx",hinge,mats);var doorCollider=hinge.gameObject.AddComponent<BoxCollider>();doorCollider.center=new Vector3(.91f,1.255f,0);doorCollider.size=new Vector3(1.82f,2.50f,.13f);
  var player=GameObject.Find("Joueur_Desktop");var camera=player.GetComponentInChildren<Camera>();hinge.gameObject.AddComponent<AccueilDoor>().Configure(camera.transform);
  TerrainAndPlants(pathRoot);
  var buildings=pathRoot.Find("06_Grands_Arbres_Maisons_Cote_V4/02_Maisons_Corses");if(buildings!=null)buildings.gameObject.SetActive(false);
  foreach(var gate in pathRoot.Find("03_Portails").GetComponentsInChildren<BiomePortal>())foreach(Transform child in gate.transform)if(child.name.EndsWith("_Arche_Granit"))child.gameObject.SetActive(true);
  var cc=player.GetComponent<CharacterController>();cc.enabled=false;player.transform.position=house.TransformPoint(new Vector3(0,.08f,-4.45f));player.transform.rotation=house.rotation;camera.transform.localPosition=new Vector3(0,1.65f,0);camera.transform.localRotation=Quaternion.identity;
  var corridor=player.GetComponent<SentierCorridor>();corridor.Configure(MaquisRouteDefinition.Corridor(),.76f);corridor.ConfigureStartRoom(house,new Vector2(0,-2.75f),new Vector2(2.60f,3.10f));cc.enabled=true;
  var depart=GameObject.Find("Depart_WebXR").transform;depart.position=player.transform.position;depart.rotation=player.transform.rotation;
  // Gentle interior fill to keep the room readable before opening the door.
  var fill=Group("Lumiere_Interieure_Douce",house);fill.transform.localPosition=new Vector3(-1.8f,2.45f,-3.5f);var light=fill.AddComponent<Light>();light.type=LightType.Point;light.range=7;light.intensity=.8f;light.color=new Color(1,.86f,.67f);light.shadows=LightShadows.None;
  Physics.SyncTransforms();AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
  return "Maison accueil complete: player inside, door closed (E), room free movement joined to maquis, passage houses hidden and standalone arches restored.";
 }
 public static string Capture(string dir,bool open){
  Directory.CreateDirectory(dir);var camera=Camera.main;var pos=camera.transform.position;var rot=camera.transform.rotation;float aspect=camera.aspect,fov=camera.fieldOfView;var oldTarget=camera.targetTexture;var oldRT=RenderTexture.active;
  var house=GameObject.Find("Maison_Accueil_Corse").transform;var door=house.Find("Porte_Accueil_Interactive");var doorRot=door.localRotation;door.localRotation=Quaternion.Euler(0,open?-105:0,0);
  try{camera.transform.position=house.TransformPoint(new Vector3(.18f,1.73f,-4.65f));camera.transform.rotation=house.rotation*Quaternion.Euler(0,-2,0);camera.fieldOfView=76;camera.aspect=1920f/1080;Physics.SyncTransforms();var rt=RenderTexture.GetTemporary(1920,1080,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB,4);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var tex=new Texture2D(1920,1080,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1920,1080),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(dir,open?"02_Interieur_Porte_Ouverte.png":"01_Interieur_Porte_Fermee.png"),tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);camera.targetTexture=null;RenderTexture.active=null;RenderTexture.ReleaseTemporary(rt);}finally{camera.transform.position=pos;camera.transform.rotation=rot;camera.aspect=aspect;camera.fieldOfView=fov;camera.targetTexture=oldTarget;RenderTexture.active=oldRT;door.localRotation=doorRot;Physics.SyncTransforms();}return "Captured interior "+(open?"open":"closed");
 }
}
}
