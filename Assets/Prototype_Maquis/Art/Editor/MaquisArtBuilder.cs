using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace CorseVR.Editor
{
    // Editor-only deterministic source. The saved scene has no runtime generator.
    public static class MaquisArtBuilder
    {
        const string Root = "Assets/Prototype_Maquis/Art";
        static System.Random rng;
        static float R(float a, float b) { return a + (float)rng.NextDouble() * (b - a); }
        static float PathX(float z)
        {
            float[] zs = {-12,-7,-2,3,8,12}; float[] xs = {-3,-3,-1,1,2,2};
            if (z <= zs[0]) return xs[0];
            for (int i=0;i<zs.Length-1;i++) if(z<=zs[i+1]) return Mathf.Lerp(xs[i],xs[i+1],Mathf.SmoothStep(0,1,Mathf.InverseLerp(zs[i],zs[i+1],z)));
            return 2;
        }
        public static float Height(float x,float z)
        {
            float d=Mathf.Abs(x-PathX(z));
            float bank=Mathf.SmoothStep(0,1,Mathf.InverseLerp(0.9f,4.5f,d));
            float bumps=0.19f+0.50f*Mathf.PerlinNoise(x*0.17f+20,z*0.17f+6)+0.18f*Mathf.Sin(z*0.42f+x*0.3f);
            float h=0.014f*(z+11)+bank*bumps;
            h+=bank*(Mathf.PerlinNoise(x*1.1f+10,z*1.1f+10)-0.5f)*0.06f;
            float far=Mathf.SmoothStep(0,1,Mathf.InverseLerp(17,42,Mathf.Max(Mathf.Abs(x),Mathf.Abs(z))));
            h+=far*(1.5f+6.0f*Mathf.PerlinNoise(x*0.047f+51,z*0.047f+31));
            return h;
        }
        sealed class Geo
        {
            public readonly List<Vector3> v=new List<Vector3>();
            public readonly List<Vector2> uv=new List<Vector2>();
            public readonly List<Color> c=new List<Color>();
            public readonly List<int> t=new List<int>();
            public void Tri(Vector3 a,Vector3 b,Vector3 d,Color color)
            {int n=v.Count; v.Add(a);v.Add(b);v.Add(d);uv.Add(Vector2.zero);uv.Add(Vector2.right);uv.Add(Vector2.up);c.Add(color);c.Add(color);c.Add(color);t.Add(n);t.Add(n+1);t.Add(n+2);}
            public void Leaf(Vector3 center,Vector3 axis,float len,float width,Color color)
            {
                axis.Normalize();Vector3 side=Vector3.Cross(axis,Vector3.up).normalized;
                if(side.sqrMagnitude<0.01f)side=Vector3.right;
                side=Quaternion.AngleAxis(R(-50,50),axis)*side;
                Vector3 a=center-axis*len*0.5f,b=center+side*width,c1=center+axis*len*0.5f,d=center-side*width;
                Tri(a,b,c1,color*0.92f);Tri(a,c1,d,color*1.05f);
            }
            public void Branch(Vector3 a,Vector3 b,float radius,Color color)
            {
                Vector3 axis=(b-a).normalized,side=Vector3.Cross(axis,Vector3.forward).normalized;
                if(side.sqrMagnitude<0.1f)side=Vector3.right;
                Vector3 up=Vector3.Cross(axis,side);
                for(int i=0;i<5;i++){float a1=i*Mathf.PI*2/5,a2=(i+1)*Mathf.PI*2/5;
                    Vector3 p=side*Mathf.Cos(a1)+up*Mathf.Sin(a1),q=side*Mathf.Cos(a2)+up*Mathf.Sin(a2);
                    Tri(a+p*radius,a+q*radius,b+q*radius*0.28f,color);Tri(a+p*radius,b+q*radius*0.28f,b+p*radius*0.28f,color);}
            }
            public void Append(Mesh mesh,Matrix4x4 matrix)
            {int n=v.Count;Vector3[] vs=mesh.vertices;Vector2[] us=mesh.uv;foreach(var p in vs){v.Add(matrix.MultiplyPoint3x4(p));c.Add(Color.white);}uv.AddRange(us);foreach(int i in mesh.triangles)t.Add(n+i);}
            public Mesh Mesh(string name)
            {var m=new Mesh {name=name}; if(v.Count>65535)m.indexFormat=IndexFormat.UInt32;m.SetVertices(v);m.SetUVs(0,uv);m.SetColors(c);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();m.RecalculateTangents();return m;}
        }
        static T Save<T>(T obj,string path) where T:UnityEngine.Object
        {
            var old=AssetDatabase.LoadAssetAtPath<T>(path);
            if(old!=null){
                var targetMesh=old as Mesh;var sourceMesh=obj as Mesh;
                if(targetMesh!=null&&sourceMesh!=null){
                    targetMesh.Clear();targetMesh.indexFormat=sourceMesh.indexFormat;
                    targetMesh.vertices=sourceMesh.vertices;targetMesh.uv=sourceMesh.uv;targetMesh.colors=sourceMesh.colors;
                    targetMesh.triangles=sourceMesh.triangles;targetMesh.normals=sourceMesh.normals;targetMesh.tangents=sourceMesh.tangents;
                    targetMesh.bounds=sourceMesh.bounds;targetMesh.UploadMeshData(false);
                }else EditorUtility.CopySerialized(obj,old);
                EditorUtility.SetDirty(old);UnityEngine.Object.DestroyImmediate(obj);return old;
            }
            AssetDatabase.CreateAsset(obj,path);return obj;
        }
        static GameObject MeshObject(string name,Mesh mesh,Material mat,Transform parent,bool collider)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=mat;
            renderer.lightProbeUsage=LightProbeUsage.Off;renderer.reflectionProbeUsage=ReflectionProbeUsage.Off;
            if(collider)go.AddComponent<MeshCollider>().sharedMesh=mesh;
            return go;
        }
        static Mesh Ground(bool apron)
        {
            int n=apron?44:60;float extent=apron?55:15;
            var geo=new Geo();
            for(int z=0;z<=n;z++)for(int x=0;x<=n;x++){
                float px=Mathf.Lerp(-extent,extent,x/(float)n),pz=Mathf.Lerp(-extent,extent,z/(float)n);
                geo.v.Add(new Vector3(px,Height(px,pz),pz));geo.uv.Add(new Vector2(px*0.5f,pz*0.5f));
                float edge=Mathf.Abs(px-PathX(pz))+(Mathf.PerlinNoise(px*3+99,pz*3+17)-0.5f)*0.28f;
                float path=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(0.55f,1.12f,edge));
                if(Mathf.Abs(pz)>13)path*=1-Mathf.InverseLerp(13,17,Mathf.Abs(pz));
                geo.c.Add(new Color(path,Mathf.PerlinNoise(px*0.4f+38,pz*0.4f+19),0,1));
            }
            for(int z=0;z<n;z++)for(int x=0;x<n;x++){
                float px=Mathf.Lerp(-extent,extent,(x+0.5f)/n),pz=Mathf.Lerp(-extent,extent,(z+0.5f)/n);
                if(apron&&Mathf.Abs(px)<15&&Mathf.Abs(pz)<15)continue;
                int a=z*(n+1)+x,b=a+1,c=a+n+1,d=c+1;
                geo.t.Add(a);geo.t.Add(c);geo.t.Add(b);geo.t.Add(b);geo.t.Add(c);geo.t.Add(d);
            }
            return geo.Mesh(apron?"Distant_Maquis_Hills":"Relief_30x30m");
        }
        static Mesh Rock(int variant, bool pebble=false)
        {
            var geo=new Geo();int lon=pebble?8:15,lat=pebble?5:9;float seed=variant*7.35f;
            for(int y=0;y<=lat;y++)for(int x=0;x<=lon;x++){
                float a=x/(float)lon*Mathf.PI*2,b=y/(float)lat*Mathf.PI;
                Vector3 p=new Vector3(Mathf.Sin(b)*Mathf.Cos(a),Mathf.Cos(b),Mathf.Sin(b)*Mathf.Sin(a));
                float rough=0.82f+0.26f*Mathf.PerlinNoise(p.x*2.4f+seed+15,p.z*2.4f+p.y*1.8f+42);
                p*=0.5f*rough;
                p.x+=0.08f*p.y*Mathf.Sin(seed);p.z+=0.1f*p.y;
                p.y=Mathf.Max(p.y,-0.34f+0.03f*Mathf.Sin(p.x*15+seed));
                geo.v.Add(p);geo.uv.Add(new Vector2(x/(float)lon*2,y/(float)lat));geo.c.Add(Color.white);
            }
            for(int y=0;y<lat;y++)for(int x=0;x<lon;x++){
                int a=y*(lon+1)+x,b=a+1,c=a+lon+1,d=c+1;
                geo.t.Add(a);geo.t.Add(b);geo.t.Add(c);geo.t.Add(b);geo.t.Add(d);geo.t.Add(c);
            }
            return geo.Mesh("Granite_Erode_"+variant);
        }
        static void Bush(Geo geo,Vector3 p,float size,int species)
        {
            float yaw=R(0,360);Color wood=new Color(0.075f,0.045f,0.018f);
            Color leaf=species==0?new Color(0.035f,0.075f,0.012f):species==1?new Color(0.085f,0.13f,0.048f):new Color(0.043f,0.09f,0.018f);
            int branches=species==2?7:6;
            for(int i=0;i<branches;i++){
                float angle=(yaw+i*137.5f)*Mathf.Deg2Rad;
                Vector3 spread=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                Vector3 tip=p+spread*R(0.30f,0.65f)*size+Vector3.up*R(0.55f,species==2?1.55f:1.12f)*size;
                Vector3 start=p+Vector3.up*0.05f;
                geo.Branch(start,tip,0.025f*size,wood);
                for(int k=0;k<3;k++){
                    float along=0.45f+k*0.22f;
                    Vector3 node=Vector3.Lerp(start,tip,along);
                    Vector3 lateral=Quaternion.Euler(0,R(0,360),0)*Vector3.forward;
                    Vector3 end=node+lateral*R(0.17f,0.34f)*size+Vector3.up*0.05f*size;
                    geo.Branch(node,end,0.007f*size,wood);
                    int pairs=species==2?3:4;
                    for(int j=0;j<pairs;j++)for(int side=-1;side<=1;side+=2){
                        Vector3 loc=Vector3.Lerp(node,end,(j+0.5f)/pairs);
                        Vector3 axis=Quaternion.Euler(R(-20,25),side*R(38,70),0)*lateral;
                        float length=(species==0?0.19f:species==1?0.21f:0.10f)*size;
                        loc+=axis*length*0.35f;
                        geo.Leaf(loc,axis,length,length*(species==2?0.20f:0.39f),leaf*R(0.68f,1.25f));
                    }
                    if(species==1&&rng.NextDouble()<0.28){
                        Vector3 flower=end+Vector3.up*0.03f;
                        for(int q=0;q<5;q++){Vector3 radial=Quaternion.Euler(0,q*72,0)*Vector3.forward;
                            geo.Leaf(flower+radial*0.022f*size,radial,0.045f*size,0.018f*size,new Color(0.84f,0.80f,0.65f));}
                    }
                }
            }
            // Uneven leafy crowns connect the twig sprays into recognizable dense shrubs.
            for(int j=0;j<90;j++){
                float angle=R(0,Mathf.PI*2),elevation=R(-0.65f,1f),radial=Mathf.Sqrt(1-elevation*elevation);
                float lobe=0.80f+0.20f*Mathf.Sin(angle*3+yaw);
                Vector3 offset=new Vector3(Mathf.Cos(angle)*radial*0.54f*lobe,0.72f+elevation*(species==2?0.56f:0.34f),Mathf.Sin(angle)*radial*0.54f*lobe)*size;
                Vector3 axis=new Vector3(Mathf.Cos(angle),R(-0.3f,0.5f),Mathf.Sin(angle));
                float len=(species==2?0.11f:0.18f)*size;
                geo.Leaf(p+offset,axis,len,len*(species==2?0.18f:0.38f),leaf*R(0.65f,1.3f));
            }
        }
        static void Grass(Geo geo,Vector3 p,float size)
        {
            for(int i=0;i<9;i++){
                Vector3 direction=Quaternion.Euler(0,R(0,360),0)*Vector3.forward;
                Vector3 side=Vector3.Cross(direction,Vector3.up)*R(0.009f,0.017f)*size;
                Vector3 root=p+direction*R(0,0.08f),tip=root+Vector3.up*R(0.22f,0.48f)*size+direction*R(0.08f,0.24f)*size;
                Vector3 mid=Vector3.Lerp(root,tip,0.6f)+Vector3.up*0.045f;
                Color col=new Color(0.43f,0.39f,0.18f)*R(0.75f,1.4f);
                geo.Tri(root-side,root+side,mid+side*0.6f,col*0.8f);
                geo.Tri(root-side,mid+side*0.6f,mid-side*0.6f,col);
                geo.Tri(mid-side*0.6f,mid+side*0.6f,tip,col*1.1f);
            }
        }
        static void ConfigureTextures()
        {
            foreach(string file in Directory.GetFiles(Root+"/Textures","*.jpg")){
                var importer=AssetImporter.GetAtPath(file.Replace('\\','/')) as TextureImporter;
                if(importer==null)continue;
                importer.textureType=file.Contains("nor_gl")?TextureImporterType.NormalMap:TextureImporterType.Default;
                importer.sRGBTexture=!file.Contains("nor_gl");importer.maxTextureSize=1024;
                importer.mipmapEnabled=true;importer.isReadable=false;importer.wrapMode=TextureWrapMode.Repeat;
                importer.anisoLevel=2;importer.textureCompression=TextureImporterCompression.Compressed;
                var web=importer.GetPlatformTextureSettings("WebGL");web.overridden=true;web.maxTextureSize=1024;
                web.format=TextureImporterFormat.ASTC_6x6;web.compressionQuality=50;importer.SetPlatformTextureSettings(web);
                importer.SaveAndReimport();
            }
        }
        [MenuItem("CorseVR/Archive/Rebuild previous natural study")]
        static void BuildFromMenu() { Debug.Log(Build()); }

        public static string Build()
        {
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(scene.path!="Assets/Scenes/Prototype_Maquis.unity"||Application.isPlaying)throw new InvalidOperationException("Open Prototype_Maquis in Edit mode.");
            Directory.CreateDirectory(Root+"/Meshes");Directory.CreateDirectory(Root+"/Materials");AssetDatabase.Refresh();
            rng=new System.Random(22026);ConfigureTextures();
            var terrainShader=Shader.Find("CorseVR/Maquis Ground Mobile");var foliageShader=Shader.Find("CorseVR/Maquis Opaque Foliage");
            if(terrainShader==null||foliageShader==null)throw new InvalidOperationException("Missing Maquis shaders");
            var soil=Save(new Material(terrainShader){name="Sol_Terre_Graviers"},Root+"/Materials/Sol_Terre_Graviers.mat");
            soil.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/rocky_trail_diff_1k.jpg"));
            soil.SetTexture("_DirtMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/brown_mud_02_diff_1k.jpg"));
            soil.SetTexture("_NormalMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/rocky_trail_nor_gl_1k.jpg"));
            soil.SetTexture("_DirtNormal",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/brown_mud_02_nor_gl_1k.jpg"));
            var rockMat=Save(new Material(Shader.Find("Universal Render Pipeline/Simple Lit")){name="Granite_Sec"},Root+"/Materials/Granite_Sec.mat");
            rockMat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/rock_boulder_dry_diff_1k.jpg"));
            rockMat.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/rock_boulder_dry_nor_gl_1k.jpg"));
            rockMat.EnableKeyword("_NORMALMAP");rockMat.SetFloat("_BumpScale",0.65f);rockMat.SetFloat("_Smoothness",0.05f);rockMat.SetColor("_BaseColor",new Color(0.82f,0.78f,0.69f));
            var foliage=Save(new Material(foliageShader){name="Feuillage_Mat"},Root+"/Materials/Feuillage_Mat.mat");
            var sky=Save(new Material(Shader.Find("Skybox/Procedural")){name="Ciel_Mediterranee"},Root+"/Materials/Ciel_Mediterranee.mat");
            sky.SetFloat("_SunSize",0.025f);sky.SetFloat("_AtmosphereThickness",0.85f);sky.SetFloat("_Exposure",1.15f);
            sky.SetColor("_SkyTint",new Color(0.50f,0.56f,0.63f));sky.SetColor("_GroundColor",new Color(0.36f,0.33f,0.26f));
            RenderSettings.skybox=sky;RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(0.43f,0.52f,0.65f);RenderSettings.ambientEquatorColor=new Color(0.35f,0.39f,0.39f);RenderSettings.ambientGroundColor=new Color(0.23f,0.20f,0.15f);
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogColor=new Color(0.66f,0.72f,0.74f);RenderSettings.fogStartDistance=28;RenderSettings.fogEndDistance=85;
            var sun=GameObject.Find("Directional Light").GetComponent<Light>();sun.transform.rotation=Quaternion.Euler(43,132,0);
            sun.color=new Color(1f,0.93f,0.79f);sun.intensity=1.35f;sun.shadows=LightShadows.Soft;sun.shadowStrength=0.75f;sun.shadowBias=0.04f;sun.shadowNormalBias=0.25f;RenderSettings.sun=sun;
            var environment=GameObject.Find("Maquis_Environment").transform;
            var old=environment.Find("Maquis_Depart_Detail");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
            var detail=new GameObject("Maquis_Depart_Detail").transform;detail.SetParent(environment,false);
            var ground=environment.Find("Sol_30x30m").gameObject;
            ground.transform.localPosition=Vector3.zero;ground.transform.localScale=Vector3.one;ground.transform.localRotation=Quaternion.identity;
            var box=ground.GetComponent<BoxCollider>();if(box!=null)UnityEngine.Object.DestroyImmediate(box);
            var mesh=Save(Ground(false),Root+"/Meshes/Relief_30x30m.asset");ground.GetComponent<MeshFilter>().sharedMesh=mesh;ground.GetComponent<MeshRenderer>().sharedMaterial=soil;
            var groundCollider=ground.GetComponent<MeshCollider>();if(groundCollider==null)groundCollider=ground.AddComponent<MeshCollider>();groundCollider.sharedMesh=mesh;
            var apron=Save(Ground(true),Root+"/Meshes/Horizon_Relief.asset");var distant=MeshObject("Collines_Lointaines",apron,soil,detail,true);distant.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;
            environment.Find("Chemin").gameObject.SetActive(false);
            var rockMeshes=new Mesh[6];for(int i=0;i<6;i++)rockMeshes[i]=Save(Rock(i),Root+"/Meshes/Granite_"+i+".asset");
            var rocks=environment.Find("Rochers");
            for(int i=0;i<rocks.childCount;i++){
                var go=rocks.GetChild(i).gameObject;var tr=go.transform;
                tr.position=new Vector3(tr.position.x,Height(tr.position.x,tr.position.z)+0.32f,tr.position.z);
                go.GetComponent<MeshFilter>().sharedMesh=rockMeshes[i%6];go.GetComponent<MeshRenderer>().sharedMaterial=rockMat;
                var sphere=go.GetComponent<SphereCollider>();if(sphere!=null)UnityEngine.Object.DestroyImmediate(sphere);
                var mc=go.GetComponent<MeshCollider>();if(mc==null)mc=go.AddComponent<MeshCollider>();mc.sharedMesh=rockMeshes[i%6];
            }
            var plants=new Dictionary<string,Geo>();var gravel=new Dictionary<string,Geo>();int bushes=0,grasses=0,stones=0;
            var locations=new List<Vector3>();
            for(int attempt=0;attempt<900&&bushes<85;attempt++){
                float z=R(-14,3),x=PathX(z)+(rng.Next(2)==0?-1:1)*R(1.7f,8.5f);
                float distance=Mathf.Abs(x-PathX(z));if(distance<1.65f)continue;
                if(z>2&&rng.NextDouble()>0.24)continue;
                float density=Mathf.PerlinNoise(x*0.3f+50,z*0.3f+37);if(density<0.36f)continue;
                float size=R(0.65f,1.35f);if(distance>7)size*=1.25f;
                var pos=new Vector3(x,Height(x,z)-0.025f,z);bool crowded=false;
                foreach(var other in locations)if((other-pos).sqrMagnitude<0.95f*size*size){crowded=true;break;}if(crowded)continue;
                bool onRock=false;for(int i=0;i<rocks.childCount;i++){var rt=rocks.GetChild(i);var delta=rt.position-pos;delta.y=0;if(delta.magnitude<Mathf.Max(rt.localScale.x,rt.localScale.z)*0.5f+0.3f){onRock=true;break;}}if(onRock)continue;
                string key=Mathf.FloorToInt((x+15)/6)+"_"+Mathf.FloorToInt((z+15)/6);if(!plants.ContainsKey(key))plants[key]=new Geo();
                Bush(plants[key],pos,size,rng.Next(3));locations.Add(pos);bushes++;
            }
            for(int i=0;i<250;i++){
                float z=R(-14,7),x=PathX(z)+(rng.Next(2)==0?-1:1)*R(1.05f,7.0f);
                if(Mathf.Abs(x)>14)continue;
                string key=Mathf.FloorToInt((x+15)/6)+"_"+Mathf.FloorToInt((z+15)/6);if(!plants.ContainsKey(key))plants[key]=new Geo();
                Grass(plants[key],new Vector3(x,Height(x,z),z),R(0.6f,1.35f));grasses++;
            }
            foreach(var kv in plants){var m=Save(kv.Value.Mesh("Maquis_"+kv.Key),Root+"/Meshes/Maquis_"+kv.Key+".asset");var go=MeshObject("Lentisques_Cistes_Bruyeres_"+kv.Key,m,foliage,detail,false);go.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.On;}
            var pebbleMeshes=new Mesh[3];for(int i=0;i<3;i++)pebbleMeshes[i]=Save(Rock(i+10,true),Root+"/Meshes/Galet_"+i+".asset");
            for(int i=0;i<150;i++){
                float z=R(-13.5f,7),x=PathX(z)+(rng.Next(2)==0?-1:1)*R(0.75f,4.5f);
                string key=Mathf.FloorToInt((x+15)/8)+"_"+Mathf.FloorToInt((z+15)/8);if(!gravel.ContainsKey(key))gravel[key]=new Geo();
                float scale=R(0.08f,0.34f);Vector3 p=new Vector3(x,Height(x,z)+scale*0.1f,z);
                gravel[key].Append(pebbleMeshes[rng.Next(3)],Matrix4x4.TRS(p,Quaternion.Euler(R(0,180),R(0,360),R(0,180)),new Vector3(scale,scale*0.6f,scale*R(0.7f,1.5f))));stones++;
            }
            foreach(var kv in gravel){var m=Save(kv.Value.Mesh("Graviers_"+kv.Key),Root+"/Meshes/Graviers_"+kv.Key+".asset");var go=MeshObject("Petites_Pierres_"+kv.Key,m,rockMat,detail,false);go.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;}
            var camera=Camera.main;camera.clearFlags=CameraClearFlags.Skybox;camera.farClipPlane=110;
            var cameraData=camera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();if(cameraData!=null)cameraData.renderPostProcessing=false;
            foreach(var renderer in detail.GetComponentsInChildren<MeshRenderer>())renderer.gameObject.isStatic=true;
            EditorUtility.SetDirty(soil);EditorUtility.SetDirty(rockMat);EditorUtility.SetDirty(foliage);EditorUtility.SetDirty(sky);AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            return "Maquis authored: "+bushes+" shrubs, "+grasses+" grass clumps, "+stones+" pebbles, "+plants.Count+" vegetation chunks. Controller preserved.";
        }
    }
}
