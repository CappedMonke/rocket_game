using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeBuff", menuName = "Game/UpgradeBuff")]
public class UpgradeBuff : ScriptableObject
{

    [Tooltip("Sprunghöhe")]
    public float jumpHeight;

    [Tooltip("Max. Oxygen Astronaut")]
    public int maxOxygenAstronaut;

    [Tooltip("Max. Oxygen Rocket")]
    public int maxOxygenRocket;

    [Tooltip("Max. Fuel Rocket")]
    public int maxFuelRocket;

    [Tooltip("Max. Speed Rocket")]
    public float maxSpeedRocket;

    [Tooltip("Acceleration Rocket")]
    public float accelerationRocket;
}