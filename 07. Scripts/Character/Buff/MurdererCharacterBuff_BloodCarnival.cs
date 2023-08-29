using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 머더러 캐릭터의 피의 축제 스킬 버프입니다.
 */
public class MurdererCharacterBuff_BloodCarnival : ACharacterBuffBase
{
	private MurdererCharacter Murderer;



	protected override void OnBuffAdded()
	{
		Murderer = TargetCharacter.GetComponent<MurdererCharacter>();

		if (Murderer != null)
		{
			Murderer.SetRestoreHealthWhenAttack(true);
		}
	}



	protected override void OnBuffRemoved()
	{
		if (Murderer != null)
		{
			Murderer.SetRestoreHealthWhenAttack(false);
		}
	}
}
