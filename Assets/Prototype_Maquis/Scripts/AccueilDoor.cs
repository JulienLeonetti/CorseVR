using UnityEngine;
using UnityEngine.InputSystem;
namespace CorseVR {
 public sealed class AccueilDoor : MonoBehaviour {
  [SerializeField] Transform playerCamera;
  [SerializeField] float openAngle=-105f;
  [SerializeField] float angularSpeed=100;
  [SerializeField] Vector3 handleLocalPosition=new Vector3(1.56f,1.13f,-.12f);
  float angle;bool requestedOpen;
  public bool IsOpen{get{return angle<-95;}}
  public bool RequestedOpen{get{return requestedOpen;}}
  public void Configure(Transform camera){playerCamera=camera;angle=0;requestedOpen=false;transform.localRotation=Quaternion.identity;}
  readonly RaycastHit[] interactionHits = new RaycastHit[16];
  BoxCollider leaf;
  CharacterController viewer;
  bool VisiblePoint(Vector3 target){
   var delta=target-playerCamera.position;
   if(delta.sqrMagnitude>2.8f*2.8f || Vector3.Angle(playerCamera.forward,delta)>58f)return false;
   int count=Physics.RaycastNonAlloc(playerCamera.position,delta.normalized,interactionHits,delta.magnitude+.025f,~0,QueryTriggerInteraction.Ignore);
   if(count==interactionHits.Length)return false;
   float first=float.PositiveInfinity;Transform obstacle=null;
   for(int i=0;i<count;i++){
    var hit=interactionHits[i];
    if(viewer && (hit.transform==viewer.transform || hit.transform.IsChildOf(viewer.transform)))continue;
    if(hit.distance<first){first=hit.distance;obstacle=hit.transform;}
   }
   return !obstacle || obstacle==transform || obstacle.IsChildOf(transform);
  }
  public bool CanInteract(){
   if(!playerCamera)return false;
   if(!viewer)viewer=playerCamera.GetComponentInParent<CharacterController>();
   if(!leaf)leaf=GetComponent<BoxCollider>();
   if(VisiblePoint(transform.TransformPoint(handleLocalPosition)))return true;
   if(!leaf)return false;
   if(VisiblePoint(transform.TransformPoint(leaf.center)))return true;
   // Once folded back, the opening is still reachable from either side.
   if(Mathf.Abs(angle)<45f || !transform.parent)return false;
   Vector3 opening=transform.parent.TransformPoint(transform.localPosition+leaf.center);
   return VisiblePoint(opening);
  }
  public void ToggleDoor(){requestedOpen=!requestedOpen;}
  public void AdvanceAnimation(float seconds){angle=Mathf.MoveTowards(angle,requestedOpen?openAngle:0,angularSpeed*Mathf.Max(0,seconds));transform.localRotation=Quaternion.Euler(0,angle,0);}
  void Update(){if(Application.isFocused&&Keyboard.current!=null&&Keyboard.current.eKey.wasPressedThisFrame&&CanInteract())ToggleDoor();AdvanceAnimation(Time.deltaTime);}
  void OnGUI(){if(!CanInteract())return;var style=new GUIStyle(GUI.skin.box);style.fontSize=18;style.alignment=TextAnchor.MiddleCenter;GUI.Box(new Rect((Screen.width-300)*.5f,Screen.height-90,300,40),requestedOpen?"E — Fermer la porte":"E — Ouvrir la porte",style);}
 }
}


