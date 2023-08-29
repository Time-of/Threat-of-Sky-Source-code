using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;



/**
 * 작성자: 20181220 이성수
 * IDamageable 인터페이스 사용법을 적어 놓은 예제 클래스입니다.
 * 절차적으로 설명합니다.
 * 1. IDamageable을 확장하기 위해, MonoBehaviourPun(또는 MonoBehaviour) 옆에 IDamageable을 추가합니다.
 * 2. void IDamageable.TakeDamage(...)를 '정의' 합니다. (public 등 접근 지정자 필요 없음!)
 * 3. IDamageable.TakeDamage에서 RPC를 호출합시다.
 * 3 - a. RPC는 먼저 Master 로 보내고, Master 가 All 에게 전송하는 방식을 추천합니다. (데이터 안정성, 실행 동시성)
 * 4. RpcTarget.All 의 대상 RPC 함수에서 실질적인 연산을 구현합시다.
 * 4 - a. 이 예제에서는, RpcTakeDamageAll(...) 함수에서 연산을 구현하고 있습니다.
 * 5. ADamageType을 구현합니다. ADamageType은 화상, 독성, 출혈 등의 공격을 구현할 수 있는 '오브젝트' 입니다.
 * 6. ADamageType은 단순히 인스턴싱하고, 함수 하나만 호출해주면 끝입니다!
 * 7. 잘 모르는 것이나 헷갈리는 것이 있다면, 언제든지 질문합시다!
 */
[RequireComponent(typeof(PhotonView))]
public class Example_HowToUse_IDamageable : MonoBehaviourPun, IDamageable
{
	[Header("현재 상태!")]

	[SerializeField, Tooltip("체력 변수입니다.")]
	protected float Health = 100.0f;

	[SerializeField, Tooltip("사망 여부입니다.")]
	protected bool bIsDead = false;



	// 인터페이스 확장! 자동완성에 뜨는 것 사용하면 됩니다
	void IDamageable.TakeDamage(float DamageAmount, Vector3 CauserPosition, DamageFramework.ADamageType DamageTypePrefab, float DamageTypeDamageMult)
	{
		// 사망했다면, 연산하지 않음
		if (bIsDead) return;


		// ★★ 중요!!! ★★
		// 마스터 클라이언트로 요청을 보내는 건, 나 하나 뿐이면 충분해요!
		// IsMine 조건문을 걸어 줍니다.
		if (photonView.IsMine)
		{
			// MasterClient 로 보내기만 하면, 마스터 클라이언트가 연산해서 결과를 뿌려줄 것입니다.
			photonView.RPC("RpcTakeDamageMaster", RpcTarget.MasterClient, DamageAmount, CauserPosition);
		}
		

		// null 체크를 수행합니다. null 값이 들어갈 때가 있기에, 반드시 수행해야 합니다.
		if (DamageTypePrefab != null)
		{
			// 피해 유형을 인스턴싱합니다! 이 예제에서는 부모 객체가 자신이 되도록 인스턴싱하고 있습니다.
			DamageFramework.ADamageType AppliedDamageType = Instantiate(DamageTypePrefab, transform);

			// 함수 하나만 호출해주면 끝입니다!
			// StartDamage(대상 게임오브젝트(보통 자기 자신), 피해 유형 배율, 추가 피해량);
			AppliedDamageType.StartDamage(this.gameObject, DamageTypeDamageMult, 0.0f);
		}
	}



	[PunRPC]
	// RPC 선언! 마스터로 보낼 RPC 입니다.
	protected void RpcTakeDamageMaster(float DamageAmount, Vector3 CauserPosition)
	{
		// RpcTarget.All 로, '마스터 클라이언트를 포함'하는 모두에게 전송합니다. (결과 뿌리기!)
		photonView.RPC("RpcTakeDamageAll", RpcTarget.All, DamageAmount, CauserPosition);
	}



	[PunRPC]
	// RPC 선언! 모두에게 보낼 RPC 입니다.
	protected void RpcTakeDamageAll(float DamageAmount, Vector3 CauserPosition)
	{
		/* 이 함수에서 실질적인 연산을 수행합니다! */


		// 체력을 계산합니다.
		Health -= DamageAmount;


		// 체력이 다해서 죽었어요!
		if (Health <= 0.0f)
		{
			bIsDead = true;

			// CauserPosition 은 공격한 대상자의 위치 변수입니다.
			Debug.Log("날 때린 놈의 위치: " + CauserPosition);
		}
	}
}
