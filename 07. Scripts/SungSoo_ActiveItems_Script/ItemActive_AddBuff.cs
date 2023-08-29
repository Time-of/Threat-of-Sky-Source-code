using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 버프를 추가하는 액티브 아이템입니다.
 */
public class ItemActive_AddBuff : AActiveItemBase
{
	[SerializeField]
	private ACharacterBuffBase BuffPrefabToAdd;



	public override void UseItem()
	{
		ownerCharacter.AddBuff(BuffPrefabToAdd);
	}
}
