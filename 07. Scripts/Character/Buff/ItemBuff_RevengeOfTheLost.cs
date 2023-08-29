using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 잃은 자의 복수 액티브 아이템용 버프입니다.
 * Tick 마다 서브 버프를 추가하는 방식으로 갱신을 구현했습니다.
 * 여기에서 체력 반비례 공격력 및 공격속도를 부여합니다.
 */
public class ItemBuff_RevengeOfTheLost : ACharacterBuffBase
{
	[SerializeField]
	private ItemBuff_RevengeOfTheLostSubBuff SubBuffPrefab;

	private ItemBuff_RevengeOfTheLostSubBuff SubBuffInstance;



	protected override void OnBuffAdded()
	{
		SubBuffInstance = Instantiate(SubBuffPrefab, transform);
		SubBuffInstance.SetBuffDuration(100.0f);
	}



	protected override void ApplyTickBuffToTarget()
	{
		float PlayerHealthInvPercent = 1.0f - TargetCharacter.GetStatusComponent().GetHealthPercent();

		SubBuffInstance.SetBuffAmount(0, Mathf.Lerp(10, 100, PlayerHealthInvPercent));
		SubBuffInstance.SetBuffAmount(1, Mathf.Lerp(1.02f, 1.3f, PlayerHealthInvPercent));

		SubBuffInstance.SetBuffDuration(1.0f);

		TargetCharacter.AddBuff(SubBuffInstance);

		SubBuffInstance.SetBuffDuration(100.0f);
	}



	protected override void OnBuffRemoved()
	{
		if (SubBuffInstance != null) Destroy(SubBuffInstance.gameObject);
	}
}
