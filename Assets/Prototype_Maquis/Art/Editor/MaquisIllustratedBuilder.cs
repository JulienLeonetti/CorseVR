using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace CorseVR.Editor
{
    // All generation happens in Edit mode. Runtime uses saved meshes and shared materials.
    public static class MaquisIllustratedBuilder
    {
        const string Root="Assets/Prototype_Maquis/Art/Illustrated";
        static System.Random rng;
        static float R(float a,float b){return a+(float)rng.NextDouble()*(b-a);}
        static Color S(string hex){Color c;ColorUtility.TryParseHtmlString("#"+hex,out c);return c;}
        static Color P(string hex){return S(hex).linear;}
        static readonly Vector2 Nepita=new Vector2(-4.9f,-4.6f), Rencontre=new Vector2(3.8f,1f);
        public static float PathX(float z)
        {
            float[] zs={-12,-7,-2,3,8,12};float[] xs={-3,-3,-1,1,2,2};
            if(z<=zs[0])return xs[0];
            for(int i=0;i<zs.Length-1;i++)if(z<=zs[i+1])return Mathf.Lerp(xs[i],xs[i+1],Mathf.SmoothStep(0,1,Mathf.InverseLerp(zs[i],zs[i+1],z)));
            return 2;
        }
        static float HalfWidth(float z){return 0.98f+0.12f*Mathf.Sin(z*0.6f)+0.30f*Mathf.Exp(-(z+5)*(z+5)/5)+0.33f*Mathf.Exp(-(z-1)*(z-1)/7);}
        static float SegmentDistance(Vector2 p,Vector2 a,Vector2 b){Vector2 ab=b-a;return Vector2.Distance(p,a+ab*Mathf.Clamp01(Vector2.Dot(p-a,ab)/ab.sqrMagnitude));}
        static bool Reserved(float x,float z,float margin)
        {
            Vector2 p=new Vector2(x,z);
            return Vector2.Distance(p,Nepita)<1.15f+margin||Vector2.Distance(p,Rencontre)<1.65f+margin
                ||SegmentDistance(p,Nepita,new Vector2(PathX(Nepita.y),Nepita.y))<0.65f+margin
                ||SegmentDistance(p,Rencontre,new Vector2(PathX(Rencontre.y),Rencontre.y))<0.8f+margin;
        }
        public static float Height(float x,float z)
        {
            float bank=Mathf.SmoothStep(0,1,Mathf.InverseLerp(HalfWidth(z),5,Mathf.Abs(x-PathX(z))));
            float h=0.017f*(z+11)+0.025f*Mathf.Sin((z+11)*0.37f);
            h+=bank*(0.36f+0.42f*Mathf.PerlinNoise(x*0.19f+41,z*0.16f+9)+0.14f*Mathf.Sin(x*0.4f+z*0.3f));
            float far=Mathf.SmoothStep(0,1,Mathf.InverseLerp(17,48,Mathf.Max(Mathf.Abs(x),Mathf.Abs(z))));
            h+=far*(2.2f+7.5f*Mathf.PerlinNoise(x*0.044f+73,z*0.044f+17));
            return h;
        }
        sealed class Geo
        {
            public readonly List<Vector3> v=new List<Vector3>(),n=new List<Vector3>();
            public readonly List<Color> c=new List<Color>();public readonly List<int> t=new List<int>();
            public void Trim(int vertices,int triangles){v.RemoveRange(vertices,v.Count-vertices);n.RemoveRange(vertices,n.Count-vertices);c.RemoveRange(vertices,c.Count-vertices);t.RemoveRange(triangles,t.Count-triangles);}
            public void Tri(Vector3 a,Vector3 b,Vector3 d,Color color)
            {int k=v.Count;var normal=Vector3.Cross(b-a,d-a).normalized;v.Add(a);v.Add(b);v.Add(d);n.Add(normal);n.Add(normal);n.Add(normal);c.Add(color);c.Add(color);c.Add(color);t.Add(k);t.Add(k+1);t.Add(k+2);}
            public void Lobe(Vector3 center,Vector3 scale,Quaternion rotation,Color color,int lon=9,int lat=5,float seed=0)
            {
                int start=v.Count;
                for(int y=0;y<=lat;y++)for(int x=0;x<=lon;x++){
                    float a=x/(float)lon*Mathf.PI*2,b=y/(float)lat*Mathf.PI;
                    Vector3 dir=new Vector3(Mathf.Sin(b)*Mathf.Cos(a),Mathf.Cos(b),Mathf.Sin(b)*Mathf.Sin(a));
                    float wobble=1+0.07f*Mathf.Sin(a*3+seed)*Mathf.Sin(b)+0.04f*Mathf.Sin(b*4+seed);
                    v.Add(center+rotation*Vector3.Scale(dir,scale)*wobble);
                    n.Add((rotation*new Vector3(dir.x/scale.x,dir.y/scale.y,dir.z/scale.z)).normalized);
                    float paint=0.78f+0.25f*(dir.y*0.5f+0.5f)+0.055f*Mathf.Sin(a*2+seed+b*4);
                    c.Add(color*paint);
                }
                for(int y=0;y<lat;y++)for(int x=0;x<lon;x++){
                    int a=start+y*(lon+1)+x,b=a+1,d=a+lon+1,e=d+1;
                    t.Add(a);t.Add(b);t.Add(d);t.Add(b);t.Add(e);t.Add(d);
                }
            }
            public void Stem(Vector3 a,Vector3 b,float radius,Color color,float bend=0.15f,int segments=3,int sides=6)
            {
                int k=v.Count;Vector3 axis=(b-a).normalized;
                Vector3 right=Vector3.Cross(axis,Vector3.forward).normalized;if(right.sqrMagnitude<0.1f)right=Vector3.right;
                Vector3 up=Vector3.Cross(axis,right);
                for(int j=0;j<=segments;j++){
                    float f=j/(float)segments;Vector3 center=Vector3.Lerp(a,b,f)+right*Mathf.Sin(f*Mathf.PI)*bend;
                    float r=radius*Mathf.Lerp(1,0.3f,f);
                    for(int i=0;i<=sides;i++){
                        float angle=i/(float)sides*Mathf.PI*2;Vector3 normal=right*Mathf.Cos(angle)+up*Mathf.Sin(angle);
                        v.Add(center+normal*r);n.Add(normal);c.Add(color*Mathf.Lerp(0.78f,1.1f,f));
                    }
                }
                for(int j=0;j<segments;j++)for(int i=0;i<sides;i++){
                    int a1=k+j*(sides+1)+i,b1=a1+1,d=a1+sides+1,e=d+1;
                    t.Add(a1);t.Add(b1);t.Add(d);t.Add(b1);t.Add(e);t.Add(d);
                }
            }
            public void Leaf(Vector3 p,Vector3 axis,float length,float width,Color color)
            {
                axis.Normalize();Vector3 side=Vector3.Cross(axis,Vector3.up).normalized;
                if(side.sqrMagnitude<0.01f)side=Vector3.right;
                Vector3 a=p-axis*length*0.5f,b=p-axis*length*0.25f+side*width,d=p+axis*length*0.25f+side*width,
                    e=p+axis*length*0.5f,f=p+axis*length*0.25f-side*width,g=p-axis*length*0.25f-side*width;
                Tri(a,b,g,color*0.9f);Tri(b,d,g,color);Tri(d,f,g,color*1.05f);Tri(d,e,f,color);
            }
            public void Flower(Vector3 p,float size,Color petals)
            {
                for(int i=0;i<5;i++){
                    float a=i*Mathf.PI*2/5;Vector3 axis=new Vector3(Mathf.Cos(a),R(-0.2f,0.4f),Mathf.Sin(a));
                    Leaf(p+axis*size*0.40f,axis,size*0.88f,size*0.30f,petals);
                }
                Lobe(p+Vector3.up*0.016f,new Vector3(size*0.26f,size*0.14f,size*0.26f),Quaternion.identity,P("DBAE44"),6,3);
            }
            public void Append(Mesh mesh,Matrix4x4 matrix)
            {int k=v.Count;var vs=mesh.vertices;var ns=mesh.normals;var cs=mesh.colors;var normalMatrix=matrix.inverse.transpose;
                for(int i=0;i<vs.Length;i++){v.Add(matrix.MultiplyPoint3x4(vs[i]));n.Add(normalMatrix.MultiplyVector(ns[i]).normalized);c.Add(cs.Length==vs.Length?cs[i]:Color.white);}foreach(int j in mesh.triangles)t.Add(k+j);}
            public Mesh Mesh(string name)
            {var m=new Mesh{name=name};if(v.Count>65535)m.indexFormat=IndexFormat.UInt32;m.SetVertices(v);m.SetNormals(n);m.SetColors(c);m.SetTriangles(t,0);m.RecalculateBounds();return m;}
        }
        static T Save<T>(T value,string path) where T:UnityEngine.Object
        {
            value.name=Path.GetFileNameWithoutExtension(path);
            var old=AssetDatabase.LoadAssetAtPath<T>(path);if(old==null){AssetDatabase.CreateAsset(value,path);return value;}
            var mesh=old as Mesh;var source=value as Mesh;
            if(mesh!=null&&source!=null){mesh.Clear();mesh.indexFormat=source.indexFormat;mesh.vertices=source.vertices;mesh.normals=source.normals;mesh.colors=source.colors;mesh.triangles=source.triangles;mesh.bounds=source.bounds;mesh.UploadMeshData(false);}
            else EditorUtility.CopySerialized(value,old);
            EditorUtility.SetDirty(old);UnityEngine.Object.DestroyImmediate(value);return old;
        }
        static GameObject MeshObject(string name,Mesh mesh,Material material,Transform parent,bool collision=false,bool shadows=true)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=shadows?ShadowCastingMode.On:ShadowCastingMode.Off;
            renderer.lightProbeUsage=LightProbeUsage.Off;renderer.reflectionProbeUsage=ReflectionProbeUsage.Off;
            if(collision)go.AddComponent<MeshCollider>().sharedMesh=mesh;go.isStatic=true;return go;
        }
        static Mesh Ground(bool apron)
        {
            int count=apron?44:72;float extent=apron?55:15;var geo=new Geo();
            for(int j=0;j<=count;j++)for(int i=0;i<=count;i++){
                float x=Mathf.Lerp(-extent,extent,i/(float)count),z=Mathf.Lerp(-extent,extent,j/(float)count);
                float surface=Height(x,z);
                // Match the coarse apron edge exactly; otherwise cracks show when looking behind.
                if(!apron&&(i==0||i==count)){
                    float low=Mathf.Floor((z+55)/2.5f)*2.5f-55;
                    surface=Mathf.Lerp(Height(x,low),Height(x,low+2.5f),(z-low)/2.5f);
                }else if(!apron&&(j==0||j==count)){
                    float low=Mathf.Floor((x+55)/2.5f)*2.5f-55;
                    surface=Mathf.Lerp(Height(low,z),Height(low+2.5f,z),(x-low)/2.5f);
                }
                geo.v.Add(new Vector3(x,surface,z));
                float epsilon=0.05f;geo.n.Add(new Vector3(Height(x-epsilon,z)-Height(x+epsilon,z),epsilon*2,Height(x,z-epsilon)-Height(x,z+epsilon)).normalized);
                float d=Mathf.Abs(x-PathX(z))+(Mathf.PerlinNoise(x*1.5f+30,z*1.5f+50)-0.5f)*0.22f;
                float w=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(HalfWidth(z)-0.28f,HalfWidth(z)+0.32f,d));
                if(z>13)w*=1-Mathf.InverseLerp(13,18,z);if(z<-13)w*=1-Mathf.InverseLerp(13,18,-z);
                var p=new Vector2(x,z);
                foreach(var slot in new[]{Nepita,Rencontre}){
                    float radius=slot==Nepita?1.15f:1.65f;
                    float clearing=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(radius-0.3f,radius+0.3f,Vector2.Distance(p,slot)));
                    float approach=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(0.5f,0.85f,SegmentDistance(p,slot,new Vector2(PathX(slot.y),slot.y))));
                    w=Mathf.Max(w,Mathf.Max(clearing,approach)*0.90f);
                }
                geo.c.Add(new Color(w,0,0,1));
            }
            for(int j=0;j<count;j++)for(int i=0;i<count;i++){
                float x=Mathf.Lerp(-extent,extent,(i+0.5f)/count),z=Mathf.Lerp(-extent,extent,(j+0.5f)/count);
                if(apron&&Mathf.Abs(x)<15&&Mathf.Abs(z)<15)continue;
                int a=j*(count+1)+i,b=a+1,d=a+count+1,e=d+1;
                geo.t.Add(a);geo.t.Add(d);geo.t.Add(b);geo.t.Add(b);geo.t.Add(d);geo.t.Add(e);
            }
            return geo.Mesh(apron?"Horizon_Peint":"Relief_Illustre_30m");
        }
        static Mesh Boulder(int index,bool pebble=false)
        {
            var geo=new Geo();Color color=P(index%3==0?"B4A283":index%3==1?"AEA68D":"C0A380");
            geo.Lobe(Vector3.zero,new Vector3(0.49f,0.45f,0.50f),Quaternion.Euler(0,index*17,0),color,pebble?8:14,pebble?4:8,index*4.9f);
            for(int i=0;i<geo.v.Count;i++){
                Vector3 v=geo.v[i];v.x+=0.12f*v.y;v.z+=0.08f*Mathf.Sin(v.y*7+index);
                v.y=Mathf.Max(-0.30f,v.y);geo.v[i]=v;
                geo.c[i]*=0.94f+0.06f*Mathf.Sin(v.y*19+v.x*7+index);
            }
            return geo.Mesh("Pierre_Douce_"+index);
        }
        static void Shrub(Geo geo,Vector3 p,float size,int species)
        {
            int firstVertex=geo.v.Count,firstTriangle=geo.t.Count;
            float yaw=R(0,360);Color leaf=P(species==0?"647D43":species==1?"93A36A":"677D58");
            leaf=Color.Lerp(leaf,P(species==0?"48623B":"829C67"),R(0,0.35f));Color wood=P("796444");
            int lobes=species==2?6:7;
            for(int i=0;i<lobes;i++){
                float a=(yaw+i*137.5f)*Mathf.Deg2Rad;Vector3 radial=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                float spread=R(0.25f,0.50f)*size;
                Vector3 center=p+radial*spread+Vector3.up*R(0.55f,species==2?1.15f:0.88f)*size;
                geo.Stem(p,center,0.022f*size,wood,0.12f*size,2,5);
                var scale=species==2?new Vector3(0.26f,0.31f,0.25f)*size:new Vector3(R(0.35f,0.46f),R(0.27f,0.39f),R(0.34f,0.45f))*size;
                if(species==2){
                    geo.Lobe(center-Vector3.up*0.14f*size,scale,Quaternion.Euler(R(-20,20),R(0,360),R(-20,20)),leaf*R(0.88f,1.10f),7,4,R(0,12));
                    geo.Lobe(center+Vector3.up*0.17f*size+radial*0.08f*size,scale*0.88f,Quaternion.Euler(R(-20,20),R(0,360),R(-20,20)),leaf*R(0.88f,1.10f),7,4,R(0,12));
                }else geo.Lobe(center,scale,Quaternion.Euler(R(-12,12),R(0,360),R(-14,14)),leaf*R(0.88f,1.10f),9,5,R(0,12));
                // Retain readable leaf characters at the silhouette, beyond the broad painted masses.
                int leaves=species==2?4:6;
                for(int j=0;j<leaves;j++){
                    Vector3 axis=Quaternion.Euler(R(-25,30),R(0,360),0)*Vector3.forward;
                    Vector3 loc=center+Vector3.Scale(axis,scale)*0.95f+Vector3.up*R(0.05f,0.18f)*size;
                    geo.Leaf(loc,axis,(species==1?0.23f:0.17f)*size,(species==2?0.012f:0.045f)*size,leaf*R(1,1.25f));
                }
                if(species==1&&i%2==0)geo.Flower(center+Vector3.up*scale.y+radial*0.06f,size*0.10f,P("F5E7CC"));
                if(species==2&&i%2==0)for(int q=0;q<3;q++)geo.Lobe(center+Vector3.up*R(0.20f,0.39f)*size+radial*R(0.12f,0.25f)*size,new Vector3(0.025f,0.045f,0.025f)*size,Quaternion.identity,P("E9D5C5"),5,3);
            }
            if(MaquisFoliageRefinement.ShouldRefine(p)){geo.Trim(firstVertex,firstTriangle);MaquisFoliageRefinement.RecordShrub(p,size,species);}
        }
        static void Grass(Geo geo,Vector3 p,float size)
        {
            for(int i=0;i<6;i++){
                Vector3 dir=Quaternion.Euler(0,R(0,360),0)*Vector3.forward;Vector3 side=Vector3.Cross(dir,Vector3.up);
                float height=R(0.25f,0.52f)*size,width=R(0.017f,0.026f)*size;Color color=P(i%3==0?"9AAB70":"C3B478")*R(0.9f,1.05f);
                Vector3 a=p+dir*R(0,0.05f),b=a+Vector3.up*height*0.55f+dir*height*0.22f,c=a+Vector3.up*height+dir*height*0.58f;
                geo.Tri(a-side*width,a+side*width,b+side*width*0.7f,color*0.8f);geo.Tri(a-side*width,b+side*width*0.7f,b-side*width*0.7f,color);
                geo.Tri(b-side*width*0.7f,b+side*width*0.7f,c,color*1.08f);
            }
        }
        static void Tree(Geo geo,Vector3 p,float size,int index)
        {
            int firstVertex=geo.v.Count,firstTriangle=geo.t.Count;
            Color bark=P("96744F"),leaves=P(index%2==0?"647B4A":"778A50");
            var bend=new Vector3(R(-0.3f,0.3f),2.4f,R(-0.2f,0.2f))*size;
            geo.Stem(p,p+bend,0.13f*size,bark,0.22f*size,4,7);
            for(int i=0;i<6;i++){
                float a=(i*137.5f+index*47)*Mathf.Deg2Rad;Vector3 spread=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                Vector3 end=p+bend+spread*R(0.65f,1.1f)*size+Vector3.up*R(0.1f,0.65f)*size;
                geo.Stem(p+bend*0.62f,end,0.06f*size,bark,0.13f*size,4,7);
                geo.Lobe(end,new Vector3(R(0.90f,1.10f),R(0.65f,0.85f),R(0.83f,1.07f))*size,Quaternion.Euler(0,R(0,360),R(-10,10)),leaves*R(0.9f,1.1f),11,6,R(0,20));
            }
            if(MaquisFoliageRefinement.ShouldRefine(p)){geo.Trim(firstVertex,firstTriangle);MaquisFoliageRefinement.RecordTree(p,size,index);}
        }
        static Geo Chunk(Dictionary<string,Geo> chunks,Vector3 p){string key=Mathf.FloorToInt((p.x+18)/6)+"_"+Mathf.FloorToInt((p.z+20)/6);if(!chunks.ContainsKey(key))chunks[key]=new Geo();return chunks[key];}
        static Transform Marker(Transform parent,string name,Vector2 point)
        {var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.position=new Vector3(point.x,Height(point.x,point.y),point.y);return go.transform;}
        [MenuItem("CorseVR/Build illustrated maquis reference")]
        static void BuildMenu(){Debug.Log(Build());}
        public static string Build()
        {
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.path!="Assets/Scenes/Prototype_Maquis.unity"||Application.isPlaying)throw new InvalidOperationException("Open Prototype_Maquis in Edit mode.");
            Directory.CreateDirectory(Root+"/Meshes");Directory.CreateDirectory(Root+"/Materials");AssetDatabase.Refresh();rng=new System.Random(72343);MaquisFoliageRefinement.Reset();
            var paletteShader=Shader.Find("CorseVR/Illustrated Matte");var groundShader=Shader.Find("CorseVR/Illustrated Ground");var skyShader=Shader.Find("CorseVR/Illustrated Sky");
            if(paletteShader==null||groundShader==null||skyShader==null)throw new InvalidOperationException("Missing illustrated shaders");
            var palette=Save(new Material(paletteShader){name="Palette_Maquis_Illustre"},Root+"/Materials/Palette_Maquis.mat");
            palette.SetColor("_ShadowTint",new Color(0.56f,0.69f,0.65f));
            var stonePalette=Save(new Material(paletteShader){name="Palette_Pierres_Chaudes"},Root+"/Materials/Palette_Pierres.mat");
            stonePalette.SetColor("_ShadowTint",new Color(0.64f,0.59f,0.55f));
            var earth=Save(new Material(groundShader){name="Sol_Ocre_Sauge"},Root+"/Materials/Sol_Ocre_Sauge.mat");
            earth.SetColor("_Earth",P("AE9870"));earth.SetColor("_Sage",P("82955F"));earth.SetColor("_Path",P("E1C08C"));
            var sky=Save(new Material(skyShader){name="Ciel_Bleu_Peint"},Root+"/Materials/Ciel_Bleu_Peint.mat");
            sky.SetColor("_Zenith",P("6CB7DE"));sky.SetColor("_Horizon",P("DEE7DF"));sky.SetColor("_Cloud",P("FFF8EC"));
            RenderSettings.skybox=sky;RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=S("B5C8BB");RenderSettings.ambientEquatorColor=S("9BA88D");RenderSettings.ambientGroundColor=S("77714F");
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogColor=S("B6CAD1");RenderSettings.fogStartDistance=23;RenderSettings.fogEndDistance=88;
            var sun=GameObject.Find("Directional Light").GetComponent<Light>();sun.transform.rotation=Quaternion.Euler(39,128,0);sun.color=S("FFF9E9");sun.intensity=1.14f;sun.shadows=LightShadows.Soft;sun.shadowStrength=0.70f;sun.shadowBias=0.035f;sun.shadowNormalBias=0.20f;RenderSettings.sun=sun;
            var environment=GameObject.Find("Maquis_Environment").transform;
            var previous=environment.Find("Maquis_Depart_Detail");if(previous!=null)UnityEngine.Object.DestroyImmediate(previous.gameObject);
            var old=environment.Find("Maquis_Illustre_Reference");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
            var root=new GameObject("Maquis_Illustre_Reference").transform;root.SetParent(environment,false);
            var ground=environment.Find("Sol_30x30m").gameObject;var groundMesh=Save(Ground(false),Root+"/Meshes/Relief_30m.asset");
            ground.GetComponent<MeshFilter>().sharedMesh=groundMesh;ground.GetComponent<MeshRenderer>().sharedMaterial=earth;
            var groundCollider=ground.GetComponent<MeshCollider>();groundCollider.sharedMesh=null;groundCollider.sharedMesh=groundMesh;
            MeshObject("Collines_Enveloppe",Save(Ground(true),Root+"/Meshes/Horizon.asset"),earth,root,true,false);
            var rocks=environment.Find("Rochers");var rockMeshes=new Mesh[6];for(int i=0;i<6;i++)rockMeshes[i]=Save(Boulder(i),Root+"/Meshes/Pierre_"+i+".asset");
            for(int i=0;i<rocks.childCount;i++){
                var go=rocks.GetChild(i).gameObject;var p=go.transform.position;
                if(i==3){p.x=6.6f;p.z=-0.4f;} // Leave the future conversation clearing fully accessible.
                p.y=Height(p.x,p.z)+0.30f;go.transform.position=p;
                go.GetComponent<MeshFilter>().sharedMesh=rockMeshes[i%6];go.GetComponent<MeshRenderer>().sharedMaterial=stonePalette;
                var mc=go.GetComponent<MeshCollider>();mc.sharedMesh=null;mc.sharedMesh=rockMeshes[i%6];
            }
            var chunks=new Dictionary<string,Geo>();var points=new List<Vector3>();int bushes=0,grass=0,flowers=0;
            float[,] groups={{-6.8f,-12.8f,1.8f,7},{0.6f,-12.5f,2,7},{-6.6f,-7,2,8},{1.2f,-6,2,7},{-5.8f,-1,1.9f,7},{5,-1.6f,2.2f,8},{-3.2f,5,2,7},{5.2f,6,2,7},{-7.2f,-18,2.7f,6},{0.5f,-18,2.7f,6},{-10,-4,2.5f,6},{9,-5,2.5f,6},{-8,6,2.5f,6},{10,6,2.8f,5},{1,13,1.8f,6},{-3,11,2,6}};
            for(int g=0;g<groups.GetLength(0);g++){
                int accepted=0;for(int attempt=0;attempt<90&&accepted<(int)groups[g,3];attempt++){
                    float a=R(0,Mathf.PI*2),r=Mathf.Sqrt(R(0,1))*groups[g,2],x=groups[g,0]+Mathf.Cos(a)*r,z=groups[g,1]+Mathf.Sin(a)*r;
                    float size=R(0.90f,1.60f);if(Mathf.Abs(x-PathX(z))<HalfWidth(z)+0.55f*size+0.24f||Reserved(x,z,size*0.4f))continue;
                    var p=new Vector3(x,Height(x,z)-0.08f,z);bool blocked=false;
                    foreach(var other in points)if((p-other).sqrMagnitude<0.65f*size*size){blocked=true;break;}if(blocked)continue;
                    foreach(Transform rock in rocks){var delta=rock.position-p;delta.y=0;if(delta.magnitude<Mathf.Max(rock.localScale.x,rock.localScale.z)*0.46f+size*0.45f){blocked=true;break;}}if(blocked)continue;
                    Shrub(Chunk(chunks,p),p,size,(g+accepted)%3);points.Add(p);bushes++;accepted++;
                }
            }
            float[,] trees={{-5.8f,-11.8f,1.10f},{0.8f,-7.0f,1.08f},{-5.8f,-16.8f,1.10f},{2.4f,-15.5f,0.98f},{5.8f,4.7f,1.15f},{-4.7f,7.4f,1.13f}};
            for(int i=0;i<trees.GetLength(0);i++){
                var p=new Vector3(trees[i,0],Height(trees[i,0],trees[i,1]),trees[i,1]);Tree(Chunk(chunks,p),p,trees[i,2],i);
                var trunk=new GameObject("Tronc_Arbousier_"+(i+1));trunk.transform.SetParent(root,false);trunk.transform.position=p;
                var col=trunk.AddComponent<CapsuleCollider>();col.radius=0.17f;col.height=2.4f;col.center=new Vector3(0,1.2f,0);
            }
            for(int i=0;i<280;i++){
                float z=R(-19,12),x=PathX(z)+(rng.Next(2)==0?-1:1)*R(HalfWidth(z)+0.25f,5.3f);
                if(Reserved(x,z,0.2f))continue;var p=new Vector3(x,Height(x,z),z);Grass(Chunk(chunks,p),p,R(0.65f,1.35f));grass++;
            }
            for(int i=0;i<75;i++){
                float z=R(-13,9),side=rng.Next(2)==0?-1:1,x=PathX(z)+side*R(HalfWidth(z)+0.25f,HalfWidth(z)+1.3f);
                if(Reserved(x,z,0.1f))continue;var p=new Vector3(x,Height(x,z),z);var geo=Chunk(chunks,p);
                geo.Stem(p,p+Vector3.up*0.25f,0.009f,P("7B8C56"),0.05f,1,4);geo.Flower(p+Vector3.up*R(0.20f,0.37f),R(0.07f,0.10f),P(i%4==0?"DDB4AA":"F8E7C5"));flowers++;
            }
            // Quiet, simplified far masses layer the view without another detailed forest.
            for(int i=0;i<20;i++){
                float x=R(-24,26),z=R(17,30);var p=new Vector3(x,Height(x,z),z);var geo=Chunk(chunks,p);
                for(int j=0;j<3;j++)geo.Lobe(p+new Vector3(j-1,R(0.7f,1.3f),R(-0.4f,0.4f)),new Vector3(1.6f,1.2f,1.4f),Quaternion.identity,P(i%2==0?"788B65":"899576"),8,4,i+j);
            }
            foreach(var pair in chunks)MeshObject("Masses_Peintes_"+pair.Key,Save(pair.Value.Mesh("Maquis_Illustre_"+pair.Key),Root+"/Meshes/Vegetation_"+pair.Key+".asset"),palette,root,false,true);
            var stones=new Geo();var pebble=Save(Boulder(12,true),Root+"/Meshes/Galet.asset");int gravel=0;
            for(int i=0;i<110;i++){
                float z=R(-15,11),x=PathX(z)+(rng.Next(2)==0?-1:1)*R(HalfWidth(z)-0.12f,HalfWidth(z)+1.2f);
                if(Reserved(x,z,0.15f))continue;float s=R(0.09f,0.26f);var p=new Vector3(x,Height(x,z)+s*0.15f,z);
                stones.Append(pebble,Matrix4x4.TRS(p,Quaternion.Euler(R(0,90),R(0,360),R(0,60)),new Vector3(s,s*0.58f,s*R(0.8f,1.35f))));gravel++;
            }
            MeshObject("Graviers_Aquarelle",Save(stones.Mesh("Graviers_Aquarelle"),Root+"/Meshes/Graviers.asset"),stonePalette,root,false,false);
            var prep=new GameObject("Preparation_Interactions").transform;prep.SetParent(root,false);
            Marker(prep,"Emplacement_Nepita",Nepita);Marker(prep,"Emplacement_Vieille_Femme",Rencontre);
            Marker(prep,"Son_Directionnel_Gauche",new Vector2(-8,-3));Marker(prep,"Son_Directionnel_Droit",new Vector2(8,6));
            string refined=MaquisFoliageRefinement.Build(root);
            var camera=Camera.main;camera.clearFlags=CameraClearFlags.Skybox;camera.farClipPlane=110;
            var data=camera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();if(data!=null)data.renderPostProcessing=false;
            EditorUtility.SetDirty(earth);EditorUtility.SetDirty(palette);EditorUtility.SetDirty(stonePalette);EditorUtility.SetDirty(sky);AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            return bushes+" shrubs, 6 arbutus-like trees, "+grass+" grass clumps, "+flowers+" flower stems, "+gravel+" stones, "+chunks.Count+" spatial chunks; 2 accessible reserved clearings. "+refined;
        }
    }
}
