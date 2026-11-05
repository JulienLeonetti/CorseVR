using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
namespace CorseVR {
 [ExecuteAlways,DisallowMultipleComponent]
 public sealed class TourDoubleDoor : MonoBehaviour {
  public AccueilDoor left;public Transform right;public TourPassageTransition passage;
  [SerializeField] Transform viewCamera;
  static readonly Vector3 Handle=new Vector3(.79f,1.12f,-.175f);
  public bool IsOpen { get { return left!=null&&left.IsOpen; } }
  public void Configure(Transform camera) { viewCamera=camera;left.enabled=false;left.Configure(camera);SyncLeaves(); }
  public bool CanInteract() {
   if(!viewCamera||!left)return false;
   var delta=left.transform.TransformPoint(Handle)-viewCamera.position;
   if(delta.magnitude>2.8f||Vector3.Angle(viewCamera.forward,delta)>58)return false;
   var controller=viewCamera.GetComponentInParent<CharacterController>();
   foreach(var hit in Physics.RaycastAll(viewCamera.position,delta.normalized,delta.magnitude+.16f,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance)) {
    if(controller&&(hit.transform==controller.transform||hit.transform.IsChildOf(controller.transform)))continue;
    return hit.transform==left.transform||hit.transform.IsChildOf(left.transform)||hit.transform==right||hit.transform.IsChildOf(right);
   }
   return true;
  }
  public bool PlayerInDoorway() {
   if(!viewCamera)return false;var controller=viewCamera.GetComponentInParent<CharacterController>();if(!controller)return false;
   var p=transform.InverseTransformPoint(controller.transform.position);return Mathf.Abs(p.x)<1.15f&&Mathf.Abs(p.z+2.63f)<.55f;
  }
  public void SetRequestedOpen(bool state) { if(left.RequestedOpen!=state)left.ToggleDoor(); }
  public bool TryToggle() { if(passage&&passage.enabled&&Application.isPlaying)return passage.Request(this); if(left.RequestedOpen&&PlayerInDoorway())return false;left.ToggleDoor();return true; }
  public bool GuardClosing() { if(!left.RequestedOpen&&PlayerInDoorway()&&Quaternion.Angle(left.transform.localRotation,Quaternion.identity)>5){left.ToggleDoor();return true;}return false; }
  public void ToggleDoor() { TryToggle(); }
  public void AdvanceAnimation(float seconds) { left.AdvanceAnimation(seconds);SyncLeaves(); }
  public void SyncLeaves() { if(left&&right)right.localRotation=Quaternion.Inverse(left.transform.localRotation); }
  void OnEnable() { if(left)left.enabled=false; }
  void Update() {
   if(!Application.isPlaying||!left)return;
   left.enabled=false;
   if(!viewCamera&&Camera.main)viewCamera=Camera.main.transform;
   if(Application.isFocused&&Keyboard.current!=null&&Keyboard.current.eKey.wasPressedThisFrame&&CanInteract())TryToggle();
   if(!passage||!passage.enabled){GuardClosing();AdvanceAnimation(Time.deltaTime);}
  }
  void LateUpdate() { SyncLeaves(); }
  void OnGUI() {
   if(!Application.isPlaying||!CanInteract())return;
   var style=new GUIStyle(GUI.skin.box);style.fontSize=18;style.alignment=TextAnchor.MiddleCenter;
   string text=passage&&passage.WaitingDoor==this?"Fermeture de la porte opposée…":left.RequestedOpen?(PlayerInDoorway()?"Reculez du seuil pour fermer":"E — Fermer la porte"):"E — Ouvrir la porte";
   GUI.Box(new Rect((Screen.width-300)*.5f,Screen.height-90,300,40),text,style);
  }
 }
}