using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DamageFramework;
using CharacterGameplay;



/**
 * 작성자: 20181220 이성수
 * 틱에 이펙트를 재생하는 DamageZone입니다.
 */
public class DZ_PlayTickFx : DamageZone
{
	[Header("Tick 이펙트")]

	[SerializeField]
	private ParticleSystem Vfx_Tick;

	[SerializeField, Tooltip("활성화하면 Vfx_Tick은 박스 내 랜덤한 위치에서 재생됩니다.")]
	private bool bIsRandomPositionTickVfxInBox = false;

	[SerializeField]
	private bool bUseTickVfxRandomRotation = true;



	[Header("TickAction 이펙트")]

	[SerializeField]
	private ParticleSystem Vfx_TickAction;



	protected override void Tick()
	{
		base.Tick();

		CharacterGameplayHelper.PlayVfx(Vfx_Tick,
			bIsRandomPositionTickVfxInBox ? CharacterGameplayHelper.GetRandomPointInBox(0.5f * BoxBound, transform.position, transform.rotation) : transform.position,
			bUseTickVfxRandomRotation ? Random.rotation : Quaternion.identity);
	}



	protected override void TickAction(GameObject DamageableObject)
	{
		base.TickAction(DamageableObject);

		CharacterGameplayHelper.PlayVfx(Vfx_TickAction,
			DamageableObject.transform.position,
			Random.rotation);
	}
}
