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
        if ((Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape)) && _root.style.display == DisplayStyle.Flex) {
            _root.style.display = DisplayStyle.None;
            RefreshUI();
        }
        if (Input.GetKeyDown(KeyCode.I) && !RocketControlsEnabled())
        {
            bool isVisible = _root.style.display != DisplayStyle.None;
            _root.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;

            if (!isVisible)
            {
                RefreshUI();
            }
        }

        // Debug Cheat Code
        if (Input.GetKeyDown(KeyCode.O) && Input.GetKey(KeyCode.LeftShift))
        {
            Debug.Log("Cheat code activated: Adding resources to inventory.");
            for(int i = 0; i < 99; i++)
            {
                _inventory.AddItemByBlockKind(BlockKind.Gold);
                _inventory.AddItemByBlockKind(BlockKind.Oxygenium);
                _inventory.AddItemByBlockKind(BlockKind.Kerosene);
            }
            RefreshUI();
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
            Debug.Log(slot.item);
            Label itemLabel = new Label($"{slot.item.BlockKind} x{slot.quantity}");
            itemLabel.AddToClassList("item-entry");
            _itemList.Add(itemLabel);
        }
    }
}