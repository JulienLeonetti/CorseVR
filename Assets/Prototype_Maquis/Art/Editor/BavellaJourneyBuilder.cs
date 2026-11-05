using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace CorseVR.Editor {
public static class BavellaJourneyBuilder {
 const string Root="Assets/Prototype_Maquis/Art/Bavella";
 static System.Random rng;
 static List<Vector2> route; static List<float> distance;
 static float R(float a,float b){return a+(float)rng.NextDouble()*(b-a);}
 static Color C(string h){Color c;ColorUtility.TryParseHtmlString("#"+h,out c);return c.linear;}
 static Transform env,land,plants,stones,peaks;
 static Material leafL,leafA,leafC,pine,wood,flowers,rock,farRock,ground,detail;
 public static void Route(){
  route=new List<Vector2>();distance=new List<float>();
  Vector2[] p={new Vector2(-3,-14),new Vector2(-3,-11),new Vector2(-3.1f,-8),new Vector2(-2.6f,-5.1f),new Vector2(-1.4f,-3.5f),new Vector2(.2f,-2.3f),new Vector2(1.7f,1.4f),new Vector2(1.5f,6.3f),new Vector2(1.8f,10.3f),new Vector2(2,14)};
  float sum=0;
  for(int i=1;i<p.Length-2;i++)for(int j=0;j<20;j++){
   float t=j/20f,t2=t*t,t3=t2*t;
   Vector2 v=.5f*((2*p[i])+(-p[i-1]+p[i+1])*t+(2*p[i-1]-5*p[i]+4*p[i+1]-p[i+2])*t2+(-p[i-1]+3*p[i]-3*p[i+1]+p[i+2])*t3);
   if(route.Count>0)sum+=Vector2.Distance(v,route[route.Count-1]);route.Add(v);distance.Add(sum);
  }
  sum+=Vector2.Distance(route[route.Count-1],p[p.Length-2]);route.Add(p[p.Length-2]);distance.Add(sum);
 }
 public static Vector2 Point(float metres){
  if(route==null)Route();if(metres<=0)return route[0];
  for(int i=1;i<route.Count;i++)if(distance[i]>=metres)return Vector2.Lerp(route[i-1],route[i],Mathf.InverseLerp(distance[i-1],distance[i],metres));
  return route[route.Count-1];
 }
 public static Vector3 Forward(float s){Vector2 d=Point(s+.25f)-Point(Mathf.Max(0,s-.25f));return new Vector3(d.x,0,d.y).normalized;}
 static float Segment(Vector2 p,Vector2 a,Vector2 b){Vector2 d=b-a;return Vector2.Distance(p,a+d*Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude));}
 public static float PathDistance(float x,float z){
  if(route==null)Route();Vector2 p=new Vector2(x,z);float best=999;
  for(int i=1;i<route.Count;i++)best=Mathf.Min(best,Segment(p,route[i-1],route[i]));
  return best;
 }
 static readonly Vector2 Nepita=new Vector2(-4.9f,-4.6f),Woman=new Vector2(3.8f,1);
 static bool Reserved(float x,float z,float margin){
  Vector2 p=new Vector2(x,z);
  return Vector2.Distance(p,Nepita)<1.2f+margin||Vector2.Distance(p,Woman)<1.7f+margin
  ||Segment(p,Nepita,new Vector2(-2.35f,-4.6f))<.65f+margin||Segment(p,Woman,new Vector2(1.6f,1))<.8f+margin;
 }
 static float G(float x,float z,float a,float b,float radius){return Mathf.Exp(-((x-a)*(x-a)+(z-b)*(z-b))/(radius*radius));}
 public static float Height(float x,float z){
  float d=PathDistance(x,z);
  float path=.022f*(z+11)+.035f*Mathf.Sin((z+11)*.65f)+.018f*(Mathf.PerlinNoise(x*2.1f+53,z*2.1f+29)-.5f);
  float bank=Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.0f,5.5f,d));
  float irregular=.5f+1.8f*Mathf.PerlinNoise(x*.13f+29,z*.13f+57);
  float near=path+bank*irregular;
  float hills=7*G(x,z,-22,27,15)+10*G(x,z,26,38,18)+4.5f*G(x,z,-23,-24,13)+5*G(x,z,23,-20,14);
  float far=Mathf.SmoothStep(0,1,Mathf.InverseLerp(12,30,Mathf.Max(Mathf.Abs(x),Mathf.Abs(z))));
  near+=hills*far;
  near+=Mathf.SmoothStep(0,1,Mathf.InverseLerp(40,95,Mathf.Max(Mathf.Abs(x),Mathf.Abs(z))))*(3+7*Mathf.PerlinNoise(x*.018f+17,z*.018f+63));
  if(Reserved(x,z,0))near=Mathf.Lerp(path+.08f,near,Mathf.SmoothStep(0,1,Mathf.InverseLerp(.2f,2,Mathf.Min(Vector2.Distance(new Vector2(x,z),Nepita),Vector2.Distance(new Vector2(x,z),Woman)))));
  return near;
 }
 static Transform Group(string name,Transform parent){var t=new GameObject(name).transform;t.SetParent(parent,false);return t;}
 static Material Mat(string name,float mode,Color tint,Texture tex=null){
  string path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(m==null){m=new Material(Shader.Find("CorseVR/Bavella Painted"));AssetDatabase.CreateAsset(m,path);}
  m.name=name;m.SetFloat("_Mode",mode);m.SetColor("_BaseColor",tint);m.SetColor("_ShadowTint",new Color(.54f,.66f,.58f));m.SetFloat("_Roughness",mode<.5f?.72f:.94f);m.SetFloat("_TextureScale",.58f);m.SetFloat("_Cull",mode<.5f?0:2);m.SetFloat("_ColorsAreSRGB",1);
  if(tex!=null)m.SetTexture("_BaseMap",tex);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;
 }
 static Mesh SaveMesh(Mesh m,string name){
  m.name=name;string path=Root+"/Meshes/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
  if(old==null){AssetDatabase.CreateAsset(m,path);return m;}EditorUtility.CopySerialized(m,old);EditorUtility.SetDirty(old);UnityEngine.Object.DestroyImmediate(m);return old;
 }
 static GameObject MeshGo(string name,Mesh mesh,Material mat,Transform parent,bool collide,bool shadows=true){
  var go=new GameObject(name);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=mat;r.shadowCastingMode=shadows?ShadowCastingMode.On:ShadowCastingMode.Off;r.lightProbeUsage=LightProbeUsage.Off;r.reflectionProbeUsage=ReflectionProbeUsage.Off;
  if(collide)go.AddComponent<MeshCollider>().sharedMesh=mesh;go.isStatic=true;return go;
 }
 static Mesh Terrain(bool outer){
  int count=outer?120:144;float extent=outer?120:18;float step=extent*2/count;
  var vs=new List<Vector3>();var ns=new List<Vector3>();var uv=new List<Vector2>();var cs=new List<Color>();var ts=new List<int>();
  for(int j=0;j<=count;j++)for(int i=0;i<=count;i++){
   float x=-extent+i*step,z=-extent+j*step,y=Height(x,z);
   if(!outer&&(i==0||i==count)){float lo=Mathf.Floor(z/2)*2;y=Mathf.Lerp(Height(x,lo),Height(x,lo+2),(z-lo)/2);}
   else if(!outer&&(j==0||j==count)){float lo=Mathf.Floor(x/2)*2;y=Mathf.Lerp(Height(lo,z),Height(lo+2,z),(x-lo)/2);}
   vs.Add(new Vector3(x,y,z));float e=.10f;ns.Add(new Vector3(Height(x-e,z)-Height(x+e,z),2*e,Height(x,z-e)-Height(x,z+e)).normalized);uv.Add(new Vector2(x,z));
   float d=PathDistance(x,z);float rough=(Mathf.PerlinNoise(x*1.6f,z*1.6f)-.5f)*.24f;
   float w=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.75f,1.25f,d+rough));
   if(Reserved(x,z,0))w=Mathf.Max(w,.55f);
   cs.Add(new Color(w,0,0,1));
  }
  for(int j=0;j<count;j++)for(int i=0;i<count;i++){
   float x=-extent+(i+.5f)*step,z=-extent+(j+.5f)*step;if(outer&&Mathf.Abs(x)<18&&Mathf.Abs(z)<18)continue;
   int a=j*(count+1)+i,b=a+1,c=a+count+1,d=c+1;ts.Add(a);ts.Add(c);ts.Add(b);ts.Add(b);ts.Add(c);ts.Add(d);
  }
  var m=new Mesh();m.indexFormat=IndexFormat.UInt32;m.SetVertices(vs);m.SetNormals(ns);m.SetUVs(0,uv);m.SetColors(cs);m.SetTriangles(ts,0);m.RecalculateBounds();return m;
 }
 static Material[] Materials(Renderer renderer,int species,bool far=false){
  var old=renderer.sharedMaterials;var result=new Material[old.Length];
  for(int i=0;i<old.Length;i++){
   string name=old[i]==null?"":old[i].name;
   result[i]=name.Contains("Wood")?wood:name.Contains("Flower")?flowers:name.Contains("Granite")?(far?farRock:rock):species==0?leafL:species==1?leafA:species==2?leafC:pine;
  }return result;
 }
 static GameObject Model(string name,string file,Transform parent,int species=0,bool far=false){
  var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/"+file+".fbx");if(asset==null)throw new Exception("Missing "+file);
  var go=new GameObject(name);go.transform.SetParent(parent,false);var imported=(GameObject)PrefabUtility.InstantiatePrefab(asset);imported.transform.SetParent(go.transform,false);
  foreach(var r in go.GetComponentsInChildren<Renderer>()){r.sharedMaterials=Materials(r,species,far);r.lightProbeUsage=LightProbeUsage.Off;r.reflectionProbeUsage=ReflectionProbeUsage.Off;r.shadowCastingMode=far?ShadowCastingMode.Off:ShadowCastingMode.On;}
  foreach(var t in go.GetComponentsInChildren<Transform>())t.gameObject.isStatic=true;return go;
 }
 static GameObject PlantPrefab(string species,int id,int type){
  string path=Root+"/Prefabs/"+species+"_"+id+".prefab";var root=new GameObject(species+"_"+id);
  var hi=Model("Branches_et_feuilles_LOD0",species+"_"+id+"_High",root.transform,type);
  var lo=Model("Branches_et_feuilles_LOD1",species+"_"+id+"_Low",root.transform,type);
  var lod=root.AddComponent<LODGroup>();lod.fadeMode=LODFadeMode.None;
  lod.SetLODs(new[]{new LOD(type==3?.13f:.15f,hi.GetComponentsInChildren<Renderer>()),new LOD(type==3?.008f:.012f,lo.GetComponentsInChildren<Renderer>())});lod.RecalculateBounds();
  var saved=PrefabUtility.SaveAsPrefabAsset(root,path);UnityEngine.Object.DestroyImmediate(root);return saved;
 }
 static GameObject[] plantPrefabs;
 static int plantCount,rockCount,pinCount;
 static GameObject Plant(int species,int variant,float x,float z,float scale,float yaw){
  string speciesName=new[]{"Lentisque","Arbousier","Ciste","Pin_Laricio"}[species];var asset=plantPrefabs[species*3+variant%3];
  var go=(GameObject)PrefabUtility.InstantiatePrefab(asset);go.name=speciesName+"_"+(++plantCount).ToString("000");go.transform.SetParent(plants,false);go.transform.position=new Vector3(x,Height(x,z)-.035f,z);go.transform.rotation=Quaternion.Euler(R(-3,3),yaw,R(-3,3));go.transform.localScale=new Vector3(scale*R(.88f,1.12f),scale,scale*R(.88f,1.12f));
  if(species==1||species==3){var col=go.AddComponent<CapsuleCollider>();col.radius=species==3?.19f:.15f;col.height=species==3?4:1.8f;col.center=new Vector3(0,col.height*.5f,0);}
  if(species==3)pinCount++;return go;
 }
 static GameObject Rock(string name,int id,Vector3 p,Vector3 size,float yaw,bool collision=true,bool far=false,bool tower=false){
  var go=Model(name,(tower?"Aiguille_":"Granit_")+(id%(tower?7:5)),far?peaks:stones,0,far);go.transform.position=p;go.transform.localScale=size;go.transform.rotation=Quaternion.Euler(0,yaw,0);
  if(collision)foreach(var mf in go.GetComponentsInChildren<MeshFilter>())mf.gameObject.AddComponent<MeshCollider>().sharedMesh=mf.sharedMesh;
  rockCount++;return go;
 }
 static void PlaceRock(string name,int id,float x,float z,Vector3 size,float yaw,bool collision=true){
  Rock(name,id,new Vector3(x,Height(x,z)-.18f*size.y,z),size,yaw,collision);
 }
 static void Grass(){
  var vs=new List<Vector3>();var cs=new List<Color>();var uv=new List<Vector2>();var ts=new List<int>();
  for(int tuft=0;tuft<190;tuft++){
   float z=R(-22,16),x=R(-15,15);float d=PathDistance(x,z);if(d<1.22f||d>7.8f||Reserved(x,z,.2f))continue;Vector3 p=new Vector3(x,Height(x,z),z);
   for(int j=0;j<6;j++){
    float a=R(0,Mathf.PI*2),h=R(.15f,.42f),w=R(.018f,.035f);Vector3 side=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*w,lean=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a))*h*.35f;Vector3 b=p+new Vector3(R(-.12f,.12f),0,R(-.12f,.12f));int k=vs.Count;
    vs.Add(b-side);vs.Add(b+side);vs.Add(b+lean*.4f+Vector3.up*h*.55f+side*.6f);vs.Add(b+lean+Vector3.up*h);vs.Add(b+lean*.4f+Vector3.up*h*.55f-side*.6f);
    Color color=C(tuft%3==0?"B7A674":"8D985E");for(int q=0;q<5;q++){cs.Add(color*(.8f+q*.05f));uv.Add(Vector2.zero);}
    ts.Add(k);ts.Add(k+2);ts.Add(k+1);ts.Add(k);ts.Add(k+4);ts.Add(k+2);ts.Add(k+4);ts.Add(k+3);ts.Add(k+2);
   }
  }
  var mesh=new Mesh();mesh.SetVertices(vs);mesh.SetColors(cs);mesh.SetUVs(0,uv);mesh.SetTriangles(ts,0);mesh.RecalculateNormals();mesh.RecalculateBounds();MeshGo("Herbes_des_bordures",SaveMesh(mesh,"Herbes_des_bordures"),detail,plants,false,false);
 }
 static void Gravel(){
  var groups=new Dictionary<string,List<CombineInstance>>();
  for(int i=0;i<235;i++){
   float s=R(0,distance[distance.Count-1]),offset=(rng.Next(2)==0?-1:1)*R(.4f,2.0f);Vector2 a=Point(s);Vector3 f=Forward(s);a+=new Vector2(f.z,-f.x)*offset;if(Reserved(a.x,a.y,.25f))continue;
   string key=Mathf.FloorToInt(a.y/5).ToString();if(!groups.ContainsKey(key))groups[key]=new List<CombineInstance>();
   var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/Galet_0.fbx");var filter=asset.GetComponentInChildren<MeshFilter>();float size=R(.08f,.30f);
   groups[key].Add(new CombineInstance{mesh=filter.sharedMesh,transform=Matrix4x4.TRS(new Vector3(a.x,Height(a.x,a.y)-size*.18f,a.y),Quaternion.Euler(R(-8,8),R(0,360),R(-9,9)),new Vector3(size,size*.45f,size*.8f))*filter.transform.localToWorldMatrix});
  }
  foreach(var g in groups){var mesh=new Mesh();mesh.indexFormat=IndexFormat.UInt32;mesh.CombineMeshes(g.Value.ToArray(),true,true);MeshGo("Pierres_Enfouies_"+g.Key,SaveMesh(mesh,"Graviers_"+g.Key),rock,stones,false,true);}
 }
 static void Marker(string name,Vector2 p,Transform root){
  var t=Group(name,root);t.position=new Vector3(p.x,Height(p.x,p.y),p.y);
 }
 [MenuItem("CorseVR/Build Corsican maquis to Bavella")]
 public static void Menu(){Debug.Log(Build());}
 public static string Build(){
  var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.path!="Assets/Scenes/Prototype_Maquis.unity"||Application.isPlaying)throw new Exception("Open Prototype_Maquis in Edit mode.");
  Route();rng=new System.Random(341987);plantCount=rockCount=pinCount=0;
  foreach(string f in new[]{"Meshes","Materials","Prefabs","Sources"})Directory.CreateDirectory(Root+"/"+f);AssetDatabase.Refresh();
  foreach(var guid in AssetDatabase.FindAssets("t:Model",new[]{Root+"/Models"})){
   var importer=(ModelImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));importer.globalScale=1;importer.importCameras=false;importer.importLights=false;importer.importAnimation=false;importer.isReadable=true;importer.meshCompression=ModelImporterMeshCompression.Low;importer.SaveAndReimport();
  }
  var ti=(TextureImporter)AssetImporter.GetAtPath(Root+"/Textures/Granit_Bavella_Peint.png");ti.maxTextureSize=1024;ti.mipmapEnabled=true;ti.wrapMode=TextureWrapMode.Repeat;ti.anisoLevel=4;ti.textureCompression=TextureImporterCompression.Compressed;
  var web=ti.GetPlatformTextureSettings("WebGL");web.overridden=true;web.maxTextureSize=1024;web.format=TextureImporterFormat.ASTC_6x6;ti.SetPlatformTextureSettings(web);ti.SaveAndReimport();
  var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/Granit_Bavella_Peint.png");var bark=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Prototype_Maquis/Art/FoliageReference/Textures/Painted_Bark.png");
  leafL=Mat("Lentisque_Cire_Olive",0,new Color(1.1f,1.08f,.96f));leafA=Mat("Arbousier_Vert_Profond",0,new Color(.94f,1.03f,1.01f));leafC=Mat("Ciste_Sauge",0,new Color(1.08f,1.06f,.94f));pine=Mat("Pin_Laricio_Aiguilles",0,new Color(.90f,1.02f,1.06f));
  wood=Mat("Bois_Rameaux_Peints",1,Color.white,bark);flowers=Mat("Ciste_Petales_Ivoire",4,Color.white);rock=Mat("Granit_Rose_Peint",2,new Color(.88f,.90f,.93f),tex);farRock=Mat("Granit_Lointain_Bleute",2,new Color(.51f,.66f,.79f),tex);ground=Mat("Arene_Granitique_Sentier",3,Color.white,tex);detail=Mat("Herbes_Sauge_Seches",4,Color.white);detail.SetFloat("_Cull",0);detail.SetFloat("_ColorsAreSRGB",0);
  env=GameObject.Find("Maquis_Environment").transform;
  // Keep the previous art archived in the same scene, inactive. Preserve interaction anchors.
  var anchors=new List<Transform>();foreach(var t in Resources.FindObjectsOfTypeAll<Transform>())if(t.gameObject.scene==scene&&(t.name=="Emplacement_Nepita"||t.name=="Emplacement_Vieille_Femme"||t.name.StartsWith("Son_Directionnel_")))anchors.Add(t);
  foreach(var t in anchors)t.SetParent(env,true);
  var old=env.Find("Parcours_Corse_Bavella");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
  foreach(string n in new[]{"Maquis_Illustre_Reference","Maquis_Depart_Detail","Sol_30x30m","Rochers","Chemin"}){var t=env.Find(n);if(t!=null)t.gameObject.SetActive(false);}
  var root=Group("Parcours_Corse_Bavella",env);land=Group("01_Terrain_et_Enveloppe",root);plants=Group("02_Maquis_Botanique_et_Pins",root);stones=Group("03_Granit_Proche_et_Passage",root);peaks=Group("04_Bavella_Plans_de_Profondeur",root);
  MeshGo("Sol_Corse_Proche",SaveMesh(Terrain(false),"Terrain_Proche_36m"),ground,land,true);
  MeshGo("Enveloppe_Paysage_240m",SaveMesh(Terrain(true),"Terrain_Enveloppe_240m"),ground,land,true,false);
  plantPrefabs=new GameObject[12];for(int species=0;species<4;species++)for(int v=0;v<3;v++)plantPrefabs[species*3+v]=PlantPrefab(new[]{"Lentisque","Arbousier","Ciste","Pin_Laricio"}[species],v,species);
  // Designed foreground groups: tall arbutus in shelter, spreading lentisk, low cistus at sunny margins.
  float[,] hero={{-5.5f,-11.8f,1,1.0f},{-.1f,-9,1,.95f},{-5.8f,-7.6f,0,1.08f},{-.1f,-6.4f,0,1.08f},{-4.1f,-9.6f,2,1.1f},{-1.2f,-10.7f,2,1.0f},{-4.9f,-6.3f,2,1.0f},{-.55f,-4.6f,2,1.05f},{-5.7f,-3.1f,1,1.05f},{-1.7f,-1,0,.92f},{3,-4.6f,1,1.0f},{4.6f,-6.8f,0,1.08f},{-5.6f,-15.7f,1,1.0f},{-1.0f,-15,0,1.2f}};
  for(int i=0;i<hero.GetLength(0);i++)Plant((int)hero[i,2],i%3,hero[i,0],hero[i,1],hero[i,3],R(0,360));
  var placed=new List<Vector2>();for(int i=0;i<hero.GetLength(0);i++)placed.Add(new Vector2(hero[i,0],hero[i,1]));
  for(int i=0;i<340;i++){
   float x=R(-17,18),z=R(-23,19);float d=PathDistance(x,z);if(d<1.8f||Reserved(x,z,.7f))continue;
   bool near=d<5&&z<-1.3f;int species=near?(rng.NextDouble()<.40?2:rng.Next(2)):(rng.NextDouble()<.5?0:2);
   float min=species==2?.8f:1.25f;bool blocked=false;foreach(var p in placed)if(Vector2.Distance(new Vector2(x,z),p)<min){blocked=true;break;}if(blocked)continue;
   // Thinning after the stone passage gives the reveal room to breathe.
   if(z>-1&&d<5&&rng.NextDouble()<.72)continue;
   Plant(species,i%3,x,z,R(.72f,1.2f),R(0,360));placed.Add(new Vector2(x,z));if(plantCount>=115)break;
  }
  // Low cistus punctuation on the path edges; sparse enough for future selectable plants.
  for(int i=0;i<18;i++){float s=R(0,18);Vector2 p=Point(s);Vector3 f=Forward(s);p+=new Vector2(f.z,-f.x)*(i%2==0?-1:1)*R(1.45f,1.8f);if(!Reserved(p.x,p.y,.4f))Plant(2,i%3,p.x,p.y,R(.55f,.8f),R(0,360));}
  // Irregular pines connect the near rocky slopes to the massif.
  float[,] pins={{-8,-16,1.05f},{4,-12,.9f},{-10,-4,1.05f},{7,3,.92f},{-8,10,1.0f},{12,12,1.1f},{-14,23,.9f},{18,26,1.15f},{-24,35,1.2f},{24,39,1.0f},{-28,-21,1.1f},{18,-25,1.0f},{-13,36,.75f},{11,34,.8f},{-22,45,.9f}};
  for(int i=0;i<pins.GetLength(0);i++)Plant(3,i%3,pins[i,0],pins[i,1],pins[i,2],R(0,360));
  // Grouped pine groves at the foot of the granite join the maquis to the mountain.
  for(int i=0;i<33;i++){
   float side=i%2==0?-1:1,x=side*R(10,30),z=R(22,51);var tree=Plant(3,i%3,x,z,R(.70f,1.20f),R(0,360));
   foreach(var rr in tree.GetComponentsInChildren<Renderer>())rr.shadowCastingMode=ShadowCastingMode.Off;
  }
  // A low, wide slit in the granite bank makes the right-hand bend legible.
  PlaceRock("Passage_Granit_Gauche",0,-3.05f,-1.9f,new Vector3(1.45f,2.2f,1.7f),-15);
  PlaceRock("Passage_Granit_Droit",2,3.25f,-3.0f,new Vector3(1.6f,1.9f,1.5f),32);
  float[,] boulders={{-6.4f,-11.3f,1.5f,1.1f},{.2f,-12,1.0f,.7f},{-5.6f,-8.8f,1.2f,.85f},{.4f,-7,1.2f,.95f},{-6.2f,-1,1.8f,1.3f},{5.9f,1,1.7f,1.15f},{-2.4f,5,1.7f,1.4f},{6,7,1.3f,.8f},{-9,9,2.1f,1.5f},{9,14,2.7f,2.0f},{-13,-13,2,1.3f},{7,-18,2.3f,1.7f}};
  boulders[5,0]=7.8f; // Keep the conversation clearing clear of the complete rock volume.
  for(int i=0;i<boulders.GetLength(0);i++)PlaceRock("Bloc_Granit_"+i.ToString("00"),i,boulders[i,0],boulders[i,1],new Vector3(boulders[i,2],boulders[i,3],boulders[i,2]*.78f),R(0,360));
  for(int i=0;i<22;i++){
   float side=i%2==0?-1:1,x=side*R(13,35),z=R(-30,40);
   PlaceRock("Affleurement_Lateral_"+i,i,x,z,new Vector3(R(2.8f,4.8f),R(2,4),R(2.5f,4.5f)),R(-35,35));
  }
  // Real volumetric granite, three overlapping tiers; no camera-facing mountain cards.
  float[] px={-28,-19,-10,-1,9,19,29};float[] pz={56,60,55,63,58,67,72};float[] sy={5.4f,6.9f,7.6f,6.4f,8.1f,6.8f,5.2f};
  for(int i=0;i<7;i++){float h=Height(px[i],pz[i])-1.4f;Rock("Bavella_Tour_"+(i+1),i,new Vector3(px[i],h,pz[i]),new Vector3(5.1f,sy[i],5.2f),R(-12,12),true,false,true);}
  for(int i=0;i<7;i++)Rock("Contrefort_Granit_"+i,i,new Vector3(px[i],Height(px[i],pz[i]-9)-1,pz[i]-9),new Vector3(5.8f,6.0f,8),R(-18,18),true,false);
  for(int i=0;i<9;i++)Rock("Crete_Lointaine_"+i,i,new Vector3(-65+i*16,-2,112+R(-5,8)),new Vector3(10,R(9.5f,14f),10),R(-15,15),false,true,true);
  for(int i=0;i<6;i++)Rock("Relief_Arriere_"+i,i,new Vector3(-48+i*19,-3,-65-R(0,10)),new Vector3(10,R(2.5f,4.2f),9),R(-25,25),false,true,true);
  for(int i=0;i<6;i++){
   float x=i%2==0?-90:90,z=-42+(i/2)*43;
   Rock("Horizon_Lateral_"+i,i,new Vector3(x,Height(x,z)-5,z),new Vector3(13,R(4.4f,6.4f),15),i%2==0?85:-85,false,true,true);
  }
  Grass();Gravel();
  var prep=Group("Preparation_Interactions",root);
  foreach(var t in anchors){t.SetParent(prep,true);var p=t.position;t.position=new Vector3(p.x,Height(p.x,p.z)+.015f,p.z);t.gameObject.SetActive(true);}
  if(!prep.Find("Emplacement_Nepita"))Marker("Emplacement_Nepita",Nepita,prep);
  if(!prep.Find("Emplacement_Vieille_Femme"))Marker("Emplacement_Vieille_Femme",Woman,prep);
  var markers=Group("Reperes_Parcours_Metres",root);foreach(float s in new[]{0f,5f,10f,14f})Marker("Parcours_"+s+"m",Point(s),markers);
  var departure=GameObject.Find("Depart_WebXR");var start=Point(0);departure.transform.position=new Vector3(start.x,Height(start.x,start.y),start.y);
  var player=GameObject.Find("Joueur_Desktop");player.transform.position=departure.transform.position+Vector3.up*.035f;player.transform.rotation=Quaternion.LookRotation(Forward(0));
  var cam=player.GetComponentInChildren<Camera>();cam.transform.localPosition=new Vector3(0,1.65f,0);cam.transform.localRotation=Quaternion.identity;cam.fieldOfView=65;cam.farClipPlane=360;cam.allowMSAA=true;cam.allowDynamicResolution=false;cam.clearFlags=CameraClearFlags.Skybox;
  var sky=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Ciel_Mediterraneen.mat");if(sky==null){sky=new Material(Shader.Find("CorseVR/Bavella Sky"));AssetDatabase.CreateAsset(sky,Root+"/Materials/Ciel_Mediterraneen.mat");}
  sky.SetColor("_Zenith",new Color(.32f,.62f,.85f));sky.SetColor("_Horizon",new Color(.74f,.82f,.80f));RenderSettings.skybox=sky;
  RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogColor=new Color(.68f,.77f,.79f);RenderSettings.fogStartDistance=62;RenderSettings.fogEndDistance=280;
  RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.63f,.73f,.78f);RenderSettings.ambientEquatorColor=new Color(.49f,.55f,.43f);RenderSettings.ambientGroundColor=new Color(.34f,.30f,.23f);
  var sun=GameObject.Find("Directional Light").GetComponent<Light>();sun.transform.rotation=Quaternion.Euler(48,-32,0);sun.color=new Color(1,.93f,.80f);sun.intensity=1.12f;sun.shadows=LightShadows.Soft;sun.shadowStrength=.78f;sun.shadowBias=.03f;sun.shadowNormalBias=.12f;RenderSettings.sun=sun;
  EditorUtility.SetDirty(sky);Physics.SyncTransforms();AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
  return plantCount+" plants including "+pinCount+" pines; "+rockCount+" rock formations; maquis transition="+Point(10)+"; routeLength="+distance[distance.Count-1]+"m. Scene saved.";
 }
 public static string Capture(string directory){
  Directory.CreateDirectory(directory);var cam=Camera.main;var oldp=cam.transform.position;var oldr=cam.transform.rotation;float oldfov=cam.fieldOfView,aspect=cam.aspect;var oldtarget=cam.targetTexture;var active=RenderTexture.active;
  string[] names={"01_Depart_Maquis","02_Milieu_5m","03_Transition_10m","04_Decouverte_Montagne","05_Alentours_Lateral"};float[] metres={0,5,10,14,6};
  try{for(int i=0;i<names.Length;i++){
   Vector2 point=Point(metres[i]);cam.transform.position=new Vector3(point.x,Height(point.x,point.y)+1.65f,point.y);Vector3 direction=Forward(metres[i]);
   if(i==2||i==3)direction=new Vector3(-3-cam.transform.position.x,4.2f,58-cam.transform.position.z).normalized;
   if(i==4)direction=new Vector3(-1,0,.25f).normalized;
   cam.transform.rotation=Quaternion.LookRotation(direction);cam.fieldOfView=65;cam.aspect=1920f/1080;
   var rt=RenderTexture.GetTemporary(1920,1080,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB,4);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1920,1080,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1920,1080),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(directory,names[i]+".png"),tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);cam.targetTexture=null;RenderTexture.active=null;RenderTexture.ReleaseTemporary(rt);
  }}finally{cam.transform.position=oldp;cam.transform.rotation=oldr;cam.fieldOfView=oldfov;cam.aspect=aspect;cam.targetTexture=oldtarget;RenderTexture.active=active;}
  return "5 native 1920x1080 captures, eye level 1.65m, MSAA4.";
 }
}
}
