using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 고스트 캐릭터의 연소 스킬 사용 시 걸리는 디버프입니다.
 * 지속시간동안 마나가 회복되지 않습니다.
 */
public class GhostCharacterBuff_CombustionRisk : ACharacterBuffBase
{
	private GhostCharacter Ghost;



	protected override void OnBuffAdded()
	{
		Ghost = TargetCharacter.GetComponent<GhostCharacter>();

		if (Ghost != null)
		{
			Ghost.SetCanRegenMana(false);
		}
	}



	protected override void OnBuffRemoved()
	{
		if (Ghost != null)
		{
			Ghost.SetCanRegenMana(true);
		}
	}
}
