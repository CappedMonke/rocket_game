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
    }

    public void AddUpgrade(Upgrade upgrade)
    {
        if (upgrade != null && !_upgrades.Contains(upgrade))
        {
            _upgrades.Add(upgrade);
        }
    }

    public void RemoveUpgrade(Upgrade upgrade)
    {
        if (upgrade != null)
        {
            _upgrades.Remove(upgrade);
        }
    }

    public Upgrade GetUpgradeById(int id)
    {
        return availableUpgrades.Find(upg => upg != null && upg.id == id);
    }
}
