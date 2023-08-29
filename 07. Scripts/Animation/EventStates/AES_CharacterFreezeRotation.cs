using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 
 * 지속시간동안 캐릭터가 컨트롤러 회전을 따라가지 않게 됩니다.
 */
public class AES_CharacterFreezeRotation : AAnimationEventStateBase
{
	private ACharacterBase TargetPlayer; 



	public override void EventBegin(GameObject EventCaller)
	{
		TargetPlayer = EventCaller.GetComponent<ACharacterBase>();

		if (TargetPlayer != null)
		{
			TargetPlayer.GetMovementComponent().bUseControllerRotationYaw = false;
		}
	}

	public override void EventEnd(GameObject EventCaller)
	{
		if (TargetPlayer != null)
		{
			TargetPlayer.GetMovementComponent().bUseControllerRotationYaw = true;
		}
	}
}
