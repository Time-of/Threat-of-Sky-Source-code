using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 
 * 캐릭터가 스킬을 쓰고 있는지를 체크하여 쓸모 없는 RPC 호출을 최소화합니다.
 * (이미 트리거가 켜져 있는데, 또 트리거를 키는 RPC 호출 방지용!)
 * 또는 스킬을 사용 중 특정 행동을 막기 위해, 스킬 사용 중인 상태를 지정하기 위해 사용합니다.
 */
public class AES_CharacterUseSkill : AAnimationEventStateBase
{
	private ACharacterBase TargetPlayer; 



	public override void EventBegin(GameObject EventCaller)
	{
		TargetPlayer = EventCaller.GetComponent<ACharacterBase>();

		if (TargetPlayer != null)
		{
			TargetPlayer.bIsUsingSkill = true;
		}
	}

	public override void EventEnd(GameObject EventCaller)
	{
		if (TargetPlayer != null)
		{
			TargetPlayer.bIsUsingSkill = false;
		}
	}
}
