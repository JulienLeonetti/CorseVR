using System.Collections.Generic;
using UnityEngine;
namespace CorseVR {
public static class CoastalRouteDefinition {
 static Vector2[] line;static float[] distances;
 static void Ensure(){if(line!=null)return;
  Vector2[] a={new Vector2(220,-7),new Vector2(220,-4),new Vector2(220,-1),new Vector2(221,3),new Vector2(219.2f,7),new Vector2(218.7f,11),new Vector2(222,15),new Vector2(223.4f,19),new Vector2(221.3f,23),new Vector2(220.5f,27),new Vector2(222,31)};
  var result=new List<Vector2>();for(int i=1;i<a.Length-2;i++)for(int j=0;j<20;j++){float t=j/20f;result.Add(.5f*((2*a[i])+(-a[i-1]+a[i+1])*t+(2*a[i-1]-5*a[i]+4*a[i+1]-a[i+2])*t*t+(-a[i-1]+3*a[i]-3*a[i+1]+a[i+2])*t*t*t));}result.Add(a[a.Length-2]);
  line=result.ToArray();distances=new float[line.Length];for(int i=1;i<line.Length;i++)distances[i]=distances[i-1]+Vector2.Distance(line[i-1],line[i]);
 }
 public static float Length{get{Ensure();return distances[distances.Length-1];}}
 public static Vector2 Point(float s){Ensure();s=Mathf.Clamp(s,0,Length);for(int i=1;i<line.Length;i++)if(distances[i]>=s)return Vector2.Lerp(line[i-1],line[i],Mathf.InverseLerp(distances[i-1],distances[i],s));return line[line.Length-1];}
 public static Vector3 Forward(float s){var p=Point(s+.15f)-Point(s-.15f);return new Vector3(p.x,0,p.y).normalized;}
 public static Vector2 Side(float s,float offset){var p=Point(s);var f=Forward(s);return p+new Vector2(f.z,-f.x)*offset;}
 public static float Distance(float x,float z){Ensure();var p=new Vector2(x,z);float best=10000;for(int i=1;i<line.Length;i++){var ab=line[i]-line[i-1];var q=line[i-1]+ab*Mathf.Clamp01(Vector2.Dot(p-line[i-1],ab)/ab.sqrMagnitude);best=Mathf.Min(best,(p-q).sqrMagnitude);}return Mathf.Sqrt(best);}
 public static float Shore(float z){return 230+2.8f*Mathf.Sin(z*.17f)+1.3f*Mathf.Sin(z*.43f+1);}
 public static float Height(float x,float z){
  float route=5.4f+.018f*(z+4)+.45f*Mathf.Sin((z+4)*.14f);
  float cliff=Shore(z)-x;float inland=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(216,220,x));float land=route+inland*Mathf.SmoothStep(0,1,Mathf.InverseLerp(2,8,Distance(x,z)))*(1.6f+1.1f*Mathf.PerlinNoise(x*.17f,z*.17f));
  return Mathf.Lerp(-.8f,land,Mathf.SmoothStep(0,1,Mathf.InverseLerp(-.6f,4.5f,cliff)));
 }
 public static Vector3[] Corridor(){Ensure();var result=new Vector3[line.Length];for(int i=0;i<line.Length;i++)result[i]=new Vector3(line[i].x,0,line[i].y);return result;}
}
}
