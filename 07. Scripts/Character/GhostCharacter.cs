using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

using SkillFramework;
using CharacterGameplay;
using DamageFramework;


/**
 * 작성자: 20181220 이성수
 * ACharacterBase를 확장한 고스트 캐릭터입니다.
 */
public class GhostCharacter : ACharacterBase
{
	[Header("마나")]

	[SerializeField]
	private float MaxMana = 100.0f;

	[SerializeField, ReadOnlyProperty]
	private float CurrentMana;

	[SerializeField]
	private float ManaRecoveryMult = 15.0f;

	[SerializeField]
	private float RequireMana = 20.0f;

	[SerializeField]
	private bool bCanRegenMana = true;

	public void SetCanRegenMana(bool NewActive) { bCanRegenMana = NewActive; }

	[SerializeField]
	private ACharacterBuffBase ManaBuff;



	[Header("공격")]

	[SerializeField]
	private AProjectileBase NormalAttackProjectile;

	[SerializeField]
	private AProjectileBase AltNormalAttackProjectile;

	[SerializeField]
	private Transform NormalAttackLeftPosition;

	[SerializeField]
	private Transform NormalAttackRightPosition;

	[SerializeField]
	private Transform AltNormalAttackPosition;

	[SerializeField, ReadOnlyProperty]
	private bool bIsFireballAttackLooping = false;

	[SerializeField]
	private float BreakthroughRange = 2.0f;

	[SerializeField]
	private float BreakthroughDamageRate = 2.0f;

	[SerializeField]
	private AudioClip BreakthroughSoundClip;

	[SerializeField]
	private ParticleSystem BreakthroughVfx;

	[SerializeField]
	private DamageZone BreakdownDamageZone;

	[SerializeField]
	private DamageZone BreakdownEndDamageZone;

	[SerializeField]
	private ACharacterBuffBase BreakdownSlowDebuff;

	[SerializeField]
	private DamageZone AnnihilationDamageZone;

	[SerializeField]
	private ACharacterBuffBase AnnihilationSlowDebuff;

	[SerializeField]
	private Projectile_GhostCreationSword CreationSword;

	[SerializeField]
	private float TeleportDistance = 8.0f;

	[SerializeField]
	private ParticleSystem TeleportVfx;

	[SerializeField]
	private ACharacterBuffBase CombustionRiskBuff;

	[SerializeField]
	private ACharacterBuffBase CombustionReinforceBuff;

	[SerializeField]
	private AudioClip CombustionSoundClip;



	protected override void Awake()
	{
		base.Awake();

		CurrentMana = MaxMana;
	}



	protected override void Update()
	{
		if (photonView.IsMine)
		{
			TryAttack();
		}

		base.Update();

		CalculateManaRecovery();
	}



	#region 입력 오버라이드
	public override void JumpInput(bool bIsPressed)
	{
		if (bIsPressed && MovementComponent.CanJump())
		{
			if (MovementComponent.CurrentJumpCount >= 1)
			{
				MovementComponent.bPressedJump = true;
			}
			else
			{
				RpcSetAnimTrigger("Jump");

				photonView.RPC("RpcSetAnimTrigger", RpcTarget.Others, "Jump");
			}
		}
	}

	public override void FireInput(bool bIsPressed)
	{
		bIsFireKeyPressed = bIsPressed;

		//TryAttack();
	}

	public override void SkillAttack1Input(bool bIsPressed)
	{
		if (bIsPressed && !bIsUsingSkill)
		{
			TrySkillWithTryManaBuff(1);
		}
	}

	public override void SkillAttack2Input(bool bIsPressed)
	{
		if (bIsPressed && !bIsUsingSkill)
		{
			TrySkillWithTryManaBuff(2);
		}
	}

	public override void SkillAttack3Input(bool bIsPressed)
	{
		if (bIsPressed && !bIsUsingSkill)
		{
			TrySkillWithTryManaBuff(3);
		}
	}
	#endregion



	#region 공격
	void TryAttack()
	{
		if (bIsFireKeyPressed)
		{
			if (!bIsUsingSkill)
			{
				bIsUsingSkill = true;

				TryInBattleState();

				RpcSetAnimTrigger(!bDoAltAttack ? "NormalAttack" : "Attack_Fireball");

				photonView.RPC("RpcSetAnimTrigger", RpcTarget.Others, !bDoAltAttack ? "NormalAttack" : "Attack_Fireball");
			}
			else if (bDoAltAttack && !bIsFireballAttackLooping)
			{
				bIsFireballAttackLooping = true;

				RpcSetAnimBool("FireballAttackLoop", true);

				photonView.RPC("RpcSetAnimBool", RpcTarget.Others, "FireballAttackLoop", true);
			}
		}
	}



	void TrySkillWithTryManaBuff(int SkillSlot)
	{
		if (SkillComponent.GetCanUseSkillByName(SkillComponent.GetSkillNameByCurrentSlot(SkillSlot)))
		{
			TryManaBuff();

			TryUseSkillBySlot(SkillSlot);
		}
	}



	void TryManaBuff()
	{
		if (CurrentMana >= RequireMana)
		{
			CurrentMana -= RequireMana;
			AddBuff(ManaBuff);
		}
	}



	void SpawnProjectile(AProjectileBase ProjectileToSpawn, Vector3 SpawnPosition)
	{
		TryManaBuff();

		AProjectileBase SpawnedProjectile = Instantiate(ProjectileToSpawn, SpawnPosition,
						CharacterGameplayHelper.VectorToRotation(PlayerCameraComponent.GetCenterWorldLocation() - transform.position));

		SpawnedProjectile.SetupHitCallback(OnAttackHit);
		SpawnedProjectile.SetupSplashHitCallback(OnAttackHit);

		SpawnedProjectile.StartFire(this.gameObject, StatusComponent.GetFinalDamage().Item1, StatusComponent.GetFinalDamage().Item1);
	}



	public void Attack_PulseSphere(bool bIsLeftHanded)
	{
		SpawnProjectile(NormalAttackProjectile,
			bIsLeftHanded ? NormalAttackLeftPosition.position : NormalAttackRightPosition.position);

		PlayerCameraComponent.PlayCameraShake(8.0f, 0.035f);
	}



	public void Attack_Fireball()
	{
		SpawnProjectile(AltNormalAttackProjectile, AltNormalAttackPosition.position);

		if (bIsFireballAttackLooping)
		{
			TryInBattleState();

			bIsFireballAttackLooping = false;

			RpcSetAnimBool("FireballAttackLoop", false);

			photonView.RPC("RpcSetAnimBool", RpcTarget.Others, "FireballAttackLoop", false);
		}

		PlayerCameraComponent.PlayCameraShake(4.0f, 0.045f);
	}



	public void Skill_Breakdown()
	{
		AddBuff(BreakdownSlowDebuff);

		DamageZone SpawnedZone = DamageHelper.SpawnDamageZone(BreakdownDamageZone,
			transform.position, transform.rotation, transform);

		SpawnedZone.SetupHitCallback(OnAttackHit);

		SpawnedZone.StartZone(StatusComponent.GetFinalDamage().Item1, StatusComponent.GetAtkSpeedMult, this.gameObject);
	}



	public void Skill_BreakdownEnd()
	{
		DamageZone SpawnedZone = DamageHelper.SpawnDamageZone(BreakdownEndDamageZone,
			transform.position, transform.rotation, transform);

		SpawnedZone.SetupHitCallback(OnAttackHit);

		SpawnedZone.StartZone(StatusComponent.GetFinalDamage().Item1, StatusComponent.GetAtkSpeedMult, this.gameObject);
	}



	public void Skill_BreakthroughStart()
	{
		CharacterGameplayHelper.MoveToWorld(this,
			transform.position + GetControllerForwardVector() * 12.0f,
			0.3f, true, true);
	}



	public void Skill_Breakthrough()
	{
		SoundManager.Instance.SpawnSoundAtLocation(BreakthroughSoundClip, transform.position, ESoundGroup.SFX, 1.0f, 0.05f);

		CharacterGameplayHelper.PlayVfx(BreakthroughVfx, transform.position, Quaternion.identity);

		List<float> DamageList;
		List<Vector3> PositionList;

		(DamageList, PositionList) = DamageHelper.ApplyRadialDamage(transform.position, BreakthroughRange,
			BreakthroughDamageRate * StatusComponent.GetFinalDamage().Item1,
			"Enemy", true, this.gameObject);

		int SplashHitCount = DamageList.Count;

		// 스플래시 당한 만큼 콜백 호출
		for (int i = 0; i < SplashHitCount; i++)
		{
			OnAttackHit(DamageList[i], PositionList[i]);
		}

		/*CharacterGameplayHelper.MoveToWorld(this,
			transform.position + MovementComponent.GetMovementInput * 13.0f,
			0.4f, true, false);*/
	}



	public void Skill_Annihilation()
	{
		TryInBattleState();

		AddBuff(AnnihilationSlowDebuff);

		DamageZone SpawnedZone = DamageHelper.SpawnDamageZone(AnnihilationDamageZone,
			NormalAttackRightPosition.position + transform.forward * 5.0f,
			transform.rotation, transform);

		SpawnedZone.SetupHitCallback(OnAttackHit);

		SpawnedZone.StartZone(StatusComponent.GetFinalDamage().Item1, StatusComponent.GetAtkSpeedMult, this.gameObject);
	}



	public void Skill_Creation()
	{
		SoundManager.Instance.SpawnSoundAtLocation("Creation", transform.position, ESoundGroup.SFX, 1.0f, 0.05f);

		for (int ProjectileOrder = 0; ProjectileOrder < 7; ProjectileOrder++)
		{
			Projectile_GhostCreationSword CreationInstance = Instantiate(CreationSword, transform);
			CreationInstance.transform.LookAt(CreationInstance.transform.position + CreationInstance.transform.up * 3.0f);
			CreationInstance.SetOrderOfSwords(ProjectileOrder);

			CreationInstance.SetupHitCallback(OnAttackHit);
			CreationInstance.SetupSplashHitCallback(OnAttackHit);

			CreationInstance.StartFire(this.gameObject, StatusComponent.GetFinalDamage().Item1);

			Debug.Log("조형 스킬 발동, 인덱스: " + ProjectileOrder + ", 인스턴스: " + CreationInstance);
		}
	}



	public void Skill_Teleport()
	{
		Cloak();

		CharacterGameplayHelper.PlayVfx(TeleportVfx, transform.position, Quaternion.identity);

		CharacterGameplayHelper.MoveToWorld(this, 
			transform.position + GetControllerForwardVector() * TeleportDistance,
			0.4f, false, true);
	}



	public void Skill_Combustion()
	{
		SoundManager.Instance.SpawnSoundAtLocation(CombustionSoundClip, transform.position, ESoundGroup.SFX, 1.0f, 0.05f, AudioRolloffMode.Linear);
		AddBuff(CombustionRiskBuff);
		AddBuff(CombustionReinforceBuff);
	}
	#endregion




	#region 연산
	void CalculateManaRecovery()
	{
		if (photonView.IsMine && CurrentMana < MaxMana && bCanRegenMana)
		{
			CurrentMana = Mathf.Min(CurrentMana + ManaRecoveryMult * Time.deltaTime, MaxMana);
		}
	}
	#endregion
}
