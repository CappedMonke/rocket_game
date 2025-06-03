using UnityEngine;
using UnityEngine.UIElements;

public class RocketUI : MonoBehaviour
{
    private VisualElement root;

    private VisualElement HealthBar;
    private VisualElement FuelBar;
    private VisualElement OxygenBar;
    private VisualElement ThrustStrengthBar;
    private VisualElement AltitudeBar;

    private void Awake()
    {
        InitializeUI();
    }

    private void OnEnable()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        HealthBar = root.Q<VisualElement>("Health");
        FuelBar = root.Q<VisualElement>("Fuel");
        OxygenBar = root.Q<VisualElement>("Oxygen");
        ThrustStrengthBar = root.Q<VisualElement>("Acceleration");
        AltitudeBar = root.Q<VisualElement>("Altitude");
    }

    public void SetHealth(float value, float maxValue)
    {
        if (HealthBar != null)
        {
            HealthBar.style.width = new StyleLength(new Length(value / maxValue * 100, LengthUnit.Percent));
        }
    }

    public void SetFuel(float value, float maxValue)
    {
        if (FuelBar != null)
        {
            FuelBar.style.width = new StyleLength(new Length(value / maxValue * 100, LengthUnit.Percent));
        }
    }

    public void SetOxygen(float value, float maxValue)
    {
        if (OxygenBar != null)
        {
            OxygenBar.style.width = new StyleLength(new Length(value / maxValue * 100, LengthUnit.Percent));
        }
    }

    public void SetThrustStrength(float value, float minValue, float maxValue)
    {
        if (ThrustStrengthBar != null)
        {
            float normalizedValue = (value - minValue) / (maxValue - minValue);
            ThrustStrengthBar.style.width = new StyleLength(new Length(normalizedValue * 100, LengthUnit.Percent));
        }
    }

    public void SetAltitude(float value, float goalValue)
    {
        if (AltitudeBar != null)
        {
            AltitudeBar.style.width = new StyleLength(new Length(value / goalValue * 100, LengthUnit.Percent));
        }
    }
}
