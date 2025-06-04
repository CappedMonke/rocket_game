using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UpgradingUI : MonoBehaviour
{
    public float oxygenPerOre = 60.0f;
    public float fuelPerOre = 50.0f;

    private VisualElement _root;
    private ScrollView _itemList;
    private ScrollView _upgradeList;
    private ScrollView _resourceTankList;
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
        _resourceTankList = _root.Q<ScrollView>("resource-tanks-list");

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
            Label itemLabel = new Label($"{slot.item.BlockKind} x{slot.quantity + 1}"); // Don't know where the 1 gets lost lol
            itemLabel.AddToClassList("item-entry");
            _itemList.Add(itemLabel);
        }

        _upgradeList.Clear();

        foreach (var upgrade in _upgradeController.availableUpgrades
            .Where(u => u != null)
            .OrderBy(u => u.id))
        {
            Button upgradeButton = new Button(() => OnUpgradeSelected(upgrade))
            {
                text = upgrade.displayName
            };
            upgradeButton.AddToClassList("upgrade-entry");
            upgradeButton.tooltip = $"Cost: {string.Join(", ", upgrade.GetCostDictionary().Select(kvp => $"{kvp.Value} x {kvp.Key.BlockKind}"))}";

            // Check if the upgrade is already acquired
            if (_upgradeController.GetAcquiredUpgrades().Contains(upgrade))
            {
                upgradeButton.style.backgroundColor = new Color(0.4f, 0.5f, 0.4f, 1f);
                upgradeButton.SetEnabled(false);
                _upgradeList.Add(upgradeButton);
                continue;
            }

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

            bool requirementsMet = false;

            if (upgrade.prerequisites == null || upgrade.prerequisites.Count == 0)
            {
                requirementsMet = true;
            }
            else
            {
                var acquired = _upgradeController.GetAcquiredUpgrades();
                requirementsMet = upgrade.prerequisites.All(prereq => acquired.Contains(prereq));
            }

            if (!canAfford || !requirementsMet)
            {
                upgradeButton.AddToClassList("disabled");
                upgradeButton.SetEnabled(false);
            }

            _upgradeList.Add(upgradeButton);
            Label costLabel = new Label($"{string.Join(", ", upgrade.GetCostDictionary().Select(kvp => $"{kvp.Value} x {kvp.Key.BlockKind}"))}");
            costLabel.AddToClassList("upgrade-cost");
            costLabel.style.fontSize = 12;
            _upgradeList.Add(costLabel);
        }

        _resourceTankList.Clear();

        var rocket = FindAnyObjectByType<Rocket>();
        if (rocket != null)
        {
            var oxygenItem = _inventory.availableItems.FirstOrDefault(item => item.BlockKind == BlockKind.Oxygenium);
            var keroseneItem = _inventory.availableItems.FirstOrDefault(item => item.BlockKind == BlockKind.Kerosene);
            var oxygenButton = new Button(() =>
            {
                if (oxygenItem != null)
                {
                    _inventory.RemoveItem(oxygenItem, 1);
                    rocket.RefillOxygen(oxygenPerOre);
                    RefreshUI();
                }
            })
            {
                text = "Replenish Oxygen"
            };
            oxygenButton.AddToClassList("resource-tank-entry");

            bool hasOxygen = _inventory.slots.Exists(slot => slot.item.BlockKind == BlockKind.Oxygenium && slot.quantity >= 0); // Check bug why the slot can be 0
            if (!hasOxygen)
            {
                oxygenButton.SetEnabled(false);
            }

            _resourceTankList.Add(oxygenButton);

            var fuelButton = new Button(() =>
            {
                if (keroseneItem != null)
                {
                    _inventory.RemoveItem(keroseneItem, 1);
                    rocket.RefillFuel(fuelPerOre);
                    RefreshUI();
                }
            })
            {
                text = "Replenish Fuel"
            };
            fuelButton.AddToClassList("resource-tank-entry");

            bool hasFuel = _inventory.slots.Exists(slot => slot.item.BlockKind == BlockKind.Kerosene && slot.quantity >= 0); // Check bug why the slot can be 0
            if (!hasFuel)
                fuelButton.SetEnabled(false);

            _resourceTankList.Add(fuelButton);
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

        bool requirementsMet = false;

        if (upgrade.prerequisites == null || upgrade.prerequisites.Count == 0)
        {
            requirementsMet = true;
        }
        else
        {
            var acquired = _upgradeController.GetAcquiredUpgrades();
            requirementsMet = upgrade.prerequisites.All(prereq => acquired.Contains(prereq));
        }

        if (!requirementsMet)
        {
            Debug.LogWarning($"Upgrade prerequisites not met for: {upgrade.displayName}");
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
        RefreshUI();
    }

    private void OnReplenishSelect(int tankType)
    {

    } 
}