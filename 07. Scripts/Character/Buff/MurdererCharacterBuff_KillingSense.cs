using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 머더러 캐릭터의 기척 죽이기 스킬 버프입니다.
 */
public class MurdererCharacterBuff_KillingSense : ACharacterBuffBase
{
	private MurdererCharacter Murderer;



	protected override void OnBuffAdded()
	{
		Murderer = TargetCharacter.GetComponent<MurdererCharacter>();

		if (Murderer != null)
		{
			Murderer.SetAlwaysEvade(true);
			Murderer.gameObject.layer = LayerMask.NameToLayer("IgnoreOnlyEnemies");
		}
	}



	protected override void OnBuffRemoved()
	{
		if (Murderer != null)
		{
			Murderer.SetAlwaysEvade(false);
			Murderer.gameObject.layer = LayerMask.NameToLayer("Player");
		}
	}
}
