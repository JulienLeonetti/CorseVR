using UnityEngine;

namespace CorseVR
{
 [DisallowMultipleComponent]
 public sealed class BiomePortal : MonoBehaviour
 {
  [SerializeField] Transform traveller;
  [SerializeField] Transform arrival;
  [SerializeField] BiomePortal linkedPortal;
  [SerializeField] Vector3[] destinationRoute;
  [SerializeField] float destinationHalfWidth=.76f;
  [SerializeField] string destinationBiome;
  bool armed;
  float blockedUntil;
  public int TraversalCount{get;private set;}
  public string DestinationBiome{get{return destinationBiome;}}

  public void Configure(Transform player,Transform destination,BiomePortal linked,Vector3[] points,float width,string biome){traveller=player;arrival=destination;linkedPortal=linked;destinationRoute=(Vector3[])points.Clone();destinationHalfWidth=width;destinationBiome=biome;}
  void Update(){CheckTraversal();}
  public bool CheckTraversal(){
   if(!Application.isPlaying||traveller==null||arrival==null||Time.realtimeSinceStartup<blockedUntil)return false;
   Vector3 p=transform.InverseTransformPoint(traveller.position);
   if(p.z<-.3f&&p.z>-3&&Mathf.Abs(p.x)<1.05f)armed=true;
   if(armed&&p.z>=-.10f&&p.z<.65f&&Mathf.Abs(p.x)<.93f&&Mathf.Abs(p.y)<.35f){Traverse();return true;}
   if(Mathf.Abs(p.x)>2||p.z<-4)armed=false;
   return false;
  }
  void Traverse(){
   var cc=traveller.GetComponent<CharacterController>();bool enabled=cc!=null&&cc.enabled;if(cc!=null)cc.enabled=false;
   traveller.SetPositionAndRotation(arrival.position,arrival.rotation);
   var corridor=traveller.GetComponent<SentierCorridor>();if(corridor!=null)corridor.Configure(destinationRoute,destinationHalfWidth);
   var explorer=traveller.GetComponent<DesktopFirstPersonExplorer>();if(explorer!=null)explorer.ResetAfterTeleport();
   Physics.SyncTransforms();if(cc!=null)cc.enabled=enabled;
   armed=false;blockedUntil=Time.realtimeSinceStartup+.9f;if(linkedPortal!=null){linkedPortal.armed=false;linkedPortal.blockedUntil=blockedUntil;}TraversalCount++;
  }
 }
}
