using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    public List<InventorySlot> slots = new();

    public List<ItemData> availableItems;

    void Awake()
    {
        availableItems = Resources.LoadAll<ItemData>("Items").ToList();
        Debug.Log($"Loaded {availableItems.Count} items.");
    }

    public void AddItemByBlockKind(BlockKind blockKind, int amount = 1)
    {
        ItemData item = availableItems.Find(i => i.BlockKind == blockKind);
        if (item == null)
        {
            Debug.LogWarning($"No item found for: {blockKind} !");
            return;
        }

        AddItem(item, amount);
    }

    public void AddItem(ItemData item, int amount)
    {
        int remainingToAdd = amount;

        foreach (var slot in slots)
        {
            if (slot.item == item && slot.quantity <= item.MaxStack)
            {
                int spaceLeft = item.MaxStack - slot.quantity;
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
            int toAdd = Mathf.Min(item.MaxStack, remainingToAdd);
            slots.Add(new InventorySlot(item, toAdd));
            remainingToAdd -= toAdd;
        }

        Debug.Log($"Retrieved: {amount} x {item.name}");
    }

    public void RemoveItem(ItemData item, int amount)
    {
        int remainingToRemove = amount;

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item == item)
            {
                if (slots[i].quantity <= remainingToRemove)
                {
                    remainingToRemove -= slots[i].quantity;
                    slots.RemoveAt(i);
                    i--; // Adjust index after removal
                }
                else
                {
                    slots[i].quantity -= remainingToRemove;
                    return;
                }
            }

            if (remainingToRemove <= 0)
            {
                return;
            }
        }

        Debug.LogWarning($"Tried to remove {amount} x {item.name}, but only {amount - remainingToRemove} were available.");
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
        if (quantity > item.MaxStack)
        {
            this.quantity = item.MaxStack;
        }
    }
}
