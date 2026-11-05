using UnityEngine;
namespace CorseVR {
 public sealed class AccueilArtLighting:MonoBehaviour {
  [SerializeField] Transform door;[SerializeField] Light thresholdLight;
  public void Configure(Transform hinge,Light light){door=hinge;thresholdLight=light;UpdateLight();}
  public void UpdateLight(){if(door==null||thresholdLight==null)return;float open=Mathf.Abs(Mathf.DeltaAngle(0,door.localEulerAngles.y))/105;thresholdLight.intensity=Mathf.Lerp(.15f,5.5f,Mathf.Clamp01(open));}
  void LateUpdate(){UpdateLight();}
 }
}
