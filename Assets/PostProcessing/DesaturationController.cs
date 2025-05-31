using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DesaturationController : MonoBehaviour
{
    public Volume volume;
    private ColorAdjustments colorAdjustments;

    [Range(0, 100)]
    public float inputValue = 100;

    private void Start()
    {
        if (volume.profile.TryGet(out colorAdjustments))
        {
            UpdateSaturation();
        }
    }

    private void Update()
    {
        UpdateSaturation();
    }

    void UpdateSaturation()
    {
        float t = Mathf.InverseLerp(20f, 0f, inputValue);
        float saturation = Mathf.Lerp(0f, -100f, t);
        colorAdjustments.saturation.value = saturation;
    }
}
