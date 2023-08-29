using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DamageFramework;



/**
 * 작성자: 20181220 이성수
 * 고스트 캐릭터의 붕괴 스킬입니다.
 */
public class DZ_GhostBreakdown : DamageZone
{
	[SerializeField]
	private ParticleSystem StartVfx;



	protected override void OnZoneStarted()
	{
		base.OnZoneStarted();

		if (StartVfx == null) return;

		ParticleSystem VfxInstance = Instantiate(StartVfx, transform.position, transform.rotation);

		VfxInstance.Play();

		Destroy(VfxInstance.gameObject, VfxInstance.main.duration + 0.1f);
	}
}
