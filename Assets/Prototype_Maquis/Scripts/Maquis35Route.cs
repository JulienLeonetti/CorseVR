using UnityEngine;
namespace CorseVR {
 public sealed class Maquis35Route:MonoBehaviour {
  public Vector3[] points; public float[] distances; public float[] widths;
  public float Length {get{return distances==null||distances.Length==0?0:distances[distances.Length-1];}}
  public Vector3 LocalAt(float s) {
   s=Mathf.Clamp(s,0,Length);int i=1;while(i<distances.Length-1&&distances[i]<s)i++;
   return Vector3.Lerp(points[i-1],points[i],Mathf.InverseLerp(distances[i-1],distances[i],s));
  }
  public Vector3 WorldAt(float s){return transform.TransformPoint(LocalAt(s));}
  public Vector3 Direction(float s){return transform.TransformDirection((LocalAt(s+.18f)-LocalAt(s-.18f)).normalized);}
 }
}
