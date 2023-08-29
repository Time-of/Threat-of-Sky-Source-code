using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DamageFramework;
using CharacterGameplay;



/**
 * 작성자: 20181220 이성수
 * 아처 캐릭터의 피해 지역(장판기)인 죽음의 비 클래스입니다.
 */
public class DZ_ArcherDeathRain : DamageZone
{
	[Header("사운드")]

	[SerializeField]
	private List<AudioClip> ArrowFlyingSoundList = new List<AudioClip>();

	[SerializeField]
	private List<AudioClip> ArrowHitSoundList = new List<AudioClip>();



	protected override void Tick()
	{
		SoundManager.Instance.SpawnSoundAtLocation(CharacterGameplayHelper.GetRandomSoundInList(ArrowFlyingSoundList),
				transform.position, ESoundGroup.SFX,
				1.5f, 0.05f, AudioRolloffMode.Linear);
	}



	protected override void TickAction(GameObject DamageableObject)
	{
		if (DamageHelper.ApplyDamage(DamageableObject, BaseDamageRate * DamageMult, this.gameObject, DamageType, DamageMult))
		{
			SoundManager.Instance.SpawnSoundAtLocation(CharacterGameplayHelper.GetRandomSoundInList(ArrowHitSoundList),
				DamageableObject.transform.position, ESoundGroup.SFX,
				1.0f, 0.05f, AudioRolloffMode.Linear);
		}
	}
}
