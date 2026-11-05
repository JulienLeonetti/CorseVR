using UnityEngine;
namespace CorseVR {
 public sealed class TourLittoralRoute : MonoBehaviour {
  public Vector3[] coastPoints;public float[] coastDistances;public Transform entranceDoor;public float seaLevel=-8;
  public float Length{get{return coastDistances==null||coastDistances.Length==0?0:coastDistances[coastDistances.Length-1];}}
  public void Configure(Vector3[] points){coastPoints=points;coastDistances=new float[points.Length];for(int i=1;i<points.Length;i++)coastDistances[i]=coastDistances[i-1]+Vector3.Distance(points[i-1],points[i]);}
  public Vector3 WorldAt(float s){if(coastPoints==null||coastPoints.Length==0)return transform.position;s=Mathf.Clamp(s,0,Length);int i=1;while(i<coastDistances.Length-1&&coastDistances[i]<s)i++;return transform.TransformPoint(Vector3.Lerp(coastPoints[i-1],coastPoints[i],Mathf.InverseLerp(coastDistances[i-1],coastDistances[i],s)));}
  public Vector3 Direction(float s){return (WorldAt(Mathf.Min(Length,s+.2f))-WorldAt(Mathf.Max(0,s-.2f))).normalized;}
 }
}
