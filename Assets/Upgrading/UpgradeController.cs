using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeController : MonoBehaviour
{
    [SerializeField]
    private List<Upgrade> _upgrades = new();

    private UpgradeBuff _queuedAstronautBuff;
    private UpgradeBuff _queuedRocketBuff;
    private bool _hasQueuedAstronautBuff = false;
    private bool _hasQueuedRocketBuff = false;

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

    void LateUpdate()
    {
        if (_hasQueuedAstronautBuff)
        {
            Astronaut astronaut = FindFirstObjectByType<Astronaut>();
            if (astronaut != null)
            {
                astronaut.ApplyStats(_queuedAstronautBuff);
                _queuedAstronautBuff = null;
                _hasQueuedAstronautBuff = false;
                Debug.Log("Applied queued astronaut upgrade buffs.");
            }
        }
        if (_hasQueuedRocketBuff)
        {
            Rocket rocket = FindFirstObjectByType<Rocket>();
            if (rocket != null)
            {
                rocket.ApplyStats(_queuedRocketBuff);
                _queuedRocketBuff = null;
                _hasQueuedRocketBuff = false;
                Debug.Log("Applied queued rocket upgrade buffs.");
            }
        }
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
        if (astronaut != null)
        {
            astronaut.ApplyStats(totalBuff);
        }
        else
        {
            Debug.LogWarning("Astronaut not found to apply upgrade buffs. Queueing upgrade buffs for later.");
            QueueAstronautUpgradeBuffs(totalBuff);
        }

        if (rocket != null)
        {
            rocket.ApplyStats(totalBuff);
        }
        else
        {
            Debug.LogWarning("Rocket not found to apply upgrade buffs. Queueing upgrade buffs for later.");
            QueueRocketUpgradeBuffs(totalBuff);
        }
    }

    public List<Upgrade> GetAcquiredUpgrades()
    {
        return _upgrades;
    }

    private void QueueAstronautUpgradeBuffs(UpgradeBuff buff)
    {
        if (_queuedAstronautBuff == null)
        {
            _queuedAstronautBuff = buff;
        }
        else
        {
            _queuedAstronautBuff.jumpHeight = Mathf.Max(_queuedAstronautBuff.jumpHeight, buff.jumpHeight);
            _queuedAstronautBuff.maxOxygenAstronaut = Mathf.Max(_queuedAstronautBuff.maxOxygenAstronaut, buff.maxOxygenAstronaut);
        }
        _hasQueuedAstronautBuff = true;
    }

    private void QueueRocketUpgradeBuffs(UpgradeBuff buff)
    {
        if (_queuedRocketBuff == null)
        {
            _queuedRocketBuff = buff;
        }
        else
        {
            _queuedRocketBuff.maxOxygenRocket = Mathf.Max(_queuedRocketBuff.maxOxygenRocket, buff.maxOxygenRocket);
            _queuedRocketBuff.maxFuelRocket = Mathf.Max(_queuedRocketBuff.maxFuelRocket, buff.maxFuelRocket);
            _queuedRocketBuff.maxSpeedRocket = Mathf.Max(_queuedRocketBuff.maxSpeedRocket, buff.maxSpeedRocket);
            _queuedRocketBuff.accelerationRocket = Mathf.Max(_queuedRocketBuff.accelerationRocket, buff.accelerationRocket);
        }
        _hasQueuedRocketBuff = true;
    }
}
