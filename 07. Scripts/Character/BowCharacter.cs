using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

using SkillFramework;
using CharacterGameplay;
using DamageFramework;



/**
 * 작성자: 20181220 이성수
 * ACharacterBase를 확장한 활 캐릭터입니다.
 */
public class BowCharacter : ACharacterBase
{
	[Header("공격")]

	[SerializeField]
	private AProjectileBase Arrow;

	[SerializeField]
	private AProjectileBase MoonlightArrow;

	[SerializeField]
	private AProjectileBase ShadowArrow;

	[SerializeField]
	private AProjectileBase SprayArrow;

	[SerializeField]
	private DamageZone DZ_DeathRain;

	[SerializeField]
	private Transform ArrowSpawnPosition;



	[Header("버프")]

	[SerializeField]
	private ACharacterBuffBase Buff_WeaknessDetection;

	[SerializeField]
	private ACharacterBuffBase Buff_Stealth;

	[SerializeField]
	private ACharacterBuffBase Buff_StealthIncreaseAtk;



	protected override void Update()
	{
		if (photonView.IsMine)
		{
			TryAttack();
		}

		base.Update();
	}



	#region 오버라이드
	protected override void OnBattleStateChanged()
	{
		AnimComponent.GetComponent<BowCharacterAnimation>().bUpdateSpineLookAtForward = bIsInBattleState;
	}



	public override void Cloak()
	{
		base.Cloak();

		AddBuff(Buff_Stealth);
	}



	public override void Decloak()
	{
		bool bWasCloacking = bIsCloaking;

		base.Decloak();

		if (bWasCloacking)
		{
			AddBuff(Buff_StealthIncreaseAtk);
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

				RpcSetAnimTrigger(!bDoAltAttack ? "NormalAttack" : "Attack_Shadow");

				photonView.RPC("RpcSetAnimTrigger", RpcTarget.Others, !bDoAltAttack ? "NormalAttack" : "Attack_Shadow");
			}
		}
	}



	public override void Attack()
	{
		SpawnArrowByType(0);

		PlayerCameraComponent.PlayCameraShake(5.0f, 0.05f);
	}



	/// <summary>
	/// 화살을 소환합니다.
	/// </summary>
	/// <param name="SkillType">0: 달 화살(기본 공격) / 1: 달빛무리 화살 / 2: 그림자 화살 / 3: 난사 화살</param>
	protected void SpawnArrowByType(int SkillType = 0)
	{
		Decloak();

		switch (SkillType)
		{
			case 0:
			default:
				{
					SpawnArrow(Arrow);

					break;
				}
			case 1:
				{
					SpawnArrow(MoonlightArrow);

					break;
				}
			case 2:
				{
					SpawnArrow(ShadowArrow);

					break;
				}
			case 3:
				{
					SpawnArrow(SprayArrow);

					break;
				}
		}
	}



	void SpawnArrow(AProjectileBase ArrowToSpawn)
	{
		AProjectileBase SpawnedArrow = Instantiate(ArrowToSpawn, ArrowSpawnPosition.position,
						CharacterGameplayHelper.VectorToRotation(PlayerCameraComponent.GetCenterWorldLocation() - transform.position));

		SpawnedArrow.SetupHitCallback(OnAttackHit);
		SpawnedArrow.SetupSplashHitCallback(OnAttackHit);

		SpawnedArrow.StartFire(this.gameObject, StatusComponent.GetFinalDamage().Item1, StatusComponent.GetFinalDamage().Item1);
	}
	


	public void Attack_Shadow()
	{
		SpawnArrowByType(2);

		PlayerCameraComponent.PlayCameraShake(4.0f, 0.05f);
	}



	public void SkillMoonlightAttack()
	{
		SpawnArrowByType(1);

		PlayerCameraComponent.PlayCameraShake(25.0f, 0.05f);
	}



	public void SkillSprayAttack()
	{
		SpawnArrowByType(3);

		PlayerCameraComponent.PlayCameraShake(3.0f, 0.05f);
	}



	public void SkillSpawnDeathRain()
	{
		TryInBattleState();

		Decloak();

		DamageZone SpawnedZone = DamageHelper.SpawnDamageZone(DZ_DeathRain,
			transform.position, transform.rotation, null);

		SpawnedZone.SetupHitCallback(OnAttackHit);

		SpawnedZone.StartZone(StatusComponent.GetFinalDamage().Item1, StatusComponent.GetAtkSpeedMult, this.gameObject);
	}



	public void SkillWeaknessDetection()
	{
		AddBuff(Buff_WeaknessDetection);

		SoundManager.Instance.SpawnSoundAtLocation("BuffApplied0", transform.position, ESoundGroup.SFX, 1.0f, 0.01f, AudioRolloffMode.Linear);

		PlayerCameraComponent.PlayCameraShake(35.0f, 0.05f);
	}
	#endregion
}
