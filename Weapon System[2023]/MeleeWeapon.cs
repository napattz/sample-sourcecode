using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    public enum MeleeDamageAreaShapes { Circle, Box }
	public enum MeleeDamageAreaModes { Generated, Existing }

	[Header("Melee Variable")]
	public MeleeDamageAreaShapes damageAreaShape;
	public Vector3 areaSize = new Vector3(1, 1);
	public Vector3 areaOffset = new Vector3(1, 0);

	public float activeDuration = 0.1f;

	protected Collider _damageAreaCollider;
	protected Collider2D _damageAreaCollider2D;
	protected Color _gizmosColor;
	protected Vector3 _gizmoSize;
	protected CircleCollider2D _circleCollider2D;
	protected BoxCollider2D _boxCollider2D;
	protected BoxCollider _boxCollider;
	protected SphereCollider _sphereCollider;
	protected DamageOnTouch _damageOnTouch;
	protected Vector3 _gizmoOffset;
	protected GameObject _damageArea;

	protected override void Initialization()
	{
		base.Initialization();

		if (_damageArea == null)
		{
			CreateDamageArea(areaSize);
			DisableDamageArea();

			if(hitBoxTransform != null)
            {
				_damageArea.transform.SetParent(hitBoxTransform);
			}
		}
		
		/*if (_character.characterController.crosshair != null)
        {
			Destroy(_character.characterController.crosshair);
		}*/
	}
    protected override void Update()
    {
        base.Update();

        if (_character.isFlip)
        {
			transform.localScale = new Vector3(1, -1, 1);
        }
        else
        {
			transform.localScale = new Vector3(1, 1, 1);
		}
    }

    public override void Attack()
    {
        if (_character.characterAction.isAttacking || _character.characterAction.isCasting || !_character.characterAction.canAttack)
            return;

        StopCoroutine(MeleeWeaponAttack());
		base.Attack();
		StartCoroutine(MeleeWeaponAttack(activeDuration));
	}

	protected void CreateDamageArea(Vector3 areaSize)
	{
		_damageArea = new GameObject();
		_damageArea.name = "DamageArea";
		_damageArea.transform.position = this.transform.position;
		_damageArea.transform.rotation = this.transform.rotation;
		_damageArea.transform.SetParent(this.transform);
		_damageArea.transform.localScale = Vector3.one;
		_damageArea.layer = this.gameObject.layer;
		_damageOnTouch = _damageArea.AddComponent<DamageOnTouch>();
		_damageOnTouch.Init(_character, weaponData);

        Rigidbody2D rigidBody = _damageArea.AddComponent<Rigidbody2D>();
        rigidBody.isKinematic = true;
        rigidBody.sleepMode = RigidbodySleepMode2D.NeverSleep;
        rigidBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (damageAreaShape == MeleeDamageAreaShapes.Circle)
		{
            _circleCollider2D = _damageArea.AddComponent<CircleCollider2D>();
			_circleCollider2D.transform.position = this.transform.position + this.transform.rotation * areaOffset;
			_circleCollider2D.radius = areaSize.x / 2;
			_damageAreaCollider2D = _circleCollider2D;
			_damageAreaCollider2D.isTrigger = true;
		}
		if (damageAreaShape == MeleeDamageAreaShapes.Box)
		{
            _boxCollider2D = _damageArea.AddComponent<BoxCollider2D>();
			_boxCollider2D.offset = areaOffset;
			_boxCollider2D.size = areaSize;
			_damageAreaCollider2D = _boxCollider2D;
			_damageAreaCollider2D.isTrigger = true;
		}
	}

	protected virtual void ClearDamageArea()
    {
		Destroy(_damageArea);
    }

	protected virtual IEnumerator MeleeWeaponAttack(float activeDuration = 0.1f)
	{
        DisableDamageArea();
        yield return new WaitForSeconds(intialDelay / _character.attackSpeed);
        EnableDamageArea();
        yield return new WaitForSeconds(activeDuration);
        DisableDamageArea();
    }

	protected virtual void EnableDamageArea()
	{
		if (weaponData.hitSound != null) { AudioManager.instance.LoadResPlay(clip: weaponData.hitSound, isPlayOneShot: true); }

		if (_damageAreaCollider2D != null)
		{
			_damageAreaCollider2D.enabled = true;
		}
		if (_damageAreaCollider != null)
		{
			_damageAreaCollider.enabled = true;
		}
	}

	protected virtual void DisableDamageArea()
	{
		if (_damageAreaCollider2D != null)
		{
			_damageAreaCollider2D.enabled = false;
		}
		if (_damageAreaCollider != null)
		{
			_damageAreaCollider.enabled = false;
		}
	}

	protected virtual void OnDrawGizmosSelected()
	{
		if (!Application.isPlaying)
		{
			DrawGizmos();
		}
	}

	protected virtual void DrawGizmos()
	{

		if (damageAreaShape == MeleeDamageAreaShapes.Box)
		{
			Gizmos.DrawWireCube(this.transform.position + areaOffset, areaSize);
		}

		if (damageAreaShape == MeleeDamageAreaShapes.Circle)
		{
			Gizmos.DrawWireSphere(this.transform.position + areaOffset, areaSize.x / 2);
		}
	}
}
