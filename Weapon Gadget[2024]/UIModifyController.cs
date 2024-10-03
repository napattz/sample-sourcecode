using DG.Tweening;
using MultiplayerARPG;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIModifyController : MonoBehaviour
{
    public UIHeaderTopLeft HeaderTopLeft;
    public GameObject CanvasInventory;
    public ModifyRotationObject RotateObjectManager;
    public UIModifyItemManager itemManager;
    [Space(20)]
    [Header("Selected UI Component")]
    public Button backBtn;
    public Transform weaponContainer;
    public TextMeshProUGUI titleNameTxt;
    public TextMeshProUGUI descriptionTxt;
    public UISelectedItemStatController prefabStat;
    public Transform statTransform;
    public UISelectedItemAmmoController prefabAmmo;
    public Transform ammoTransform;
    [Header("Modify Inventory")]
    public Transform gadgetContainer;
    public GameObject catagoryPanel;
    public GameObject statPanel;
    public TextMeshProUGUI titleCatagotytxt;
    public Button SightItemBtn, MagazineItemBtn, MuzzleItemBtn, FlashlightItemBtn, GripItemBtn;
    public Toggle SightItemToggle, MagazineItemToggle, MuzzleItemToggle, FlashlightItemToggle, GripItemToggle;
    public Image IconSight, IconMagazine, IconMuzzle, IconFlashlight, IconGrip;
    public GameObject PlaceHolderSight, PlaceHolderMagazine, PlaceHolderMuzzle, PlaceHolderFlashlight, PlaceHolderGrip;
    public UICharacterItem FromCharacterItem { get; set; }
    public WeaponSocketAppearances WeaponSocket { get; set; }

    private bool _hasSight, _hasflashLight, _hasGrip, _hasMuzzle, _hasMagazine;
    private CanvasGroup _cg;

    private void OnEnable()
    {
        _cg = GetComponent<CanvasGroup>();
        if (isActiveAndEnabled)
        {
            _cg.DOFade(1, 0.2f);
        }
    }

    private void Start()
    {
        Toggle[] toggles = new Toggle[] {  SightItemToggle, MuzzleItemToggle, FlashlightItemToggle
            , GripItemToggle, MagazineItemToggle };

        backBtn.onClick.AddListener(() => OnClickBackToInventoryButton());
        FlashlightItemToggle.onValueChanged.AddListener((isOn) =>
        {
            OnToggleItemCatagory(isOn, FlashlightItemToggle, "FLASHLIGHT", WeaponSocketAppearances.ArmSocketEnhancerType.FlashLight);
            OnClickAttachmentButton(WeaponSocketAppearances.ArmSocketEnhancerType.FlashLight);
        });
        SightItemToggle.onValueChanged.AddListener((isOn) =>
        {
            OnToggleItemCatagory(isOn, SightItemToggle, "SIGHT", WeaponSocketAppearances.ArmSocketEnhancerType.Sight);
            OnClickAttachmentButton(WeaponSocketAppearances.ArmSocketEnhancerType.Sight);
        });
        GripItemToggle.onValueChanged.AddListener((isOn) =>
        {
            OnToggleItemCatagory(isOn, GripItemToggle, "GRIP", WeaponSocketAppearances.ArmSocketEnhancerType.Grip);
            OnClickAttachmentButton(WeaponSocketAppearances.ArmSocketEnhancerType.Grip);
        });
        MuzzleItemToggle.onValueChanged.AddListener((isOn) =>
        {
            OnToggleItemCatagory(isOn, MuzzleItemToggle, "MUZZLE", WeaponSocketAppearances.ArmSocketEnhancerType.Muzzle);
            OnClickAttachmentButton(WeaponSocketAppearances.ArmSocketEnhancerType.Muzzle);
        });
        MagazineItemToggle.onValueChanged.AddListener((isOn) =>
        {
            OnToggleItemCatagory(isOn, MagazineItemToggle, "MAGAZINE", WeaponSocketAppearances.ArmSocketEnhancerType.Muzzle, true);
            OnClickAttechmentMagazine();
        });

        FlashlightItemBtn.onClick.AddListener(() =>
        {
            OnToggleItemCatagory(true, FlashlightItemToggle, "FLASHLIGHT", WeaponSocketAppearances.ArmSocketEnhancerType.FlashLight);
            OnClickAttachmentButton(WeaponSocketAppearances.ArmSocketEnhancerType.FlashLight);
        });
        SightItemBtn.onClick.AddListener(() =>
        {
            OnToggleItemCatagory(true, SightItemToggle, "SIGHT", WeaponSocketAppearances.ArmSocketEnhancerType.Sight);
            OnClickAttachmentButton(WeaponSocketAppearances.ArmSocketEnhancerType.Sight);
        });
        GripItemBtn.onClick.AddListener(() =>
        {
            OnToggleItemCatagory(true, GripItemToggle, "GRIP", WeaponSocketAppearances.ArmSocketEnhancerType.Grip);
            OnClickAttachmentButton(WeaponSocketAppearances.ArmSocketEnhancerType.Grip);
        });
        MuzzleItemBtn.onClick.AddListener(() =>
        {
            OnToggleItemCatagory(true, MuzzleItemToggle, "MUZZLE", WeaponSocketAppearances.ArmSocketEnhancerType.Muzzle);
            OnClickAttachmentButton(WeaponSocketAppearances.ArmSocketEnhancerType.Muzzle);
        });
        MagazineItemBtn.onClick.AddListener(() =>
        {
            OnToggleItemCatagory(true, MagazineItemToggle, "MAGAZINE", WeaponSocketAppearances.ArmSocketEnhancerType.Muzzle, true);
            OnClickAttechmentMagazine();
        });

        foreach (var toggle in toggles)
        {
            toggle.isOn = false;
            toggle.GetComponent<ToggleSkinsAnimate>().AnimateToggle(false);
        }
        OnMainModify();
    }

    public void UpdateIcons()
    {
        if (WeaponSocket == null)
            return;
        IconSight.SetImageGameDataIcon(WeaponSocket.SightSettings.GetData(), true, PlaceHolderSight);
        IconMagazine.SetImageGameDataIcon(WeaponSocket.MagazineSettings.GetData(), true, PlaceHolderMagazine);
        IconMuzzle.SetImageGameDataIcon(WeaponSocket.MuzzleSettings.GetData(), true, PlaceHolderMuzzle);
        IconFlashlight.SetImageGameDataIcon(WeaponSocket.FlashLightSettings.GetData(), true, PlaceHolderFlashlight);
        IconGrip.SetImageGameDataIcon(WeaponSocket.GripSettings.GetData(), true, PlaceHolderGrip);
    }

    public void OnClickBackToInventoryButton()
    {
        _cg.DOFade(0, 0.2f).OnComplete(() => { gameObject.SetActive(false); CanvasInventory.SetActive(true); });
        // TODO: Localization
        HeaderTopLeft.Title = "INVENTORY";
    }
    public void InitialModifyUI()
    {
        Toggle[] toggles = new Toggle[] { SightItemToggle, MuzzleItemToggle, FlashlightItemToggle
            , GripItemToggle , MagazineItemToggle };

        foreach (var toggle in toggles)
        {
            toggle.isOn = false;
            toggle.GetComponent<ToggleSkinsAnimate>().AnimateToggle(false);
        }

        titleNameTxt.text = FromCharacterItem.Item.DefaultTitle;
        descriptionTxt.text = FromCharacterItem.Item.Description;
        HeaderTopLeft.textTitle.text = "MODIFY";

        _hasMuzzle = false; _hasGrip = false; _hasSight = false; _hasflashLight = false; _hasMagazine = false;
        bool[] hasEnhancer = new bool[5];
        Transform[] targetTransforms = new Transform[5];

        for (int i = 0; i < FromCharacterItem.WeaponItem.AvailableSocketEnhancerTypes.Length; i++)
        {
            switch ((WeaponSocketAppearances.ArmSocketEnhancerType)FromCharacterItem.WeaponItem.AvailableSocketEnhancerTypes[i])
            {
                case WeaponSocketAppearances.ArmSocketEnhancerType.Muzzle:
                    hasEnhancer[0] = true;
                    _hasMuzzle = hasEnhancer[0];
                    targetTransforms[0] = WeaponSocket.MuzzleSettings.attachmentPoint;
                    break;
                case WeaponSocketAppearances.ArmSocketEnhancerType.Grip:
                    hasEnhancer[1] = true;
                    _hasGrip = hasEnhancer[1];
                    targetTransforms[1] = WeaponSocket.GripSettings.attachmentPoint;
                    break;
                case WeaponSocketAppearances.ArmSocketEnhancerType.Sight:
                    hasEnhancer[2] = true;
                    _hasSight = hasEnhancer[2];
                    targetTransforms[2] = WeaponSocket.SightSettings.attachmentPoint;
                    break;
                case WeaponSocketAppearances.ArmSocketEnhancerType.FlashLight:
                    hasEnhancer[3] = true;
                    _hasflashLight = hasEnhancer[3];
                    targetTransforms[3] = WeaponSocket.FlashLightSettings.attachmentPoint;
                    break;
            }
        }
        itemManager.WeaponSocket = WeaponSocket;
        itemManager.FromCharacterItem = FromCharacterItem;
        itemManager.UIController = this;
        itemManager.UpdateOwningCharacterData();
        switch (WeaponSocket.AppearanceWeapon)
        {
            case WeaponSocketAppearances.AppearanceWeaponType.Secondary:
                RotateObjectManager.gameObject.transform.localPosition = RotateObjectManager.SecondaryWeaponTransform;
                break;
            case WeaponSocketAppearances.AppearanceWeaponType.Primary:
                RotateObjectManager.gameObject.transform.localPosition = RotateObjectManager.PrimaryWeaponTransform;
                break;
            case WeaponSocketAppearances.AppearanceWeaponType.Sniper:
                RotateObjectManager.gameObject.transform.localPosition = RotateObjectManager.SniperWeaponTransform;
                break;
            default:
                break;
        }
        if (WeaponSocket.MagazineSettings.attachmentPoint != null)
        {
            hasEnhancer[4] = true;
            _hasMagazine = hasEnhancer[4];
            targetTransforms[4] = WeaponSocket.MagazineSettings.attachmentPoint;
        }
        MuzzleItemBtn.gameObject.SetActive(hasEnhancer[0]);
        MuzzleItemToggle.gameObject.SetActive(hasEnhancer[0]);
        GripItemBtn.gameObject.SetActive(hasEnhancer[1]);
        GripItemToggle.gameObject.SetActive(hasEnhancer[1]);
        SightItemBtn.gameObject.SetActive(hasEnhancer[2]);
        SightItemToggle.gameObject.SetActive(hasEnhancer[2]);
        FlashlightItemBtn.gameObject.SetActive(hasEnhancer[3]);
        FlashlightItemToggle.gameObject.SetActive(hasEnhancer[3]);
        MagazineItemBtn.gameObject.SetActive(hasEnhancer[4]);
        MagazineItemToggle.gameObject.SetActive(hasEnhancer[4]);

        if (RotateObjectManager.modifyLineControllers != null && RotateObjectManager.modifyLineControllers.Length > 0)
        {
            for (int i = 0; i < RotateObjectManager.modifyLineControllers.Length; i++)
            {
                RotateObjectManager.modifyLineControllers[i].gameObject.SetActive(hasEnhancer[i]);
                RotateObjectManager.modifyLineControllers[i].IsOn = hasEnhancer[i];
                RotateObjectManager.modifyLineControllers[i].TargetTransform = targetTransforms[i];
                RotateObjectManager.modifyLineControllers[i].SetupLine();
            }
        }
        OnMainModify();
    }

    public void OnMainModify()
    {
        catagoryPanel.GetComponent<CanvasGroup>().DOFade(0, 0.1f);
        catagoryPanel.GetComponent<CanvasGroup>().interactable = false;
        statPanel.GetComponent<CanvasGroup>().DOFade(1, 0.1f);
        RotateObjectManager.Target = null;
        SightItemBtn.gameObject.SetActive(_hasSight);
        GripItemBtn.gameObject.SetActive(_hasGrip);
        FlashlightItemBtn.gameObject.SetActive(_hasflashLight);
        MuzzleItemBtn.gameObject.SetActive(_hasMuzzle);
        MagazineItemBtn.gameObject.SetActive(_hasMagazine);
        backBtn.onClick.RemoveAllListeners();
        backBtn.onClick.AddListener(() => OnClickBackToInventoryButton());
        if (itemManager != null && itemManager.uiDialog != null)
            itemManager.uiDialog.Hide();
    }

    public void OnClickAttachmentButton(WeaponSocketAppearances.ArmSocketEnhancerType socketType)
    {
        if (WeaponSocket != null)
        {
            switch (socketType)
            {
                case WeaponSocketAppearances.ArmSocketEnhancerType.Muzzle:
                    if (WeaponSocket.MuzzleSettings.attachmentPoint != null)
                        RotateObjectManager.Target = WeaponSocket.MuzzleSettings.attachmentPoint;
                    break;
                case WeaponSocketAppearances.ArmSocketEnhancerType.Grip:
                    if (WeaponSocket.GripSettings.attachmentPoint != null)
                        RotateObjectManager.Target = WeaponSocket.GripSettings.attachmentPoint;
                    break;
                case WeaponSocketAppearances.ArmSocketEnhancerType.Sight:
                    if (WeaponSocket.SightSettings.attachmentPoint != null)
                        RotateObjectManager.Target = WeaponSocket.SightSettings.attachmentPoint;
                    break;
                case WeaponSocketAppearances.ArmSocketEnhancerType.FlashLight:
                    if (WeaponSocket.FlashLightSettings.attachmentPoint != null)
                        RotateObjectManager.Target = WeaponSocket.FlashLightSettings.attachmentPoint;
                    break;
            }
        }
        SightItemBtn.gameObject.SetActive(false);
        GripItemBtn.gameObject.SetActive(false);
        FlashlightItemBtn.gameObject.SetActive(false);
        MuzzleItemBtn.gameObject.SetActive(false);
        MagazineItemBtn.gameObject.SetActive(false);
        backBtn.onClick.RemoveAllListeners();
        backBtn.onClick.AddListener(() => OnMainModify());
        catagoryPanel.GetComponent<CanvasGroup>().DOFade(1, 0.1f);
        catagoryPanel.GetComponent<CanvasGroup>().interactable = true;
        statPanel.GetComponent<CanvasGroup>().DOFade(0, 0.1f);
    }

    public void OnClickAttechmentMagazine()
    {
        if (WeaponSocket.MagazineSettings.attachmentPoint != null)
            RotateObjectManager.Target = WeaponSocket.MagazineSettings.attachmentPoint;

        itemManager.FilterMagazine();
        SightItemBtn.gameObject.SetActive(false);
        GripItemBtn.gameObject.SetActive(false);
        FlashlightItemBtn.gameObject.SetActive(false);
        MuzzleItemBtn.gameObject.SetActive(false);
        MagazineItemBtn.gameObject.SetActive(false);
    }

    public void OnToggleItemCatagory(bool isOn, Toggle toggle, string text, WeaponSocketAppearances.ArmSocketEnhancerType type = WeaponSocketAppearances.ArmSocketEnhancerType.Muzzle, bool magazine = false)
    {
        Toggle[] toggles = new Toggle[] { SightItemToggle, MuzzleItemToggle, FlashlightItemToggle
            , GripItemToggle,MagazineItemToggle };
        foreach (var toggless in toggles)
        {
            toggless.GetComponent<ToggleSkinsAnimate>().AnimateToggle(false);
        }
        toggle.GetComponent<ToggleSkinsAnimate>().AnimateToggle(isOn);
        if (!isOn)
            return;
        AnimateTextCatagoryItem(text);
        CanvasGroup gadgetCG = gadgetContainer.GetComponent<CanvasGroup>();
        gadgetCG.DOFade(0, 0.1f).OnComplete(() =>
        {
            if (!magazine)
            {
                itemManager.FilterSocketItem(type);
            }
            else
            {
                itemManager.FilterMagazine();
            }
            gadgetCG.DOFade(1, 0.15f);
        });
        backBtn.onClick.RemoveAllListeners();
        backBtn.onClick.AddListener(() => OnMainModify());
        catagoryPanel.GetComponent<CanvasGroup>().DOFade(1, 0.1f);
        catagoryPanel.GetComponent<CanvasGroup>().interactable = true;
        statPanel.GetComponent<CanvasGroup>().DOFade(0, 0.1f);
    }

    private void AnimateTextCatagoryItem(string text)
    {
        // อย่าลืมมาเปลี่ยนเป็น localize
        titleCatagotytxt.DOFade(0, 0.1f).OnComplete(() =>
        {
            titleCatagotytxt.text = text;
            titleCatagotytxt.DOFade(1, 0.15f);
        });
    }
}
