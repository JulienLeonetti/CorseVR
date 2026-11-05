using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CorseVR
{
    [ExecuteAlways, DisallowMultipleComponent]
    public sealed class MaquisRenderProfile : MonoBehaviour
    {
        [SerializeField] private UniversalRenderPipelineAsset profile;
        [SerializeField] private RenderPipelineAsset fallbackProfile;
        private RenderPipelineAsset previous;
        private int previousAA;
        private bool applied;

        private void OnEnable() { Apply(); }

        public void Apply()
        {
            if (profile == null) return;
            if (!applied)
            {
                bool alreadySelected = QualitySettings.renderPipeline == profile;
                previous = alreadySelected ? fallbackProfile : QualitySettings.renderPipeline;
                previousAA = alreadySelected ? 0 : QualitySettings.antiAliasing;
            }
            QualitySettings.renderPipeline = profile;
            QualitySettings.antiAliasing = profile.msaaSampleCount;
            applied = true;
        }

        private void OnDisable()
        {
            if (applied && QualitySettings.renderPipeline == profile)
            {
                QualitySettings.renderPipeline = previous;
                QualitySettings.antiAliasing = previousAA;
            }
            applied = false;
        }
    }
}
