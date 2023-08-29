using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 캐릭터 버프 중, 지속 이펙트가 있는 버프 클래스입니다.
 */
public class CharacterBuff_ContinuousVfx : ACharacterBuffBase
{
	[Header("이펙트")]

	[SerializeField, Tooltip("지속시간 동안 재생되는 이펙트입니다. 보통 Loop 가 켜져 있습니다.")]
	private ParticleSystem BuffVfx;

	private ParticleSystem BuffVfxInstance;

	[SerializeField, Tooltip("버프가 추가될 때 한 번만 재생되는 이펙트입니다.")]
	private ParticleSystem BuffVfxPlayOnce;



	protected override void OnBuffAdded()
	{
		if (BuffVfx != null)
		{
			BuffVfxInstance = Instantiate(BuffVfx, transform);

			BuffVfxInstance.Play();
		}

		CharacterGameplay.CharacterGameplayHelper.PlayVfx(BuffVfxPlayOnce, transform.position, Quaternion.identity, transform);
	}



	protected override void OnBuffRemoved()
	{
		if (BuffVfx != null)
			Destroy(BuffVfxInstance.gameObject, BuffVfxInstance.main.duration + 0.1f);
	}
}
