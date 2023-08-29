using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Photon.Pun;

using DamageFramework;
using BehaviourTree;



/**
 * 작성자: 20181220 이성수
 * 보스 몬스터의 베이스입니다.
 */
public abstract class ABossBase : MonoBehaviourPun, IDamageable, IPunObservable
{
	[Header("보스 몬스터 상태")]

	[SerializeField, ReadOnlyProperty]
	protected bool bIsDead = false;

	[SerializeField]
	protected bool bCanBeDamaged = true;

	[SerializeField, ReadOnlyProperty]
	protected int LevelDifficulty = 1;



	[Header("체력")]

	[SerializeField]
	protected float BaseHealth = 10000.0f;

	[SerializeField, ReadOnlyProperty]
	protected float Health;

	[SerializeField, ReadOnlyProperty]
	protected float MaxHealth;

	[SerializeField, ReadOnlyProperty]
	protected bool bIsInAction = false;



	[Header("공격력")]

	[SerializeField]
	protected float AtkPower = 3.7f;



	[Header("회전")]

	[SerializeField]
	private float RotationSpeed = 5.0f;

	[SerializeField, ReadOnlyProperty]
	private Quaternion TargetRotation;

	[SerializeField, ReadOnlyProperty]
	private bool bNeedToFixRotation = false;

	public void SetTargetRotation(Quaternion NewRotation) { TargetRotation = NewRotation; bNeedToFixRotation = true; }



	[Header("AI")]

	[SerializeField, ReadOnlyProperty]
	protected BT_ATree RunningTree;



	[Header("HUD")]

	[SerializeField]
	private Image Healthbar;

	[SerializeField]
	private TMPro.TMP_Text HealthText;



	[Header("네트워킹")]

	[SerializeField, ReadOnlyProperty]
	private Vector3 NetworkingPosition;

	[SerializeField, ReadOnlyProperty]
	private Quaternion NetworkingRotation;

	[SerializeField]
	private float NetworkingInterpSpeed = 10.0f;

	[ReadOnlyProperty]
	public bool bIsMovingInNetwork = false;



	[Header("컴포넌트")]

	private ABossAnimationBase AnimComponent;

	protected NavMeshAgent NavAgentComponent;



	protected virtual void Awake()
	{
		RunningTree = GetComponent<BT_ATree>();

		AnimComponent = GetComponentInChildren<ABossAnimationBase>();

		NavAgentComponent = GetComponent<NavMeshAgent>();
	}



	protected virtual void Start()
	{
		// 플레이어 수만큼 체력 증가
		MaxHealth = BaseHealth * PhotonNetwork.PlayerList.Length;
		ApplyHealth(MaxHealth);
	}



	void Update()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			if (bNeedToFixRotation)
			{
				transform.rotation = Quaternion.Slerp(transform.rotation,
					TargetRotation, Time.deltaTime * RotationSpeed);

				if (transform.rotation.AlmostEquals(TargetRotation, 1.0f))
				{
					bNeedToFixRotation = false;
				}
			}
		}

		UpdateNetworkingTransform();
	}



	#region 피해 관련 기능
	void IDamageable.TakeDamage(float DamageAmount, Vector3 CauserPosition, ADamageType DamageTypePrefab, float DamageTypeDamageMult)
	{
		if (bIsDead || !bCanBeDamaged) return;


		if (photonView.IsMine)
		{
			photonView.RPC("RpcTakeDamageMaster", RpcTarget.MasterClient, DamageAmount, CauserPosition);
		}


		if (DamageTypePrefab != null)
		{
			ADamageType AppliedDamageType = Instantiate(DamageTypePrefab, transform);

			AppliedDamageType.StartDamage(this.gameObject, DamageTypeDamageMult, 0.0f);
		}
	}



	[PunRPC]
	protected void RpcTakeDamageMaster(float DamageAmount, Vector3 CauserPosition)
	{
		photonView.RPC("RpcTakeDamageAll", RpcTarget.AllViaServer, DamageAmount, CauserPosition);
	}



	[PunRPC]
	protected void RpcTakeDamageAll(float DamageAmount, Vector3 CauserPosition)
	{
		ApplyHealth(Health - DamageAmount);
	}



	void ApplyHealth(float NewAmount)
	{
		float OriginalHealth = Health;

		Health = Mathf.Clamp(NewAmount, 0.0f, MaxHealth);

		CharacterGameplay.CharacterGameplayHelper.SpawnDamageFloaterAutoColor(transform.position, OriginalHealth - Health);

		Healthbar.fillAmount = Health / MaxHealth;
		HealthText.text = Health.ToString() + " / " + MaxHealth.ToString();

		if (Health <= 0.0f) Die();
	}



	void Die()
	{
		bCanBeDamaged = false;
		bIsDead = true;

		NavAgentComponent.isStopped = true;

		GetComponent<Collider>().enabled = false;

		if (PhotonNetwork.IsMasterClient)
			photonView.RPC("RpcSetAnimTrigger", RpcTarget.AllViaServer, "Die");

		//GameManager.Instance.OnBossEnemyDied(transform.position);
	}
	#endregion



	#region 공격 관련
	public virtual void TryAttack(int AttackType)
	{
		if (bIsDead) return;

		bIsInAction = true;
		RunningTree.SetData("IsInAction", bIsInAction);

		NavAgentComponent.isStopped = true;
	}



	public void AttackStarted()
	{
		bIsInAction = true;
		RunningTree.SetData("IsInAction", bIsInAction);

		NavAgentComponent.isStopped = true;
	}



	public void AttackEnded()
	{
		bIsInAction = false;
		RunningTree.SetData("IsInAction", bIsInAction);

		NavAgentComponent.isStopped = false;
	}



	public void TeleportToRandomPlayer()
	{
		List<ACharacterBase> AliveCharacterList = new List<ACharacterBase>();

		foreach (ACharacterBase PlayerCharacter in FindObjectsOfType<ACharacterBase>())
		{
			if (PlayerCharacter.IsDead) continue;

			AliveCharacterList.Add(PlayerCharacter);
		}

		int PlayerCount = AliveCharacterList.Count;

		if (PlayerCount <= 0) return;

		Vector3 TargetPoint = AliveCharacterList[Random.Range(0, PlayerCount - 1)].transform.position;

		//transform.position = TargetPoint;
		NavAgentComponent.Warp(TargetPoint);
	}
	#endregion



	#region 애니메이션 관련
	[PunRPC]
	protected void RpcSetAnimBool(string BoolName, bool NewActive)
	{
		AnimComponent.GetAnimator().SetBool(BoolName, NewActive);
	}


	[PunRPC]
	protected void RpcSetAnimTrigger(string TriggerName)
	{
		AnimComponent.GetAnimator().SetTrigger(TriggerName);
	}
	#endregion



	#region 네트워킹 관련
	void UpdateNetworkingTransform()
	{
		// 마스터 클라이언트가 아닌 경우에만 업데이트
		if (!photonView.IsMine)
		{
			bIsMovingInNetwork = (NetworkingPosition - transform.position).sqrMagnitude > 1.0f;
			transform.position = Vector3.Lerp(transform.position, NetworkingPosition, NetworkingInterpSpeed * Time.deltaTime);

			transform.rotation = Quaternion.Slerp(transform.rotation, NetworkingRotation,
				NetworkingInterpSpeed * Time.deltaTime);
		}
	}



	void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			Debug.Log("쓰는 중");

			stream.SendNext(transform.position);

			stream.SendNext(transform.rotation.eulerAngles.y);
		}
		else
		{
			Debug.Log("읽는 중");

			NetworkingPosition = (Vector3)stream.ReceiveNext();

			NetworkingRotation = Quaternion.Euler(0.0f, (float)stream.ReceiveNext(), 0.0f);
		}
	}
	#endregion



	// 플레이어가 닿은 경우 넉백 처리
	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.TryGetComponent<ACharacterBase>(out ACharacterBase PlayerCharacter))
		{
			PlayerCharacter.GetMovementComponent().AddKnockback(Vector3.up * 8.0f + transform.forward * 5.0f);
		}
	}



	public void SetLevelDifficulty(int NewDifficulty)
	{
		photonView.RPC("RpcSetLevelDifficulty", RpcTarget.AllViaServer, NewDifficulty);
	}



	[PunRPC]
	protected void RpcSetLevelDifficulty(int NewDifficulty)
	{
		LevelDifficulty = NewDifficulty;

		MaxHealth *= (LevelDifficulty * 1.1f);
		AtkPower *= (LevelDifficulty * 1.1f);

		ApplyHealth(MaxHealth);

		SetLevelDifficultyActions(LevelDifficulty);
	}



	protected virtual void SetLevelDifficultyActions(int NewDifficulty)
	{

	}
}
