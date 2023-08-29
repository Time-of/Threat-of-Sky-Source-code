using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CharacterGameplay;



/**
 * 작성자: 20181220 이성수
 * 잃은 자의 복수 액티브 아이템용 서브 버프입니다.
 */
public class ItemBuff_RevengeOfTheLostSubBuff : ACharacterBuffBase
{
	[SerializeField]
	private ParticleSystem BuffVfx;

	[SerializeField]
	private AudioClip BuffSound;



	protected override void OnBuffAdded()
	{
		CharacterGameplayHelper.PlayVfx(BuffVfx, transform.position, Quaternion.identity, TargetCharacter.transform);
		SoundManager.Instance.SpawnSoundAtLocation(BuffSound, transform.position, ESoundGroup.SFX, 0.6f, 0.05f);
	}
}
