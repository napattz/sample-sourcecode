using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectMovementType
{
    None,
    Straight,
    Projectile,
    Around,
    Homing
}

public enum ObjectHitType
{
    None,
    Pirece,
    Bounce,
    AfterHit,
}

public class ObjectMovement : MonoBehaviour
{
    public ObjectMovementType movementType;
    public ObjectHitType hitType;
    public GameObject target;

    public GameObject afterHitPrefab;
    public float afterHitDestroy;
    public float speed;
    public float destroyTime;

    private int _hitCount;
    private bool _afterDamage;
    private Character _character;

    public int maxHitCount;

    public AudioClip afterHitSound;
    public string afterHitSoundPath;

    public void InitObjMovement(ObjectMovementType movementType = ObjectMovementType.None, GameObject targetObject = null, float moveSpeed = 0f, float destroyObjectTime = 0.1f, Character character = null, bool hasAfterDamaged = false)
    {
        _character = character != null ? (Character)character.Clone() : null;

        this.movementType = movementType;
        target = targetObject;
        speed = moveSpeed;
        destroyTime = destroyObjectTime;
        _afterDamage = hasAfterDamaged;
    }

    private void Update()
    {
        switch (movementType)
        {
            case ObjectMovementType.None:
                break;
            case ObjectMovementType.Straight:
                transform.position += transform.right * Time.deltaTime * speed;
                break;
            case ObjectMovementType.Projectile:
                break;
            case ObjectMovementType.Around:
                transform.RotateAround(target.transform.position, new Vector3(0, 0, 1), speed);
                break;
            case ObjectMovementType.Homing:
                break;
            default:
                break;
        }

        ObjectDuration();
    }

    private void ObjectDuration()
    {
        if (destroyTime <= 0f)
        {
            Destroy(gameObject);

            if (afterHitPrefab != null)
            {
                Instantiate(afterHitPrefab, gameObject.transform);
            }
        }
        else
        {
            destroyTime -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.GetComponent<Character>() || other.GetComponent<DamageOnTouch>())
            return;

        if (_character == null)
            return;

        if (other.transform.root.GetComponent<CompositeCollider2D>() && !other.isTrigger)
            return;

        if (!_character.characterType.HasFlag(other.gameObject.GetComponent<Character>().characterType))
        {
            switch (hitType)
            {
                case ObjectHitType.Pirece:

                    if (_hitCount >= maxHitCount || other.gameObject.GetComponent<Character>().characterType == CharacterType.Structure)
                    {
                        Destroy(this.gameObject);
                    }
                    else
                    {
                        _hitCount++;
                    }
                    break;

                case ObjectHitType.Bounce:
                    break;

                case ObjectHitType.AfterHit:

                    GameObject obj = Instantiate(afterHitPrefab, transform.position, Quaternion.identity);

                    if (afterHitSound != null || !string.IsNullOrEmpty(afterHitSoundPath))
                    {
                        AudioManager.instance.LoadResPlay(customLocation: afterHitSoundPath, clip: afterHitSound, isPlayOneShot: true);
                    }

                    if (_afterDamage)
                    {
                        DamageOnTouch OnTouchExplos = obj.AddComponent<DamageOnTouch>();
                        OnTouchExplos.Init(_character, _character.characterWeapon.weaponData, damageAreaType: DamageAreaType.AOE);
                    }
                    Destroy(this.gameObject);
                    Destroy(obj, afterHitDestroy);
                    break;

                default:
                    Destroy(this.gameObject);
                    break;
            }
        }
    }

}
