using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade", menuName = "Game/Upgrade")]
public class Upgrade : ScriptableObject
{
    public int id;
    public string displayName;

    [Tooltip("Upgrades, die freigeschaltet sein müssen.")]
    public List<Upgrade> prerequisites;

    [Tooltip("Benötigte Ressourcen (Item, Menge).")]
    public  Dictionary<ItemData, int> cost;
}