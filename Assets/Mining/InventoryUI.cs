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
        if (Input.GetKeyDown(KeyCode.I) && !RocketControlsEnabled())
        {
            bool isVisible = _root.style.display != DisplayStyle.None;
            _root.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;

            if (!isVisible)
            {
                RefreshUI();
            }
        }
    }

    private bool RocketControlsEnabled()
    {
        var rocket = FindAnyObjectByType<Rocket>();
        if (rocket != null)
        {
            return rocket.GetControls().Rocket.enabled;
        }
        return false;
    }

    private void RefreshUI()
    {
        _itemList.Clear();

        foreach (var slot in _inventory.slots)
        {
            Label itemLabel = new Label($"{slot.item.itemName} x{slot.quantity + 1}"); // Don't know where the 1 gets lost lol
            itemLabel.AddToClassList("item-entry");
            _itemList.Add(itemLabel);
        }
    }
}