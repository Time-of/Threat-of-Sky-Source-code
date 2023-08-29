using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

using DamageFramework;



public enum ECharacterStatus : int
{
	MAX_HEALTH, DEFENSE, ATK_POWER, CRIT_CHANCE,
	DAMAGE_REDUCE_MULT, ATK_SPEED, MOVE_SPEED
}



/**
 * 작성자: 20181220 이성수
 * 캐릭터의 스테이터스를 관리하는 스테이터스 클래스입니다.
 * 이 클래스에 접근하려면, 먼저 캐릭터에 접근하여 GetStatusComponent()를 사용하세요.
 * 주요 기능으로 사용할 만한 것은 SetAdditionalVariable()과 GetFinalDamage() 입니다.
 * 받는 피해 감소량, 공격 속도, 이동 속도 변수는 곱연산만 적용됩니다.
 */
public class CharacterStatus : MonoBehaviourPun
{
	private ACharacterBase OwnerCharacter;

	[Tooltip("로컬 게임에 있는지 여부입니다. 후에 게임 매니저 등으로 이동할 예정인 변수입니다. false 인 경우, 멀티 플레이어 중이라고 간주합니다.")]
	public bool bIsInLocalGame = false;



	[Header("캐릭터 상태")]

	[SerializeField, Tooltip("true 인 경우, 치명타 확률이 100%를 초과하는 경우 향상된 치명타를 가할 수 있습니다.")]
	// 기본 치명타 피해: 200%, 향상된 치명타 피해: 300%
	private bool bCanDealImprovedCritical = false;

	[SerializeField]
	private float BaseMaxHealth = 100.0f;

	[SerializeField]
	private float BaseDefense = 0.0f;

	[SerializeField]
	private float BaseAtkPower = 10.0f;

	[SerializeField]
	private float BaseCritChance = 0.0f;

	[SerializeField, ReadOnlyProperty]
	private float AdditionalMaxHealth = 0.0f;

	[SerializeField, ReadOnlyProperty]
	private float MaxHealthMult = 1.0f;

	[SerializeField, ReadOnlyProperty]
	private float AdditionalDefense = 0.0f;

	[SerializeField, ReadOnlyProperty]
	private float DefenseMult = 1.0f;

	[SerializeField, ReadOnlyProperty]
	private float AdditionalAtkPower = 0.0f;

	[SerializeField, ReadOnlyProperty]
	private float AtkPowerMult = 1.0f;

	[SerializeField, ReadOnlyProperty]
	private float AdditionalCritChance = 0.0f;

	[SerializeField, ReadOnlyProperty]
	private float CritChanceMult = 1.0f;



	[Header("최종 캐릭터 상태")]

	[SerializeField, ReadOnlyProperty]
	private float FinalHealth = 0.0f;

	[SerializeField, ReadOnlyProperty]
	private float CurrentMaxHealth = 0.0f;

	[SerializeField, ReadOnlyProperty]
	private float FinalDefense = 0.0f;

	[SerializeField, ReadOnlyProperty]
	private float FinalAtkPower = 0.0f;

	[SerializeField, ReadOnlyProperty]
	private float FinalCritChance = 0.0f;

	[SerializeField, ReadOnlyProperty, Tooltip("피해를 얼마만큼 덜 받는지의 여부입니다. 곱연산만 가능합니다.")]
	private float DamageReduceMult = 0.0f;

	[SerializeField, ReadOnlyProperty, Tooltip("공격속도 변수입니다. 곱연산만 가능합니다.")]
	private float AtkSpeedMult = 1.0f;

	[SerializeField, ReadOnlyProperty, Tooltip("이동속도 변수입니다. 곱연산만 가능합니다.")]
	private float MoveSpeedMult = 1.0f;

	public float GetAtkSpeedMult { get => AtkSpeedMult; }

	public float GetMoveSpeedMult { get => MoveSpeedMult; }



	[Header("컴포넌트")]

	private Anim_HPBar Hud_HpBar = null;

	private InGame_UIManager Hud_InGameHud = null;



	void Awake()
	{
		OwnerCharacter = GetComponent<ACharacterBase>();

		Hud_HpBar = FindObjectOfType<Anim_HPBar>();

		Hud_InGameHud = FindObjectOfType<InGame_UIManager>();
	}



	void Start()
	{
		InitializeStatus();

		if (photonView.IsMine)
			Hud_HpBar.SetCurrentHp(Mathf.Round(FinalHealth));
	}



	public void UpdateHealthHUDFull()
	{
		Hud_HpBar.SetCurrentHp(Mathf.Round(1.0f));
	}



	void InitializeStatus()
	{
		CurrentMaxHealth = BaseMaxHealth + AdditionalMaxHealth;

		if (!bIsInLocalGame)
		{
			SetAllValues(AdditionalMaxHealth, MaxHealthMult,
				AdditionalDefense, DefenseMult,
				AdditionalAtkPower, AtkPowerMult,
				AdditionalCritChance, CritChanceMult,
				DamageReduceMult, AtkSpeedMult,
				MoveSpeedMult);

			FinalHealth = CurrentMaxHealth;

			Debug.Log("스테이터스 초기화!");
		}
		else
		{
			RpcSetAllValuesAll(AdditionalMaxHealth, MaxHealthMult,
				AdditionalDefense, DefenseMult,
				AdditionalAtkPower, AtkPowerMult,
				AdditionalCritChance, CritChanceMult,
				DamageReduceMult, AtkSpeedMult,
				MoveSpeedMult);

			FinalHealth = CurrentMaxHealth;
		}
	}



	void ApplyHealth(float NewAmount)
	{
		FinalHealth = Mathf.Clamp(NewAmount, 0.0f, CurrentMaxHealth);

		if (photonView.IsMine)
		{
			Hud_HpBar.SetCurrentHp(Mathf.Round(FinalHealth));
		}

		if (FinalHealth <= 0.0f) OwnerCharacter.CharacterDie();
	}



	#region 사용자 기능 모음
	public void SetCanDealImprovedCritical(bool NewActive)
	{
		bCanDealImprovedCritical = NewActive;
	}



	/// <summary>
	/// 변수를 새 값으로 변경합니다.
	/// </summary>
	/// <param name="StatusToSet"> 새로 설정할 추가 스테이터스 변수입니다.</param>
	/// <param name="AdditionalValue"> 스테이터스 합연산 변수에 할당할 값입니다.</param>
	/// <param name="MultValue"> 스테이터스 곱연산 변수에 할당할 값입니다.</param>
	public void SetAdditionalVariable(ECharacterStatus StatusToSet, float AdditionalValue, float MultValue)
	{
		if (photonView.IsMine)
			photonView.RPC("RpcSetAdditionalVariableMaster", RpcTarget.MasterClient, (int)StatusToSet, AdditionalValue, MultValue);
	}



	/// <summary>
	/// 모든 변수를 모조리 새로 할당합니다.
	/// A 로 시작하는 변수는 합연산, M 으로 시작하는 변수는 곱연산입니다.
	/// </summary>
	void SetAllValues(float A_MaxHp, float M_MaxHp, float A_Defense, float M_Defense, float A_AtkPower, float M_AtkPower, float A_CritChance, float M_CritChance, float M_DamageReduce, float M_AtkSpeed, float M_MoveSpeed)
	{
		if (photonView.IsMine)
		{
			photonView.RPC("RpcSetAllValuesMaster", RpcTarget.MasterClient, A_MaxHp, M_MaxHp, A_Defense, M_Defense, A_AtkPower, M_AtkPower, A_CritChance, M_CritChance, M_DamageReduce, M_AtkSpeed, M_MoveSpeed);
		}
		
	}



	/// <summary>
	/// 현재 능력치를 모두 계산하여 업데이트합니다.
	/// 비용이 큽니다.
	/// </summary>
	public void UpdateAllStatus()
	{
		AdditionalMaxHealth = 0.0f;
		MaxHealthMult = 1.0f;
		AdditionalDefense = 0.0f;
		DefenseMult = 1.0f;
		AdditionalAtkPower = 0.0f;
		AtkPowerMult = 1.0f;
		AdditionalCritChance = 0.0f;
		CritChanceMult = 1.0f;
		DamageReduceMult = 0.0f;
		AtkSpeedMult = 1.0f;
		MoveSpeedMult = 1.0f;

		foreach (ACharacterBuffBase Buff in OwnerCharacter.GetBuffList)
		{
			foreach (CharacterBuffInfo BuffInfo in Buff.GetBuffInfoList)
			{
				// 합연산
				if (BuffInfo.ApplyType == EApplyStatusType.ADDITIVE)
				{
					switch (BuffInfo.StatusToBuff)
					{
						case ECharacterStatus.MAX_HEALTH:
							AdditionalMaxHealth += BuffInfo.Amount;
							break;
						case ECharacterStatus.DEFENSE:
							AdditionalDefense += BuffInfo.Amount;
							break;
						case ECharacterStatus.ATK_POWER:
							AdditionalAtkPower += BuffInfo.Amount;
							break;
						case ECharacterStatus.CRIT_CHANCE:
							AdditionalCritChance += BuffInfo.Amount;
							break;
					}
				}
				// 곱연산
				else if (BuffInfo.ApplyType == EApplyStatusType.MULT)
				{
					switch (BuffInfo.StatusToBuff)
					{
						case ECharacterStatus.MAX_HEALTH:
							MaxHealthMult *= BuffInfo.Amount;
							break;
						case ECharacterStatus.DEFENSE:
							DefenseMult *= BuffInfo.Amount;
							break;
						case ECharacterStatus.ATK_POWER:
							AtkPowerMult *= BuffInfo.Amount;
							break;
						case ECharacterStatus.CRIT_CHANCE:
							CritChanceMult *= BuffInfo.Amount;
							break;
						case ECharacterStatus.DAMAGE_REDUCE_MULT:
							DamageReduceMult *= BuffInfo.Amount;
							break;
						case ECharacterStatus.ATK_SPEED:
							AtkSpeedMult *= BuffInfo.Amount;
							break;
						case ECharacterStatus.MOVE_SPEED:
							MoveSpeedMult *= BuffInfo.Amount;
							break;
					}
				}
			}
		}

		SetAllValues(AdditionalMaxHealth, MaxHealthMult,
				AdditionalDefense, DefenseMult,
				AdditionalAtkPower, AtkPowerMult,
				AdditionalCritChance, CritChanceMult,
				DamageReduceMult, AtkSpeedMult,
				MoveSpeedMult);
	}



	/// <summary>
	/// 버프로 인한 상태를 업데이트합니다.
	/// </summary>
	/// <param name="Buff"> 업데이트될 버프입니다.</param>
	/// <param name="bAdded"> true 인 경우, 버프가 추가되었음을 의미하고, false 인 경우, 버프가 제거되었음을 의미합니다.</param>
	public void UpdateBuffStatus(ACharacterBuffBase Buff, bool bAdded)
	{
		foreach (CharacterBuffInfo BuffInfo in Buff.GetBuffInfoList)
		{
			// 버프 추가됨
			if (bAdded)
			{
				// 합연산
				if (BuffInfo.ApplyType == EApplyStatusType.ADDITIVE)
				{
					switch (BuffInfo.StatusToBuff)
					{
						case ECharacterStatus.MAX_HEALTH:
							AdditionalMaxHealth += BuffInfo.Amount;
							break;
						case ECharacterStatus.DEFENSE:
							AdditionalDefense += BuffInfo.Amount;
							break;
						case ECharacterStatus.ATK_POWER:
							AdditionalAtkPower += BuffInfo.Amount;
							break;
						case ECharacterStatus.CRIT_CHANCE:
							AdditionalCritChance += BuffInfo.Amount;
							break;
					}
				}
				// 곱연산
				else if (BuffInfo.ApplyType == EApplyStatusType.MULT)
				{
					switch (BuffInfo.StatusToBuff)
					{
						case ECharacterStatus.MAX_HEALTH:
							MaxHealthMult *= BuffInfo.Amount;
							break;
						case ECharacterStatus.DEFENSE:
							DefenseMult *= BuffInfo.Amount;
							break;
						case ECharacterStatus.ATK_POWER:
							AtkPowerMult *= BuffInfo.Amount;
							break;
						case ECharacterStatus.CRIT_CHANCE:
							CritChanceMult *= BuffInfo.Amount;
							break;
						case ECharacterStatus.DAMAGE_REDUCE_MULT:
							DamageReduceMult *= BuffInfo.Amount;
							break;
						case ECharacterStatus.ATK_SPEED:
							AtkSpeedMult *= BuffInfo.Amount;
							break;
						case ECharacterStatus.MOVE_SPEED:
							MoveSpeedMult *= BuffInfo.Amount;
							break;
					}
				}
			}
			// 버프 제거됨
			else
			{
				// 합연산
				if (BuffInfo.ApplyType == EApplyStatusType.ADDITIVE)
				{
					switch (BuffInfo.StatusToBuff)
					{
						case ECharacterStatus.MAX_HEALTH:
							AdditionalMaxHealth -= BuffInfo.Amount;
							break;
						case ECharacterStatus.DEFENSE:
							AdditionalDefense -= BuffInfo.Amount;
							break;
						case ECharacterStatus.ATK_POWER:
							AdditionalAtkPower -= BuffInfo.Amount;
							break;
						case ECharacterStatus.CRIT_CHANCE:
							AdditionalCritChance -= BuffInfo.Amount;
							break;
					}
				}
				// 곱연산
				else if (BuffInfo.ApplyType == EApplyStatusType.MULT)
				{
					switch (BuffInfo.StatusToBuff)
					{
						case ECharacterStatus.MAX_HEALTH:
							MaxHealthMult /= BuffInfo.Amount;
							break;
						case ECharacterStatus.DEFENSE:
							DefenseMult /= BuffInfo.Amount;
							break;
						case ECharacterStatus.ATK_POWER:
							AtkPowerMult /= BuffInfo.Amount;
							break;
						case ECharacterStatus.CRIT_CHANCE:
							CritChanceMult /= BuffInfo.Amount;
							break;
						case ECharacterStatus.DAMAGE_REDUCE_MULT:
							DamageReduceMult /= BuffInfo.Amount;
							break;
						case ECharacterStatus.ATK_SPEED:
							AtkSpeedMult /= BuffInfo.Amount;
							break;
						case ECharacterStatus.MOVE_SPEED:
							MoveSpeedMult /= BuffInfo.Amount;
							break;
					}
				}
			}
		}

		SetAllValues(AdditionalMaxHealth, MaxHealthMult,
				AdditionalDefense, DefenseMult,
				AdditionalAtkPower, AtkPowerMult,
				AdditionalCritChance, CritChanceMult,
				DamageReduceMult, AtkSpeedMult,
				MoveSpeedMult);
	}



	public void UpdateStatusItemStatus(AStatusItemBase StatusItem, bool bAdded)
	{
		foreach (CharacterStatusItemInfo ItemStatusInfo in StatusItem.GetItemInfoList)
		{
			// 버프 추가됨
			if (bAdded)
			{
				// 합연산
				if (ItemStatusInfo.ApplyType == EApplyStatusType.ADDITIVE)
				{
					switch (ItemStatusInfo.StatusToItem)
					{
						case ECharacterStatus.MAX_HEALTH:
							AdditionalMaxHealth += ItemStatusInfo.Amount;
							break;
						case ECharacterStatus.DEFENSE:
							AdditionalDefense += ItemStatusInfo.Amount;
							break;
						case ECharacterStatus.ATK_POWER:
							AdditionalAtkPower += ItemStatusInfo.Amount;
							break;
						case ECharacterStatus.CRIT_CHANCE:
							AdditionalCritChance += ItemStatusInfo.Amount;
							break;
					}
				}
				// 곱연산
				else if (ItemStatusInfo.ApplyType == EApplyStatusType.MULT)
				{
					switch (ItemStatusInfo.StatusToItem)
					{
						case ECharacterStatus.MAX_HEALTH:
							MaxHealthMult *= ItemStatusInfo.Amount;
							break;
						case ECharacterStatus.DEFENSE:
							DefenseMult *= ItemStatusInfo.Amount;
							break;
						case ECharacterStatus.ATK_POWER:
							AtkPowerMult *= ItemStatusInfo.Amount;
							break;
						case ECharacterStatus.CRIT_CHANCE:
							CritChanceMult *= ItemStatusInfo.Amount;
							break;
						case ECharacterStatus.DAMAGE_REDUCE_MULT:
							DamageReduceMult *= ItemStatusInfo.Amount;
							break;
						case ECharacterStatus.ATK_SPEED:
							AtkSpeedMult *= ItemStatusInfo.Amount;
							break;
						case ECharacterStatus.MOVE_SPEED:
							MoveSpeedMult *= ItemStatusInfo.Amount;
							break;
					}
				}
			}
			// 버프 제거됨
			else
			{
				// 합연산
				if (ItemStatusInfo.ApplyType == EApplyStatusType.ADDITIVE)
				{
					switch (ItemStatusInfo.StatusToItem)
					{
						case ECharacterStatus.MAX_HEALTH:
							AdditionalMaxHealth -= ItemStatusInfo.Amount;
							break;
						case ECharacterStatus.DEFENSE:
							AdditionalDefense -= ItemStatusInfo.Amount;
							break;
						case ECharacterStatus.ATK_POWER:
							AdditionalAtkPower -= ItemStatusInfo.Amount;
							break;
						case ECharacterStatus.CRIT_CHANCE:
							AdditionalCritChance -= ItemStatusInfo.Amount;
							break;
					}
				}
				// 곱연산
				else if (ItemStatusInfo.ApplyType == EApplyStatusType.MULT)
				{
					switch (ItemStatusInfo.StatusToItem)
					{
						case ECharacterStatus.MAX_HEALTH:
							MaxHealthMult /= ItemStatusInfo.Amount;
							break;
						case ECharacterStatus.DEFENSE:
							DefenseMult /= ItemStatusInfo.Amount;
							break;
						case ECharacterStatus.ATK_POWER:
							AtkPowerMult /= ItemStatusInfo.Amount;
							break;
						case ECharacterStatus.CRIT_CHANCE:
							CritChanceMult /= ItemStatusInfo.Amount;
							break;
						case ECharacterStatus.DAMAGE_REDUCE_MULT:
							DamageReduceMult /= ItemStatusInfo.Amount;
							break;
						case ECharacterStatus.ATK_SPEED:
							AtkSpeedMult /= ItemStatusInfo.Amount;
							break;
						case ECharacterStatus.MOVE_SPEED:
							MoveSpeedMult /= ItemStatusInfo.Amount;
							break;
					}
				}
			}
		}

		SetAllValues(AdditionalMaxHealth, MaxHealthMult,
				AdditionalDefense, DefenseMult,
				AdditionalAtkPower, AtkPowerMult,
				AdditionalCritChance, CritChanceMult,
				DamageReduceMult, AtkSpeedMult,
				MoveSpeedMult);
	}



	public float GetCurrentHealth() { return FinalHealth; }

	public float GetMaxHealth() { return BaseMaxHealth + AdditionalMaxHealth; }

	public float GetHealthPercent() { return FinalHealth / CurrentMaxHealth; }

	/// <summary>
	/// 치명타가 적용된 최종 피해량을 가져옵니다.
	/// </summary>
	/// <returns>최종 피해량, 치명타 배율</returns>
	public (float, float) GetFinalDamage()
	{
		float CritRate = GetCriticalDamageRate();
		return (FinalAtkPower * CritRate, CritRate);
	}
	#endregion



	#region 체력 관련
	public void TakeDamage(float DamageAmount, Vector3 CauserPosition)
	{
		if (photonView.IsMine)
			photonView.RPC("RpcTakeDamageMaster", RpcTarget.MasterClient, 
				Mathf.Max(0.0f, (DamageAmount - FinalDefense) * (1 - DamageReduceMult)),
				CauserPosition);
	}



	public void RestoreHealth(float HealAmount)
	{
		if (photonView.IsMine)
			photonView.RPC("RpcRestoreHealthMaster", RpcTarget.MasterClient, HealAmount);
	}
	#endregion



	#region RPC 모음
	[PunRPC]
	protected void RpcTakeDamageMaster(float DamageAmount, Vector3 CauserPosition)
	{
		photonView.RPC("RpcTakeDamageAll", RpcTarget.All, DamageAmount, CauserPosition);
	}


	[PunRPC]
	protected void RpcTakeDamageAll(float DamageAmount, Vector3 CauserPosition)
	{
		ApplyHealth(FinalHealth - DamageAmount);

		OwnerCharacter.AfterTakeDamage(CauserPosition);

		if (photonView.IsMine)
		{
			photonView.RPC("RpcUpdateNetworkHealth", RpcTarget.AllViaServer, PhotonNetwork.LocalPlayer.ActorNumber);
		}
	}


	[PunRPC]
	protected void RpcRestoreHealthMaster(float HealAmount)
	{
		photonView.RPC("RpcRestoreHealthAll", RpcTarget.All, HealAmount);
	}


	[PunRPC]
	protected void RpcRestoreHealthAll(float HealAmount)
	{
		ApplyHealth(FinalHealth + HealAmount);

		OwnerCharacter.AfterRestoreHealth();

		if (photonView.IsMine)
		{
			photonView.RPC("RpcUpdateNetworkHealth", RpcTarget.AllViaServer, PhotonNetwork.LocalPlayer.ActorNumber);
		}
	}


	[PunRPC]
	protected void RpcUpdateNetworkHealth(int PlayerID)
	{
		Hud_InGameHud.UpdateNetworkHealth(PlayerID, GetHealthPercent());
	}


	[PunRPC]
	protected void RpcSetAdditionalVariableMaster(int StatusToSetInt, float AdditionalValue, float MultValue)
	{
		photonView.RPC("RpcSetAdditionalVariableAll", RpcTarget.All, StatusToSetInt, AdditionalValue, MultValue);
	}


	[PunRPC]
	protected void RpcSetAdditionalVariableAll(int StatusToSetInt, float AdditionalValue, float MultValue)
	{
		switch (StatusToSetInt)
		{
			case (int)ECharacterStatus.MAX_HEALTH:
				{
					AdditionalMaxHealth = AdditionalValue;

					break;
				}
			case (int)ECharacterStatus.DEFENSE:
				{
					AdditionalDefense = AdditionalValue;

					FinalDefense = (BaseDefense + AdditionalDefense) * DefenseMult;

					break;
				}
			case (int)ECharacterStatus.ATK_POWER:
				{
					AdditionalAtkPower = AdditionalValue;

					FinalAtkPower = (BaseAtkPower + AdditionalAtkPower) * AtkPowerMult;

					break;
				}
			case (int)ECharacterStatus.CRIT_CHANCE:
				{
					AdditionalCritChance = AdditionalValue;

					FinalCritChance = (BaseCritChance + AdditionalCritChance) * CritChanceMult;

					break;
				}
			case (int)ECharacterStatus.DAMAGE_REDUCE_MULT:
				{
					DamageReduceMult = MultValue;

					break;
				}
			case (int)ECharacterStatus.ATK_SPEED:
				{
					AtkSpeedMult = MultValue;

					OwnerCharacter.GetAnimComponent().GetAnimator().SetFloat("AtkSpeedMult", AtkSpeedMult);

					break;
				}
			case (int)ECharacterStatus.MOVE_SPEED:
				{
					MoveSpeedMult = MultValue;

					OwnerCharacter.GetAnimComponent().GetAnimator().SetFloat("MoveSpeedMult", MoveSpeedMult);

					break;
				}
		}
	}


	[PunRPC]
	protected void RpcSetAllValuesMaster(float A_MaxHp, float M_MaxHp, float A_Defense, float M_Defense, float A_AtkPower, float M_AtkPower, float A_CritChance, float M_CritChance, float M_DamageReduce, float M_AtkSpeed, float M_MoveSpeed)
	{
		photonView.RPC("RpcSetAllValuesAll", RpcTarget.All, A_MaxHp, M_MaxHp, A_Defense, M_Defense, A_AtkPower, M_AtkPower, A_CritChance, M_CritChance, M_DamageReduce, M_AtkSpeed, M_MoveSpeed);
	}


	[PunRPC]
	protected void RpcSetAllValuesAll(float A_MaxHp, float M_MaxHp, float A_Defense, float M_Defense, float A_AtkPower, float M_AtkPower, float A_CritChance, float M_CritChance, float M_DamageReduce, float M_AtkSpeed, float M_MoveSpeed)
	{
		AdditionalMaxHealth = A_MaxHp;
		MaxHealthMult = M_MaxHp;

		AdditionalDefense = A_Defense;
		DefenseMult = M_Defense;
		AdditionalAtkPower = A_AtkPower;
		AtkPowerMult = M_AtkPower;
		AdditionalCritChance = A_CritChance;
		CritChanceMult = M_CritChance;
		DamageReduceMult = M_DamageReduce;
		AtkSpeedMult = M_AtkSpeed;
		MoveSpeedMult = M_MoveSpeed;

		FinalDefense = (BaseDefense + AdditionalDefense) * DefenseMult;
		FinalAtkPower = (BaseAtkPower + AdditionalAtkPower) * AtkPowerMult;
		FinalCritChance = (BaseCritChance + AdditionalCritChance) * CritChanceMult;

		OwnerCharacter.GetAnimComponent().GetAnimator().SetFloat("AtkSpeedMult", AtkSpeedMult);
		OwnerCharacter.GetAnimComponent().GetAnimator().SetFloat("MoveSpeedMult", MoveSpeedMult);

		float AfterMaxHealth = (BaseMaxHealth + A_MaxHp) * M_MaxHp;
		float TempCurMaxHealth = CurrentMaxHealth;

		//Debug.Log("<color=green>After:</color> " + AfterMaxHealth + ", <color=green>Current:</color> " + TempCurMaxHealth);

		CurrentMaxHealth = (BaseMaxHealth + AdditionalMaxHealth) * MaxHealthMult;

		if (AfterMaxHealth - TempCurMaxHealth > 0.0f)
		{
			//Debug.Log("<color=green>체력회복!!!</color>");
			RpcRestoreHealthAll(AfterMaxHealth - TempCurMaxHealth);
		}

		if (photonView.IsMine)
		{
			Hud_HpBar.SetMaxHp(BaseMaxHealth + AdditionalMaxHealth);
			Hud_HpBar.SetCurrentHp(Mathf.Round(FinalHealth));
		}
	}
	#endregion



	#region 연산
	/**
	 * 치명타가 적용된 피해량을 계산합니다.
	 * bCanDealImprovedCritical이고 bIsImprovedCritical이라면 향상된 치명타를 가합니다.
	 * @RETURN 치명타 배율입니다.
	 */
	float GetCriticalDamageRate()
	{
		bool bIsImprovedCritical = (FinalCritChance > 100.0f && bCanDealImprovedCritical);
		bool bIsCritical = Random.Range(0.0f, 100.0f) <= (bIsImprovedCritical ? FinalCritChance - 100.0f : FinalCritChance);

		return bIsImprovedCritical ? (bIsCritical ? 3.0f : 2.0f) : (bIsCritical ? 2.0f : 1.0f);
	}
	#endregion
}
