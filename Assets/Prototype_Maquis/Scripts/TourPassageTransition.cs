using UnityEngine;
namespace CorseVR {
 [DisallowMultipleComponent]
 public sealed class TourPassageTransition : MonoBehaviour {
  public TourDoubleDoor maquisDoor,littoralDoor;
  public TourZoneSeparation zones;
  public Transform player;
  TourDoubleDoor pending,entryDoor;float closedDwell;
  public TourDoubleDoor WaitingDoor { get { return pending; } }
  public int CompletedTransfers { get; private set; }
  public bool InsideRoom { get { if(!player)return false;var p=transform.InverseTransformPoint(player.position);return Mathf.Abs(p.z)<2.4f&&new Vector2(p.x,p.z).magnitude<3.2f; } }
  public static float Angle(TourDoubleDoor d) { return Quaternion.Angle(d.left.transform.localRotation,Quaternion.identity); }
  public bool BothClosed { get { return !maquisDoor.left.RequestedOpen&&!littoralDoor.left.RequestedOpen&&Angle(maquisDoor)<.05f&&Angle(littoralDoor)<.05f; } }
  public void Initialize() {
   pending=null;entryDoor=null;closedDwell=0;CompletedTransfers=0;
   var view=Camera.main?Camera.main.transform:null;
   maquisDoor.Configure(view);littoralDoor.Configure(view);
  }
  public bool Request(TourDoubleDoor door) {
   if(door!=maquisDoor&&door!=littoralDoor)return false;
   if(pending==door){pending=null;closedDwell=0;return true;}
   if(door.left.RequestedOpen) {
    if(door.PlayerInDoorway())return false;
    door.SetRequestedOpen(false);if(entryDoor==door)entryDoor=null;return true;
   }
   var wanted=door==maquisDoor?TourZoneSeparation.Side.Maquis:TourZoneSeparation.Side.Littoral;
   var p=transform.InverseTransformPoint(player.position);
   if(!InsideRoom&&wanted!=zones.CurrentSide)return false;
   var other=door==maquisDoor?littoralDoor:maquisDoor;
   if(!InsideRoom)entryDoor=door;
   if(Angle(other)<.05f&&!other.left.RequestedOpen&&wanted==zones.CurrentSide) {
    pending=null;closedDwell=0;door.SetRequestedOpen(true);return true;
   }
   pending=door;closedDwell=0;door.SetRequestedOpen(false);
   if(!other.PlayerInDoorway())other.SetRequestedOpen(false);
   return true;
  }
  public void Advance(float seconds) {
   float dt=Mathf.Max(0,seconds);var p=transform.InverseTransformPoint(player.position);
   if(entryDoor&&InsideRoom&&Mathf.Abs(p.z)<1.65f){entryDoor.SetRequestedOpen(false);entryDoor=null;}
   if(pending) {
    if(pending==littoralDoor&&p.z< -3.35f||pending==maquisDoor&&p.z>3.35f){pending=null;closedDwell=0;}
    else {
     var other=pending==maquisDoor?littoralDoor:maquisDoor;
     pending.SetRequestedOpen(false);
     if(!other.PlayerInDoorway())other.SetRequestedOpen(false);
    }
   }
   maquisDoor.GuardClosing();littoralDoor.GuardClosing();
   maquisDoor.AdvanceAnimation(dt);littoralDoor.AdvanceAnimation(dt);
   if(pending&&BothClosed&&InsideRoom) {
    closedDwell+=dt;
    if(closedDwell>=.12f) {
     var wanted=pending==maquisDoor?TourZoneSeparation.Side.Maquis:TourZoneSeparation.Side.Littoral;
     if(zones.CurrentSide!=wanted){zones.ApplySide(wanted);CompletedTransfers++;}
     var door=pending;pending=null;closedDwell=0;door.SetRequestedOpen(true);
    }
   }else closedDwell=0;
  }
 }
}
