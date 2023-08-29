using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

using SkillFramework;
using CharacterGameplay;
using DamageFramework;


/**
 * 작성자: 20181220 이성수
 * 머더러 캐릭터입니다.
 */
public class MurdererCharacter : ACharacterBase, IDamageable
{
	[Header("회피")]

	[SerializeField, Tooltip("회피 확률입니다.")]
	private float EvadeChance = 15.0f;

	[SerializeField, ReadOnlyProperty, Tooltip("활성화되면 항상 회피합니다.")]
	private bool bAlwaysEvade = false;

	public void SetAlwaysEvade(bool NewActive) { bAlwaysEvade = NewActive; }



	[Header("공격")]

	[SerializeField]
	private bool bDebugDrawAttackBound = false;

	[SerializeField]
	private float HorizontalAttackDistance = 0.5f;

	[SerializeField]
	private Vector3 HorizontalAttackBound;

	[SerializeField]
	private float VerticalAttackDistance = 0.4f;

	[SerializeField]
	private Vector3 VerticalAttackBound;

	[SerializeField]
	private float HorizontalAttackDamageRate = 1.3f;

	[SerializeField]
	private float VerticalAttackDamageRate = 2.2f;

	[SerializeField]
	private ParticleSystem Vfx_Slash;

	[SerializeField]
	private DamageZone DZ_Slash;

	[SerializeField]
	private ACharacterBuffBase Buff_ChaoticStep;

	[SerializeField]
	private ACharacterBuffBase Buff_BloodCarnival;

	[SerializeField, ReadOnlyProperty, Tooltip("활성화되면 피해를 줄 때 피해의 일정 비율만큼 체력을 회복합니다.")]
	private bool bRestoreHealthWhenAttack = false;

	public void SetRestoreHealthWhenAttack(bool NewActive) { bRestoreHealthWhenAttack = NewActive; }

	[SerializeField]
	private float HealthRestoreRate = 0.2f;

	[SerializeField]
	private bool bNextAttackMustBeCritical = false;

	[SerializeField]
	private ParticleSystem VFX_DetectVital;

	[SerializeField]
	private ACharacterBuffBase Buff_KillingSense;

	[SerializeField]
	private ACharacterBuffBase Buff_Sadist;



	[Header("사운드")]

	[SerializeField]
	private List<AudioClip> AttackSoundClipList;

	[SerializeField]
	private List<AudioClip> AttackHitSoundClipList;

	[SerializeField]
	private AudioClip Sfx_ChaoticStep;

	[SerializeField]
	private AudioClip Sfx_DetectVital;

	[SerializeField]
	private AudioClip Sfx_BloodCarnival;

	[SerializeField]
	private AudioClip Sfx_KillingSense;

	[SerializeField]
	private AudioClip Sfx_Sadist;



	protected override void Update()
	{
		if (photonView.IsMine)
		{
			TryAttack();
		}

		base.Update();
	}



	#region 오버라이드
	void IDamageable.TakeDamage(float DamageAmount, Vector3 CauserPosition, ADamageType DamageTypePrefab, float DamageTypeDamageMult)
	{
		if (!bCanBeDamaged || IsDead) return;

		if (photonView.IsMine)
		{
			// 반드시 회피 상태가 아니고, 회피에 실패한다면 피해를 입는다.
			if (!bAlwaysEvade && Random.Range(0.0f, 100.0f) > EvadeChance)
			{
				StatusComponent?.TakeDamage(DamageAmount, CauserPosition);
			}
			else Debug.Log("회피 성공!");

			if (DamageTypePrefab != null)
			{
				ADamageType AppliedDamageType = Instantiate(DamageTypePrefab, transform);

				AppliedDamageType.StartDamage(this.gameObject, DamageTypeDamageMult, 0.0f);
			}
		}
	}



	protected override void OnAttackHitAction(float HitDamage, Vector3 HitPosition)
	{
		base.OnAttackHitAction(HitDamage, HitPosition);

		if (bRestoreHealthWhenAttack) RestoreHealth(HitDamage * HealthRestoreRate);
	}



	protected override void OnSkillUsed(string SkillName)
	{
		if (!photonView.IsMine) return;

		switch (SkillName)
		{
			case "SK_ChaoticStep":
				Skill_ChaoticStep();
				break;
			case "SK_BloodCarnival":
				Skill_BloodCarnival();
				break;
			case "SK_DetectVital":
				Skill_DetectVital();
				break;
			case "SK_KillingSense":
				Skill_KillingSense();
				break;
			default:
				break;
		}
	}
	#endregion



	#region 입력 오버라이드
	public override void HorizontalInput(float Value)
	{
		if (bIsUsingSkill) return;

		base.HorizontalInput(Value);
	}

	public override void VerticalInput(float Value)
	{
		if (bIsUsingSkill) return;

		base.VerticalInput(Value);
	}

	public override void FireInput(bool bIsPressed)
	{
		bIsFireKeyPressed = bIsPressed;

		//TryAttack();
	}

	public override void SkillAttack1Input(bool bIsPressed)
	{
		if (bIsPressed && bCanAttack)
		{
			TryUseSkillBySlot(1);
		}
	}

	public override void SkillAttack2Input(bool bIsPressed)
	{
		if (bIsPressed && bCanAttack)
		{
			TryUseSkillBySlot(2);
		}
	}

	public override void SkillAttack3Input(bool bIsPressed)
	{
		if (bIsPressed && bCanAttack)
		{
			TryUseSkillBySlot(3);
		}
	}
	#endregion



	#region 공격
	void TryAttack()
	{
		if (bIsFireKeyPressed)
		{
			if (!bIsUsingSkill && bCanAttack)
			{
				bCanAttack = false;

				TryInBattleState();

				RpcSetAnimTrigger(!bDoAltAttack ? "NormalAttack" : "Attack_Vertical");

				photonView.RPC("RpcSetAnimTrigger", RpcTarget.Others, !bDoAltAttack ? "NormalAttack" : "Attack_Vertical");
			}
		}
	}



	public void Attack_Horizontal()
	{
		Vector3 AttackPosition = transform.position + transform.forward * HorizontalAttackDistance;

		CharacterGameplayHelper.PlayVfx(Vfx_Slash, transform.position + transform.forward * HorizontalAttackDistance, transform.rotation);
		SoundManager.Instance.SpawnSoundAtLocation(CharacterGameplayHelper.GetRandomSoundInList(AttackSoundClipList),
			AttackPosition, ESoundGroup.SFX, 1.0f, 0.05f);

		float HorizontalAttackFinalDamage = HorizontalAttackDamageRate * CalculateFinalDamage();

		foreach (Collider col in Physics.OverlapBox(AttackPosition,
			HorizontalAttackBound, transform.rotation,
			1 << LayerMask.NameToLayer("Enemy"), QueryTriggerInteraction.Ignore))
		{
			if (DamageHelper.ApplyDamage(col.gameObject,
				HorizontalAttackFinalDamage,
				this.gameObject, null, 1.0f))
			{
				OnAttackHit(HorizontalAttackFinalDamage, col.transform.position);

				SoundManager.Instance.SpawnSoundAtLocation(CharacterGameplayHelper.GetRandomSoundInList(AttackHitSoundClipList),
					AttackPosition, ESoundGroup.SFX, 1.0f, 0.05f);

				Debug.Log("머더러 가로베기 데미지" + HorizontalAttackFinalDamage);
			}
		}
	}



	public void Attack_Vertical()
	{
		Vector3 AttackPosition = transform.position + transform.forward * VerticalAttackDistance;

		CharacterGameplayHelper.PlayVfx(Vfx_Slash, transform.position + transform.forward * VerticalAttackDistance,
			Quaternion.Euler(0, 0, 90));
		SoundManager.Instance.SpawnSoundAtLocation(CharacterGameplayHelper.GetRandomSoundInList(AttackSoundClipList),
			AttackPosition, ESoundGroup.SFX, 1.0f, 0.05f);

		float VerticalAttackFinalDamage = VerticalAttackDamageRate * CalculateFinalDamage();

		foreach (Collider col in Physics.OverlapBox(AttackPosition,
			VerticalAttackBound, transform.rotation,
			1 << LayerMask.NameToLayer("Enemy"), QueryTriggerInteraction.Ignore))
		{
			if (DamageHelper.ApplyDamage(col.gameObject,
				VerticalAttackFinalDamage,
				this.gameObject, null, 1.0f))
			{
				OnAttackHit(VerticalAttackFinalDamage, col.transform.position);

				SoundManager.Instance.SpawnSoundAtLocation(CharacterGameplayHelper.GetRandomSoundInList(AttackHitSoundClipList),
					AttackPosition, ESoundGroup.SFX, 1.0f, 0.05f);
			}
		}
	}



	public void Skill_Slash()
	{
		DamageZone SpawnedZone = DamageHelper.SpawnDamageZone(DZ_Slash, transform.position, transform.rotation, null);
		
		SpawnedZone.SetupHitCallback(OnAttackHit);

		SpawnedZone.StartZone(CalculateFinalDamage(), StatusComponent.GetAtkSpeedMult, this.gameObject);
	}



	public void SlashWalk()
	{
		Vector3 ForwardVector = Quaternion.AngleAxis(GetControllerRotation().eulerAngles.y, Vector3.up) * Vector3.forward;

		CharacterGameplayHelper.MoveToWorld(this,
			transform.position + ForwardVector * 7.0f,
			1.0f / StatusComponent.GetAtkSpeedMult,
			true, true);

		transform.rotation = Quaternion.Euler(0.0f, Controller.GetControllerYaw(), 0.0f);
	}



	void Skill_ChaoticStep()
	{
		photonView.RPC("RpcAddBuffMaster", RpcTarget.MasterClient, 1);
	}



	void Skill_BloodCarnival()
	{
		photonView.RPC("RpcAddBuffMaster", RpcTarget.MasterClient, 2);
	}



	void Skill_DetectVital()
	{
		photonView.RPC("RpcAddBuffMaster", RpcTarget.MasterClient, 4);
		bNextAttackMustBeCritical = true;
	}



	void Skill_KillingSense()
	{
		photonView.RPC("RpcAddBuffMaster", RpcTarget.MasterClient, 3);
	}



	public void Skill_Sadist()
	{
		AddBuff(Buff_Sadist);
		SoundManager.Instance.SpawnSoundAtLocation(Sfx_Sadist,
					transform.position, ESoundGroup.SFX, 1.0f, 0.05f);
	}
	#endregion



	#region RPC
	/// <summary>
	/// 버프 추가 RPC
	/// </summary>
	/// <param name="BuffType"> 1: 흉흉한 발걸음, 2: 피의 축제, 3: 기척 죽이기, 4: 급소 탐지</param>
	[PunRPC]
	public void RpcAddBuffMaster(int BuffType)
	{
		photonView.RPC("RpcAddBuffAll", RpcTarget.All, BuffType);
	}



	[PunRPC]
	public void RpcAddBuffAll(int BuffType)
	{
		switch (BuffType)
		{
			case 1:
				AddBuff(Buff_ChaoticStep);
				SoundManager.Instance.SpawnSoundAtLocation(Sfx_ChaoticStep,
					transform.position, ESoundGroup.SFX, 1.0f, 0.05f);

				break;
			case 2:
				AddBuff(Buff_BloodCarnival);
				SoundManager.Instance.SpawnSoundAtLocation(Sfx_BloodCarnival,
					transform.position, ESoundGroup.SFX, 1.0f, 0.05f);

				break;
			case 3:
				AddBuff(Buff_KillingSense);
				SoundManager.Instance.SpawnSoundAtLocation(Sfx_KillingSense,
					transform.position, ESoundGroup.SFX, 1.0f, 0.05f);

				break;
			case 4:
				CharacterGameplayHelper.PlayVfx(VFX_DetectVital, transform.position, Quaternion.identity, transform);
				SoundManager.Instance.SpawnSoundAtLocation(Sfx_DetectVital,
					transform.position, ESoundGroup.SFX, 1.0f, 0.05f);

				break;
			default:
				break;
		}
	}
	#endregion



	#region 연산
	float CalculateFinalDamage()
	{
		float CalculatedDamage;
		float CritRate;

		(CalculatedDamage, CritRate) = StatusComponent.GetFinalDamage();

		float FinalDamage;

		if (bNextAttackMustBeCritical)
		{
			bNextAttackMustBeCritical = false;

			FinalDamage = (CritRate > 1.1f) ? CalculatedDamage : CalculatedDamage * 2.0f;
		}
		else
		{
			FinalDamage = CalculatedDamage;
		}
		
		return FinalDamage;
	}
	#endregion



	#region 디버그
	void OnDrawGizmos()
	{
		if (!bDebugDrawAttackBound) return;

		Gizmos.color = Color.magenta;

		Matrix4x4 H_TRMatrix = Matrix4x4.TRS(transform.position + transform.forward * HorizontalAttackDistance,
			transform.rotation, Vector3.one);

		Gizmos.matrix = H_TRMatrix;

		Gizmos.DrawWireCube(Vector3.zero, 2f * HorizontalAttackBound);

		Gizmos.color = Color.cyan;

		Matrix4x4 V_TRMatrix = Matrix4x4.TRS(transform.position + transform.forward * VerticalAttackDistance,
			transform.rotation, Vector3.one);

		Gizmos.matrix = V_TRMatrix;

		Gizmos.DrawWireCube(Vector3.zero, 2f * VerticalAttackBound);
	}
	#endregion
}
