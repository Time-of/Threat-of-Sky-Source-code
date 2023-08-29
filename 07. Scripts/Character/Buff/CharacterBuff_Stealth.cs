using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 은신 버프입니다. 끝날 때 은신을 해제합니다.
 */
public class CharacterBuff_Stealth : ACharacterBuffBase
{
	[Header("이펙트")]

	[SerializeField, Tooltip("버프가 추가될 때 한 번만 재생되는 이펙트입니다.")]
	private ParticleSystem BuffVfxPlayOnce;



	protected override void OnBuffAdded()
	{
		CharacterGameplay.CharacterGameplayHelper.PlayVfx(BuffVfxPlayOnce, transform.position, Quaternion.identity, transform);
	}



	protected override void OnBuffRemoved()
	{
		TargetCharacter.Decloak();
	}
}
