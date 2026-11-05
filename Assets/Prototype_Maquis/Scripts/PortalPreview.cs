using UnityEngine;
namespace CorseVR {
 [DisallowMultipleComponent]
 public sealed class PortalPreview : MonoBehaviour {
  [SerializeField] Camera previewCamera;
  [SerializeField] Transform viewer;
  float nextFrame;
  public void Configure(Camera camera,Transform player){previewCamera=camera;viewer=player;}
  void LateUpdate(){
   if(previewCamera==null||viewer==null||Time.unscaledTime<nextFrame)return;
   if((viewer.position-transform.position).sqrMagnitude<144){previewCamera.Render();nextFrame=Time.unscaledTime+.12f;}
  }
 }
}
