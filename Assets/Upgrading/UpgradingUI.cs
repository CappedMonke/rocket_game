using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UpgradingUI : MonoBehaviour
{
    public float oxygenPerOre = 25.0f;
    public float fuelPerOre = 25.0f;
    public float healthPerOre = 25.0f;

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
        if ((Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape)) && _root.style.display == DisplayStyle.Flex) {
            _root.style.display = DisplayStyle.None;
            RefreshUI();
        }
        if (Input.GetKeyDown(KeyCode.I) && RocketControlsEnabled() && !RocketIsFlying())
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

    private bool RocketIsFlying()
    {
        var rocket = FindAnyObjectByType<Rocket>();
        if (rocket != null)
        {
            return rocket.IsFlying;
        }
        return false;
    }

    private void RefreshUI()
    {
        _itemList.Clear();

        foreach (var slot in _inventory.slots)
        {
            Label itemLabel = new Label($"{slot.item.BlockKind} x{slot.quantity}");
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
                text = "Replenish Oxygen (Oxygenium)"
            };
            oxygenButton.AddToClassList("resource-tank-entry");

            bool hasOxygen = _inventory.slots.Exists(slot => slot.item.BlockKind == BlockKind.Oxygenium && slot.quantity > 0);
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
                text = "Replenish Fuel (Kerosene)"
            };
            fuelButton.AddToClassList("resource-tank-entry");

            bool hasFuel = _inventory.slots.Exists(slot => slot.item.BlockKind == BlockKind.Kerosene && slot.quantity > 0);
            if (!hasFuel)
                fuelButton.SetEnabled(false);

            _resourceTankList.Add(fuelButton);

            var goldItem = _inventory.availableItems.FirstOrDefault(item => item.BlockKind == BlockKind.Gold);
            var hasGold = _inventory.slots.Exists(slot => slot.item.BlockKind == BlockKind.Gold && slot.quantity > 0);
            var healthField = rocket.GetType().GetField("health", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var maxHealthField = rocket.GetType().GetField("maxHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            int health = (int)healthField.GetValue(rocket);
            int maxHealth = (int)maxHealthField.GetValue(rocket);
            var isDamaged = health < maxHealth;

            var repairButton = new Button(() =>
            {
                if (goldItem != null && rocket != null)
                {
                    _inventory.RemoveItem(goldItem, 1);
                    var setHealthMethod = rocket.GetType().GetMethod("SetHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    int newHealth = Mathf.Min(health + Mathf.RoundToInt(healthPerOre), maxHealth);
                    setHealthMethod.Invoke(rocket, new object[] { newHealth });
                    RefreshUI();
                }
            })
            {
                text = $"Repair Rocket (Gold)"
            };
            repairButton.AddToClassList("resource-tank-entry");

            if (!hasGold || !isDamaged)
            {
                repairButton.SetEnabled(false);
            }

            _resourceTankList.Add(repairButton);
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