using UnityEngine;
using UnityEngine.UIElements;

public class AstronautUI : MonoBehaviour
{
    private VisualElement root;

    private VisualElement HealthBar;
    private VisualElement OxygenBar;

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
        OxygenBar = root.Q<VisualElement>("Oxygen");
    }

    public void SetHealth(float value, float maxValue)
    {
        if (HealthBar != null)
        {
            HealthBar.style.width = new StyleLength(new Length(value / maxValue * 100, LengthUnit.Percent));
        }
    }

    public void SetOxygen(float value, float maxValue)
    {
        if (OxygenBar != null)
        {
            OxygenBar.style.width = new StyleLength(new Length(value / maxValue * 100, LengthUnit.Percent));
        }
    }
}
