using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace CorseVR.Editor {
 public static class CadreCorseBuilder {
  const string Root="Assets/Prototype_Maquis/Art/MaisonArtistique";
  [Serializable] class ContourData {public float[] vertices;public int[] triangles;}
  public static string Apply(bool save=true){
   if(Application.isPlaying)throw new Exception("Edit mode required");
   var house=GameObject.Find("Maison_Accueil_Corse").transform;
   var old=house.GetComponentsInChildren<MeshRenderer>(true).First(r=>r.name=="Carte_Corse");
   var asset=AssetDatabase.LoadAssetAtPath<TextAsset>(Root+"/Sources/CorseContour.json");if(asset==null)throw new Exception("Missing CorseContour.json");
   var data=JsonUtility.FromJson<ContourData>(asset.text);var vertices=new Vector3[data.vertices.Length/3];var colors=new Color[vertices.Length];var uv=new Vector2[vertices.Length];Color color;ColorUtility.TryParseHtmlString("#6C7C6B",out color);
   for(int i=0;i<vertices.Length;i++){vertices[i]=new Vector3(data.vertices[i*3],data.vertices[i*3+1],data.vertices[i*3+2]);colors[i]=color;uv[i]=new Vector2(vertices[i].x+.5f,vertices[i].y-1.35f);}
   string path=Root+"/Corse_Cadre_Contour_Fidele.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(mesh==null){mesh=new Mesh();mesh.name="Corse_Contour_Fidele";AssetDatabase.CreateAsset(mesh,path);}else mesh.Clear();
   mesh.vertices=vertices;mesh.colors=colors;mesh.uv=uv;mesh.triangles=data.triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();mesh.UploadMeshData(false);EditorUtility.SetDirty(mesh);
   var t=house.Find("Carte_Corse_Contour_Fidele");GameObject go;if(t==null){go=new GameObject("Carte_Corse_Contour_Fidele");go.transform.SetParent(house,false);go.AddComponent<MeshFilter>();go.AddComponent<MeshRenderer>();}else go=t.gameObject;
   go.GetComponent<MeshFilter>().sharedMesh=mesh;var rr=go.GetComponent<MeshRenderer>();rr.sharedMaterial=old.sharedMaterial;rr.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;old.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(old);
   EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();if(save)EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
   return "Only the map silhouette replaced: "+vertices.Length+" outline points, "+data.triangles.Length/3+" triangles; original frame and backing preserved.";
  }
 }
}
