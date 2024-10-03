using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NaughtyAttributes;

public class SkillIndicator : Skill
{
    [Header("Direction")]
    [ShowIf("skillIndicatorType", SkillIndicatorType.Direction)] public float directionRange;
    protected SpriteRenderer directionSprite;

    [Header("AOE")]
    [ShowIf("skillIndicatorType", SkillIndicatorType.AOE)] public float AOERadius;
    [ShowIf("skillIndicatorType", SkillIndicatorType.AOE)] public float AOERange;
    protected SpriteRenderer AOERangeSprite;
    protected SpriteRenderer AOETargetSprite;

    [Header("Cone")]
    [ShowIf("skillIndicatorType", SkillIndicatorType.Cone)] public float coneRange;
    [ShowIf("skillIndicatorType", SkillIndicatorType.Cone)] public float coneWidth;
    protected SpriteRenderer coneRangeSprite;

    protected Transform indicatorTarget
    {
        get
        {
            if (skillIndicatorType == SkillIndicatorType.Direction)
                return directionSprite.transform;
            else if (skillIndicatorType == SkillIndicatorType.AOE)
                return AOETargetSprite.transform;
            else if (skillIndicatorType == SkillIndicatorType.Cone)
                return coneRangeSprite.transform;

            return null;
        }
    }

    protected override void Start()
    {
        base.Start();

        if (isActiveSkill)
            SetupIndicator(skillIndicatorType);
    }

    protected override void Update()
    {
        base.Update();

        if (isActiveSkill)
            InputSkillIndicator(skillIndicatorType);
    }

    protected virtual void SetupIndicator(SkillIndicatorType skilltype)
    {
        switch (skilltype)
        {
            case SkillIndicatorType.Direction:
                directionSprite = skillJoystickTransform.GetComponentInChildren<SpriteRenderer>();
                Transform pivotDirect = skillJoystickTransform.GetChild(0).GetComponent<Transform>();
                pivotDirect.transform.localScale = new Vector3(directionRange, directionSprite.transform.localScale.y, directionSprite.transform.localScale.z);
                break;
            case SkillIndicatorType.AOE:
                AOERangeSprite = skillJoystickTransform.GetChild(0).GetComponent<SpriteRenderer>();
                AOETargetSprite = skillJoystickTransform.GetChild(1).GetComponent<SpriteRenderer>();
                AOERangeSprite.transform.localScale = new Vector3(AOERange * 2, AOERange * 2, AOERange * 2);
                AOETargetSprite.transform.localScale = new Vector3(AOERadius, AOERadius, AOERadius);
                break;
            case SkillIndicatorType.Cone:
                coneRangeSprite = skillJoystickTransform.GetComponentInChildren<SpriteRenderer>();
                Transform pivotCone = skillJoystickTransform.GetChild(0).GetComponent<Transform>();
                pivotCone.transform.localScale = new Vector3(coneRange, coneWidth, coneRangeSprite.transform.localScale.z);
                break;
            case SkillIndicatorType.Tab:
                break;
            default:
                break;
        }
        skillJoystick.enabled = true;
        skillJoystick.gameObject.SetActive(true);
    }

    protected virtual void InputSkillIndicator(SkillIndicatorType skilltype)
    {
        float x = skillJoystick.Horizontal;
        float y = skillJoystick.Vertical;
        Vector2 direction = skillJoystick.Direction;

        switch (skilltype)
        {
            case SkillIndicatorType.Direction:

                if (direction != Vector2.zero)
                {
                    _uimanager.gameplayManager.skillWeaponButton.joystick.alpha = 1;
                    skillJoystickTransform.gameObject.SetActive(true);
                    if (_character.characterController.crosshair != null)
                        _character.characterController.crosshair.SetActive(false);
                    float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

                    skillJoystickTransform.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                    if (skillJoystickTransform.transform.eulerAngles.z <= 270 && skillJoystickTransform.transform.eulerAngles.z >= 90)
                    {
                        _character.FlipCharacter(true);
                        _character.characterController.aimPlayer.transform.rotation = Quaternion.AngleAxis(180, Vector3.forward);
                    }
                    else
                    {
                        _character.FlipCharacter(false);
                        _character.characterController.aimPlayer.transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
                    }
                }
                else
                {
                    _uimanager.gameplayManager.skillWeaponButton.joystick.alpha = 0;
                    skillJoystickTransform.gameObject.SetActive(false);
                    if (_character.characterController.crosshair != null)
                        _character.characterController.crosshair.SetActive(true);
                }
                break;

            case SkillIndicatorType.AOE:

                if (direction != Vector2.zero)
                {
                    _uimanager.gameplayManager.skillWeaponButton.joystick.alpha = 1;
                    skillJoystickTransform.gameObject.SetActive(true);
                    if (_character.characterController.crosshair != null)
                        _character.characterController.crosshair.SetActive(false);
                    Vector2 radian = new Vector2(x * AOERange, y * AOERange);

                    AOETargetSprite.transform.localPosition = radian;

                    if (skillJoystickTransform.transform.eulerAngles.z <= 270 && skillJoystickTransform.transform.eulerAngles.z >= 90)
                    {
                        _character.FlipCharacter(true);
                        _character.characterController.aimPlayer.transform.rotation = Quaternion.AngleAxis(180, Vector3.forward);
                    }
                    else
                    {
                        _character.FlipCharacter(false);
                        _character.characterController.aimPlayer.transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
                    }
                }
                else
                {
                    _uimanager.gameplayManager.skillWeaponButton.joystick.alpha = 0;
                    skillJoystickTransform.gameObject.SetActive(false);
                    if (_character.characterController.crosshair != null)
                        _character.characterController.crosshair.SetActive(true);
                    AOETargetSprite.transform.localPosition = new Vector3(0, 0, 0);
                }
                break;

            case SkillIndicatorType.Cone:

                if (direction != Vector2.zero)
                {
                    _uimanager.gameplayManager.skillWeaponButton.joystick.alpha = 1;
                    skillJoystickTransform.gameObject.SetActive(true);
                    if (_character.characterController.crosshair != null)
                        _character.characterController.crosshair.SetActive(false);
                    float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

                    skillJoystickTransform.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                    if (skillJoystickTransform.transform.eulerAngles.z <= 270 && skillJoystickTransform.transform.eulerAngles.z >= 90)
                    {
                        _character.FlipCharacter(true);
                        _character.characterController.aimPlayer.transform.rotation = Quaternion.AngleAxis(180, Vector3.forward);
                    }
                    else
                    {
                        _character.FlipCharacter(false);
                        _character.characterController.aimPlayer.transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
                    }
                }
                else
                {
                    _uimanager.gameplayManager.skillWeaponButton.joystick.alpha = 0;
                    skillJoystickTransform.gameObject.SetActive(false);
                    if (_character.characterController.crosshair != null)
                        _character.characterController.crosshair.SetActive(true);
                }
                break;

            default:
                break;
        }
    }

    protected IEnumerator ResetJoystick()
    {
        skillJoystick.cancelPointer = true;
        yield return new WaitForSeconds(0.2f);
        skillJoystick.cancelPointer = false;
    }
}
