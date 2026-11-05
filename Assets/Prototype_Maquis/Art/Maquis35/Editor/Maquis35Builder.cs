using System;using System.IO;using System.Linq;using System.Collections.Generic;
using UnityEngine;using UnityEngine.Rendering;using UnityEditor;using UnityEditor.SceneManagement;
namespace CorseVR.Editor {
 public static class Maquis35Builder {
  public const string Root="Assets/Prototype_Maquis/Art/Maquis35";
  static System.Random rng;static Maquis35Route route;static Dictionary<string,GameObject> prefabs;static Material plants,stone,ground;
  static float R(float a,float b){return a+(float)rng.NextDouble()*(b-a);}
  static Color Hex(string s){Color c;ColorUtility.TryParseHtmlString("#"+s,out c);return c;}
  static Vector3 Cat(Vector3 a,Vector3 b,Vector3 c,Vector3 d,float t){return .5f*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t);}
  static float Elevation(float z){float t=Mathf.Max(0,z-.45f);return .064f+(1-Mathf.Exp(-t/4))*(.27f*Mathf.Sin(t*.24f)+.012f*t);}
  static Vector3[] Path(float scale){
   var k=new[]{new Vector3(0,0,.45f),new Vector3(.15f,0,2.35f),new Vector3(2.8f,0,4.4f),new Vector3(4.6f,0,7f),new Vector3(4.1f,0,9.7f),new Vector3(1.5f,0,12.1f),new Vector3(1.0f,0,15f),new Vector3(3.5f,0,17.7f),new Vector3(5.5f,0,19.6f),new Vector3(5.2f,0,22.5f),new Vector3(2.7f,0,25.3f),new Vector3(3.1f,0,28f)};
   var list=new List<Vector3>();for(int i=0;i<k.Length-1;i++)for(int j=0;j<50;j++){var p=Cat(k[Mathf.Max(0,i-1)],k[i],k[i+1],k[Mathf.Min(k.Length-1,i+2)],j/50f);p=new Vector3(p.x*scale,0,.45f+(p.z-.45f)*scale);p.y=Elevation(p.z);list.Add(p);}var last=k[k.Length-1];last=new Vector3(last.x*scale,0,.45f+(last.z-.45f)*scale);last.y=Elevation(last.z);list.Add(last);return list.ToArray();
  }
  static float Length(Vector3[] p){float s=0;for(int i=1;i<p.Length;i++)s+=Vector3.Distance(p[i-1],p[i]);return s;}
  static Vector3[] Resample(Vector3[] dense){var ds=new float[dense.Length];for(int i=1;i<dense.Length;i++)ds[i]=ds[i-1]+Vector3.Distance(dense[i-1],dense[i]);var list=new List<Vector3>();int q=1;for(float s=0;s<ds[ds.Length-1];s+=.15f){while(q<ds.Length-1&&ds[q]<s)q++;list.Add(Vector3.Lerp(dense[q-1],dense[q],Mathf.InverseLerp(ds[q-1],ds[q],s)));}list.Add(dense[dense.Length-1]);return list.ToArray();}
  static void Nearest(Vector3 p,out Vector3 q,out float distance,out float along,out Vector3 tangent){
   float best=float.MaxValue;int idx=1;float f=0;q=route.points[0];
   for(int i=1;i<route.points.Length;i++){var a=route.points[i-1];var b=route.points[i];var ab=b-a;ab.y=0;var ap=p-a;ap.y=0;float t=Mathf.Clamp01(Vector3.Dot(ap,ab)/Mathf.Max(.00001f,ab.sqrMagnitude));var near=Vector3.Lerp(a,b,t);float d=(new Vector2(p.x-near.x,p.z-near.z)).sqrMagnitude;if(d<best){best=d;q=near;idx=i;f=t;}}
   distance=Mathf.Sqrt(best);along=Mathf.Lerp(route.distances[idx-1],route.distances[idx],f);tangent=(route.points[idx]-route.points[idx-1]).normalized;
  }
  static float Width(float s){return 1.43f+.10f*Mathf.Sin(s*.63f)+.045f*Mathf.Sin(s*1.13f)+.65f*Mathf.Exp(-Mathf.Pow((s-11.4f)/.95f,2))+.60f*Mathf.Exp(-Mathf.Pow((s-26.2f)/.9f,2))+.75f*Mathf.SmoothStep(0,1,(s-32)/3);}
  public static float Height(float x,float z){
   Vector3 q,t;float d,s;Nearest(new Vector3(x,0,z),out q,out d,out s,out t);
   float edge=Mathf.Max(0,d-Width(s)*.5f);float bank=Mathf.SmoothStep(0,1,edge/4f);float hill=.55f+1.35f*Mathf.PerlinNoise(x*.12f+11,z*.11f+7)+.6f*Mathf.PerlinNoise(x*.22f+1,z*.19f);
   float y=q.y+bank*hill+Mathf.SmoothStep(0,1,(d-7)/6)*2.5f;
   y+=Mathf.SmoothStep(0,1,edge/.9f)*.05f*Mathf.Sin(x*3.2f+z*.9f);
   if(x> -3.8f&&x<3.8f&&z> -6.7f&&z<.96f){float doorway=Mathf.SmoothStep(0,1,(z+.15f)/.55f)*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.85f,1.25f,Mathf.Abs(x))));y=Mathf.Lerp(.015f,.064f,doorway);}
   return y;
  }
  static bool House(Vector3 p,float margin){return p.x> -3.7f-margin&&p.x<3.7f+margin&&p.z> -6.5f-margin&&p.z<.95f+margin;}
  static Transform Group(string name,Transform root){var g=new GameObject(name);g.transform.SetParent(root,false);return g.transform;}
  static Material Mat(string name,int mode,Texture tex){
   string path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("CorseVR/Maquis35 Peint"));AssetDatabase.CreateAsset(m,path);}m.SetFloat("_Mode",mode);m.SetFloat("_ColorsAreSRGB",1);m.SetFloat("_Roughness",.94f);m.SetFloat("_Cull",0);m.SetColor("_ShadowTint",new Color(.24f,.36f,.30f));m.SetColor("_BaseColor",Color.white);if(tex)m.SetTexture("_BaseMap",tex);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;
  }
  static GameObject Prefab(string name,bool lod){
   string path=Root+"/Prefabs/"+name+".prefab";var root=new GameObject(name);var lods=new List<LOD>();
   for(int i=0;i<(lod?3:1);i++){
    string file=name+(lod?"_LOD"+i:"");var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/"+file+".fbx");if(!asset)throw new Exception(file);
    var inst=(GameObject)PrefabUtility.InstantiatePrefab(asset,root.transform);inst.name=lod?"LOD"+i:file;inst.transform.localScale=Vector3.Scale(inst.transform.localScale,new Vector3(-1,1,1));PrefabUtility.RecordPrefabInstancePropertyModifications(inst.transform);
    var rs=inst.GetComponentsInChildren<Renderer>();foreach(var r in rs){r.sharedMaterial=name.StartsWith("Granit")?stone:plants;r.shadowCastingMode=(!lod||i==2)?ShadowCastingMode.Off:ShadowCastingMode.On;r.receiveShadows=true;r.reflectionProbeUsage=ReflectionProbeUsage.Off;r.lightProbeUsage=LightProbeUsage.Off;PrefabUtility.RecordPrefabInstancePropertyModifications(r);}
    if(lod)lods.Add(new LOD(i==0?.28f:i==1?.085f:.008f,rs));
   }
   if(lod){var group=root.AddComponent<LODGroup>();group.SetLODs(lods.ToArray());group.RecalculateBounds();group.fadeMode=LODFadeMode.None;}
   var prefab=PrefabUtility.SaveAsPrefabAsset(root,path);UnityEngine.Object.DestroyImmediate(root);return prefab;
  }
  static GameObject Place(string key,Vector3 p,float scale,float yaw,Transform parent,bool trunk=false){
   var ob=(GameObject)PrefabUtility.InstantiatePrefab(prefabs[key],parent);ob.transform.localPosition=p;ob.transform.localRotation=Quaternion.Euler(0,yaw,0);ob.transform.localScale=Vector3.one*scale;PrefabUtility.RecordPrefabInstancePropertyModifications(ob.transform);
   if(trunk){var c=ob.AddComponent<CapsuleCollider>();c.radius=.20f;c.height=2.6f;c.center=new Vector3(0,1.3f,0);}
   return ob;
  }
  static void Terrain(Transform parent){
   const int nx=137,nz=177;const float step=.4f;var vs=new Vector3[nx*nz];var cs=new Color[vs.Length];var uv=new Vector2[vs.Length];var tris=new int[(nx-1)*(nz-1)*6];int k=0;
   for(int j=0;j<nz;j++)for(int i=0;i<nx;i++){
    float x=-25.2f+i*step,z=-18f+j*step;Vector3 q,t;float d,s;Nearest(new Vector3(x,0,z),out q,out d,out s,out t);
    float border=Width(s)*.5f+.06f*Mathf.Sin(s*2.1f)+.045f*Mathf.Sin(s*4.37f);float mask=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(border-.18f,border+.4f,d));
    int idx=j*nx+i;vs[idx]=new Vector3(x,Height(x,z),z);cs[idx]=new Color(mask,.5f,.5f,1);uv[idx]=new Vector2(x,z);
    if(i<nx-1&&j<nz-1){tris[k++]=idx;tris[k++]=idx+nx;tris[k++]=idx+1;tris[k++]=idx+1;tris[k++]=idx+nx;tris[k++]=idx+nx+1;}
   }
   var mesh=new Mesh();mesh.name="Terrain_Sentier_Continu";mesh.vertices=vs;mesh.triangles=tris;mesh.colors=cs;mesh.uv=uv;mesh.RecalculateNormals();mesh.RecalculateBounds();string path=Root+"/Meshes/Terrain_Sentier_Continu.asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(mesh,old);UnityEngine.Object.DestroyImmediate(mesh);mesh=old;}else AssetDatabase.CreateAsset(mesh,path);
   var go=new GameObject("Sol_Continu_Mineral");go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=ground;go.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;go.AddComponent<MeshCollider>().sharedMesh=mesh;
  }
  static void Vegetation(Transform close,Transform middle,Transform back,Transform trees,Transform rocks,Transform details){
   string[] low={"Lentisque","Myrte","Ciste"};string[] tall={"Arbousier","Bruyere"};
   // Clumps follow local path normals, with deliberately different gaps and heights.
   for(float s=1.35f;s<35;s+=R(.78f,1.31f)){
    var p=route.LocalAt(s);var t=(route.LocalAt(s+.2f)-route.LocalAt(s-.2f)).normalized;var n=Vector3.Cross(Vector3.up,t);
    for(int sign=-1;sign<=1;sign+=2){
     float edge=Width(s)*.5f;var q=p+n*sign*(edge+R(.72f,1.08f));q.y=Height(q.x,q.z)-.035f;if(House(q,.8f))continue;
     string species=low[rng.Next(low.Length)];int variant=rng.Next(species=="Lentisque"?3:2);float scale=species=="Ciste"?R(.70f,1.1f):R(.62f,.85f);
     Place(species+"_"+variant,q,scale,R(0,360),close);
     if(s>3){q=p+n*sign*(edge+R(2.5f,3.6f))+t*R(-.7f,.7f);q.y=Height(q.x,q.z)-.12f;string sp=tall[rng.Next(2)];if(!House(q,1))Place(sp+"_"+rng.Next(3),q,R(.86f,1.12f),R(0,360),middle);}
    }
   }
   // Larger trees alternate sides, with irregular spacing and broad high crowns.
   float[] stations={2.8f,7.1f,10.6f,14.6f,17.5f,20.9f,24.0f,28.3f,31.5f,34.5f};
   for(int i=0;i<stations.Length;i++){
    float s=stations[i];var p=route.LocalAt(s);var t=(route.LocalAt(s+.2f)-route.LocalAt(s-.2f)).normalized;var n=Vector3.Cross(Vector3.up,t);var q=p+n*(i%2==0?-1:1)*R(3.2f,4.1f);q.y=Height(q.x,q.z)-.07f;
    if(!House(q,1.0f))Place((i==1||i==7?"Oleastre":"CheneVert")+"_"+(i%2),q,R(1.0f,1.20f),R(0,360),trees,true);
   }
   // Irregular background patches on terrain ridges close the envelope in all directions.
   for(float z=-13;z<43;z+=2.45f)for(float x=-19;x<22;x+=2.6f){
    var p=new Vector3(x+R(-.8f,.8f),0,z+R(-.8f,.8f));Vector3 q,t;float d,s;Nearest(p,out q,out d,out s,out t);if(d<4.1f||House(p,2))continue;
    float pattern=Mathf.PerlinNoise(p.x*.29f+4,p.z*.23f);if(pattern<.28f)continue;
    string species=rng.NextDouble()<.86?(rng.NextDouble()<.72?"Arbousier":"Bruyere"):"CheneVert";p.y=Height(p.x,p.z)-.15f;Place(species+"_"+rng.Next(species=="CheneVert"?2:3),p,species=="CheneVert"?R(.83f,1.2f):R(1.05f,1.6f),R(0,360),back);
   }
   // A lower irregular layer covers the banks beneath the open trunks.
   for(float z=.7f;z<36;z+=1.18f)for(float x=-5.5f;x<11;x+=1.22f){
    var p=new Vector3(x+R(-.42f,.42f),0,z+R(-.4f,.4f));Vector3 q,t;float d,s;Nearest(p,out q,out d,out s,out t);
    if(d<Width(s)*.5f+.9f||d>7||House(p,.35f)||rng.NextDouble()<.25)continue;
    string sp=low[rng.Next(3)];p.y=Height(p.x,p.z)-.045f;Place(sp+"_"+rng.Next(sp=="Lentisque"?3:2),p,sp=="Ciste"?R(.65f,1.05f):R(.45f,.78f),R(0,360),middle);
   }
   // Boulders make turns legible: three rock groups, nested into the banks.
   foreach(float s in new[]{4.6f,8.8f,14.7f,17.6f,21.4f,25.2f,30.7f,34.8f}){
    var p=route.LocalAt(s);var t=(route.LocalAt(s+.2f)-route.LocalAt(s-.2f)).normalized;var n=Vector3.Cross(Vector3.up,t);int sign=s<13?1:s<23?-1:1;
    for(int i=0;i<3;i++){var q=p+n*sign*R(2.25f,3.35f)+t*R(-.9f,.9f);q.y=Height(q.x,q.z)-R(.25f,.55f);float scale=R(.65f,1.05f);if(House(q,1))continue;var ob=Place("Granit_"+rng.Next(4),q,scale,R(0,360),rocks);var col=ob.AddComponent<BoxCollider>();var meshBounds=new Bounds(new Vector3(0,.6f,0),new Vector3(1.7f,1.2f,1.3f));col.center=meshBounds.center;col.size=meshBounds.size;}
   }
   // Low boulder backdrop completes the final space and conceals the unbuilt continuation.
   {var p=route.LocalAt(35);var t=(route.LocalAt(35)-route.LocalAt(34.5f)).normalized;var n=Vector3.Cross(Vector3.up,t);for(int i=-2;i<=2;i++){var q=p+t*4.5f+n*i*1.75f;q.y=Height(q.x,q.z)-.32f;Place("Granit_"+(i==0?1:(i+3)%4),q,R(.75f,1.15f),R(0,360),rocks);q+=t*1.6f;q.y=Height(q.x,q.z)-.12f;Place("Arbousier_"+(i+3)%3,q,R(1.0f,1.35f),R(0,360),back);}}
   // Fine details are batched later by prototype mesh. The main walking surface stays smooth.
   for(int i=0;i<430;i++){
    float s=R(.9f,35);var p=route.LocalAt(s);var t=(route.LocalAt(s+.18f)-route.LocalAt(s-.18f)).normalized;var n=Vector3.Cross(Vector3.up,t);float sign=rng.Next(2)==0?-1:1;float lateral=Width(s)*.5f+R(-.06f,1.05f);var q=p+n*sign*lateral+t*R(-.3f,.3f);q.y=Height(q.x,q.z)-.015f;
    if(House(q,.1f))continue;string key=i%5==0?"Granit_4":i%3==0?"Litiere_Feuilles":i%2==0?"Granit_5":"Graminees_"+rng.Next(2);Place(key,q,R(.55f,1.1f),R(0,360),details);
   }
   for(int i=0;i<10;i++){float s=R(3,34);var p=route.LocalAt(s);var t=(route.LocalAt(s+.1f)-route.LocalAt(s-.1f)).normalized;var n=Vector3.Cross(Vector3.up,t);p+=n*(i%2==0?1:-1)*Width(s)*.65f;p.y=Height(p.x,p.z)-.018f;Place("Racines_Affleurantes",p,R(.6f,1),R(0,360),details);}
   for(int i=0;i<100;i++){float s=R(1.15f,35);var p=route.LocalAt(s);var t=(route.LocalAt(s+.1f)-route.LocalAt(s-.1f)).normalized;p+=Vector3.Cross(Vector3.up,t)*R(-.50f,.50f);p.y=Height(p.x,p.z)-.022f;if(!House(p,.1f))Place("Granit_5",p,R(.22f,.40f),R(0,360),details);}
  }
  static void BatchDetails(Transform parent){
   foreach(var group in parent.GetComponentsInChildren<MeshFilter>().GroupBy(f=>f.sharedMesh).ToArray()){
    var list=group.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=parent.worldToLocalMatrix*f.transform.localToWorldMatrix}).ToArray();var mesh=new Mesh();mesh.indexFormat=IndexFormat.UInt32;mesh.name="Details_"+group.Key.name;mesh.CombineMeshes(list,true,true);string path=Root+"/Meshes/"+mesh.name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(mesh,old);UnityEngine.Object.DestroyImmediate(mesh);mesh=old;}else AssetDatabase.CreateAsset(mesh,path);
    var go=new GameObject(mesh.name);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=group.Key.name.StartsWith("Granit")?stone:plants;r.shadowCastingMode=ShadowCastingMode.Off;
   }
   foreach(Transform t in parent.Cast<Transform>().Where(t=>!t.name.StartsWith("Details_")).ToArray())UnityEngine.Object.DestroyImmediate(t.gameObject);
  }
  public static string Build(){
   if(Application.isPlaying)throw new Exception("Edit mode required");var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.name!="Prototype_Maquis")throw new Exception("Wrong scene");
   rng=new System.Random(351024);foreach(string folder in new[]{"Materials","Prefabs","Meshes"})Directory.CreateDirectory(Root+"/"+folder);AssetDatabase.Refresh();
   var tex=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Prototype_Maquis/Art/MaisonRefonte/Textures/Pierre_Creme_Peinte.png");plants=Mat("Vegetation_Peinte",0,null);stone=Mat("Granit_Peint",2,tex);ground=Mat("Terre_Minerale",3,tex);ground.SetFloat("_ColorsAreSRGB",0);
   plants.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Prototype_Maquis/Art/MaisonRefonte/Textures/Bois_Miel_Peint.png"));ground.SetColor("_ShadowTint",new Color(.24f,.36f,.30f));
   prefabs=new Dictionary<string,GameObject>();foreach(string sp in new[]{"Arbousier","Bruyere","Lentisque","Myrte","Ciste","CheneVert","Oleastre"})for(int i=0;i<(sp=="Arbousier"||sp=="Bruyere"||sp=="Lentisque"?3:2);i++){string key=sp+"_"+i;prefabs[key]=Prefab(key,true);}foreach(string key in new[]{"Granit_0","Granit_1","Granit_2","Granit_3","Granit_4","Granit_5","Graminees_0","Graminees_1","Litiere_Feuilles","Racines_Affleurantes"})prefabs[key]=Prefab(key,false);
   var oldRoot=GameObject.Find("Maquis_35m");if(oldRoot)UnityEngine.Object.DestroyImmediate(oldRoot);var root=new GameObject("Maquis_35m");var h=GameObject.Find("Maison_Accueil_Corse").transform;root.transform.SetPositionAndRotation(h.position,h.rotation);route=root.AddComponent<Maquis35Route>();
   float lo=.6f,hi=1.3f;for(int i=0;i<25;i++){float mid=(lo+hi)/2;if(Length(Resample(Path(mid)))<35)lo=mid;else hi=mid;}route.points=Resample(Path((lo+hi)/2));route.distances=new float[route.points.Length];route.widths=new float[route.points.Length];for(int i=0;i<route.points.Length;i++){if(i>0)route.distances[i]=route.distances[i-1]+Vector3.Distance(route.points[i-1],route.points[i]);route.widths[i]=Width(route.distances[i]);}
   var terrain=Group("01_Terrain_Chemin",root.transform);var rock=Group("02_Rochers",root.transform);var near=Group("03_Vegetation_Proche",root.transform);var midLayer=Group("04_Grands_Arbustes",root.transform);var tree=Group("05_Arbres",root.transform);var bg=Group("06_Enveloppe_Fond",root.transform);var detail=Group("07_Sol_Details",root.transform);Group("08_Collisions_Parcours",root.transform);var future=Group("09_Reserves_Botaniques",root.transform);
   Terrain(terrain);Vegetation(near,midLayer,bg,tree,rock,detail);
   var groundCollider=terrain.GetComponentInChildren<MeshCollider>();
   foreach(Transform ob in rock){var b=ob.GetComponentInChildren<Renderer>().bounds;float minGround=float.MaxValue;for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z++){RaycastHit hit;var origin=new Vector3(b.center.x+b.extents.x*x*.88f,b.max.y+10,b.center.z+b.extents.z*z*.88f);if(groundCollider.Raycast(new Ray(origin,Vector3.down),out hit,30))minGround=Mathf.Min(minGround,hit.point.y);}float delta=Mathf.Min(0,minGround-.11f-b.min.y);if(delta<-.005f){ob.position+=Vector3.up*delta;PrefabUtility.RecordPrefabInstancePropertyModifications(ob);}}
   BatchDetails(detail);
   foreach(var r in bg.GetComponentsInChildren<Renderer>()){r.shadowCastingMode=ShadowCastingMode.Off;PrefabUtility.RecordPrefabInstancePropertyModifications(r);}
   foreach(float s in new[]{11.4f,26.2f}){var p=route.LocalAt(s);var t=(route.LocalAt(s+.2f)-route.LocalAt(s-.2f)).normalized;p+=Vector3.Cross(Vector3.up,t)*(s<15?1:-1)*.85f;var e=Group("Reserve_Botanique_"+s.ToString("F0"),future);e.localPosition=p;}
   var corridor=GameObject.Find("Joueur_Desktop").GetComponent<SentierCorridor>();var pts=new List<Vector3>{h.TransformPoint(new Vector3(0,.064f,-4.7f)),h.TransformPoint(new Vector3(0,.064f,-.6f))};pts.AddRange(route.points.Select(p=>root.transform.TransformPoint(p)));corridor.Configure(pts.ToArray(),.45f);var widths=new List<float>{.50f,.50f};widths.AddRange(route.widths.Select(w=>Mathf.Max(.31f,w*.5f-.30f)));corridor.ConfigureWidths(widths.ToArray());EditorUtility.SetDirty(corridor);
   var skyShader=Shader.Find("CorseVR/Maquis35 Ciel");if(skyShader){string path=Root+"/Materials/Ciel_Mediterraneen.mat";var sky=AssetDatabase.LoadAssetAtPath<Material>(path);if(!sky){sky=new Material(skyShader);AssetDatabase.CreateAsset(sky,path);}sky.SetColor("_Zenith",new Color(.31f,.55f,.69f));sky.SetColor("_Horizon",new Color(.75f,.80f,.75f));RenderSettings.skybox=sky;EditorUtility.SetDirty(sky);}
   RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=24;RenderSettings.fogEndDistance=65;RenderSettings.fogColor=new Color(.66f,.73f,.67f);
   // Preserve the existing sun and house lights. All outside lighting comes from that sun.
   Camera.main.farClipPlane=85;Camera.main.clearFlags=CameraClearFlags.Skybox;DynamicGI.UpdateEnvironment();AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
   string report="Length="+route.Length.ToString("F5")+"m; samples="+route.points.Length+"; plants="+root.GetComponentsInChildren<LODGroup>().Length+"; renderers including LOD="+root.GetComponentsInChildren<Renderer>().Length;return report;
  }
 }
}
