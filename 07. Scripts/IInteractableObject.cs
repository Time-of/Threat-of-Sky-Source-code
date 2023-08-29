using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/**
 * 작성자: 20181220 이성수
 * IInteractableObject는 상호작용 가능한 오브젝트를 확장할 인터페이스 클래스입니다.
 * Interact() 함수를 확장하여 상호작용 시 발생할 일을 정의해 주면 됩니다.
 * 상호작용을 편리하게 하기 위해 존재하는 클래스입니다.
 * 예를 들면, HP 회복 포션이라면 Interact()의 구현은 InteractingCharacter.RestoreHealth(Value); 등을 호출하면 됩니다.
 */
public interface IInteractableObject
{
	/// <summary>
	/// 상호작용 확장 기능입니다.
	/// 아래는 ViewID를 이용해 대상 게임 오브젝트를 포톤에서 찾는 방법 예시입니다.
	/// var TempCharacter = PhotonView.Find(InteractingCharacterViewID).gameObject;
	/// ACharacterBase InteractinCharacter = TempCharacter.GetComponent<ACharacterBase>();
	/// </summary>
	/// <param name="InteractingCharacterViewID">상호작용한 캐릭터가 가진 PhotonView 컴포넌트의 ViewID입니다. (즉, 상호작용을 누른 캐릭터입니다.)</param>
	public void Interact(int InteractingCharacterViewID);
}
