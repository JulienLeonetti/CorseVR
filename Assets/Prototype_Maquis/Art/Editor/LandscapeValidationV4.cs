using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using CorseVR;
namespace CorseVR.Editor {
public static class LandscapeValidationV4 {
 static GameObject player;static DesktopFirstPersonExplorer explorer;static MethodInfo move;
 static void Setup(){if(!Application.isPlaying)throw new Exception("Play mode required");player=GameObject.Find("Joueur_Desktop");explorer=player.GetComponent<DesktopFirstPersonExplorer>();explorer.enabled=false;move=typeof(DesktopFirstPersonExplorer).GetMethod("MovePlayer",BindingFlags.NonPublic|BindingFlags.Instance);}
 static void Step(Vector3 target){var d=target-player.transform.position;d.y=0;if(d.sqrMagnitude<.0001f)return;player.transform.rotation=Quaternion.LookRotation(d);move.Invoke(explorer,new object[]{Vector2.up,.02f});}
 static float Horizontal(Vector3 p,Vector3 q){p.y=0;q.y=0;return Vector3.Distance(p,q);}
 public static string WalkForestAndEnter(){
  Setup();var cc=player.GetComponent<CharacterController>();cc.enabled=false;var start=MaquisRouteDefinition.Point(0);player.transform.position=new Vector3(start.x,MaquisZigzagBuilder.Height(start.x,start.y)+.035f,start.y);player.GetComponent<SentierCorridor>().Configure(MaquisRouteDefinition.Corridor(),.76f);cc.enabled=true;explorer.ResetAfterTeleport();Physics.SyncTransforms();
  var gate=GameObject.Find("Portail_Vers_Littoral").GetComponent<BiomePortal>();float maxError=0;int steps=0;
  for(float s=.1f;s<MaquisRouteDefinition.Length-.5f;s+=.1f){var p=MaquisRouteDefinition.Point(s);var target=new Vector3(p.x,0,p.y);for(int j=0;j<12&&Horizontal(player.transform.position,target)>.061f;j++){Step(target);steps++;}float error=Horizontal(player.transform.position,target);maxError=Mathf.Max(error,maxError);if(error>.16f)return "FAIL forest blocked at "+s+"m, error="+error+" pos="+player.transform.position;gate.CheckTraversal();}
  var end=gate.transform.position+gate.transform.forward*.15f;for(int j=0;j<60&&player.transform.position.x<100;j++){Step(end);gate.CheckTraversal();steps++;}
  return "Forest max tracking error="+maxError+" steps="+steps+" gate count="+gate.TraversalCount+" position="+player.transform.position+" corridor="+player.GetComponent<SentierCorridor>().HalfWidth;
 }
 public static string WalkCoastAndReturn(){
  Setup();if(player.transform.position.x<100)return "FAIL not on coast";
  float maxError=0,maxGroundError=0;int steps=0;
  for(float s=2.3f;s<CoastalRouteDefinition.Length-.3f;s+=.1f){var p=CoastalRouteDefinition.Point(s);var target=new Vector3(p.x,0,p.y);for(int j=0;j<12&&Horizontal(player.transform.position,target)>.061f;j++){Step(target);steps++;}float error=Horizontal(player.transform.position,target);maxError=Mathf.Max(error,maxError);maxGroundError=Mathf.Max(maxGroundError,Mathf.Abs(player.transform.position.y-CoastalRouteDefinition.Height(player.transform.position.x,player.transform.position.z)));if(error>.16f)return "FAIL coast blocked at "+s+" error="+error+" pos="+player.transform.position;}
  for(float s=CoastalRouteDefinition.Length-.3f;s>1;s-=.1f){var p=CoastalRouteDefinition.Point(s);var target=new Vector3(p.x,0,p.y);for(int j=0;j<12&&Horizontal(player.transform.position,target)>.061f;j++){Step(target);steps++;}}
  var gate=GameObject.Find("Portail_Retour_Maquis").GetComponent<BiomePortal>();var end=gate.transform.position+gate.transform.forward*.15f;
  for(int j=0;j<60&&player.transform.position.x>100;j++){Step(end);gate.CheckTraversal();steps++;}
  return "Coast max tracking error="+maxError+" max ground difference="+maxGroundError+" steps="+steps+" return gate count="+gate.TraversalCount+" position="+player.transform.position+" corridor="+player.GetComponent<SentierCorridor>().HalfWidth;
 }
 public static string SideLimits(bool coastal){
  Setup();var cc=player.GetComponent<CharacterController>();var corridor=player.GetComponent<SentierCorridor>();corridor.Configure(coastal?CoastalRouteDefinition.Corridor():MaquisRouteDefinition.Corridor(),coastal?.78f:.76f);float max=0;
  foreach(float s in new[]{3f,9f,17f,25f,31f})foreach(int side in new[]{-1,1}){
   var p=coastal?CoastalRouteDefinition.Point(s):MaquisRouteDefinition.Point(s);cc.enabled=false;player.transform.position=new Vector3(p.x,(coastal?CoastalRouteDefinition.Height(p.x,p.y):MaquisZigzagBuilder.Height(p.x,p.y))+.035f,p.y);player.transform.rotation=Quaternion.LookRotation(coastal?CoastalRouteDefinition.Forward(s):MaquisRouteDefinition.Forward(s));cc.enabled=true;explorer.ResetAfterTeleport();Physics.SyncTransforms();for(int j=0;j<85;j++)move.Invoke(explorer,new object[]{new Vector2(side,0),.02f});float d=coastal?CoastalRouteDefinition.Distance(player.transform.position.x,player.transform.position.z):MaquisRouteDefinition.Distance(player.transform.position.x,player.transform.position.z);max=Mathf.Max(max,d);
  }
  return (coastal?"Coast":"Forest")+" max lateral distance="+max+" expected <= "+corridor.HalfWidth;
 }
}
}
