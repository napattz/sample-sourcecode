using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SkillIndicatorType { Direction, AOE, Cone,Tab }
public class Skill : MonoBehaviour
{
    [Header("Info")]
    [Space(5)]
    public CharacterSkillData characterSkillData;
    public GameObject skillPrefab;
    public int skillLevel = 1;
    public bool isActiveSkill;
    [ShowIf("isActiveSkill")] public float skillCooldown;

    [Header("Indicator")]
    [Space(5)]
    public SkillIndicatorType skillIndicatorType;
    public GameObject skillIndicatorPrefab;
    public GameObject skillIndicatorObject;

    protected GameObject skillObject;
    protected Transform skillJoystickTransform;
    protected ButtonController skillJoystick;
    protected UIManager _uimanager; 
    protected DamageOnTouch _damageOnTouch;
    protected Collider2D _damageAreaCollider2D;
    protected GameObject _damageArea;
    protected Animator _skillAnimator;
    protected Character _character;
    protected Weapon _weapon;

    protected bool _isCasting;
    protected bool _isStandaloneCast;

    protected virtual void Start()
    {
        Init();
    }

    protected virtual void Update()
    {
        if (_weapon != null && _character.characterType.HasFlag(CharacterType.Player))
        {
            if (_character.characterStamina.currentStamina >= _weapon.weaponData.allWeaponRefine[_weapon.currentRefine - 1].skillStamina)
            {
                CanUseSkillUI(true);

                skillJoystick.enabled = !_isCasting;
            }
            else
            {
                CanUseSkillUI(false);
            }
        }
    }

    public void Init()
    {
        if (!_weapon)
        {
            skillLevel = characterSkillData.skillLevel;

            if (isActiveSkill)
            {
                skillCooldown = characterSkillData.allSkillLevel[skillLevel - 1].cooldown;
            }
        }
    }

    public virtual void CastSkill()
    {
        if (skillPrefab != null)
        {
            skillObject = Instantiate(skillPrefab);
            _damageAreaCollider2D = skillObject.GetComponentInChildren<Collider2D>();
            _skillAnimator = skillObject.GetComponent<Animator>();
            _damageOnTouch = _damageAreaCollider2D.gameObject.AddComponent<DamageOnTouch>();
            _damageOnTouch.Init(_character, _character.characterWeapon.weaponData);
        }

        if (_character.characterStamina.currentStamina >= _weapon.weaponData.allWeaponRefine[_weapon.currentRefine - 1].skillStamina)
        {
            _character.characterStamina.ChangeStamina(-_weapon.weaponData.allWeaponRefine[_weapon.currentRefine - 1].skillStamina);
        }

        ClearSkill();
    }
    
    public virtual void CastCharacterSkill()
    {
        AudioManager.instance.LoadResPlay(customLocation: "SFX/Player/UseCharacterSkill");
        _character.characterAction.canSpell = false;
        _character.characterAction.UpdateMaxCooldown(this);
        _character.characterAction.SkillDelayLogo();
        _character.characterBehavior.ChangeBehavior(BehaviorType.Cast);
    }

    public void InitializeWeapon()
    {
        _uimanager = UIManager.instance;
        _character = RogueManager.instance.character;
        _weapon = GetComponent<Weapon>();
        skillJoystick = _uimanager.gameplayManager.skillWeaponButton;
        if (!skillIndicatorType.HasFlag(SkillIndicatorType.Tab))
        {
            skillIndicatorObject = Instantiate(skillIndicatorPrefab, _character.transform);
            skillJoystickTransform = skillIndicatorObject.transform;
        }
        _uimanager.gameplayManager.weaponSkillSprite.sprite = _weapon.weaponData.weaponSkillLogo;
    }

    public void InitializeSkillCharacter(Character character)
    {
        _character = character;

        if (_character.characterType.HasFlag(CharacterType.Player) && isActiveSkill)
        {
            _uimanager = UIManager.instance;

            skillJoystick = _uimanager.gameplayManager.skillCharacterButton;

            if (!skillIndicatorType.HasFlag(SkillIndicatorType.Tab))
            {
                skillIndicatorObject = Instantiate(skillIndicatorPrefab, _character.transform);
                skillJoystickTransform = skillIndicatorObject.transform;
            }

            _uimanager.gameplayManager.skillCharacterButton.joystick.alpha = 0;
            _uimanager.gameplayManager.characterSkillSprite.sprite = characterSkillData.characterSkillLogo;
        }
    }
    protected void CanUseSkillUI(bool canuse)
    {
        if (canuse)
        {
            UIManager.instance.FillWeaponSkillImage(false);
            UIManager.instance.gameplayManager.skillWeaponButton.GetComponent<Mask>().enabled = false;
            UIManager.instance.gameplayManager.skillWeaponButton.enabled = true;
        }
        else
        {
            UIManager.instance.FillWeaponSkillImage(true);
            UIManager.instance.gameplayManager.skillWeaponButton.GetComponent<Mask>().enabled = true;
            UIManager.instance.gameplayManager.skillWeaponButton.enabled = false;
        }
    }

    protected IEnumerator DestroySkillPrefab(float destroyTime = 0f)
    {
        yield return new WaitForSeconds(destroyTime);
        Destroy(skillObject);
    }

    public virtual void ClearSkill()
    {
        _character.characterRigidbody.isKinematic = false;
        _character.characterMovement.canMove = true;
        _character.characterMovement.canDash = true;
        _character.characterAction.isChanelling = false;

        UIManager.instance.gameplayManager.clearSkillArea.GetComponent<ClearSkill>().HideClearArea();
    }
}