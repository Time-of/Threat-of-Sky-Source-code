using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

using DamageFramework;



/**
 * 작성자: 20181220 이성수
 * 드래곤 보스입니다.
 */
public class BossEnemy_Dragon : ABossBase
{
	[Header("드래곤 공격 패턴")]

	[SerializeField]
	private DamageZone DZ_Breath;

	[SerializeField]
	private Transform BreathPoint;

	[SerializeField]
	private float BiteAtkRadius = 2.4f;

	[SerializeField]
	private float Skill_LiftLand_Radius = 7.0f;

	[SerializeField]
	private float Skill_LiftLand_Cooldown = 16.0f;

	[SerializeField, Tooltip("드래곤 보스가 로어 스킬 사용 시 공격력 증가량")]
	private float Skill_Roar_AtkIncreasement = 0.2f;

	[SerializeField]
	private float Skill_Roar_Cooldown = 15.0f;



	[Header("디버그")]

	[SerializeField]
	private bool bDrawDebugs = false;



	[Header("사운드")]

	[SerializeField]
	private AudioClip Clip_Bite;

	[SerializeField]
	private AudioClip Clip_Breath;

	private string PrevBgmName = "";



	[Header("이펙트")]

	[SerializeField]
	private float PhasingShowFull = 6.0f;

	[SerializeField]
	private float PhasingUnshowFull = -9.0f;

	[SerializeField]
	private ParticleSystem VFX_Land;



	[Header("컴포넌트")]

	private Renderer RenderComponent = null;



	protected override void Awake()
	{
		base.Awake();

		RenderComponent = GetComponentInChildren<Renderer>();

		RenderComponent.material.SetFloat("_PhasingValue", PhasingUnshowFull);
	}



	protected override void Start()
	{
		base.Start();

		PrevBgmName = SoundManager.Instance.GetBgmNowPlayingName();

		// 시작 브금
		SoundManager.Instance.PlayBGM("03_DragonFire", 0.65f, true);

		SoundManager.Instance.SpawnSound2D("BossAppear", ESoundGroup.SFX, 1.0f, 0.0f);

		StartCoroutine(ShowPhasing());
	}



	IEnumerator ShowPhasing()
	{
		float PhasingValue = PhasingUnshowFull;

		while (PhasingValue < PhasingShowFull)
		{
			RenderComponent.material.SetFloat("_PhasingValue", PhasingValue);

			PhasingValue += 0.1f;

			yield return null;
		}

		RenderComponent.material.SetFloat("_PhasingValue", PhasingShowFull);
	}



	IEnumerator UnshowPhasing()
	{
		float PhasingValue = PhasingShowFull;

		while (PhasingValue > PhasingUnshowFull)
		{
			RenderComponent.material.SetFloat("_PhasingValue", PhasingValue);

			PhasingValue -= 0.1f;

			yield return null;
		}

		RenderComponent.material.SetFloat("_PhasingValue", PhasingUnshowFull);
	}



	public void OnDied()
	{
		SoundManager.Instance.PlayBGM(PrevBgmName, 1.0f, true);

		GameManager.Instance.OnBossEnemyDied(transform.position);
	}



	public override void TryAttack(int AttackType)
	{
		if (bIsDead) return;

		bIsInAction = true;
		RunningTree.SetData("IsInAction", bIsInAction);

		NavAgentComponent.velocity = Vector3.zero;
		NavAgentComponent.isStopped = true;

		object NearestPlayer = RunningTree.GetData("NearestPlayer");

		if (NearestPlayer != null)
		{
			Quaternion LookAtRot = Quaternion.Euler(0,
				Quaternion.LookRotation(((ACharacterBase)NearestPlayer).transform.position - transform.position, Vector3.up).eulerAngles.y,
				0);

			SetTargetRotation(LookAtRot);
		}

		switch (AttackType)
		{
			case 0:
				photonView.RPC("RpcSetAnimTrigger", RpcTarget.AllViaServer, "Atk_Bite");
				break;
			case 1:
				photonView.RPC("RpcSetAnimTrigger", RpcTarget.AllViaServer, "Atk_Breath");
				break;
			default:
				break;
		}

		Debug.Log("<color=yellow>드래곤 보스의 공격</color> : " + AttackType);
	}



	protected override void SetLevelDifficultyActions(int NewDifficulty)
	{
		Skill_Roar_AtkIncreasement *= NewDifficulty;
	}



	#region 공격
	public void BiteAttack()
	{
		SoundManager.Instance.SpawnSoundAtLocation(Clip_Bite, BreathPoint.position, ESoundGroup.SFX, 0.8f, 0.05f, AudioRolloffMode.Linear);

		DamageHelper.ApplyRadialDamage(BreathPoint.position, BiteAtkRadius, AtkPower * 4.8f, "Player", true, this.gameObject);
	}



	public void SpawnBreathZone()
	{
		SoundManager.Instance.SpawnSoundAtLocation(Clip_Breath, BreathPoint.position, ESoundGroup.SFX, 0.9f, 0.05f, AudioRolloffMode.Linear);

		DamageZone SpawnedZone = DamageHelper.SpawnDamageZone(DZ_Breath,
			BreathPoint.position + BreathPoint.transform.forward * 7.0f,
			BreathPoint.rotation, BreathPoint);

		SpawnedZone.StartZone(AtkPower, 1.0f, this.gameObject);
	}



	public void TrySkill_LiftLand()
	{
		AttackStarted();
		StartCoroutine(LiftLandSkillCooldown());

		photonView.RPC("RpcSetAnimTrigger", RpcTarget.AllViaServer, "SK_LiftLand");
	}



	IEnumerator LiftLandSkillCooldown()
	{
		yield return new WaitForSeconds(Skill_LiftLand_Cooldown);

		RunningTree.SetData("CanUseLiftLandSkill", true);
	}



	public void StartPhasing()
	{
		StartCoroutine(UnshowPhasing());
	}



	public void EndPhasing()
	{
		StartCoroutine(ShowPhasing());
	}



	public void SetCollisionEnabled(bool NewActive)
	{
		GetComponent<Collider>().enabled = NewActive;
	}



	public void LandAttack()
	{
		CharacterGameplay.CharacterGameplayHelper.PlayVfx(VFX_Land, transform.position, Quaternion.identity, null);

		DamageHelper.ApplyRadialDamage(transform.position, Skill_LiftLand_Radius,
			AtkPower * 9.0f, "Player", false, this.gameObject);

		// 넉백 주기
		foreach (Collider CharacterCol in Physics.OverlapSphere(transform.position,
			Skill_LiftLand_Radius, 1 << LayerMask.NameToLayer("Player"), QueryTriggerInteraction.Ignore))
		{
			if (CharacterCol.TryGetComponent<ACharacterBase>(out ACharacterBase PlayerCharacter))
			{
				if (!PlayerCharacter.IsDead)
				{
					PlayerCharacter.RigidBody.position += Vector3.up * 2.0f;

					PlayerCharacter.GetMovementComponent().
						AddKnockback(0.6f * (PlayerCharacter.transform.position - transform.position) + 12.0f * Vector3.up);
				}
			}
		}
	}



	public void TrySkill_Roar()
	{
		AttackStarted();
		StartCoroutine(RoarSkillCooldown());

		photonView.RPC("RpcSetAnimTrigger", RpcTarget.AllViaServer, "SK_Roar");
	}



	IEnumerator RoarSkillCooldown()
	{
		yield return new WaitForSeconds(Skill_Roar_Cooldown);

		RunningTree.SetData("CanUseRoar", true);
	}



	public void Roar()
	{
		AtkPower += Skill_Roar_AtkIncreasement;
	}
	#endregion



	void OnDrawGizmos()
	{
		if (!bDrawDebugs) return;

		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(BreathPoint.position, BiteAtkRadius);

		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(transform.position, Skill_LiftLand_Radius);
	}
}
