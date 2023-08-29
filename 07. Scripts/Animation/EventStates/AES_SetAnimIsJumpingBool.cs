using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 캐릭터의 IsJumping 애니메이터 Bool 값을 지속시간 동안 true 로 만듭니다.
 */
public class AES_SetAnimIsJumpingBool : AAnimationEventStateBase
{
	public override void EventBegin(GameObject EventCaller)
	{
		EventCaller.GetComponent<BowCharacter>().GetAnimComponent().GetAnimator().SetBool("IsJumping", true);
	}



	public override void EventEnd(GameObject EventCaller)
	{
		EventCaller.GetComponent<BowCharacter>().GetAnimComponent().GetAnimator().SetBool("IsJumping", true);
	}
}
