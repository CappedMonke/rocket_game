using UnityEngine;
using UnityEngine.UIElements;

public class UpgradingUI : MonoBehaviour
{
    private VisualElement _root;
    private ScrollView _itemList;
    private ScrollView _upgradeList;
    private Inventory _inventory;

    void OnEnable()
    {
        _inventory = FindAnyObjectByType<Inventory>();

        UIDocument uiDocument = GetComponent<UIDocument>();
        _root = uiDocument.rootVisualElement;

        _itemList = _root.Q<ScrollView>("item-list");
        _upgradeList = _root.Q<ScrollView>("upgrade-list");

        _root.style.display = DisplayStyle.None;
        _root.pickingMode = PickingMode.Ignore;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
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
            Label itemLabel = new Label($"{slot.item.itemName} x{slot.quantity + 1}"); // Don't know where the 1 gets lost lol
            itemLabel.AddToClassList("item-entry");
            _itemList.Add(itemLabel);
        }
    }
}