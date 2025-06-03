using UnityEngine;

[CreateAssetMenu(menuName = "Items/Item")]
public class ItemData : ScriptableObject
{
    public BlockKind BlockKind;
    public Sprite Icon;
    public int MaxStack = 99;
}
