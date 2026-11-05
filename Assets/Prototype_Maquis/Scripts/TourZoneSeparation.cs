using UnityEngine;
namespace CorseVR {
 [ExecuteAlways,DisallowMultipleComponent,DefaultExecutionOrder(200)]
 public sealed class TourZoneSeparation : MonoBehaviour {
  public enum Side { Maquis,Littoral }
  public Transform player;public TourPassageTransition passage;
  public Renderer[] maquisRenderers,littoralRenderers;
  public bool[] maquisInitial,littoralInitial;
  public Collider[] maquisColliders,littoralColliders; public bool[] maquisColliderInitial,littoralColliderInitial;
  [Tooltip("Authoring only: show both environments while editing. Runtime remains separated.")] public bool previewBothZones;
  [SerializeField] Side currentSide=Side.Maquis;
  Vector3 previousLocal;bool hasPrevious;
  public Side CurrentSide { get { return currentSide; } }
  public int SwitchCount { get; private set; }
  void Start() { if(!Application.isPlaying)return;var p=transform.InverseTransformPoint(player.position);ApplySide(p.z>1.8f?Side.Littoral:Side.Maquis);SwitchCount=0;if(passage)passage.Initialize(); }
  void LateUpdate() { if(!player)return;if(Application.isPlaying)Tick();else if(!previewBothZones){Set(maquisRenderers,maquisInitial,currentSide==Side.Maquis);Set(littoralRenderers,littoralInitial,currentSide==Side.Littoral);SetPhysics(currentSide);} }
  public void Tick() {
   if(passage&&passage.enabled&&Application.isPlaying){passage.Advance(Time.deltaTime);return;}
   var p=transform.InverseTransformPoint(player.position);
   if(!hasPrevious){previousLocal=p;hasPrevious=true;return;}
   float plane=currentSide==Side.Maquis?-.32f:.32f;
   bool crossed=currentSide==Side.Maquis?previousLocal.x>=plane&&p.x<plane:previousLocal.x<=plane&&p.x>plane;
   if(crossed) {
    float t=Mathf.InverseLerp(previousLocal.x,p.x,plane);
    float crossingZ=Mathf.Lerp(previousLocal.z,p.z,t);
    // Both portals are occluded along these two planes; no camera or player transform changes.
    if(Mathf.Abs(crossingZ)<.55f)ApplySide(currentSide==Side.Maquis?Side.Littoral:Side.Maquis);
   }
   previousLocal=p;hasPrevious=true;
  }
  public void ApplySide(Side side) {
   if(currentSide!=side)SwitchCount++;
   currentSide=side;SetPhysics(side);Set(maquisRenderers,maquisInitial,side==Side.Maquis);Set(littoralRenderers,littoralInitial,side==Side.Littoral);
   if(player){previousLocal=transform.InverseTransformPoint(player.position);hasPrevious=true;}
  }
  static void Set(Renderer[] renderers,bool[] initial,bool visible) { if(renderers==null)return;for(int i=0;i<renderers.Length;i++)if(renderers[i])renderers[i].enabled=visible&&(initial==null||i>=initial.Length||initial[i]); }
  static void SetCollision(Collider[] colliders,bool[] initial,bool visible){if(colliders==null)return;for(int i=0;i<colliders.Length;i++)if(colliders[i])colliders[i].enabled=visible&&(initial==null||i>=initial.Length||initial[i]);}
  void SetPhysics(Side side){SetCollision(maquisColliders,maquisColliderInitial,side==Side.Maquis);SetCollision(littoralColliders,littoralColliderInitial,side==Side.Littoral);}
  public void RestoreAll() { SetCollision(maquisColliders,maquisColliderInitial,true);SetCollision(littoralColliders,littoralColliderInitial,true); Set(maquisRenderers,maquisInitial,true);Set(littoralRenderers,littoralInitial,true); }
  void OnDisable() { RestoreAll(); }
 }
}