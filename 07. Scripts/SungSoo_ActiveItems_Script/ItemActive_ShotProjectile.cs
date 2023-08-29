using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CharacterGameplay;



/**
 * 작성자: 20181220 이성수
 * AProjectileBase 기반 투사체를 발사하는 액티브 아이템입니다.
 */
public class ItemActive_ShotProjectile : AActiveItemBase
{
	[SerializeField]
	private AProjectileBase ProjectileToShot;



	public override void UseItem()
	{
		AProjectileBase SpawnedProjectile = GameObject.Instantiate<AProjectileBase>(ProjectileToShot, transform.position,
							ownerCharacter.GetCameraForwardRotation());

		float DamageMult = ownerCharacter.GetStatusComponent().GetFinalDamage().Item1;

		SpawnedProjectile.StartFire(ownerCharacter.gameObject, DamageMult, DamageMult);
	}
}
