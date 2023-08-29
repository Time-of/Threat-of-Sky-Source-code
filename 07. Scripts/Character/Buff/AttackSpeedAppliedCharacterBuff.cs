using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 공격속도의 영향을 받는 버프입니다.
 */
public class AttackSpeedAppliedCharacterBuff : ACharacterBuffBase
{
	protected override void OnBuffAdded()
	{
		if (TargetCharacter.GetStatusComponent() != null)
		{
			// 공속에 반비례
			CurrentDotDuration = CurrentDotDuration / TargetCharacter.GetStatusComponent().GetAtkSpeedMult;
		}
	}
}
