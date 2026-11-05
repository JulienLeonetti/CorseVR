using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace CorseVR.Editor
{
    public static class MaquisFoliageRefinement
    {
        const string Root="Assets/Prototype_Maquis/Art/FoliageReference";
        struct Plant {public Vector3 p;public float size;public int species;public bool tree;}
        static readonly List<Plant> plants=new List<Plant>();
        static System.Random random;static int cards;
        static float R(float a,float b){return a+(float)random.NextDouble()*(b-a);}
        public static bool ShouldRefine(Vector3 p){return p.x>=-8&&p.x<=3.3f&&p.z>=-13.5f&&p.z<=-5;}
        public static void Reset(){plants.Clear();}
        public static void RecordShrub(Vector3 p,float size,int species){plants.Add(new Plant{p=p,size=size,species=species});}
        public static void RecordTree(Vector3 p,float size,int species){plants.Add(new Plant{p=p,size=size,species=species,tree=true});}
        sealed class Geo
        {
            public readonly List<Vector3> v=new List<Vector3>(),n=new List<Vector3>();
            public readonly List<Vector2> uv=new List<Vector2>();public readonly List<Color> c=new List<Color>();public readonly List<int> t=new List<int>();
            public void Card(Vector3 basePoint,Vector3 growth,float width,float height,float roll,Color tint)
            {
                Vector3 middle=basePoint+growth.normalized*height*0.5f;
                if(middle.y<MaquisIllustratedBuilder.Height(middle.x,middle.z)+2.1f&&Mathf.Abs(middle.x-MaquisIllustratedBuilder.PathX(middle.z))<1.40f)return;
                growth.Normalize();Vector3 side=Vector3.Cross(growth,Vector3.up).normalized;if(side.sqrMagnitude<0.01f)side=Vector3.right;
                side=Quaternion.AngleAxis(roll,growth)*side;Vector3 normal=Vector3.Cross(side,growth).normalized;int first=v.Count;
                for(int j=0;j<=3;j++){
                    float f=j/3f;Vector3 center=basePoint+growth*height*f+normal*Mathf.Sin(f*Mathf.PI)*height*0.10f;
                    for(int k=0;k<2;k++){v.Add(center+side*(k-0.5f)*width);n.Add((normal*0.75f+Vector3.up*0.3f+growth*0.2f).normalized);uv.Add(new Vector2(k,f));c.Add(tint*Mathf.Lerp(0.82f,1.08f,f));}
                }
                for(int j=0;j<3;j++){int a=first+j*2,b=a+1,d=a+2,e=a+3;t.Add(a);t.Add(b);t.Add(d);t.Add(b);t.Add(e);t.Add(d);}cards++;
            }
            public void Wood(Vector3 a,Vector3 b,float radius,float bend,int rings=4,int sides=7)
            {
                Vector3 axis=(b-a).normalized,right=Vector3.Cross(axis,Vector3.forward).normalized;if(right.sqrMagnitude<0.01f)right=Vector3.right;
                Vector3 up=Vector3.Cross(axis,right);float length=Vector3.Distance(a,b);int first=v.Count;
                for(int j=0;j<=rings;j++){
                    float f=j/(float)rings;Vector3 center=Vector3.Lerp(a,b,f)+right*Mathf.Sin(f*Mathf.PI)*bend;
                    float r=radius*Mathf.Lerp(1,0.24f,Mathf.Pow(f,0.8f));
                    for(int k=0;k<=sides;k++){float angle=k/(float)sides*Mathf.PI*2;Vector3 normal=right*Mathf.Cos(angle)+up*Mathf.Sin(angle);v.Add(center+normal*r);n.Add(normal);uv.Add(new Vector2(k/(float)sides*1.3f,f*length*1.2f));c.Add(Color.white*Mathf.Lerp(0.78f,1.10f,f));}
                }
                for(int j=0;j<rings;j++)for(int k=0;k<sides;k++){int a1=first+j*(sides+1)+k,b1=a1+1,d=a1+sides+1,e=d+1;t.Add(a1);t.Add(b1);t.Add(d);t.Add(b1);t.Add(e);t.Add(d);}
            }
            public void Flower(Vector3 p,float size)
            {
                Color cream=new Color(0.92f,0.82f,0.61f);Color gold=new Color(0.75f,0.46f,0.08f);
                for(int j=0;j<5;j++){
                    float a=j*Mathf.PI*2/5;Vector3 axis=new Vector3(Mathf.Cos(a),0.25f,Mathf.Sin(a)).normalized,side=Vector3.Cross(axis,Vector3.up);
                    Vector3 q=p+axis*size*0.65f;Tri(p,q+side*size*0.45f,q+axis*size*0.45f,cream);Tri(p,q+axis*size*0.45f,q-side*size*0.45f,cream);
                }
                for(int j=0;j<8;j++){float a=j*Mathf.PI/4,b=(j+1)*Mathf.PI/4;Tri(p+Vector3.up*0.012f,p+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*size*0.33f,p+new Vector3(Mathf.Cos(b),0,Mathf.Sin(b))*size*0.33f,gold);}
            }
            void Tri(Vector3 a,Vector3 b,Vector3 d,Color color){int k=v.Count;var normal=Vector3.Cross(b-a,d-a).normalized;v.Add(a);v.Add(b);v.Add(d);n.Add(normal);n.Add(normal);n.Add(normal);uv.Add(Vector2.zero);uv.Add(Vector2.zero);uv.Add(Vector2.zero);c.Add(color);c.Add(color);c.Add(color);t.Add(k);t.Add(k+1);t.Add(k+2);}
            public Mesh Mesh(string name){var m=new Mesh{name=name};if(v.Count>65535)m.indexFormat=IndexFormat.UInt32;m.SetVertices(v);m.SetNormals(n);m.SetUVs(0,uv);m.SetColors(c);m.SetTriangles(t,0);m.RecalculateBounds();return m;}
        }
        static void Spray(Geo leaves,Vector3 p,Vector3 growth,float size,Color tint,int count=3)
        {for(int i=0;i<count;i++)leaves.Card(p+growth*R(-0.08f,0.10f)*size,growth,R(0.43f,0.57f)*size,R(0.57f,0.74f)*size,i*62+R(-35,35),tint*R(0.90f,1.12f));}
        static void Shrub(Plant plant,Geo leaves,Geo wood,Geo flowers)
        {
            Vector3 p=plant.p;float s=plant.size;Color tint=plant.species==1?new Color(1.03f,1.08f,0.94f):plant.species==2?new Color(0.86f,1.08f,1.03f):Color.white;
            Vector3 root=p+Vector3.up*0.05f*s;wood.Wood(root,root+Vector3.up*0.40f*s,0.043f*s,0.04f*s);
            for(int i=0;i<6;i++){
                float angle=(i*137.5f+R(-20,20))*Mathf.Deg2Rad;Vector3 radial=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                Vector3 end=p+radial*R(0.38f,0.65f)*s+Vector3.up*R(0.62f,1.08f)*s;
                wood.Wood(root+Vector3.up*0.15f*s,end,0.024f*s,R(-0.09f,0.09f)*s,3,6);
                for(int j=0;j<3;j++){
                    Vector3 direction=(Quaternion.Euler(0,R(-60,60),0)*radial+Vector3.up*R(0.5f,1.2f)).normalized;
                    Vector3 start=Vector3.Lerp(root,end,0.45f+j*0.22f),tip=start+direction*R(0.35f,0.57f)*s;
                    wood.Wood(start,tip,0.010f*s,0.035f*s,2,5);
                    for(int k=0;k<2;k++){
                        Vector3 sprayDirection=(Quaternion.Euler(R(-18,18),R(-45,45),0)*direction+Vector3.up*R(0,0.15f)).normalized;
                        Spray(leaves,Vector3.Lerp(start,tip,0.55f+k*0.32f),sprayDirection,s*(plant.species==2?0.82f:1.02f),tint,3);
                    }
                    if(plant.species==1&&i%2==0&&j==2)flowers.Flower(tip+direction*0.20f*s,0.085f*s);
                }
            }
        }
        static void Tree(Plant plant,Geo leaves,Geo wood)
        {
            Vector3 p=plant.p;float s=plant.size;Vector3 lean=new Vector3(plant.species%2==0?0.35f:-0.38f,3.3f,0.1f)*s;
            wood.Wood(p,p+lean,0.19f*s,0.20f*s,8,12);
            for(int i=0;i<7;i++){
                float a=(i*137.5f+plant.species*40)*Mathf.Deg2Rad;Vector3 radial=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                Vector3 start=p+lean*R(0.48f,0.78f),end=p+lean+radial*R(0.7f,1.65f)*s+Vector3.up*R(-0.1f,0.5f)*s;
                wood.Wood(start,end,0.070f*s,R(-0.20f,0.20f)*s,5,8);
                for(int j=0;j<4;j++){
                    Vector3 direction=(Quaternion.Euler(0,R(-65,65),0)*radial+Vector3.up*R(0.20f,0.85f)).normalized;
                    Vector3 node=Vector3.Lerp(start,end,0.45f+j*0.17f),tip=node+direction*R(0.65f,1.05f)*s;
                    wood.Wood(node,tip,0.025f*s,0.09f*s,3,6);
                    for(int k=0;k<3;k++){
                        Vector3 sprayDirection=(Quaternion.Euler(R(-25,25),R(-65,65),R(-10,10))*direction+Vector3.up*R(0,0.2f)).normalized;
                        Vector3 side=Vector3.Cross(direction,Vector3.up).normalized;
                        Vector3 point=Vector3.Lerp(node,tip,0.40f+k*0.27f)+side*R(-0.22f,0.22f)*s;
                        Spray(leaves,point,sprayDirection,s*R(1.22f,1.48f),new Color(0.95f,1.04f,0.96f),4);
                    }
                }
            }
        }
        static Mesh SaveMesh(Mesh source,string path)
        {
            var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old==null){AssetDatabase.CreateAsset(source,path);return source;}
            old.Clear();old.indexFormat=source.indexFormat;old.vertices=source.vertices;old.normals=source.normals;old.uv=source.uv;old.colors=source.colors;old.triangles=source.triangles;old.bounds=source.bounds;old.UploadMeshData(false);EditorUtility.SetDirty(old);UnityEngine.Object.DestroyImmediate(source);return old;
        }
        static void Object(string name,Geo geometry,Material material,Transform parent)
        {
            if(geometry.v.Count==0)return;var mesh=SaveMesh(geometry.Mesh(name),Root+"/Meshes/"+name+".asset");var go=new GameObject(name);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.lightProbeUsage=LightProbeUsage.Off;renderer.reflectionProbeUsage=ReflectionProbeUsage.Off;go.isStatic=true;
        }
        static Material Material(string path,Shader shader)
        {var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(shader);AssetDatabase.CreateAsset(m,path);}return m;}
        public static string Build(Transform parent)
        {
            random=new System.Random(97131);cards=0;System.IO.Directory.CreateDirectory(Root+"/Meshes");System.IO.Directory.CreateDirectory(Root+"/Materials");AssetDatabase.Refresh();
            foreach(string name in new[]{"Painted_Leaf_Spray.png","Painted_Bark.png"}){
                var importer=(TextureImporter)AssetImporter.GetAtPath(Root+"/Textures/"+name);bool alpha=name.Contains("Leaf");
                importer.maxTextureSize=alpha?1024:512;importer.mipmapEnabled=true;importer.alphaIsTransparency=alpha;importer.sRGBTexture=true;importer.anisoLevel=4;importer.wrapMode=alpha?TextureWrapMode.Clamp:TextureWrapMode.Repeat;
                importer.mipMapsPreserveCoverage=alpha;importer.alphaTestReferenceValue=0.32f;importer.textureCompression=TextureImporterCompression.Compressed;
                var web=importer.GetPlatformTextureSettings("WebGL");web.overridden=true;web.maxTextureSize=alpha?1024:512;web.format=alpha?TextureImporterFormat.ASTC_4x4:TextureImporterFormat.ASTC_6x6;importer.SetPlatformTextureSettings(web);importer.SaveAndReimport();
            }
            var shader=Shader.Find("CorseVR/Painted Plant");if(shader==null)throw new Exception("Painted Plant shader missing");
            var leaf=Material(Root+"/Materials/Leaf_Painted.mat",shader);leaf.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/Painted_Leaf_Spray.png"));leaf.SetColor("_Tint",new Color(0.88f,1.18f,1.06f));leaf.SetFloat("_Cutoff",0.32f);leaf.SetFloat("_AlphaToMask",1);leaf.SetFloat("_Cull",0);leaf.EnableKeyword("_ALPHATEST_ON");leaf.renderQueue=2450;
            var bark=Material(Root+"/Materials/Bark_Painted.mat",shader);bark.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/Painted_Bark.png"));bark.SetColor("_Tint",new Color(0.88f,0.90f,0.90f));bark.SetFloat("_AlphaToMask",0);bark.SetFloat("_Cull",2);bark.DisableKeyword("_ALPHATEST_ON");bark.renderQueue=2000;
            var flowerMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Prototype_Maquis/Art/Illustrated/Materials/Palette_Maquis.mat");
            var root=new GameObject("Foliage_Reference_Zone").transform;root.SetParent(parent,false);
            var foliage=new Dictionary<string,Geo>();var woods=new Dictionary<string,Geo>();var blooms=new Dictionary<string,Geo>();int shrubs=0,trees=0;
            foreach(var plant in plants){string key=Mathf.FloorToInt((plant.p.x+10)/4)+"_"+Mathf.FloorToInt((plant.p.z+15)/4);if(!foliage.ContainsKey(key)){foliage[key]=new Geo();woods[key]=new Geo();blooms[key]=new Geo();}if(plant.tree){Tree(plant,foliage[key],woods[key]);trees++;}else{Shrub(plant,foliage[key],woods[key],blooms[key]);shrubs++;}}
            foreach(var key in foliage.Keys){Object("Feuillage_"+key,foliage[key],leaf,root);Object("Branches_"+key,woods[key],bark,root);Object("Cistes_Fleurs_"+key,blooms[key],flowerMaterial,root);}
            EditorUtility.SetDirty(leaf);EditorUtility.SetDirty(bark);AssetDatabase.SaveAssets();return shrubs+" refined shrubs, "+trees+" refined trees, "+cards+" curved leaf sprays.";
        }
    }
}
