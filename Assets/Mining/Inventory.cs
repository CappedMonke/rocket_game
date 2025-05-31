using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<ItemData> availableItems;
    public List<InventorySlot> slots = new();

    void Awake()
    {
        availableItems = Resources.LoadAll<ItemData>("Items").ToList();
        Debug.Log($"Loaded {availableItems.Count} items.");
    }

    public void AddItemByTileName(string tileName, int amount = 1)
    {
        ItemData item = availableItems.Find(i => i.itemName == tileName);
        if (item == null)
        {
            Debug.LogWarning($"No item found for: {tileName} !");
            return;
        }

        AddItem(item, amount);
    }

    public void AddItem(ItemData item, int amount)
    {
        int remainingToAdd = amount;

        foreach (var slot in slots)
        {
            if (slot.item == item && slot.quantity <= item.maxStack)
            {
                int spaceLeft = item.maxStack - slot.quantity;
                int toAdd = Mathf.Min(spaceLeft, remainingToAdd);

                slot.quantity += toAdd;
                remainingToAdd -= toAdd;

                if (remainingToAdd <= 0)
                {
                    return;
                }
            }
        }

        while (remainingToAdd > 0)
        {
            int toAdd = Mathf.Min(item.maxStack, remainingToAdd);
            slots.Add(new InventorySlot(item, toAdd));
            remainingToAdd -= toAdd;
        }

        Debug.Log($"Retrieved: {amount} x {item.name}");
    }

}

[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int quantity;

    public InventorySlot(ItemData item, int quantity)
    {
        this.item = item;
        if (quantity > item.maxStack)
        {
            this.quantity = item.maxStack;
        }
    }
}
