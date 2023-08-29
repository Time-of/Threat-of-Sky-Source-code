using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 
 * BowCharacter가 스킬을 쓰는 동안 이동을 멈추고, 상체 블렌딩을 적용하지 않게 하기 위해 쓰입니다.
 * BowCharacter의 bIsUsingSkill 변수를 설정합니다.
 */
public class AES_BowCharacterUseSkill : AAnimationEventStateBase
{
	public override void EventBegin(GameObject EventCaller)
	{
		Debug.Log("Begin!");

		BowCharacter BowPlayer = EventCaller.GetComponent<BowCharacter>();

		if (BowPlayer != null)
		{
			BowPlayer.bIsUsingSkill = true;
			BowPlayer.GetMovementComponent().bUseControllerRotationYaw = false;

			BowCharacterAnimation BowPlayerAnim = BowPlayer.GetAnimComponent().GetComponent<BowCharacterAnimation>();

			if (BowPlayerAnim != null)
			{
				BowPlayerAnim.bUpdateSpineLookAtForward = false;
			}
		}
	}

	public override void EventEnd(GameObject EventCaller)
	{
		Debug.Log("End!");

		BowCharacter BowPlayer = EventCaller.GetComponent<BowCharacter>();

		if (BowPlayer != null)
		{
			BowPlayer.bIsUsingSkill = false;
			BowPlayer.GetMovementComponent().bUseControllerRotationYaw = true;

			BowCharacterAnimation BowPlayerAnim = BowPlayer.GetAnimComponent().GetComponent<BowCharacterAnimation>();

			if (BowPlayerAnim != null)
			{
				BowPlayerAnim.bUpdateSpineLookAtForward = true;
			}
		}
	}
}
