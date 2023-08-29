using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 액티브 아이템인 '세계 파괴자' 입니다.
 * 사용 시 거대한 핵폭탄 투하 오브젝트를 소환합니다.
 */
public class ItemActive_WorldDestroyer : AActiveItemBase
{
	[SerializeField]
	private WorldDestroyerNuclearMachine NuclearMachine;



	public override void UseItem()
	{
		WorldDestroyerNuclearMachine SpawnedNuclearMachine = Instantiate(NuclearMachine, transform.position, Quaternion.identity);

		SpawnedNuclearMachine.StartNuclearCount(ownerCharacter.GetStatusComponent().GetFinalDamage().Item1);
	}
}
