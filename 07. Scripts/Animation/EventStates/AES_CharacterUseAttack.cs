using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 
 * 지속시간동안 캐릭터가 공격을 할 수 없는 상태가 됩니다.
 */
public class AES_CharacterUseAttack : AAnimationEventStateBase
{
	private ACharacterBase TargetPlayer; 



	public override void EventBegin(GameObject EventCaller)
	{
		TargetPlayer = EventCaller.GetComponent<ACharacterBase>();

		if (TargetPlayer != null)
		{
			TargetPlayer.SetCanAttack(false);
		}
	}

	public override void EventEnd(GameObject EventCaller)
	{
		if (TargetPlayer != null)
		{
			TargetPlayer.SetCanAttack(true);
		}
	}
}
