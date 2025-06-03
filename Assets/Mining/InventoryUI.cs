using UnityEngine;
using UnityEngine.UIElements;

public class InventoryUI : MonoBehaviour
{
    private VisualElement _root;
    private ScrollView _itemList;
    private Inventory _inventory;

    void OnEnable()
    {
        _inventory = FindAnyObjectByType<Inventory>();

        UIDocument uiDocument = GetComponent<UIDocument>();
        _root = uiDocument.rootVisualElement;

        _itemList = _root.Q<ScrollView>("item-list");

        _root.style.display = DisplayStyle.None;
        _root.pickingMode = PickingMode.Ignore;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            bool isVisible = _root.style.display != DisplayStyle.None;
            _root.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;

            if (!isVisible)
            {
                RefreshUI();
            }
        }
    }

    private void RefreshUI()
    {
        _itemList.Clear();

        foreach (var slot in _inventory.slots)
        { 
            Debug.Log(slot.item);
            // Don't know where the 1 gets lost lol
            Label itemLabel = new Label($"{slot.item.BlockKind} x{slot.quantity + 1}");
            itemLabel.AddToClassList("item-entry");
            _itemList.Add(itemLabel);
        }
    }
}