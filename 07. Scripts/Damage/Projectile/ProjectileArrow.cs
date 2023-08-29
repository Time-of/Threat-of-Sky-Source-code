using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 화살 투사체입니다.
 * BowCharacter가 사용합니다.
 */
public class ProjectileArrow : AProjectileBase
{
	[Header("VFX")]

	[SerializeField]
	protected ParticleSystem StartFx;

	[SerializeField]
	protected ParticleSystem HitFx;



	[Header("SFX")]

	[SerializeField]
	protected AudioClip ShotSound;

	[SerializeField]
	protected AudioClip HitSound;



	protected override void OnEnable()
	{
		base.OnEnable();

		SpawnParticle(StartFx, true);

		SoundManager.Instance.SpawnSoundAtLocation(ShotSound, transform.position, ESoundGroup.SFX, 1.0f, 0.05f);
	}



	protected void SpawnParticle(ParticleSystem FxToSpawn, bool ApplyLookAt)
	{
		CharacterGameplay.CharacterGameplayHelper.PlayVfx(FxToSpawn, transform.position,
			!ApplyLookAt ? Quaternion.identity : Quaternion.LookRotation(transform.forward, Vector3.up));
	}



	protected override void OnProjectileHitSuccessed(Collider Other)
	{
		//Debug.Log("<color=green>ProjectileArrow 명중!</color> <color=blue>대상 레이어: " + Other.gameObject.layer + "</color>");
	}



	protected override void OnProjectileHitFailed(Collider Other)
	{
		//Debug.Log("<color=red>ProjectileArrow 명중 실패!</color> <color=blue>대상 레이어: " + Other.gameObject.layer + "</color>");
	}



	protected override void OnProjectileLateEnterTrigger(Collider Other)
	{
		SpawnParticle(HitFx, false);

		SoundManager.Instance.SpawnSoundAtLocation(HitSound, transform.position, ESoundGroup.SFX, 1.0f, 0.05f);

		base.OnProjectileLateEnterTrigger(Other);
	}
}
