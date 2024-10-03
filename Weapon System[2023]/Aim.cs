using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Aim : MonoBehaviour
{
    private ButtonController aimJoystick;
    public bool canAim;

    private Character _character;
    [HideInInspector] public Vector2 playerRotation;

    void Start()
    {
        if(_character == null)
        {
            _character = RogueManager.instance.character;
            aimJoystick = UIManager.instance.gameplayManager.attackButton;
        }
    }

    private void Update()
    {
        playerRotation = transform.right;

        if (_character.characterController.isPlayerAim)
        {
            float x = aimJoystick.Horizontal;
            float y = aimJoystick.Vertical;

            if (x != 0.0 || y != 0.0)
            {
                float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;

                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                _character.characterController.isPlayerAim = true;
                if (transform.eulerAngles.z <= 270 && transform.eulerAngles.z >= 90)
                {
                    _character.FlipCharacter(true);
                }
                else
                {
                    _character.FlipCharacter(false);
                }
            }
            else
            {
                _character.characterController.isPlayerAim = false;
            }
        }

    }
}
