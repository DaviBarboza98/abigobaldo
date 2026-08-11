using UnityEngine;

namespace Abigobaldo.Demo
{
    public class DemoPerformanceBootstrap : MonoBehaviour
    {
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private float fixedDeltaTime = 0.02f;
        [SerializeField] private float shadowDistance = 35f;
        [SerializeField] private int pixelLightCount = 2;

        private void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFrameRate;
            Time.fixedDeltaTime = fixedDeltaTime;
            QualitySettings.shadowDistance = shadowDistance;
            QualitySettings.pixelLightCount = pixelLightCount;
        }

        private void OnValidate()
        {
            targetFrameRate = Mathf.Clamp(targetFrameRate, 30, 144);
            fixedDeltaTime = Mathf.Clamp(fixedDeltaTime, 0.01f, 0.05f);
            shadowDistance = Mathf.Max(0f, shadowDistance);
            pixelLightCount = Mathf.Clamp(pixelLightCount, 0, 8);
        }
    }
}
