using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UI;
using System;
using Spine;

public class Weapon : MonoBehaviour
{
    public static event Action equipedWeaponAction, unequipWeaponAction;

    [Header("Initialize Data")]

    public bool InitializeOnStart = false;
    [Expandable]
    public WeaponData weaponData;
    public int currentRefine;
    public AnimatorOverrideController animator;
    public Transform hitBoxTransform;
    public float intialDelay = 0f;
    public float particleEffectDestroyTime = 1f;
    public List<GameObject> hitEffect = new List<GameObject>();
    public AudioClip attackSound;

    protected Character _character;

    private float _timeSinceAtk;
    protected float attackInterval;
    protected float attackSpeed;

    [Header("SKILL")]
    public Skill weaponSkill;

    protected virtual void Start()
    {
        if (InitializeOnStart)
        {
            Initialization();
        }
    }

    protected void CheckWeaponUI()
    {
        switch (weaponData.weaponType)
        {
            case WeaponData.WeaponTypes.Bow:
                UIManager.instance.gameplayManager.reloadBtn.gameObject.SetActive(false);
                UIManager.instance.ChangeAmmoBar(true);
                UIManager.instance.ChangeMeleeBar(false);
                break;
            case WeaponData.WeaponTypes.Catalyst:
                UIManager.instance.gameplayManager.reloadBtn.gameObject.SetActive(false);
                UIManager.instance.ChangeAmmoBar(false);
                UIManager.instance.ChangeMeleeBar(false);
                break;
            case WeaponData.WeaponTypes.Gun:
                UIManager.instance.gameplayManager.reloadBtn.gameObject.SetActive(true);
                UIManager.instance.ChangeAmmoBar(true);
                UIManager.instance.ChangeMeleeBar(false);
                break;
            default:
                UIManager.instance.gameplayManager.reloadBtn.gameObject.SetActive(false);
                UIManager.instance.ChangeMeleeBar(true);
                break;
        }
    }
    protected virtual void Update()
    {
        WeaponAttack();
    }

    protected virtual void Initialization()
    {
        // init Animation , setup AnimationState , weaponstate, owener component
        if (transform.root.tag == "Enemy")
        {
            _character = transform.root.GetComponentInParent<Character>();
        }
        else
        {
            _character = GetComponentInParent<Character>();
            CheckWeaponUI();
            _character.animator.runtimeAnimatorController = animator;
        }

        if (weaponSkill != null)
        {
            Skill skill = CopyComponent<Skill>(weaponSkill, gameObject);
            skill = GetComponent<Skill>();
            skill.InitializeWeapon();
        }
        else
        {
            if (_character.characterType.HasFlag(CharacterType.Player))
            {
                _character.characterController.skillWeaponButton.enabled = false;
                _character.characterController.skillWeaponButton.GetComponent<Image>().sprite = UIManager.instance.gameplayManager.skillEmptySprite;
            }
        }

        if (_character.characterType.HasFlag(CharacterType.Player))
        {
            attackInterval = weaponData.allWeaponRefine[currentRefine - 1].attackInterval;
            attackSpeed = weaponData.allWeaponRefine[currentRefine - 1].attackSpeed;

            _character.characterController.ModifyRadiusAimAssist(weaponData.radiusAimAssist);
        }

        if (_character.characterType.HasFlag(CharacterType.Enemy))
        {
            attackInterval = weaponData.allWeaponRefine[0].attackInterval;
            attackSpeed = weaponData.allWeaponRefine[0].attackSpeed;
        }

        attackSound = weaponData.attackSound;
    }

    public virtual void Equip()
    {
        if (_character == null)
            _character = GetComponentInParent<Character>();

        AudioManager.instance.LoadResPlay(customLocation: "SFX/UI/SwitchWeapon");

        _character.attribute.additionalStat.attack += weaponData.allWeaponRefine[currentRefine - 1].baseAttack;
        _character.attribute.attackSpeedMultiplier += weaponData.allWeaponRefine[currentRefine - 1].attackSpeed;

        _character.ChangeSkin(weaponData);

        if (_character.characterType.HasFlag(CharacterType.Player))
        {
            _character.characterAction.CheckWeaponUI();
        }

        foreach (Status status in weaponData.allWeaponRefine[currentRefine - 1].applyAttributeStatus)
        {
            _character.characterStatus.AddStatus(status);
        }

        equipedWeaponAction?.Invoke();
    }

    public virtual void Unequip()
    {
        if (_character == null)
            _character = GetComponentInParent<Character>();

        _character.attribute.additionalStat.attack -= weaponData.allWeaponRefine[currentRefine - 1].baseAttack;
        _character.attribute.attackSpeedMultiplier -= attackSpeed;

        foreach (Status status in weaponData.allWeaponRefine[currentRefine - 1].applyAttributeStatus)
        {
            _character.characterStatus.RemoveStatus(status);
        }

        unequipWeaponAction?.Invoke();
    }

    public virtual void Attack()
    {
        if (_character.characterAction.isAttacking || _character.characterAction.isCasting || !_character.characterAction.canAttack)
            return;

        _character.characterBehavior.ChangeBehavior(BehaviorType.Attack);
        AudioManager.instance.LoadResPlay(clip: attackSound, isReplayIfExist: true);
        //for normal atk some weapon need improve

        RecoilForce();

        if (weaponData.particleEffect != null)
        {
            StartCoroutine(delayParticle());
        }
    }

    private IEnumerator delayParticle()
    {
        yield return new WaitForSeconds(intialDelay / _character.attackSpeed);
        
        GameObject obj = Instantiate(weaponData.particleEffect, _character.weaponPivot);
        obj.transform.SetParent(null);
        Destroy(obj, particleEffectDestroyTime);
    }

    protected virtual void RecoilForce()
    {

    }

    protected virtual void WeaponAttack()
    {
        if (_character.characterType.HasFlag(CharacterType.Player) && !UIManager.instance.interactMode)
        {
            _timeSinceAtk += Time.deltaTime;

            if (_character.characterController.fireButton.isTab && _timeSinceAtk >= attackInterval)
            {
                Attack();
                _timeSinceAtk = 0;
            }

            if (_character.characterController.fireButton.isHold)
            {
                _character.characterController.isPlayerAim = true;
                _character.characterController.fireButton.joystick.alpha = 1;

                if (_timeSinceAtk >= attackInterval)
                {
                    Attack();
                    _timeSinceAtk = 0;
                }
            }
            else
            {
                _character.characterController.fireButton.joystick.alpha = 0;
            }
        }

    }

    public void EnemyAttack()
    {
        if (_character.characterAction.isAttacking || !_character.characterAction.canAttack)
            return;

        if (_character.characterType.HasFlag(CharacterType.Enemy))
        {
            if (_timeSinceAtk >= attackInterval)
            {
                Attack();
                _timeSinceAtk = 0;
            }

            _timeSinceAtk += Time.deltaTime;
        }
    }

    public void ResetAttackInterval()
    {
        _timeSinceAtk = attackInterval;
    }

    public static T CopyComponent<T>(T original, GameObject destination) where T : Component
    {
        var type = original.GetType();
        var copy = destination.AddComponent(type);
        var fields = type.GetFields();
        foreach (var field in fields) field.SetValue(copy, field.GetValue(original));
        return copy as T;
    }
}

