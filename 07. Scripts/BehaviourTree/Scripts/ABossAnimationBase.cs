using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;



/**
 * 작성자: 20181220 이성수
 * 보스 몬스터의 애니메이션 베이스입니다.
 */
public abstract class ABossAnimationBase : MonoBehaviour
{
	protected ABossBase OwnerBossEnemy;

	protected Animator AnimComponent;

	public Animator GetAnimator() { return AnimComponent; }

	protected NavMeshAgent NavAgentComponent;


	
	[Header("네트워킹")]

	[SerializeField, ReadOnlyProperty]
	protected bool bIsMine = false;

	[SerializeField, ReadOnlyProperty]
	private float NetworkingMoveSpeed = 0.0f;



	protected virtual void Awake()
	{
		OwnerBossEnemy = GetComponentInParent<ABossBase>();

		AnimComponent = GetComponent<Animator>();

		NavAgentComponent = GetComponentInParent<NavMeshAgent>();

		bIsMine = OwnerBossEnemy.photonView.IsMine;
	}



	protected virtual void Update()
	{
		if (bIsMine)
		{
			Vector3 VelocityXZ = NavAgentComponent.velocity;
			VelocityXZ.y = 0;

			AnimComponent.SetFloat("SpeedSqr", VelocityXZ.sqrMagnitude);
		}
		else
		{
			NetworkingMoveSpeed = Mathf.Lerp(NetworkingMoveSpeed,
				(OwnerBossEnemy.bIsMovingInNetwork) ? 4.0f : 0.0f, 3.0f * Time.deltaTime);

			AnimComponent.SetFloat("SpeedSqr", NetworkingMoveSpeed);
		}
	}



	#region 이벤트
	public void Event_AttackStart()
	{
		OwnerBossEnemy.AttackStarted();
	}



	public void Event_AttackEnd()
	{
		OwnerBossEnemy.AttackEnded();
	}



	public void Event_TeleportToRandomPlayer()
	{
		OwnerBossEnemy.TeleportToRandomPlayer();
	}



	public void Event_SpawnSound(string SoundName)
	{
		SoundManager.Instance.SpawnSoundAtLocation(SoundName, transform.position, ESoundGroup.SFX, 0.7f, 0.05f, AudioRolloffMode.Linear);
	}
	#endregion



	protected void OnAnimatorMove()
	{
		OwnerBossEnemy.transform.position += AnimComponent.deltaPosition;
	}
}
