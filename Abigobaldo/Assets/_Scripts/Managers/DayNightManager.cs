using UnityEngine;

namespace Abigobaldo.Game
{
    [AddComponentMenu("Abigobaldo/Managers/Day Night Manager")]
    public sealed class DayNightManager : MonoBehaviour
    {
        public enum Period { Morning, Afternoon, Night }

        [SerializeField] private LightingManager lightingManager;
        [Header("Clock times")]
        [Range(0f, 24f)] [SerializeField] private float morningTime = 9f;
        [Range(0f, 24f)] [SerializeField] private float afternoonTime = 15f;
        [Range(0f, 24f)] [SerializeField] private float nightTime = 20.5f;
        [Header("Extra ambient presentation")]
        [SerializeField] private Color morningAmbient = new Color(0.58f, 0.68f, 0.82f, 1f);
        [SerializeField] private Color afternoonAmbient = new Color(0.72f, 0.48f, 0.31f, 1f);
        [SerializeField] private Color nightAmbient = new Color(0.08f, 0.11f, 0.22f, 1f);
        [SerializeField, Range(0f, 2f)] private float morningIntensity = 0.9f;
        [SerializeField, Range(0f, 2f)] private float afternoonIntensity = 0.75f;
        [SerializeField, Range(0f, 2f)] private float nightIntensity = 0.28f;

        public void SetPeriod(Period period)
        {
            if (lightingManager == null) lightingManager = FindObjectOfType<LightingManager>();
            float time = period == Period.Morning ? morningTime : period == Period.Afternoon ? afternoonTime : nightTime;
            if (lightingManager != null) lightingManager.SetClockTime(time);

            if (period == Period.Morning)
            {
                RenderSettings.ambientSkyColor = morningAmbient;
                RenderSettings.ambientIntensity = morningIntensity;
            }
            else if (period == Period.Afternoon)
            {
                RenderSettings.ambientSkyColor = afternoonAmbient;
                RenderSettings.ambientIntensity = afternoonIntensity;
            }
            else
            {
                RenderSettings.ambientSkyColor = nightAmbient;
                RenderSettings.ambientIntensity = nightIntensity;
            }
        }
    }
}
