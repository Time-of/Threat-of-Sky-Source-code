using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DamageFramework;



/**
 * 작성자: 20181220 이성수
 * 고스트 캐릭터의 섬멸 스킬입니다.
 */
public class DZ_GhostAnnihilation : DamageZone
{
	[SerializeField]
	private ParticleSystem HitVfx;



	protected override void TickAction(GameObject DamageableObject)
	{
		if (DamageHelper.ApplyDamage(DamageableObject, BaseDamageRate * DamageMult, this.gameObject, DamageType, DamageMult))
		{
			SoundManager.Instance.SpawnSoundAtLocation(TickActionSoundClip, transform.position, ESoundGroup.SFX, 1.0f, 0.05f, AudioRolloffMode.Linear);

			ParticleSystem VfxInstance = Instantiate(HitVfx, DamageableObject.transform.position, Quaternion.identity);

			VfxInstance.Play();

			Destroy(VfxInstance.gameObject, VfxInstance.main.duration + 0.1f);
		}
	}
}
