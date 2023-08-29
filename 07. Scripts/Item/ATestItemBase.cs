using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;



/**
 * 작성자: 20181220 이성수
 * 
 * !! 캐릭터에게 아이템 획득 적용이 가능한지 '테스트하기 위해 만든 클래스'입니다 !!
 * 
 * 사용형 아이템인 경우, Use()를 재정의하여 사용 시 동작을 재정의합니다.
 */
//[RequireComponent(typeof(PhotonView))]
public abstract class ATestItemBase : MonoBehaviourPun
{
	[Header("Item Property")]

	[SerializeField, ReadOnlyProperty]
	protected ACharacterBase OwnerCharacter;

	[SerializeField]
	protected string ItemName = "";

	public string Item_Name { get { return ItemName; } }

	public string GetItemName() { return ItemName; }

	[SerializeField]
	protected bool bIsUsableItem = false;

	public bool IsUsableItem { get { return bIsUsableItem; } }



	public virtual void Use()
	{

	}



	#region 장착 및 장착 해제
	// 아이템을 장착합니다. 만약 NewOwner가 없다면, 무시됩니다.
	public void Equip(ACharacterBase NewOwner)
	{
		if (NewOwner == null) return;

		if (OwnerCharacter != null) UnEquip();

		OwnerCharacter = NewOwner;

		//OwnerCharacter.OnItemEquipped(this);

		gameObject.SetActive(false);
	}



	// 아이템 장착을 해제합니다.
	public void UnEquip()
	{
		//OwnerCharacter.OnItemUnEquipped(this);

		OwnerCharacter = null;
	}
	#endregion
}
