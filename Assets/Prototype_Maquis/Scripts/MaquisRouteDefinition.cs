using System.Collections.Generic;
using UnityEngine;

namespace CorseVR
{
 public static class MaquisRouteDefinition
 {
  static Vector2[] line;
  static float[] cumulative;
  static void Ensure(){
   if(line!=null)return;
   Vector2[] p={new Vector2(-4,-15),new Vector2(-3.5f,-12),new Vector2(-3.4f,-10),new Vector2(-.8f,-7),new Vector2(1.3f,-4.4f),new Vector2(-.5f,-1.9f),new Vector2(-3.7f,.8f),new Vector2(-3,3.5f),new Vector2(.7f,6),new Vector2(3.2f,8.6f),new Vector2(2.5f,11),new Vector2(.8f,13.2f),new Vector2(1.2f,15.3f),new Vector2(1.6f,18.3f)};
   var points=new List<Vector2>();var lengths=new List<float>();float total=0;
   for(int i=1;i<p.Length-2;i++)for(int j=0;j<24;j++){
    float t=j/24f,t2=t*t,t3=t2*t;
    Vector2 v=.5f*((2*p[i])+(-p[i-1]+p[i+1])*t+(2*p[i-1]-5*p[i]+4*p[i+1]-p[i+2])*t2+(-p[i-1]+3*p[i]-3*p[i+1]+p[i+2])*t3);
    if(points.Count>0)total+=Vector2.Distance(v,points[points.Count-1]);points.Add(v);lengths.Add(total);
   }
   total+=Vector2.Distance(points[points.Count-1],p[p.Length-2]);points.Add(p[p.Length-2]);lengths.Add(total);line=points.ToArray();cumulative=lengths.ToArray();
  }
  public static float Length{get{Ensure();return cumulative[cumulative.Length-1];}}
  public static Vector2 Point(float s){Ensure();if(s<=0)return line[0];for(int i=1;i<line.Length;i++)if(cumulative[i]>=s)return Vector2.Lerp(line[i-1],line[i],Mathf.InverseLerp(cumulative[i-1],cumulative[i],s));return line[line.Length-1];}
  public static Vector3 Forward(float s){Vector2 d=Point(Mathf.Clamp(s+.18f,0,Length))-Point(Mathf.Clamp(s-.18f,0,Length));if(d.sqrMagnitude<.00001f)d=Vector2.up;return new Vector3(d.x,0,d.y).normalized;}
  public static Vector2 Side(float s,float offset){var f=Forward(Mathf.Clamp(s,.2f,Length-.2f));Vector2 p=Point(s);if(s<0)p+=new Vector2(f.x,f.z)*s;if(s>Length)p+=new Vector2(f.x,f.z)*(s-Length);return p+new Vector2(f.z,-f.x)*offset;}
  public static float Distance(float x,float z){Ensure();Vector2 p=new Vector2(x,z);float best=float.PositiveInfinity;for(int i=1;i<line.Length;i++){var a=line[i-1];var ab=line[i]-a;var q=a+ab*Mathf.Clamp01(Vector2.Dot(p-a,ab)/Mathf.Max(ab.sqrMagnitude,.000001f));best=Mathf.Min(best,(p-q).sqrMagnitude);}return Mathf.Sqrt(best);}
  public static Vector3[] Corridor(){Ensure();var r=new Vector3[line.Length];for(int i=0;i<line.Length;i++)r[i]=new Vector3(line[i].x,0,line[i].y);return r;}
  public static float Grade(float x,float z){return .018f*(z+12)+.012f*Mathf.Sin(z*.45f)+.006f*(Mathf.PerlinNoise(x*2+19,z*2+38)-.5f);}
 }
}
