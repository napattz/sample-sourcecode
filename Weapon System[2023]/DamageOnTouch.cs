using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum DamageType { Physical, Art, True }
public enum DamageAreaType { Single, AOE }

public class DamageOnTouch : MonoBehaviour
{
    Character _character;
    WeaponData _weaponData;
    DamageType _damageType;
    DamageAreaType _damageSubType;

    List<Status> _applyStatus = new List<Status>();
    Action _action;
    float _knockbackForce;
    float _damageModifier;
    int _hitEffectIndex;

    bool _isFixedDamage = false;
    float _fixedDamage = 1f;

    public void Init(Character character, WeaponData weaponData, DamageType damageType = DamageType.Physical, DamageAreaType damageAreaType = DamageAreaType.Single, float damageModifier = 1f, int hitEffectIndex = 0)
    {
        _character = (Character)character.Clone();
        _weaponData = weaponData;
        _damageType = damageType;
        _damageSubType = damageAreaType;
        _hitEffectIndex = hitEffectIndex;
        _damageModifier = damageModifier;
        SetKnockbackForce(weaponData.allWeaponRefine[weaponData.refine - 1].knockbackForce);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        try
        {
            if (collision.transform.root.GetComponent<CompositeCollider2D>() && !collision.isTrigger)
                return;

            if (!collision.transform.root.GetComponent<Character>() || collision.GetComponent<DamageOnTouch>())
                return;

            if (!collision.gameObject)
                return;

            if (collision.GetComponent<AimAssist>()) return;
        }
        catch { return; }


        Character target = collision.transform.root.GetComponent<Character>();
        Rigidbody2D targetRigidbody = target.GetComponent<Rigidbody2D>();

        if ((target.characterType.HasFlag(CharacterType.Enemy) && _character.characterType.HasFlag(CharacterType.Player)) ||
            (target.characterType.HasFlag(CharacterType.Player) && _character.characterType.HasFlag(CharacterType.Enemy)))
        {
            Vector3 direction = (collision.bounds.center - transform.position).normalized;

            targetRigidbody.AddForce(direction * _knockbackForce);

            AudioManager.instance.LoadResPlay(customLocation: target.characterType.HasFlag(CharacterType.Player) ? "SFX/Player/PlayerHit" : "SFX/Enemy/MonsterHit");

            float damageDealt = target.characterHealth.CalculateDamage(_character, _damageType, _damageModifier);

            foreach (Status status in _applyStatus)
            {
                target.characterStatus.AddStatus(status);
            }
            if (_applyStatus.Count != 0)
            {
                target.characterHealth._status = _applyStatus[0];
            }
            target.characterHealth.ChangeHealth(_isFixedDamage ? -_fixedDamage : - damageDealt);

            if (_character.characterWeapon.hitEffect.Count > 0 && _character.characterWeapon.hitEffect[_hitEffectIndex] != null)
            {
                GameObject hitEffect = Instantiate(_character.characterWeapon.hitEffect[_hitEffectIndex], target.transform.position, Quaternion.identity);

                /*if (hitEffect.GetComponent<FloatingDamage>())
                {
                    hitEffect.GetComponent<FloatingDamage>().CreateFloatingDamage(target.transform, (int)Mathf.Ceil(damageDealt));
                }*/
            }

            if (_action != null)
                _action?.Invoke();
        }
    }

    public void SetKnockbackForce(float knockbackForce)
    {
        _knockbackForce = knockbackForce;
    }

    public void SetAction(Action action)
    {
        _action = action;
    }

    public void SetFixedDamage(float fixedDamage)
    {
        _isFixedDamage = true;
        _fixedDamage = fixedDamage;
    }

    public void ApplyStatus(Status status)
    {
        _applyStatus.Add(status);
    }

    public void ClearApplyStatus()
    {
        _applyStatus.Clear();
    }

    public Character GetOwner()
    {
        return _character;
    }
}
