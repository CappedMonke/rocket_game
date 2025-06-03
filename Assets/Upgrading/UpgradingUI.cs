using UnityEngine;
using UnityEngine.UIElements;

public class UpgradingUI : MonoBehaviour
{
    private VisualElement _root;
    private ScrollView _itemList;
    private ScrollView _upgradeList;
    private Inventory _inventory;
    private UpgradeController _upgradeController;

    void OnEnable()
    {
        _inventory = FindAnyObjectByType<Inventory>();
        _upgradeController = FindAnyObjectByType<UpgradeController>();

        UIDocument uiDocument = GetComponent<UIDocument>();
        _root = uiDocument.rootVisualElement;

        _itemList = _root.Q<ScrollView>("item-list");
        _upgradeList = _root.Q<ScrollView>("upgrade-list");

        _root.style.display = DisplayStyle.None;
        _root.pickingMode = PickingMode.Ignore;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && RocketControlsEnabled())
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

        _upgradeList.Clear();

        foreach (var upgrade in _upgradeController.availableUpgrades)
        {
            if (upgrade == null) continue;

            Button upgradeButton = new Button(() => OnUpgradeSelected(upgrade))
            {
                text = upgrade.displayName
            };
            upgradeButton.AddToClassList("upgrade-entry");

            // Check prerequisites
            bool canAfford = true;
            if (upgrade.GetCostDictionary() == null || upgrade.GetCostDictionary().Count == 0)
            {
                canAfford = true;
            }
            else
            {
                foreach (var cost in upgrade.GetCostDictionary())
                {
                    if (!_inventory.slots.Exists(slot => slot.item == cost.Key && slot.quantity >= cost.Value))
                    {
                        canAfford = false;
                        break;
                    }
                }
            }
            
            if (!canAfford)
            {
                upgradeButton.AddToClassList("disabled");
            }

            _upgradeList.Add(upgradeButton);
        }
    }

    private void OnUpgradeSelected(Upgrade upgrade)
    {
        if (upgrade == null) return;

        bool canAfford = true;
        if (upgrade.GetCostDictionary() == null || upgrade.GetCostDictionary().Count == 0)
        {
            canAfford = true;
        }
        else
        {
            foreach (var cost in upgrade.GetCostDictionary())
            {
                if (!_inventory.slots.Exists(slot => slot.item == cost.Key && slot.quantity >= cost.Value))
                {
                    canAfford = false;
                    break;
                }
            }
        }

        if (!canAfford)
        {
            Debug.LogWarning($"Cannot afford upgrade: {upgrade.displayName}");
            return;
        }

        // Deduct costs from inventory
        if (upgrade.GetCostDictionary() != null && upgrade.GetCostDictionary().Count > 0)
        {
            foreach (var cost in upgrade.GetCostDictionary())
            {
                _inventory.RemoveItem(cost.Key, cost.Value);
            }
        }

        _upgradeController.AddUpgrade(upgrade);
        Debug.Log($"Upgrade applied: {upgrade.displayName}");
    }
}