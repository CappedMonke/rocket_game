using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade", menuName = "Game/Upgrade")]
public class Upgrade : ScriptableObject
{
    public int id;
    public string displayName;

    [Tooltip("Upgrades, die freigeschaltet sein müssen.")]
    public List<Upgrade> prerequisites;

    [Tooltip("Benötigte Ressourcen (Item, Menge). Einfach so tun als wären die Listen ein Dictionary.")]
    public List<ItemData> costItems;
    public List<int> costAmounts;

    public Dictionary<ItemData, int> GetCostDictionary()
    {
        Dictionary<ItemData, int> costDictionary = new Dictionary<ItemData, int>();
        for (int i = 0; i < costItems.Count; i++)
        {
            if (i < costAmounts.Count)
            {
                costDictionary[costItems[i]] = costAmounts[i];
            }
        }
        return costDictionary;
    }
}