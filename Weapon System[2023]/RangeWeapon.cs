using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GunSubType { SG, MG, RG }

public class RangeWeapon : Weapon
{
    [Header("Range Variable")]
    public GameObject bulletPrefab;
    public Transform spawnBulletPivot;

    protected DamageOnTouch _damageOnTouch;
    protected ObjectMovement _objectMovement;
    protected GameObject _bulletObject;
    protected Image ammoBar;
    protected Image reloadFillImage;

    public float updatedAmmo;
    protected int _usedAmmo;
    protected bool _isReload;
    protected int _maxAmmo;

    // Gun ��ҡ�����Ŵ �ѹ������¹ type SG,MG,RG ��� �����ԧ������Ŵ���ѧ໹ type �������������Ŵ�ҹ����

    protected override void Initialization()
    {
        base.Initialization();
        switch (weaponData.weaponType)
        {
            case WeaponData.WeaponTypes.Catalyst:
                _maxAmmo = weaponData.catalystMax;
                _usedAmmo = weaponData.catalystSP;
                updatedAmmo = _maxAmmo;
                break;

            default:
                _maxAmmo = weaponData.magazine;
                _usedAmmo = 1;
                updatedAmmo = _maxAmmo;
                break;
        }

        SetupUI();
    }
    protected override void Update()
    {
        base.Update();

        switch (weaponData.weaponType)
        {
            case WeaponData.WeaponTypes.Catalyst:

                if (updatedAmmo < _maxAmmo)
                {
                    updatedAmmo += weaponData.allWeaponRefine[weaponData.refine - 1].catalystRegen * Time.deltaTime;
                }
                else
                {
                    updatedAmmo = _maxAmmo;
                }

                break;

            default:

                if (updatedAmmo <= 0 && !_isReload)
                {
                    Reload();
                }

                break;
        }

        if (_character.characterType.HasFlag(CharacterType.Player))
        {
            ammoBar.DOFillAmount(updatedAmmo / _maxAmmo, 0.25f);
            _character.characterController.ammoText.text = updatedAmmo.ToString("00");
        }
    }

    public override void Attack()
    {
        if (_character.characterAction.isAttacking || _character.characterAction.isCasting || !_character.characterAction.canAttack)
            return;

        switch (weaponData.weaponType)
        {
            case WeaponData.WeaponTypes.Catalyst:

                if (updatedAmmo < _usedAmmo)
                {
                    //can't fire spell it's can be UI caution or something..
                }
                else
                {
                    StartCoroutine(DelaySpawnRangeAttack());
                }

                break;

            default:

                if (!_isReload)
                {
                    if (updatedAmmo < _usedAmmo)
                    {
                        Reload();
                    }
                    else
                    {
                        StartCoroutine(DelaySpawnRangeAttack());
                    }
                }

                break;
        }
    }

    IEnumerator DelaySpawnRangeAttack()
    {
        base.Attack();
        yield return new WaitForSeconds(intialDelay / attackInterval);
        SpawnBullet();
    }

    private void SetupUI()
    {
        if (_character.characterType.HasFlag(CharacterType.Player))
        {
            GameObject crosshair = Instantiate(UIManager.instance.gameplayManager.crosshairObject, _character.weaponPivot);
            _character.characterController.crosshair = crosshair;
            _character.characterController.ammoText.text = updatedAmmo.ToString();
            ammoBar = UIManager.instance.gameplayManager.ammoBar;
            reloadFillImage = UIManager.instance.gameplayManager.ammoBarFillImage;
            UIManager.instance.gameplayManager.reloadBtn.onClick.AddListener(() => Reload());
        }
    }

    protected virtual void Reload()
    {
        if (!_character.characterAction.canReload)
            return;

        //start anim and for unqiue reload change weapon style;
        AudioManager.instance.LoadResPlay(customLocation: "SFX/Player/Reloading");
        StartCoroutine(ReloadTimer());
    }

    protected virtual void SpawnBullet()
    {
        if (weaponData.fireBulletSound != null) { AudioManager.instance.LoadResPlay(clip: weaponData.fireBulletSound, isReplayIfExist: true); }

        _bulletObject = Instantiate(bulletPrefab, spawnBulletPivot);
        _bulletObject.transform.SetParent(null);

        if (_bulletObject.GetComponent<Collider2D>())
        {
            _objectMovement = _bulletObject.AddComponent<ObjectMovement>();
            _damageOnTouch = _bulletObject.AddComponent<DamageOnTouch>();
            _damageOnTouch.Init(_character, weaponData);
            _objectMovement.InitObjMovement(ObjectMovementType.Straight, _bulletObject, weaponData.bulletSpeed, weaponData.delayBulletDestroy, _character);
            _objectMovement.afterHitSound = weaponData.hitSound;
        }
        else
        {
            foreach (Transform subBullet in _bulletObject.transform)
            {
                _objectMovement = subBullet.gameObject.AddComponent<ObjectMovement>();
                _damageOnTouch = subBullet.gameObject.AddComponent<DamageOnTouch>();
                _damageOnTouch.Init(_character, weaponData);
                _objectMovement.InitObjMovement(ObjectMovementType.Straight, subBullet.gameObject, weaponData.bulletSpeed, weaponData.delayBulletDestroy, _character);
                _objectMovement.afterHitSound = weaponData.hitSound;
            }
        }

        updatedAmmo -= _usedAmmo;

        if (_character.characterType.HasFlag(CharacterType.Player))
        {
            _character.characterController.ammoText.text = updatedAmmo.ToString();
        }
    }

    private IEnumerator ReloadTimer()
    {
        _isReload = true;

        if (_character.characterType.HasFlag(CharacterType.Player))
        {
            reloadFillImage.fillAmount = 1;
            reloadFillImage.DOFillAmount(0, weaponData.reloadSpeed);
            _character.characterBehavior.ChangeBehavior(BehaviorType.Reload);
            Animator anim = Instantiate(_character.reloadAnim, _character.transform);
            anim.Play("ReloadOpen", 0, 0f);

            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
            float clipLength = state.length;
            
            anim.speed = clipLength / weaponData.reloadSpeed;
            Debug.Log(clipLength / weaponData.reloadSpeed);

            Destroy(anim.gameObject, weaponData.reloadSpeed + 0.5f);
        }
        UIManager.instance.gameplayManager.reloadBtn.enabled = false;
        yield return new WaitForSeconds(weaponData.reloadSpeed);
        UIManager.instance.gameplayManager.reloadBtn.enabled = true;
        _isReload = false;
        updatedAmmo = _maxAmmo;

        if (_character.characterType.HasFlag(CharacterType.Player))
        {
            _character.characterController.ammoText.text = updatedAmmo.ToString();
        }
    }
}
