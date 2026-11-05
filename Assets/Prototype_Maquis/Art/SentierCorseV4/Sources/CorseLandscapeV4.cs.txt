using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using CorseVR;
namespace CorseVR.Editor {
public static class CorseLandscapeV4 {
 const string Root="Assets/Prototype_Maquis/Art/SentierCorseV4", V1="Assets/Prototype_Maquis/Art/SentierCorse", V2="Assets/Prototype_Maquis/Art/SentierCorseV2", V3="Assets/Prototype_Maquis/Art/SentierCorseV3";
 static System.Random rng;
 static Material stone,wood,housewood,leaves,earth,roof,mortar,shutters,detail,water;
 static Transform forest,coast,houses;
 static float R(float a,float b){return a+(float)rng.NextDouble()*(b-a);}
 static Color C(string h){Color c;ColorUtility.TryParseHtmlString("#"+h,out c);return c.linear;}
 static Transform Group(string n,Transform parent){var t=new GameObject(n).transform;t.SetParent(parent,false);return t;}
 static Material Mat(string n,string path){var p=Root+"/Materials/"+n+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);var src=AssetDatabase.LoadAssetAtPath<Material>(path);if(src==null)throw new Exception("Missing "+path);if(m==null){m=new Material(src);AssetDatabase.CreateAsset(m,p);}else EditorUtility.CopySerialized(src,m);m.name=n;m.enableInstancing=true;EditorUtility.SetDirty(m);return m;}
 static Mesh Save(Mesh m,string n){m.name=n;var path=Root+"/Meshes/"+n+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old==null){AssetDatabase.CreateAsset(m,path);return m;}old.Clear();old.indexFormat=m.indexFormat;old.vertices=m.vertices;old.normals=m.normals;old.colors=m.colors;if(m.uv.Length>0)old.uv=m.uv;old.triangles=m.triangles;old.RecalculateBounds();old.UploadMeshData(false);EditorUtility.SetDirty(old);UnityEngine.Object.DestroyImmediate(m);return old;}
 static GameObject MeshGo(string n,Mesh m,Material mat,Transform parent,bool collider=false,bool cast=true){var go=new GameObject(n);go.transform.SetParent(parent,false);m=Save(m,n);go.AddComponent<MeshFilter>().sharedMesh=m;var rr=go.AddComponent<MeshRenderer>();rr.sharedMaterial=mat;rr.shadowCastingMode=cast?ShadowCastingMode.On:ShadowCastingMode.Off;rr.lightProbeUsage=LightProbeUsage.Off;go.isStatic=true;if(collider)go.AddComponent<MeshCollider>().sharedMesh=m;return go;}
 class Buffer {
  public List<Vector3> v=new List<Vector3>();public List<Color> c=new List<Color>();public List<int> t=new List<int>();
  public void Quad(Vector3 a,Vector3 b,Vector3 d,Vector3 e,Color color){int k=v.Count;v.AddRange(new[]{a,b,d,e});for(int i=0;i<4;i++)c.Add(color);t.AddRange(new[]{k,k+1,k+2,k,k+2,k+3});}
  public void Box(Vector3 p,Vector3 size,Color col,float rough=0){
   var lo=p-size*.5f;var hi=p+size*.5f;var q=new[]{new Vector3(lo.x,lo.y,lo.z),new Vector3(hi.x,lo.y,lo.z),new Vector3(hi.x,hi.y,lo.z),new Vector3(lo.x,hi.y,lo.z),new Vector3(lo.x,lo.y,hi.z),new Vector3(hi.x,lo.y,hi.z),new Vector3(hi.x,hi.y,hi.z),new Vector3(lo.x,hi.y,hi.z)};
   for(int i=0;i<8;i++)q[i]+=new Vector3(R(-rough,rough),R(-rough,rough),R(-rough,rough));
   Quad(q[0],q[3],q[2],q[1],col);Quad(q[4],q[5],q[6],q[7],col*.94f);Quad(q[0],q[4],q[7],q[3],col*.91f);Quad(q[1],q[2],q[6],q[5],col*.97f);Quad(q[3],q[7],q[6],q[2],col*1.03f);Quad(q[0],q[1],q[5],q[4],col*.85f);
  }
  public Mesh Mesh(){var mesh=new Mesh();mesh.indexFormat=IndexFormat.UInt32;mesh.SetVertices(v);mesh.SetColors(c);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;}
 }
 static void BoxColliderAt(Transform parent,string n,Vector3 pos,Vector3 size){var t=Group(n,parent);t.localPosition=pos;t.gameObject.AddComponent<BoxCollider>().size=size;}
 static GameObject Model(string n,string path,Transform parent,Vector3 pos,Vector3 scale,Quaternion rotation,Material bark,Material leafmat){
  var wrapper=Group(n,parent);wrapper.position=pos;wrapper.rotation=rotation;wrapper.localScale=scale;
  var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(asset==null)throw new Exception("Missing model "+path);var ob=(GameObject)PrefabUtility.InstantiatePrefab(asset);ob.transform.SetParent(wrapper,false);
  foreach(var rr in ob.GetComponentsInChildren<Renderer>()){var mats=rr.sharedMaterials;for(int j=0;j<mats.Length;j++)mats[j]=mats[j].name.Contains("Wood")?bark:leafmat;rr.sharedMaterials=mats;rr.lightProbeUsage=LightProbeUsage.Off;}return wrapper.gameObject;
 }
 static GameObject Tree(string n,bool pine,int variant,Transform parent,Vector3 pos,float scale,float stretch,float angle){
  var t=Group(n,parent);t.position=pos;t.rotation=Quaternion.Euler(R(-2,2),angle,R(-3,3));t.localScale=new Vector3(scale,scale*stretch,scale*R(.85f,1.15f));
  string model=Root+"/Models/"+(pine?"Pin_Cotier_":"Chene_Ancien_")+variant;
  var hi=Model(n+"_Proche",model+"_High.fbx",t,Vector3.zero,Vector3.one,Quaternion.identity,wood,leaves);hi.transform.localPosition=Vector3.zero;hi.transform.localRotation=Quaternion.identity;
  var lo=Model(n+"_Lointain",model+"_Low.fbx",t,Vector3.zero,Vector3.one,Quaternion.identity,wood,leaves);lo.transform.localPosition=Vector3.zero;lo.transform.localRotation=Quaternion.identity;
  var lod=t.gameObject.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.16f,hi.GetComponentsInChildren<Renderer>()),new LOD(.012f,lo.GetComponentsInChildren<Renderer>())});lod.RecalculateBounds();
  var capsule=t.gameObject.AddComponent<CapsuleCollider>();capsule.center=new Vector3(0,2.5f,0);capsule.height=5.4f;capsule.radius=.32f;return t.gameObject;
 }
 static int BuildForest(Transform old){
  foreach(Transform t in old.Find("02_Maquis_Occultant"))if(t.name.StartsWith("Chene_Couvert_"))t.gameObject.SetActive(false);
  var used=new List<Vector2>();var finish=MaquisRouteDefinition.Point(MaquisRouteDefinition.Length);int count=0;
  // Two irregular belts; reject plants against ANY segment of the winding trail.
  foreach(int side in new[]{-1,1})for(float s=-1.5f;s<MaquisRouteDefinition.Length+1;s+=R(1.45f,2.8f)){
   var p=MaquisRouteDefinition.Side(s,side*R(3.0f,6.7f));float d=MaquisRouteDefinition.Distance(p.x,p.y);
   if(d<2.5f||Vector2.Distance(p,finish)<5.0f||used.Any(q=>Vector2.Distance(p,q)<1.7f))continue;
   used.Add(p);bool pine=count%6==2;Tree("Grand_Arbre_"+count,pine,count%(pine?2:3),forest,new Vector3(p.x,MaquisZigzagBuilder.Height(p.x,p.y)-.11f,p.y),R(.88f,1.17f),R(.94f,1.24f),R(0,360));count++;
  }
  // Staggered background groves break up the regular shrub band and hide bare terrain.
  for(int i=0;i<25;i++){
   var p=MaquisRouteDefinition.Side(R(-1,MaquisRouteDefinition.Length), (i%2==0?-1:1)*R(7.5f,11.5f));
   if(MaquisRouteDefinition.Distance(p.x,p.y)<5||Vector2.Distance(p,finish)<6||used.Any(q=>Vector2.Distance(p,q)<2.1f))continue;
   used.Add(p);Tree("Arbre_Arriere_Plan_"+i,i%4==0,i%(i%4==0?2:3),forest,new Vector3(p.x,MaquisZigzagBuilder.Height(p.x,p.y)-.1f,p.y),R(.85f,1.15f),R(.9f,1.15f),R(0,360));count++;
  }
  return count;
 }
 static void House(BiomePortal portal,string suffix){
  var t=Group("Maison_Corse_"+suffix,houses);t.position=portal.transform.position;t.rotation=portal.transform.rotation;
  // A compact rural house, with an actual opening aligned with the existing passage.
  var b=new Buffer();var joints=new Buffer();float half=3.5f,depth=4.4f;
  // Recessed masonry backing, kept clear of door and windows.
  joints.Box(new Vector3(-2.35f,1.66f,.20f),new Vector3(2.25f,3.32f,.35f),C("777B70"));joints.Box(new Vector3(2.35f,1.66f,.20f),new Vector3(2.25f,3.32f,.35f),C("777B70"));
  joints.Box(new Vector3(0,3.3f,.22f),new Vector3(2.3f,.50f,.35f),C("777B70"));
  string[] palette={"A9A99B","B4B1A1","989D91","B2AD99","A0A08F","B7B4A5","9B9C91"};
  for(int row=0;row<12;row++){
   float y0=row*.29f;var spans=new List<Vector2>{new Vector2(-half,half)};
   var holes=new List<Vector2>();if(y0<3.04f){float hw=y0<2?1.08f:Mathf.Sqrt(Mathf.Max(0,1.08f*1.08f-(y0-2)*(y0-2)));holes.Add(new Vector2(-hw,hw));}
   if(y0<2.47f&&y0+.28f>1.45f){holes.Add(new Vector2(-2.76f,-1.84f));holes.Add(new Vector2(1.84f,2.76f));}
   foreach(var hole in holes){var next=new List<Vector2>();foreach(var span in spans){if(hole.y<=span.x||hole.x>=span.y){next.Add(span);continue;}if(hole.x>span.x)next.Add(new Vector2(span.x,hole.x));if(hole.y<span.y)next.Add(new Vector2(hole.y,span.y));}spans=next;}
   foreach(var span in spans)for(float x=span.x;x<span.y-.01f;){float width=Mathf.Min(R(.35f,.83f),span.y-x);b.Box(new Vector3(x+width*.5f,y0+.139f,R(-.03f,.025f)),new Vector3(Mathf.Max(.02f,width-.014f),R(.265f,.284f),R(.30f,.41f)),C(palette[rng.Next(palette.Length)]),.018f);x+=width;}
  }
  joints.Quad(new Vector3(-3.5f,3.32f,.16f),new Vector3(0,4.86f,.16f),new Vector3(3.5f,3.32f,.16f),new Vector3(-3.5f,3.32f,.16f),C("A9AA9A"));
  // Front gable, triangular rather than a floating rectangular roof.
  for(int row=0;row<5;row++){
   float y=3.48f+row*.29f;float width=2*half*Mathf.Clamp01((4.9f-y)/1.5f);
   for(float x=-width*.5f;x<width*.5f;){float w=Mathf.Min(R(.37f,.74f),width*.5f-x);if(w<.01f)break;b.Box(new Vector3(x+w*.5f,y,.07f),new Vector3(w-.012f,.278f,.42f),C(palette[rng.Next(7)]),.015f);x+=w;}
  }
  for(int side=-1;side<=1;side+=2)for(int row=0;row<12;row++)for(float z=.3f;z<depth;){float d=Mathf.Min(R(.35f,.85f),depth-z);b.Box(new Vector3(side*half,row*.29f+.14f,z+d*.5f),new Vector3(.36f,.278f,d-.016f),C(palette[rng.Next(7)]),.02f);z+=d;}
  // Heavy stone surrounds give the passage a plausible load-bearing lintel and arch.
  var archAsset=AssetDatabase.LoadAssetAtPath<Mesh>(V3+"/Meshes/Portail_Vers_Littoral_Arche_Granit.asset");
  if(archAsset!=null)MeshGo("Encadrement_Porte_"+suffix,UnityEngine.Object.Instantiate(archAsset),stone,t);
  // Small deep windows and weathered grey-green louvered shutters.
  var timber=new Buffer();var painted=new Buffer();var dark=new Buffer();
  foreach(int sign in new[]{-1,1}){
   float x=sign*2.3f;dark.Box(new Vector3(x,1.98f,.06f),new Vector3(.85f,1.05f,.08f),C("242F29"));
   b.Box(new Vector3(x,1.38f,-.13f),new Vector3(1.1f,.15f,.55f),C("B3B2A1"),.01f);b.Box(new Vector3(x,2.55f,-.10f),new Vector3(1.02f,.21f,.43f),C("B9B4A3"),.012f);
   for(int s=-1;s<=1;s+=2){float cx=x+s*.23f;painted.Box(new Vector3(cx,1.98f,-.24f),new Vector3(.42f,1.04f,.055f),C("64746B"),.007f);for(int j=0;j<11;j++)painted.Box(new Vector3(cx,1.53f+j*.085f,-.28f),new Vector3(.36f,.042f,.045f),C(j%3==0?"819087":"6F7F73"));timber.Box(new Vector3(cx,1.66f,-.32f),new Vector3(.39f,.05f,.025f),C("4B544C"));timber.Box(new Vector3(cx,2.28f,-.32f),new Vector3(.39f,.05f,.025f),C("4B544C"));}
  }
  // Open wooden door leaves sit against the sides, leaving the view through to the other biome.
  for(int sign=-1;sign<=1;sign+=2)for(int i=0;i<4;i++)timber.Box(new Vector3(sign*(1.25f+i*.18f),1.16f,-.27f),new Vector3(.172f,2.21f,.075f),C(i%2==0?"766C58":"877B64"),.009f);
  foreach(int sign in new[]{-1,1}){timber.Box(new Vector3(sign*1.53f,.5f,-.325f),new Vector3(.74f,.11f,.07f),C("554E40"));timber.Box(new Vector3(sign*1.53f,1.80f,-.325f),new Vector3(.74f,.11f,.07f),C("554E40"));}
  // Timber eaves under the terracotta canal tiles.
  foreach(int side in new[]{-1,1})timber.Box(new Vector3(side*3.6f,3.45f,2.1f),new Vector3(.14f,.18f,4.95f),C("71634D"));
  var tiles=new Buffer();
  // Barrel tiles: six curved segments, staggered rows, no repeated flat slabs.
  foreach(int side in new[]{-1,1})for(int column=0;column<23;column++)for(int row=0;row<10;row++){
   float z=-.35f+column*.226f;float x0=side*(row*.392f-.04f);float x1=side*((row+1)*.392f+.07f);Color col=C(new[]{"AA785D","B3886A","956D58","B18A70","A57D64","BC9377"}[rng.Next(6)]);
   Func<float,float,float,Vector3> pt=(xx,zz,angle)=>new Vector3(xx,4.91f-Mathf.Abs(xx)*.385f+Mathf.Sin(angle)*.08f,zz+Mathf.Cos(angle)*.105f);
   for(int j=0;j<6;j++){float a=j*Mathf.PI/6,beta=(j+1)*Mathf.PI/6;var p0=pt(x0,z,a);var p1=pt(x1,z,a);var p2=pt(x1,z,beta);var p3=pt(x0,z,beta);if(side<0)tiles.Quad(p0,p3,p2,p1,col);else tiles.Quad(p0,p1,p2,p3,col);}
  }
  // Low chimney, irregular cap, and a worn step below the opening.
  b.Box(new Vector3(2.18f,4.45f,3.25f),new Vector3(.62f,1.18f,.67f),C("A6A697"),.035f);b.Box(new Vector3(2.18f,5.09f,3.25f),new Vector3(.82f,.15f,.85f),C("8E9287"),.02f);
  b.Box(new Vector3(0,.015f,-.53f),new Vector3(2.14f,.07f,.94f),C("B6B09D"),.01f);
  MeshGo("Maison_Maconnerie_"+suffix,b.Mesh(),stone,t);MeshGo("Maison_Joints_"+suffix,joints.Mesh(),mortar,t);MeshGo("Maison_Bois_"+suffix,timber.Mesh(),housewood,t);MeshGo("Maison_Volets_"+suffix,painted.Mesh(),shutters,t);MeshGo("Maison_Fenetres_"+suffix,dark.Mesh(),detail,t);MeshGo("Maison_Tuiles_Canal_"+suffix,tiles.Mesh(),roof,t);
  BoxColliderAt(t,"Mur_Gauche",new Vector3(-2.37f,1.7f,.16f),new Vector3(2.23f,3.4f,.45f));BoxColliderAt(t,"Mur_Droit",new Vector3(2.37f,1.7f,.16f),new Vector3(2.23f,3.4f,.45f));BoxColliderAt(t,"Linteau",new Vector3(0,3.35f,.16f),new Vector3(2.3f,.60f,.45f));
  foreach(int side in new[]{-1,1})BoxColliderAt(t,"Mur_Lateral_"+side,new Vector3(side*3.5f,1.7f,2.3f),new Vector3(.4f,3.4f,4.4f));
 }
 static void Rock(string n,Vector2 p,float size,float vertical){
  var t=Group(n,coast);t.position=new Vector3(p.x,CoastalRouteDefinition.Height(p.x,p.y)-size*.18f,p.y);t.rotation=Quaternion.Euler(R(-15,15),R(0,360),R(-15,15));t.localScale=new Vector3(size,vertical,size*R(.65f,1.3f));
  var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/Granit_Erode_"+rng.Next(5)+".fbx");var imported=(GameObject)PrefabUtility.InstantiatePrefab(asset);imported.transform.SetParent(t,false);foreach(var rr in imported.GetComponentsInChildren<Renderer>())rr.sharedMaterial=stone;
 }
 static void BuildCoast(){
  // A raised, irregular granite coast. Path follows the ledge, with the sea below on the right.
  var v=new List<Vector3>();var c=new List<Color>();var triangles=new List<int>();int nx=136,nz=176;float step=.5f;
  for(int j=0;j<=nz;j++)for(int i=0;i<=nx;i++){
   float x=185+i*step,z=-30+j*step,y=CoastalRouteDefinition.Height(x,z);v.Add(new Vector3(x,y,z));float d=CoastalRouteDefinition.Distance(x,z);float path=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.78f,1.18f,d));float noise=Mathf.PerlinNoise(x*.5f,z*.5f);float stoneBlend=Mathf.SmoothStep(0,1,Mathf.InverseLerp(7,2.0f,CoastalRouteDefinition.Shore(z)-x));
   Color ground=Color.Lerp(C("70804D"),C("9A9E70"),noise);ground=Color.Lerp(ground,Color.Lerp(C("969D91"),C("B3AB95"),noise),stoneBlend);c.Add(Color.Lerp(ground,Color.Lerp(C("B5A27D"),C("CCBC98"),noise),path));
  }
  for(int j=0;j<nz;j++)for(int i=0;i<nx;i++){int a=j*(nx+1)+i,b=a+1,d=a+nx+1,e=d+1;triangles.AddRange(new[]{a,d,b,b,d,e});}
  var mesh=new Mesh();mesh.indexFormat=IndexFormat.UInt32;mesh.SetVertices(v);mesh.SetColors(c);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();MeshGo("Cote_Granit_et_Sentier_Haut",mesh,earth,coast,true,false);
  var sea=new Buffer();sea.Quad(new Vector3(211,-.08f,-280),new Vector3(211,-.08f,280),new Vector3(720,-.08f,280),new Vector3(720,-.08f,-280),Color.white);MeshGo("Mer_En_Contrebas",sea.Mesh(),water,coast,false,false);
  // Shore outcrops appear in clusters, never as an evenly spaced row.
  for(int i=0;i<34;i++){float z=R(-15,49);float x=CoastalRouteDefinition.Shore(z)-R(.2f,2.8f);if(CoastalRouteDefinition.Distance(x,z)<3.3f)continue;Rock("Roc_Cotier_"+i,new Vector2(x,z),R(.75f,2.0f),R(.5f,1.3f));}
  for(int i=0;i<37;i++){float s=R(2,CoastalRouteDefinition.Length+2);var p=CoastalRouteDefinition.Side(s,-R(1.8f,5.2f));float size=R(.3f,1.1f);if(CoastalRouteDefinition.Distance(p.x,p.y)<1.15f+size*1.4f||Vector2.Distance(p,new Vector2(220,-4))<5)continue;Rock("Rochers_Maquis_Cotier_"+i,p,size,R(.2f,.7f));}
  for(int i=0;i<120;i++){
   float s=R(1.8f,CoastalRouteDefinition.Length+2);int side=i%5==0?1:-1;var p=CoastalRouteDefinition.Side(s,side*R(1.6f,6.0f));float d=CoastalRouteDefinition.Distance(p.x,p.y);if(d<1.55f||p.x>CoastalRouteDefinition.Shore(p.y)-4.7f||Vector2.Distance(p,new Vector2(220,-4))<5.1f)continue;
   bool ciste=i%3!=0;string spec=ciste?"CisteSauge":"LentisqueRampant";int variant=i%4;float scale=R(.55f,.9f);
   var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(V2+"/Prefabs/"+spec+"_"+variant+".prefab"));go.name="Maquis_Marin_"+i;go.transform.SetParent(coast,false);go.transform.position=new Vector3(p.x,CoastalRouteDefinition.Height(p.x,p.y)-.06f,p.y);go.transform.rotation=Quaternion.Euler(R(-6,6),R(0,360),R(-9,9));go.transform.localScale=new Vector3(scale,scale*R(.6f,.9f),scale);
  }
  for(int i=0;i<8;i++){float s=3+i*4;var p=CoastalRouteDefinition.Side(s,-R(6,11));if(CoastalRouteDefinition.Distance(p.x,p.y)<3.8f)continue;Tree("Pin_Cotier_Vent_"+i,true,i%2,coast,new Vector3(p.x,CoastalRouteDefinition.Height(p.x,p.y)-.1f,p.y),R(.65f,.9f),R(.8f,1),R(0,360));}
  // Pebbles, grasses and small dry tufts soften the path edge without obscuring the sea.
  var herbs=new Buffer();for(int i=0;i<390;i++){var p=CoastalRouteDefinition.Side(R(2,CoastalRouteDefinition.Length), (i%2==0?-1:1)*R(1.15f,2.4f));if(p.x>CoastalRouteDefinition.Shore(p.y)-4)continue;Vector3 baseP=new Vector3(p.x,CoastalRouteDefinition.Height(p.x,p.y),p.y);for(int j=0;j<4;j++){float a=R(0,6.28f),h=R(.10f,.29f);var dir=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));var cross=Vector3.Cross(dir,Vector3.up)*.014f;herbs.Quad(baseP-cross,baseP+Vector3.up*h+dir*.1f,baseP+cross,baseP-cross,C(i%3==0?"A7A06B":"86905D"));}}
  MeshGo("Herbes_Seches_Cotieres",herbs.Mesh(),detail,coast,false,false);
 }
 public static string Build(){
  var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(Application.isPlaying||scene.path!="Assets/Scenes/Prototype_Maquis.unity")throw new Exception("Edit mode in Prototype_Maquis required");
  rng=new System.Random(594278);var old=GameObject.Find("Sentier_Zigzag_Portail_Littoral").transform;var existing=old.Find("06_Grands_Arbres_Maisons_Cote_V4");if(existing!=null)UnityEngine.Object.DestroyImmediate(existing.gameObject);
  var root=Group("06_Grands_Arbres_Maisons_Cote_V4",old);forest=Group("01_Grands_Arbres",root);houses=Group("02_Maisons_Corses",root);coast=Group("03_Sentier_Littoral",root);old.Find("04_Arrivee_Littoral").gameObject.SetActive(false);
  stone=Mat("Granit_Maison",V3+"/Materials/Granit_Arches.mat");stone.SetFloat("_ColorsAreSRGB",0);wood=Mat("Bois_Ancien",V2+"/Materials/Ecorces_Chaudes.mat");leaves=Mat("Feuilles_Grands_Arbres",V1+"/Materials/Feuillages_Mats.mat");
  housewood=Mat("Bois_Maison",V2+"/Materials/Ecorces_Chaudes.mat");housewood.SetFloat("_ColorsAreSRGB",0);earth=Mat("Terre_Cote",V3+"/Materials/Sable_Littoral.mat");roof=Mat("Tuiles_Vieillies",V3+"/Materials/Sable_Littoral.mat");roof.SetFloat("_Cull",0);mortar=Mat("Joints_Recesses",V3+"/Materials/Sable_Littoral.mat");shutters=Mat("Volets_Patines",V3+"/Materials/Sable_Littoral.mat");detail=Mat("Details_Mats",V3+"/Materials/Sable_Littoral.mat");detail.SetFloat("_Cull",0);water=Mat("Mer_Cotiere",V3+"/Materials/Mer_Littoral.mat");water.SetFloat("_ShoreX",231);
  earth.shader=Shader.Find("CorseVR/Coastal Painted");earth.SetFloat("_Mode",5);water.shader=Shader.Find("CorseVR/Coastal Water V4");water.SetFloat("_ShoreX",230);
  int trees=BuildForest(old);BuildCoast();
  var portalRoot=old.Find("03_Portails");var gate=portalRoot.Find("Portail_Vers_Littoral").GetComponent<BiomePortal>();var back=portalRoot.Find("Portail_Retour_Maquis").GetComponent<BiomePortal>();
  foreach(var p in new[]{gate,back})foreach(Transform t in p.transform)if(t.name.EndsWith("_Arche_Granit"))t.gameObject.SetActive(false);
  var p0=CoastalRouteDefinition.Point(0);back.transform.position=new Vector3(p0.x,CoastalRouteDefinition.Height(p0.x,p0.y),p0.y);back.transform.rotation=Quaternion.LookRotation(-CoastalRouteDefinition.Forward(0));
  House(gate,"Maquis");House(back,"Littoral");
  var arrival=portalRoot.Find("Arrivee_Depuis_Maquis");var p1=CoastalRouteDefinition.Point(2.1f);arrival.position=new Vector3(p1.x,CoastalRouteDefinition.Height(p1.x,p1.y)+.035f,p1.y);arrival.rotation=Quaternion.LookRotation(CoastalRouteDefinition.Forward(2.1f));
  var player=GameObject.Find("Joueur_Desktop");gate.Configure(player.transform,arrival,back,CoastalRouteDefinition.Corridor(),.78f,"Littoral");back.Configure(player.transform,portalRoot.Find("Arrivee_Retour_Maquis"),gate,MaquisRouteDefinition.Corridor(),.76f,"Maquis");
  var pc=portalRoot.Find("Fenetre_Littoral_Camera").GetComponent<Camera>();pc.transform.position=arrival.position+Vector3.up*1.65f;pc.transform.rotation=Quaternion.LookRotation(Quaternion.Euler(0,28,0)*CoastalRouteDefinition.Forward(2.1f));
  var cc=player.GetComponent<CharacterController>();cc.enabled=false;var start=MaquisRouteDefinition.Point(0);player.transform.position=new Vector3(start.x,MaquisZigzagBuilder.Height(start.x,start.y)+.035f,start.y);player.transform.rotation=Quaternion.LookRotation(MaquisRouteDefinition.Forward(0));player.GetComponent<SentierCorridor>().Configure(MaquisRouteDefinition.Corridor(),.76f);cc.enabled=true;
  var cam=player.GetComponentInChildren<Camera>();cam.transform.localPosition=new Vector3(0,1.65f,0);cam.transform.localRotation=Quaternion.identity;
  Physics.SyncTransforms();AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);foreach(var preview in portalRoot.GetComponentsInChildren<Camera>())if(preview.targetTexture!=null)preview.Render();
  return "Saved V4: "+trees+" large forest trees, 2 old stone houses, raised coastal path "+CoastalRouteDefinition.Length.ToString("F1")+" m, two-way passage retained.";
 }
 public static string Capture(string directory){
  Directory.CreateDirectory(directory);foreach(var camera in GameObject.Find("Sentier_Zigzag_Portail_Littoral").GetComponentsInChildren<Camera>())if(camera.targetTexture!=null)camera.Render();
  var cam=Camera.main;var p=cam.transform.position;var rot=cam.transform.rotation;float aspect=cam.aspect,fov=cam.fieldOfView;var target=cam.targetTexture;var active=RenderTexture.active;
  string[] names={"01_Depart_Grands_Arbres","02_Sentier_Arbres","03_Fin_Foret","04_Maison_Corse","05_Arrivee_Cotiere","06_Sentier_Littoral","07_Cote_et_Mer","08_Retour_Maison"};
  float[] s={0,10,25,MaquisRouteDefinition.Length-5.8f,2.2f,14,CoastalRouteDefinition.Length-2,3.8f};
  try{for(int i=0;i<names.Length;i++){
   bool littoral=i>=4;var pt=littoral?CoastalRouteDefinition.Point(s[i]):MaquisRouteDefinition.Point(s[i]);var fw=littoral?CoastalRouteDefinition.Forward(s[i]):MaquisRouteDefinition.Forward(s[i]);
   float y=littoral?CoastalRouteDefinition.Height(pt.x,pt.y):MaquisZigzagBuilder.Height(pt.x,pt.y);cam.transform.position=new Vector3(pt.x,y+1.65f,pt.y);
   if(i==3){var gate=GameObject.Find("Portail_Vers_Littoral").transform;fw=(gate.position+Vector3.up*2.1f-cam.transform.position).normalized;}
   if(i==4||i==6)fw=Quaternion.Euler(0,35,0)*fw;if(i==7)fw=-fw;
   cam.transform.rotation=Quaternion.LookRotation(fw);cam.fieldOfView=65;cam.aspect=1920f/1080;
   var rt=RenderTexture.GetTemporary(1920,1080,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB,4);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1920,1080,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1920,1080),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(directory,names[i]+".png"),tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);cam.targetTexture=null;RenderTexture.active=null;RenderTexture.ReleaseTemporary(rt);
  }}finally{cam.transform.position=p;cam.transform.rotation=rot;cam.fieldOfView=fov;cam.aspect=aspect;cam.targetTexture=target;RenderTexture.active=active;}return "8 captures at 1920x1080";
 }
}
}
