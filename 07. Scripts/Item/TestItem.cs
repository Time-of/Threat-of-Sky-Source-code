using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 아이템 테스트를 위해 사용하는 스크립트입니다.
 */
public class TestItem : ATestItemBase
{
	public ATestItemBase ActualItem;

	


	public override void Use()
	{
		base.Use();

		Debug.Log("테스트 아이템 사용");
	}



	// 테스트용!

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			ACharacterBase Player = other.gameObject.GetComponent<ACharacterBase>();

			//Player?.EquipItem(this);
		}
	}



	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			ACharacterBase Player = other.gameObject.GetComponent<ACharacterBase>();

			//Player?.UnEquipItem(this);
		}
	}
}
