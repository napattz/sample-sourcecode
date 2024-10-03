using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimAssist : MonoBehaviour
{
    public List<Transform> allTargetsList = new List<Transform>();
    public bool horizontalAim;
    public bool verticalAim;

    private Character _character;
    private float _turnRate = 1000f;

    private const float _defaultTurnRate = 1000f;

    void Start()
    {
        _character = GetComponentInParent<Character>();
        _turnRate = _defaultTurnRate;
    }

    void Update()
    {
        if (allTargetsList.Count > 0)
        {
            Transform closestTarget = GetClosestTarget(allTargetsList);
            Quaternion targetRotation = GetRotationTo(closestTarget);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnRate * Time.deltaTime);

            if (transform.eulerAngles.z <= 270 && transform.eulerAngles.z >= 90)
            {
                if (transform.eulerAngles.z >= 80 && transform.eulerAngles.z < 110)
                    return;

                _character.FlipCharacter(true);
            }
            else
            {
                _character.FlipCharacter(false);
            }
        }
    }

    public void ChangeTurnRate(float turnRate = _defaultTurnRate)
    {
        _turnRate = turnRate;
    }

    Quaternion GetRotationTo(Transform target)
    {
        Vector3 dir = target.GetComponent<Collider2D>().bounds.center - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (horizontalAim)
        {
            return Quaternion.AngleAxis(dir.x > 0 ? 0 : 180, Vector3.forward);
        }
        else if (verticalAim)
        {
            return Quaternion.AngleAxis(dir.y > 0 ? 90 : 270, Vector3.forward);
        }

        return Quaternion.AngleAxis(angle, Vector3.forward);
    }

    Transform GetClosestTarget(List<Transform> targets)
    {
        Transform closest = null;
        float minDistance = float.MaxValue;

        if (targets.Count > 0)
        {
            foreach (var t in targets)
            {
                if (t != null)
                {
                    float distance = Vector2.Distance(t.position, transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closest = t;
                    }
                }
            }
        }

        return closest;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.GetComponent<Character>())
            return;

        if (collision.gameObject.GetComponent<Character>().characterType.HasFlag(CharacterType.Enemy)
            && _character.characterType.HasFlag(CharacterType.Player))
        {
            _character.characterController.aimAssistMode = true;
            allTargetsList.Add(collision.GetComponent<Transform>());
        }

        if (collision.gameObject.GetComponent<Character>().characterType.HasFlag(CharacterType.Player)
            && _character.characterType.HasFlag(CharacterType.Enemy))
        {
            allTargetsList.Add(collision.GetComponent<Transform>());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.gameObject.GetComponent<Character>())
            return;

        if (collision.gameObject.GetComponent<Character>().characterType.HasFlag(CharacterType.Enemy)
            && _character.characterType.HasFlag(CharacterType.Player))
        {
            //_character.characterController.aimAssistMode = false;
            allTargetsList.Remove(collision.GetComponent<Transform>());
        }

        if (collision.gameObject.GetComponent<Character>().characterType.HasFlag(CharacterType.Player)
            && _character.characterType.HasFlag(CharacterType.Enemy))
        {
            allTargetsList.Remove(collision.GetComponent<Transform>());
        }
    }
}
