using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeController : MonoBehaviour
{
    [SerializeField]
    private List<Upgrade> _upgrades = new();

    public List<Upgrade> availableUpgrades;

    void Awake()
    {
        availableUpgrades = Resources.LoadAll<Upgrade>("Upgrades").ToList();
        Debug.Log($"Loaded {availableUpgrades.Count} upgrades.");

        // Load upgrades from Save eventually
    }

    void Start()
    {
        ApplyUpgradeBuffs();
    }

    public void AddUpgrade(Upgrade upgrade)
    {
        if (upgrade != null && !_upgrades.Contains(upgrade))
        {
            _upgrades.Add(upgrade);
        }
        ApplyUpgradeBuffs();
    }

    public void RemoveUpgrade(Upgrade upgrade)
    {
        if (upgrade != null)
        {
            _upgrades.Remove(upgrade);
        }
        ApplyUpgradeBuffs();
    }

    public Upgrade GetUpgradeById(int id)
    {
        return availableUpgrades.Find(upg => upg != null && upg.id == id);
    }

    public void ApplyUpgradeBuffs()
    {
        UpgradeBuff totalBuff = ScriptableObject.CreateInstance<UpgradeBuff>();

        foreach (var upgrade in _upgrades)
        {
            if (upgrade != null && upgrade.upgradeBuff != null)
            {
                totalBuff.jumpHeight = Mathf.Max(totalBuff.jumpHeight, upgrade.upgradeBuff.jumpHeight);
                totalBuff.maxOxygenAstronaut = Mathf.Max(totalBuff.maxOxygenAstronaut, upgrade.upgradeBuff.maxOxygenAstronaut);
                totalBuff.maxOxygenRocket = Mathf.Max(totalBuff.maxOxygenRocket, upgrade.upgradeBuff.maxOxygenRocket);
                totalBuff.maxFuelRocket = Mathf.Max(totalBuff.maxFuelRocket, upgrade.upgradeBuff.maxFuelRocket);
                totalBuff.maxSpeedRocket = Mathf.Max(totalBuff.maxSpeedRocket, upgrade.upgradeBuff.maxSpeedRocket);
                totalBuff.accelerationRocket = Mathf.Max(totalBuff.accelerationRocket, upgrade.upgradeBuff.accelerationRocket);
            }
        }

        Astronaut astronaut = FindFirstObjectByType<Astronaut>();
        Rocket rocket = FindFirstObjectByType<Rocket>();
        if (astronaut != null && rocket != null)
        {
            astronaut.ApplyStats(totalBuff);
            rocket.ApplyStats(totalBuff);
        }
        else
        {
            Debug.LogWarning("Astronaut or Rocket not found to apply upgrade buffs.");
        }
    }
}
