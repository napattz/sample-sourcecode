using LiteNetLibManager;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class UIModifyItemManager : UICharacterItems
{
    [SerializeField] private Button buttonEquipAttachment, buttonUnEquipAttachment;
    [SerializeField] private UICharacterItem uiAttachedItem;

    public UICharacterItem FromCharacterItem { get; set; }
    public WeaponSocketAppearances WeaponSocket { get; set; }
    public UIModifyController UIController { get; set; }
    public WeaponSocketAppearances.ArmSocketEnhancerType SelectedSocketType { get; set; }
    public bool SelectedMagazine { get; set; }

    private CharacterItem _attachedCharacterItem;
    private HashSet<int> _filteredItemDataIds = new HashSet<int>();

    protected override void Awake()
    {
        overrideGetFilteredListFunction += GetFilteredList;
        base.Awake();
    }

    protected override void OnDestroy()
    {
        overrideGetFilteredListFunction -= GetFilteredList;
        base.OnDestroy();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        inventoryType = InventoryType.NonEquipItems;
        CacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
        buttonEquipAttachment.onClick.RemoveListener(OnClickEquipAttachment);
        buttonEquipAttachment.onClick.AddListener(OnClickEquipAttachment);
        buttonUnEquipAttachment.onClick.RemoveListener(OnClickUnEquipAttachment);
        buttonUnEquipAttachment.onClick.AddListener(OnClickUnEquipAttachment);
        CacheSelectionManager.eventOnSelect.RemoveListener(OnSelectAttachment);
        CacheSelectionManager.eventOnSelect.AddListener(OnSelectAttachment);
        CacheSelectionManager.eventOnDeselect.RemoveListener(OnDeselectAttachment);
        CacheSelectionManager.eventOnDeselect.AddListener(OnDeselectAttachment);
        RegisterOwningCharacterEvents();
    }

    protected override void OnDisable()
    {
        buttonEquipAttachment.onClick.RemoveListener(OnClickEquipAttachment);
        buttonUnEquipAttachment.onClick.RemoveListener(OnClickUnEquipAttachment);
        CacheSelectionManager.eventOnSelect.RemoveListener(OnSelectAttachment);
        CacheSelectionManager.eventOnDeselect.RemoveListener(OnDeselectAttachment);
        UnregisterOwningCharacterEvents();
        base.OnDisable();
    }

    public void RegisterOwningCharacterEvents()
    {
        UnregisterOwningCharacterEvents();
        if (!GameInstance.PlayingCharacterEntity) return;
        GameInstance.PlayingCharacterEntity.onSelectableWeaponSetsOperation += OnSelectableWeaponSetsOperation;
        GameInstance.PlayingCharacterEntity.onEquipItemsOperation += OnEquipItemsOperation;
        GameInstance.PlayingCharacterEntity.onNonEquipItemsOperation += OnNonEquipItemsOperation;
    }

    public void UnregisterOwningCharacterEvents()
    {
        if (!GameInstance.PlayingCharacterEntity) return;
        GameInstance.PlayingCharacterEntity.onSelectableWeaponSetsOperation -= OnSelectableWeaponSetsOperation;
        GameInstance.PlayingCharacterEntity.onEquipItemsOperation -= OnEquipItemsOperation;
        GameInstance.PlayingCharacterEntity.onNonEquipItemsOperation -= OnNonEquipItemsOperation;
    }

    private void OnSelectableWeaponSetsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (FromCharacterItem == null)
            return;
        if (operation != LiteNetLibSyncList.Operation.Set && operation != LiteNetLibSyncList.Operation.Dirty)
            return;
        EquipWeapons weaponWeapons = GameInstance.PlayingCharacter.SelectableWeaponSets[index];
        UpdateSourceCharacterItemIfSameId(weaponWeapons.rightHand);
        UpdateSourceCharacterItemIfSameId(weaponWeapons.leftHand);
        if (string.Equals(weaponWeapons.rightHand.id, FromCharacterItem.CharacterItem.id) ||
            string.Equals(weaponWeapons.leftHand.id, FromCharacterItem.CharacterItem.id))
            UIController.UpdateIcons();
    }

    private void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (FromCharacterItem == null)
            return;
        if (operation != LiteNetLibSyncList.Operation.Set && operation != LiteNetLibSyncList.Operation.Dirty)
            return;
        CharacterItem characterItem = GameInstance.PlayingCharacter.EquipItems[index];
        UpdateSourceCharacterItemIfSameId(characterItem);
        if (string.Equals(characterItem.id, FromCharacterItem.CharacterItem.id))
            UIController.UpdateIcons();
    }

    private void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (FromCharacterItem == null)
            return;
        if (operation != LiteNetLibSyncList.Operation.Set && operation != LiteNetLibSyncList.Operation.Dirty)
        {
            UpdateOwningCharacterData();
            return;
        }
        CharacterItem characterItem = GameInstance.PlayingCharacter.NonEquipItems[index];
        UpdateSourceCharacterItemIfSameId(characterItem);
        UpdateOwningCharacterData();
        if (string.Equals(characterItem.id, FromCharacterItem.CharacterItem.id))
            UIController.UpdateIcons();
    }

    private void AddItemIdsToSet<T>(HashSet<int> idSet, WeaponSocketAppearances.SocketAttachmentPivot<T>[] options)
        where T : BaseItem
    {
        foreach (WeaponSocketAppearances.SocketAttachmentPivot<T> item in options)
        {
            idSet.Add(item.item.DataId);
        }
    }

    public void UpdateSourceCharacterItemIfSameId(CharacterItem characterItem)
    {
        if (string.Equals(characterItem.id, FromCharacterItem.CharacterItem.id))
            FromCharacterItem.Data = new UICharacterItemData(characterItem, FromCharacterItem.Data.targetLevel, FromCharacterItem.Data.inventoryType);
    }

    public void UpdateOwningCharacterData()
    {
        if (GameInstance.PlayingCharacter == null) return;
        CacheSelectionManager.DeselectSelectedUI();
        UpdateData(GameInstance.PlayingCharacter, GameInstance.PlayingCharacter.NonEquipItems);

        if (FromCharacterItem == null)
            return;

        IEquipmentItem equipmentItem = FromCharacterItem.EquipmentItem;
        if (equipmentItem == null)
            return;

        _attachedCharacterItem = CharacterItem.Empty;
        if (SelectedMagazine)
        {
            if (GameInstance.Items.ContainsKey(FromCharacterItem.CharacterItem.ammoDataId) && FromCharacterItem.CharacterItem.ammo > 0)
            {
                _attachedCharacterItem = CharacterItem.Create(FromCharacterItem.CharacterItem.ammoDataId, 1, FromCharacterItem.CharacterItem.ammo);
            }
        }
        else
        {
            int socketIndex = equipmentItem.IndexOfSocket((SocketEnhancerType)SelectedSocketType);
            if (FromCharacterItem.CharacterItem.sockets != null &&
                FromCharacterItem.CharacterItem.sockets.Count > socketIndex &&
                GameInstance.Items.ContainsKey(FromCharacterItem.CharacterItem.sockets[socketIndex]))
            {
                _attachedCharacterItem = CharacterItem.Create(FromCharacterItem.CharacterItem.sockets[socketIndex], 1, 1);
            }
        }
        uiAttachedItem.Data = new UICharacterItemData(_attachedCharacterItem, InventoryType.Unknow);
    }

    public void FilterSocketItem(WeaponSocketAppearances.ArmSocketEnhancerType type)
    {
        _filteredItemDataIds.Clear();
        SelectedSocketType = type;
        SelectedMagazine = false;
        switch (type)
        {
            case WeaponSocketAppearances.ArmSocketEnhancerType.Muzzle:
                AddItemIdsToSet(_filteredItemDataIds, WeaponSocket.MuzzleSettings.options);
                break;
            case WeaponSocketAppearances.ArmSocketEnhancerType.Grip:
                AddItemIdsToSet(_filteredItemDataIds, WeaponSocket.GripSettings.options);
                break;
            case WeaponSocketAppearances.ArmSocketEnhancerType.Sight:
                AddItemIdsToSet(_filteredItemDataIds, WeaponSocket.SightSettings.options);
                break;
            case WeaponSocketAppearances.ArmSocketEnhancerType.FlashLight:
                AddItemIdsToSet(_filteredItemDataIds, WeaponSocket.FlashLightSettings.options);
                break;
        }
        UpdateOwningCharacterData();
    }

    public void FilterMagazine()
    {
        _filteredItemDataIds.Clear();
        SelectedMagazine = true;
        AddItemIdsToSet(_filteredItemDataIds, WeaponSocket.MagazineSettings.options);
        UpdateOwningCharacterData();
    }

    private List<KeyValuePair<int, CharacterItem>> GetFilteredList(List<CharacterItem> list, List<string> filterCategories, List<ItemType> filterItemTypes, List<SocketEnhancerType> filterSocketEnhancerTypes, bool doNotShowEmptySlots)
    {
        List<KeyValuePair<int, CharacterItem>> result = new List<KeyValuePair<int, CharacterItem>>();
        if (FromCharacterItem == null)
            return result;

        IEquipmentItem equipmentItem = FromCharacterItem.EquipmentItem;
        if (equipmentItem == null)
            return result;
        for (int i = 0; i < list.Count; ++i)
        {
            if (!_filteredItemDataIds.Contains(list[i].dataId))
                continue;
            result.Add(new KeyValuePair<int, CharacterItem>(i, list[i]));
        }
        result = result.OrderBy(o => o.Value.GetItem().Id).ToList();
        return result;
    }

    protected virtual void OnClickEquipAttachment()
    {
        UICharacterItem uiCharacterItem = CacheSelectionManager.SelectedUI;

        if (FromCharacterItem == null)
            return;

        if (WeaponSocket == null || !WeaponSocket.TryGetComponent(out WeaponSocketAppearances comp))
            return;

        if (SelectedMagazine)
        {
            comp.ChangeMagazine(uiCharacterItem.Item.DataId);

            if (uiCharacterItem.IndexOfData < 0)
            {
                // Do nothing, this item was equipped
                return;
            }

            GameInstance.ClientInventoryHandlers.RequestChangeAmmoItem(new RequestChangeAmmoItemMessage()
            {
                inventoryType = FromCharacterItem.InventoryType,
                index = FromCharacterItem.IndexOfData,
                ammoItemId = uiCharacterItem.CharacterItem.id,
            }, ClientInventoryActions.ResponseChangeAmmoItem);
        }
        else
        {
            switch (SelectedSocketType)
            {
                case WeaponSocketAppearances.ArmSocketEnhancerType.Muzzle:
                    comp.ChangeMuzzle(uiCharacterItem.Item.DataId);
                    break;
                case WeaponSocketAppearances.ArmSocketEnhancerType.Grip:
                    comp.ChangeGrip(uiCharacterItem.Item.DataId);
                    break;
                case WeaponSocketAppearances.ArmSocketEnhancerType.Sight:
                    comp.ChangeSight(uiCharacterItem.Item.DataId);
                    break;
                case WeaponSocketAppearances.ArmSocketEnhancerType.FlashLight:
                    comp.ChangeFlashLight(uiCharacterItem.Item.DataId);
                    break;
            }

            if (uiCharacterItem.IndexOfData < 0)
            {
                // Do nothing, this item was equipped
                return;
            }

            IEquipmentItem equipmentItem = FromCharacterItem.EquipmentItem;
            if (equipmentItem == null)
            {
                // Invalid data
                Debug.LogError("Unable to enhance socket item, invalid equipment item");
                return;
            }
            int socketIndex = equipmentItem.IndexOfSocket((SocketEnhancerType)SelectedSocketType);
            if (socketIndex < 0)
            {
                // Invalid data
                Debug.LogError("Unable to enhance socket item, invalid socket index");
                return;
            }
            // Remove the old one if existed
            if (FromCharacterItem.CharacterItem.sockets != null &&
                FromCharacterItem.CharacterItem.sockets.Count > socketIndex &&
                FromCharacterItem.CharacterItem.sockets[socketIndex] != 0)
            {
                GameInstance.ClientInventoryHandlers.RequestRemoveEnhancerFromItem(new RequestRemoveEnhancerFromItemMessage()
                {
                    inventoryType = FromCharacterItem.InventoryType,
                    index = FromCharacterItem.IndexOfData,
                    socketIndex = socketIndex,
                }, ClientInventoryActions.ResponseRemoveEnhancerFromItem);
            }
            // Put a new one
            GameInstance.ClientInventoryHandlers.RequestEnhanceSocketItem(new RequestEnhanceSocketItemMessage()
            {
                inventoryType = FromCharacterItem.InventoryType,
                index = FromCharacterItem.IndexOfData,
                enhancerId = uiCharacterItem.Item.DataId,
                socketIndex = socketIndex,
            }, ClientInventoryActions.ResponseEnhanceSocketItem);
        }
    }

    protected virtual void OnClickUnEquipAttachment()
    {
        UICharacterItem uiCharacterItem = CacheSelectionManager.SelectedUI;

        if (FromCharacterItem == null)
            return;

        if (WeaponSocket == null || !WeaponSocket.TryGetComponent(out WeaponSocketAppearances comp))
            return;

        if (SelectedMagazine)
        {
            comp.ChangeMagazine(0);

            GameInstance.ClientInventoryHandlers.RequestRemoveAmmoFromItem(new RequestRemoveAmmoFromItemMessage()
            {
                inventoryType = FromCharacterItem.InventoryType,
                index = FromCharacterItem.IndexOfData,
            }, ClientInventoryActions.ResponseRemoveAmmoFromItem);
        }
        else
        {
            switch (SelectedSocketType)
            {
                case WeaponSocketAppearances.ArmSocketEnhancerType.Muzzle:
                    comp.ChangeMuzzle(0);
                    break;
                case WeaponSocketAppearances.ArmSocketEnhancerType.Grip:
                    comp.ChangeGrip(0);
                    break;
                case WeaponSocketAppearances.ArmSocketEnhancerType.Sight:
                    comp.ChangeSight(0);
                    break;
                case WeaponSocketAppearances.ArmSocketEnhancerType.FlashLight:
                    comp.ChangeFlashLight(0);
                    break;
            }

            IEquipmentItem equipmentItem = FromCharacterItem.EquipmentItem;
            if (equipmentItem == null)
            {
                // Invalid data
                Debug.LogError("Unable to enhance socket item, invalid equipment item");
                return;
            }
            int socketIndex = equipmentItem.IndexOfSocket((SocketEnhancerType)SelectedSocketType);
            if (socketIndex < 0)
            {
                // Invalid data
                Debug.LogError("Unable to enhance socket item, invalid socket index");
                return;
            }
            // Remove the old one if existed
            if (FromCharacterItem.CharacterItem.sockets != null &&
                FromCharacterItem.CharacterItem.sockets.Count > socketIndex &&
                FromCharacterItem.CharacterItem.sockets[socketIndex] != 0)
            {
                GameInstance.ClientInventoryHandlers.RequestRemoveEnhancerFromItem(new RequestRemoveEnhancerFromItemMessage()
                {
                    inventoryType = FromCharacterItem.InventoryType,
                    index = FromCharacterItem.IndexOfData,
                    socketIndex = socketIndex,
                }, ClientInventoryActions.ResponseRemoveEnhancerFromItem);
            }
        }
    }

    protected virtual void OnSelectAttachment(UICharacterItem uiCharacterItem)
    {
        uiAttachedItem.Data = uiCharacterItem.Data;
        buttonEquipAttachment.gameObject.SetActive(true);
        buttonUnEquipAttachment.gameObject.SetActive(false);
    }

    protected virtual void OnDeselectAttachment(UICharacterItem uiCharacterItem)
    {
        uiAttachedItem.Data = new UICharacterItemData(_attachedCharacterItem, InventoryType.Unknow);
        buttonEquipAttachment.gameObject.SetActive(false);
        buttonUnEquipAttachment.gameObject.SetActive(true);
    }
}

