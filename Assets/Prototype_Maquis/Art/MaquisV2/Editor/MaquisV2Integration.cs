using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEngine.Rendering;using UnityEditor.SceneManagement;
namespace CorseVR.Editor {
 public static class MaquisV2Integration {
  public const string Root="Assets/Prototype_Maquis/Art/MaquisV2";
  static Material plant,stone,ground;static System.Random rng=new System.Random(260024);
  static float R(float a,float b){return a+(float)rng.NextDouble()*(b-a);}
  static Material Mat(string name,int mode,string map,string normal){
   string path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("CorseVR/Maquis V2 Mineral Botanique"));AssetDatabase.CreateAsset(m,path);}
   m.SetFloat("_Mode",mode);m.SetFloat("_ColorsAreSRGB",mode==3?0:1);m.SetFloat("_Roughness",.86f);m.SetFloat("_TextureScale",.8f);m.SetFloat("_Cull",0);m.SetColor("_ShadowTint",mode==0?new Color(.48f,.56f,.49f):new Color(.30f,.40f,.34f));m.SetColor("_BaseColor",Color.white);
   m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/"+map+".png"));m.SetTexture("_NormalMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/"+normal+".png"));
   m.SetTexture("_BarkMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/Ecorce_Fibres.png"));m.SetTexture("_BarkNormal",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/Ecorce_Normal.png"));m.SetTexture("_FoliageAtlas",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/Feuillages_11_Especes.png"));m.enableInstancing=true;EditorUtility.SetDirty(m);return m;
  }
  public static string Prepare(){
   if(EditorApplication.isPlaying)throw new Exception("Edit mode required");Directory.CreateDirectory(Root+"/Materials");Directory.CreateDirectory(Root+"/Prefabs");Directory.CreateDirectory(Root+"/Meshes");AssetDatabase.Refresh();
   foreach(string f in Directory.GetFiles(Root+"/Textures","*.png")){var i=(TextureImporter)AssetImporter.GetAtPath(f.Replace('\\','/'));if(i==null)continue;bool normal=f.Contains("Normal");if(i.sRGBTexture==normal||i.maxTextureSize!=(f.Contains("11_Especes")?2048:1024)){i.sRGBTexture=!normal;i.maxTextureSize=f.Contains("11_Especes")?2048:1024;i.mipmapEnabled=true;i.anisoLevel=4;i.textureCompression=TextureImporterCompression.CompressedHQ;i.SaveAndReimport();}}
   plant=Mat("Botanique_Fibres_Nervures",0,"Feuille_Nervures","Feuille_Normal");stone=Mat("Granit_Quartz_Mica",2,"Granit_Quartz_Mica","Granit_Normal");ground=Mat("Terre_Graviers",3,"Terre_Graviers","Terre_Normal");
   int count=0;foreach(string f in Directory.GetFiles(Root+"/Models","*_LOD0.fbx")){
    string key=Path.GetFileNameWithoutExtension(f).Replace("_LOD0","");var root=new GameObject(key);var lods=new List<LOD>();bool rock=key.StartsWith("Granit");
    for(int lod=0;lod<(rock?1:3);lod++){
     string path=Root+"/Models/"+key+"_LOD"+lod+".fbx";var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!model)throw new Exception("Missing "+path);
     var ob=(GameObject)PrefabUtility.InstantiatePrefab(model,root.transform);ob.name="LOD"+lod;ob.transform.localScale=Vector3.Scale(ob.transform.localScale,new Vector3(-1,1,1));PrefabUtility.RecordPrefabInstancePropertyModifications(ob.transform);
     var rs=ob.GetComponentsInChildren<Renderer>();foreach(var rr in rs){rr.sharedMaterial=rock?stone:plant;rr.shadowCastingMode=lod==2?ShadowCastingMode.Off:ShadowCastingMode.On;rr.reflectionProbeUsage=ReflectionProbeUsage.Off;rr.lightProbeUsage=LightProbeUsage.Off;PrefabUtility.RecordPrefabInstancePropertyModifications(rr);}if(!rock)lods.Add(new LOD(lod==0?.58f:lod==1?.18f:.012f,rs));
    }
    if(!rock){var group=root.AddComponent<LODGroup>();group.SetLODs(lods.ToArray());group.RecalculateBounds();}
    PrefabUtility.SaveAsPrefabAsset(root,Root+"/Prefabs/"+key+".prefab");UnityEngine.Object.DestroyImmediate(root);count++;
   }AssetDatabase.SaveAssets();return "Prefabs: "+count;
  }
  static string[] Keys(string sp){return Directory.GetFiles(Root+"/Prefabs",sp+"_*.prefab").Select(Path.GetFileNameWithoutExtension).OrderBy(s=>s).ToArray();}
  static GameObject Place(string key,Transform parent,Vector3 p,float targetHeight,float yaw){
   var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/"+key+".prefab");var ob=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);ob.transform.localPosition=p;ob.transform.localRotation=Quaternion.Euler(0,yaw,0);
   var f=ob.GetComponentsInChildren<MeshFilter>().First();float h=f.sharedMesh.bounds.size.y*Mathf.Abs(f.transform.lossyScale.y)/Mathf.Abs(parent.lossyScale.y);ob.transform.localScale=Vector3.one*targetHeight/Mathf.Max(.1f,h);PrefabUtility.RecordPrefabInstancePropertyModifications(ob.transform);return ob;
  }
  static float Along(Maquis35Route route,Vector3 p){float best=float.MaxValue,s=0;for(int i=0;i<route.points.Length;i++){var q=route.points[i];float d=new Vector2(q.x-p.x,q.z-p.z).sqrMagnitude;if(d<best){best=d;s=route.distances[i];}}return s;}
  static float Ground(Transform root,MeshCollider col,Vector3 p){RaycastHit hit;Vector3 world=root.TransformPoint(new Vector3(p.x,40,p.z));return col.Raycast(new Ray(world,Vector3.down),out hit,100)?root.InverseTransformPoint(hit.point).y:p.y;}
  public static string Integrate(){
   if(EditorApplication.isPlaying)throw new Exception("Edit mode required");var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.name!="Prototype_Maquis")throw new Exception("Wrong scene");
   plant=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Botanique_Fibres_Nervures.mat");stone=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Granit_Quartz_Mica.mat");ground=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Terre_Graviers.mat");
   var root=GameObject.Find("Maquis_35m").transform;var route=root.GetComponent<Maquis35Route>();if(root.Find("10_Refonte_Botanique_V2"))throw new Exception("V2 already integrated, use targeted edits");var col=root.Find("01_Terrain_Chemin").GetComponentInChildren<MeshCollider>();
   var fresh=new GameObject("10_Refonte_Botanique_V2").transform;fresh.SetParent(root,false);var groups=new Dictionary<string,Transform>();foreach(string key in new[]{"A_Bordures_Clairieres","B_Maquis_Haut","C_Chenes_Oleastres","D_Enveloppe","E_Roches","F_Ecotones"}){var g=new GameObject(key);g.transform.SetParent(fresh,false);groups[key]=g.transform;}
   int n=0;foreach(string oldName in new[]{"03_Vegetation_Proche","04_Grands_Arbustes","05_Arbres","06_Enveloppe_Fond"}){
    var old=root.Find(oldName);var originals=old.Cast<Transform>().ToArray();int i=0;
    foreach(var src in originals){
     var p=root.InverseTransformPoint(src.position);float s=Along(route,p);string species;float height;string group;
     bool shaded=s>12&&s<22,opening=s<5||(s>23&&s<30);
     if(oldName=="05_Arbres"){species=src.name.StartsWith("Oleastre")?"Oleastre":"CheneVert";height=species=="Oleastre"?R(3.8f,4.6f):R(5.5f,7.0f);group="C_Chenes_Oleastres";}
     else if(oldName=="03_Vegetation_Proche"){
      string[] palette=shaded?new[]{"Myrte","Ciste","Herbes","Myrte","Bruyere"}:opening?new[]{"Romarin","Lavande","Ciste","Immortelle","Herbes"}:new[]{"Lentisque","Ciste","Myrte","Romarin"};species=palette[(i+(int)(s*1.7))%palette.Length];height=species=="Lentisque"||species=="Myrte"?R(.8f,1.35f):species=="Bruyere"?R(.8f,1.4f):species=="Herbes"?R(.20f,.40f):R(.40f,.72f);group="A_Bordures_Clairieres";
     }else if(oldName=="04_Grands_Arbustes"){
      string[] palette=shaded?new[]{"Arbousier","Myrte","Arbousier","Bruyere"}:opening?new[]{"Lentisque","Ciste","Romarin","Arbousier","Lentisque"}:new[]{"Arbousier","Bruyere","Myrte","Lentisque"};species=palette[(i/3+(int)(s*.6))%palette.Length];height=species=="Arbousier"?R(1.8f,3.4f):species=="Bruyere"?R(1.8f,3.1f):species=="Myrte"||species=="Lentisque"?R(1.1f,1.85f):R(.55f,.85f);group="B_Maquis_Haut";
     }else{
      // Deliberately retain fewer broad crowns than the previous grid of narrow stems.
      if(i++%4==1)continue;string[] palette={"Arbousier","Arbousier","Bruyere","CheneVert","Lentisque"};species=palette[(i/2+(int)(s*.2))%palette.Length];height=species=="CheneVert"?R(5.0f,7.6f):species=="Lentisque"?R(2.1f,2.8f):R(3.3f,4.8f);group="D_Enveloppe";
     }
     var keys=Keys(species);string key=keys[(i*7+(int)(s*3.1))%keys.Length];p.y=Ground(root,col,p)-.055f;var ob=Place(key,groups[group],p,height,src.localEulerAngles.y+R(-35,35));
     if(group=="D_Enveloppe")foreach(var rr in ob.GetComponentsInChildren<Renderer>())rr.shadowCastingMode=ShadowCastingMode.Off;n++;i++;
    }
    foreach(var g in old.GetComponentsInChildren<LODGroup>())g.enabled=false;foreach(var rr in old.GetComponentsInChildren<Renderer>())rr.enabled=false;
   }
   int rocks=0;var oldRocks=root.Find("02_Rochers");foreach(Transform src in oldRocks){
    var oldRender=src.GetComponentsInChildren<Renderer>();if(oldRender.Length==0)continue;var bounds=oldRender[0].bounds;foreach(var rr in oldRender)bounds.Encapsulate(rr.bounds);
    var p=root.InverseTransformPoint(src.position);int variant=rocks%4;var ob=Place("Granit_V2_"+variant,groups["E_Roches"],p,Mathf.Max(.5f,bounds.size.y),src.localEulerAngles.y+R(-18,18));
    var rs=ob.GetComponentsInChildren<Renderer>();var bb=rs[0].bounds;foreach(var rr in rs)bb.Encapsulate(rr.bounds);float min=100;for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z++){var q=root.InverseTransformPoint(bb.center+new Vector3(x*bb.extents.x*.85f,0,z*bb.extents.z*.85f));min=Mathf.Min(min,Ground(root,col,q));}
    var lp=ob.transform.localPosition;lp.y+=Mathf.Min(0,min-.10f-root.InverseTransformPoint(new Vector3(bb.center.x,bb.min.y,bb.center.z)).y);ob.transform.localPosition=lp;PrefabUtility.RecordPrefabInstancePropertyModifications(ob.transform);foreach(var rr in oldRender)rr.enabled=false;rocks++;
   }
   // Intentional sunny patches and narrow grassy seams, avoiding the walking capsule.
   foreach(float station in new[]{3.5f,6.2f,10.8f,23.8f,26.2f,29.4f,33.5f})for(int side=-1;side<=1;side+=2){
    var p=route.LocalAt(station);var tangent=(route.LocalAt(station+.15f)-route.LocalAt(station-.15f)).normalized;var normal=Vector3.Cross(Vector3.up,tangent);
    for(int j=0;j<5;j++){var q=p+normal*side*R(1.15f,2.0f)+tangent*R(-.7f,.7f);q.y=Ground(root,col,q)-.025f;string species=j==0?"Lavande":j==1?"Immortelle":j==2?"Romarin":"Herbes";var keys=Keys(species);Place(keys[(j+(int)station)%keys.Length],groups["F_Ecotones"],q,species=="Herbes"?R(.18f,.35f):R(.35f,.62f),R(0,360));n++;}
   }
   foreach(var rr in root.Find("01_Terrain_Chemin").GetComponentsInChildren<Renderer>())rr.sharedMaterial=ground;
   foreach(var rr in root.Find("07_Sol_Details").GetComponentsInChildren<Renderer>())rr.sharedMaterial=rr.name.Contains("Granit")?stone:plant;
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();return "V2 plants: "+n+"; rocks: "+rocks+"; old physical colliders retained; route/house untouched";
  }
 }
}
