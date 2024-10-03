using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.Serialization;

public class WeaponSocketAppearances : MonoBehaviour
{
    [System.Serializable]
    public class SocketAttachmentPivot<T>
        where T : BaseItem
    {
        [FormerlySerializedAs("socketItem")]
        public T item;
        [Range(-1f, 1f)] public float pivotZ;
        public int DataId
        {
            get
            {
                if (item == null)
                    return 0;
                return item.DataId;
            }
        }
    }


    [System.Serializable]
    public abstract class BaseWeaponAttachmentSettings<T>
        where T : BaseItem
    {
        public GameObject baseObject;
        public GameObject requiredObject;
        public SocketAttachmentPivot<T>[] options = new SocketAttachmentPivot<T>[0];
        [FormerlySerializedAs("AttachmentPoint")]
        public Transform attachmentPoint;

        private int _currentDataId = 0;
        private Dictionary<int, SocketAttachmentPivot<T>> _optionsDict;
        public Dictionary<int, SocketAttachmentPivot<T>> Options
        {
            get
            {
                if (_optionsDict == null)
                {
                    _optionsDict = new Dictionary<int, SocketAttachmentPivot<T>>();
                    SocketAttachmentPivot<T> tempOption;
                    for (int i = 0; i < options.Length; ++i)
                    {
                        tempOption = options[i];
                        if (tempOption == null || _optionsDict.ContainsKey(tempOption.DataId))
                            continue;
                        _optionsDict[tempOption.DataId] = tempOption;
                    }
                }
                return _optionsDict;
            }
        }

#if !UNITY_SERVER
        public async UniTask<int> ChangeObject(int dataId, bool force, int layer, bool updateWhenOffScreen)
        {
            if (_currentDataId == dataId && !force)
                return dataId;

            if (attachmentPoint != null)
                attachmentPoint.transform.RemoveChildren();
            if (Options.TryGetValue(dataId, out SocketAttachmentPivot<T> option))
            {
                if (baseObject != null)
                    baseObject.SetActive(false);
                if (requiredObject != null)
                    requiredObject.SetActive(true);
                _currentDataId = dataId;
                GameObject currentEquipModel = await GetEquipModel(option.item);
                if (currentEquipModel != null)
                {
                    GameObject currentAttachment = Instantiate(currentEquipModel, attachmentPoint);
                    currentAttachment.transform.localPosition = new Vector3(0, 0, option.pivotZ);
                    currentAttachment.transform.localEulerAngles = Vector3.zero;
                    currentAttachment.transform.localScale = Vector3.one;
                    currentAttachment.SetLayerRecursively(layer, true);
                    SkinnedMeshRenderer[] renderers = currentAttachment.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    foreach (SkinnedMeshRenderer renderer in renderers)
                    {
                        if (renderer.updateWhenOffscreen != updateWhenOffScreen)
                            renderer.updateWhenOffscreen = updateWhenOffScreen;
                    }
                }
                return dataId;
            }
            else
            {
                if (baseObject != null)
                    baseObject.gameObject.SetActive(true);
                if (requiredObject != null)
                    requiredObject.SetActive(false);
                _currentDataId = 0;
                return 0;
            }
        }
#endif

        public bool TryGetData(out BaseItem item)
        {
            return GameInstance.Items.TryGetValue(_currentDataId, out item);
        }

        public BaseItem GetData()
        {
            if (TryGetData(out BaseItem item))
                return item;
            return null;
        }

        public async UniTask<Sprite> GetIcon()
        {
#if UNITY_EDITOR || !UNITY_SERVER
            if (TryGetData(out BaseItem item))
                return await item.GetIcon();
            return null;
#else
                return null;
#endif
        }

        public abstract UniTask<GameObject> GetEquipModel(T data);
    }

    [System.Serializable]
    public class WeaponAttachmentSettings : BaseWeaponAttachmentSettings<SocketEnhancerItem>
    {
        public override UniTask<GameObject> GetEquipModel(SocketEnhancerItem data)
        {
#if UNITY_EDITOR || !UNITY_SERVER
            return data.GetSocketEnhancerAttachModel();
#else
                return UniTask.FromResult<GameObject>(null);
#endif
        }
    }

    [System.Serializable]
    public class MagazineAttachmentSetting : BaseWeaponAttachmentSettings<AmmoItem>
    {
        public override UniTask<GameObject> GetEquipModel(AmmoItem data)
        {
#if UNITY_EDITOR || !UNITY_SERVER
            return data.GetAmmoAttachModel();
#else
                return UniTask.FromResult<GameObject>(null);
#endif
        }
    }

    /// <summary>
    /// Because ARM project is having following fixed types, so we create this one to make the codes easier to read
    /// </summary>
    public enum ArmSocketEnhancerType
    {
        Muzzle,
        Grip,
        Sight,
        FlashLight,
    }

    public enum AppearanceWeaponType
    {
        Secondary,
        Primary,
        Sniper
    }

    public AppearanceWeaponType AppearanceWeapon;
    public SocketEnhancerType CurrentSocket { get; private set; }
    public WeaponAttachmentSettings MuzzleSettings = new WeaponAttachmentSettings();
    public WeaponAttachmentSettings GripSettings = new WeaponAttachmentSettings();
    public WeaponAttachmentSettings SightSettings = new WeaponAttachmentSettings();
    public WeaponAttachmentSettings FlashLightSettings = new WeaponAttachmentSettings();
    public MagazineAttachmentSetting MagazineSettings = new MagazineAttachmentSetting();
    public UnityLayer Layer = new UnityLayer(17);
    public bool UpdateWhenOffScreen = false;

    public BaseEquipmentEntity EquipmentEntity { get; private set; }

    private void Awake()
    {
        EquipmentEntity = GetComponent<BaseEquipmentEntity>();
        OnItemChanged(EquipmentEntity.Item);
        EquipmentEntity.onItemChanged.AddListener(OnItemChanged);
    }

    private void OnDestroy()
    {
        EquipmentEntity.onItemChanged.RemoveListener(OnItemChanged);
    }

    private void OnItemChanged(CharacterItem item)
    {
        ChangeMagazine(item.ammoDataId);
        if (item.sockets == null ||
            item.sockets.Count == 0)
            return;
        IEquipmentItem equipmentItem = item.GetEquipmentItem();
        if (equipmentItem == null)
            return;
        if (equipmentItem.AvailableSocketEnhancerTypes == null ||
            equipmentItem.AvailableSocketEnhancerTypes.Length == 0)
            return;
        for (int i = 0; i < equipmentItem.AvailableSocketEnhancerTypes.Length; ++i)
        {
            switch ((ArmSocketEnhancerType)equipmentItem.AvailableSocketEnhancerTypes[i])
            {
                case ArmSocketEnhancerType.Muzzle:
                    if (i < item.sockets.Count)
                        ChangeMuzzle(item.sockets[i]);
                    break;
                case ArmSocketEnhancerType.Grip:
                    if (i < item.sockets.Count)
                        ChangeGrip(item.sockets[i]);
                    break;
                case ArmSocketEnhancerType.Sight:
                    if (i < item.sockets.Count)
                        ChangeSight(item.sockets[i]);
                    break;
                case ArmSocketEnhancerType.FlashLight:
                    if (i < item.sockets.Count)
                        ChangeFlashLight(item.sockets[i]);
                    break;
            }
        }
    }

    public async void ChangeMuzzle(int dataId, bool force = false)
    {
#if !UNITY_SERVER
        await MuzzleSettings.ChangeObject(dataId, force, Layer.LayerIndex, UpdateWhenOffScreen);
#else
            await UniTask.Yield();
#endif
    }

    public async void ChangeGrip(int dataID, bool force = false)
    {
#if !UNITY_SERVER
        await GripSettings.ChangeObject(dataID, force, Layer.LayerIndex, UpdateWhenOffScreen);
#else
            await UniTask.Yield();
#endif
    }

    public async void ChangeSight(int dataID, bool force = false)
    {
#if !UNITY_SERVER
        await SightSettings.ChangeObject(dataID, force, Layer.LayerIndex, UpdateWhenOffScreen);
#else
            await UniTask.Yield();
#endif
    }

    public async void ChangeFlashLight(int dataID, bool force = false)
    {
#if !UNITY_SERVER
        await FlashLightSettings.ChangeObject(dataID, force, Layer.LayerIndex, UpdateWhenOffScreen);
#else
            await UniTask.Yield();
#endif
    }

    public async void ChangeMagazine(int dataID, bool force = false)
    {
#if !UNITY_SERVER
        await MagazineSettings.ChangeObject(dataID, force, Layer.LayerIndex, UpdateWhenOffScreen);
#else
            await UniTask.Yield();
#endif
    }
}
