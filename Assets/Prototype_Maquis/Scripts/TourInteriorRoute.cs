using UnityEngine;
namespace CorseVR {
 [DisallowMultipleComponent]
 public sealed class TourInteriorRoute : MonoBehaviour {
  public Vector3[] points; public float[] distances; public TourDoubleDoor maquisDoor,littoralDoor;
  public float Length { get { return distances!=null&&distances.Length>0?distances[distances.Length-1]:0; } }
  public Vector3 WorldAt(float distance) { if(points==null||points.Length==0)return transform.position;distance=Mathf.Clamp(distance,0,Length);int i=1;while(i<distances.Length-1&&distances[i]<distance)i++;float t=Mathf.InverseLerp(distances[i-1],distances[i],distance);return transform.TransformPoint(Vector3.Lerp(points[i-1],points[i],t)); }
  public Vector3 Direction(float distance) { return (WorldAt(Mathf.Min(Length,distance+.1f))-WorldAt(Mathf.Max(0,distance-.1f))).normalized; }
 }
}
