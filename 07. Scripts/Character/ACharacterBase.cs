using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Photon.Pun;

using DamageFramework;
using SkillFramework;
using CharacterGameplay;



[System.Serializable]
public class ItemAmountInfo
{
	public string ItemName;
	public int ItemAmount;
}



/**
 * 작성자: 20181220 이성수
 * 캐릭터 베이스 클래스입니다.
 * 캐릭터들의 기본 행동을 구현하는 추상 클래스입니다. (따라서 클래스 이름 앞에 A)
 * 이 클래스로부터 상속하여 구체적인 기능을 재정의하여 사용할 수 있습니다.
 */
[RequireComponent(typeof(CharacterMovement), typeof(PhotonView))]
public abstract class ACharacterBase : MonoBehaviourPun, IDamageable, IPunObservable
{
	[Header("Controller <컨트롤러>")]

	[SerializeField, ReadOnlyProperty]
	protected PlayerController Controller;



	[Header("플레이어 정보")]

	[SerializeField, ReadOnlyProperty]
	private string PlayerName = "DefaultName";

	[SerializeField]
	private TMP_Text PlayerNameText;

	[SerializeField, ReadOnlyProperty]
	private int PlayerID = -1;



	[Header("캐릭터 정보")]

	[SerializeField]
	private string CharacterName = "DefaultCharacter";



	[Header("Money <돈>")]

	[SerializeField, ReadOnlyProperty]
	protected float MoneyHas;



	[Header("Items <아이템>")]

	[SerializeField, ReadOnlyProperty, Tooltip("액티브 아이템입니다.")]
	protected AActiveItemBase UsableItem;

	[SerializeField, ReadOnlyProperty]
	protected List<AStatusItemBase> StatusItemList = new List<AStatusItemBase>();

	[SerializeField, ReadOnlyProperty]
	protected List<ItemAmountInfo> StatusItemNameList = new List<ItemAmountInfo>();



	[Header("Buffs <버프>")]

	[SerializeField, ReadOnlyProperty, Tooltip("버프 리스트입니다. 절대 버프를 직접 리스트에 추가하지 마세요. AddBuff 기능을 사용하세요.")]
	protected List<ACharacterBuffBase> BuffList = new List<ACharacterBuffBase>();

	public List<ACharacterBuffBase> GetBuffList { get => BuffList; }




	[Header("Status <상태>")]

	[Tooltip("다른 버전의 공격을 사용합니다.")]
	public bool bDoAltAttack = false;

	[SerializeField, ReadOnlyProperty(true)]
	private bool bIsDead = false;

	public bool IsDead
	{
		get { return bIsDead; }
		protected set { bIsDead = value; }
	}

	[SerializeField]
	protected bool bCanBeDamaged = true;

	[SerializeField, Tooltip("투명화 시 얼마만큼 투명해지는지를 결정합니다.")]
	protected float CloakingRate = 2.0f;

	[SerializeField, ReadOnlyProperty, Tooltip("은신 상태임을 의미합니다.")]
	protected bool bIsCloaking = false;

	[SerializeField, ReadOnlyProperty, Tooltip("전투 상태임을 의미합니다.")]
	protected bool bIsInBattleState = false;

	[SerializeField, Tooltip("비전투 상태가 되기까지 걸리는 전투 상태 지속시간입니다.")]
	private float BattleStateDuration = 5;

	[SerializeField, ReadOnlyProperty]
	private float LastTryBattleStateTime = 0;

	[ReadOnlyProperty, Tooltip("스킬 사용 중임을 의미합니다.")]
	public bool bIsUsingSkill = false;

	[SerializeField, ReadOnlyProperty, Tooltip("공격 키를 누르고 있음을 의미합니다.")]
	protected bool bIsFireKeyPressed = false;

	[SerializeField, ReadOnlyProperty, Tooltip("공격이 가능한가?")]
	protected bool bCanAttack = true;



	[Header("Interact <상호작용>")]

	protected float InteractDistance = 3.5f;



	[Header("Networking <네트워킹>")]

	[SerializeField]
	protected float NetworkingRotationInterpSpeed = 25.0f;

	protected Vector3 NetworkingPosition;

	protected Quaternion NetworkingRotation;

	protected float NetworkingDistance;



	[Header("크로스헤어")]

	[SerializeField]
	private Texture2D Tex_BaseCrosshair;

	[SerializeField]
	private Color Color_BaseCrosshair = Color.white;

	[SerializeField]
	private Texture2D Tex_CrosshairHit;

	[SerializeField, Tooltip("Hit 텍스쳐가 사라지는 속도 배율입니다.")]
	private float CrosshairHitDisappearMult = 0.65f;

	[SerializeField, ReadOnlyProperty]
	private float CrosshairHitAlpha = 0.0f;

	[SerializeField]
	private float CrosshairSize = 96.0f;



	[Header("Components <컴포넌트>")]

	protected CharacterMovement MovementComponent = null;

	protected CameraArm CameraArmComponent = null;

	protected PlayerCamera PlayerCameraComponent = null;

	protected ACharacterAnimationBase AnimComponent = null;

	protected Rigidbody RigidComponent = null;

	protected CapsuleCollider CapsuleComponent = null;

	protected CharacterStatus StatusComponent = null;

	protected CharacterSkills SkillComponent = null;

	protected Anim_SkillCoolTime Hud_SkillCooltime = null;

	private InGame_UIManager Hud_InGameHud = null;

	public CharacterMovement GetMovementComponent() { return MovementComponent; }

	public Camera GetCameraComponent() { return CameraArmComponent.GetCameraObject(); }

	public PlayerCamera GetPlayerCameraComponent() { return PlayerCameraComponent; }

	public ACharacterAnimationBase GetAnimComponent() { return AnimComponent; }

	public Rigidbody RigidBody { get => RigidComponent; }

	public CapsuleCollider CapsuleComp { get => CapsuleComponent; }

	public CharacterStatus GetStatusComponent() { return StatusComponent; }

	public InGame_UIManager GetHud_InGameHud() { return Hud_InGameHud; }



	protected virtual void Awake()
	{
		MovementComponent = GetComponent<CharacterMovement>();
		CameraArmComponent = GetComponentInChildren<CameraArm>();
		AnimComponent = GetComponentInChildren<ACharacterAnimationBase>();
		RigidComponent = GetComponent<Rigidbody>();
		CapsuleComponent = GetComponent<CapsuleCollider>();
		PlayerCameraComponent = GetComponentInChildren<PlayerCamera>();
		StatusComponent = GetComponent<CharacterStatus>();
		SkillComponent = GetComponent<CharacterSkills>();

		Hud_SkillCooltime = FindObjectOfType<Anim_SkillCoolTime>();
		Hud_InGameHud = FindObjectOfType<InGame_UIManager>();

		NetworkingPosition = Vector3.zero;
		NetworkingRotation = Quaternion.identity;

		SkillComponent?.SetupUseSkillCallback(OnSkillUsed);
	}



	protected void Start()
	{
		
	}



	protected virtual void Update()
	{
		if (photonView.IsMine)
		{
			CalculateBattleStateDuration();
		}
	}



	protected void FixedUpdate()
	{
		UpdateNetworkingTransform();
	}



	public Quaternion GetCameraForwardRotation()
	{
		return CharacterGameplayHelper.VectorToRotation(PlayerCameraComponent.GetCenterWorldLocation() - transform.position);
	}



	#region 콜백
	protected void OnAttackHit(float HitDamage, Vector3 HitPosition)
	{
		if (photonView.IsMine)
		{
			//photonView.RPC("RpcOnAttackHitMaster", RpcTarget.MasterClient, HitDamage, HitPosition);

			//Debug.Log("마스터로 RPC 호출 당시 HitDamage: " + HitDamage);

			OnAttackHitAction(HitDamage, HitPosition);
		}
	}



	protected virtual void OnAttackHitAction(float HitDamage, Vector3 HitPosition)
	{
		CrosshairHitAlpha = 1.2f;

		Debug.Log("피해 입힘! 피해량: " + HitDamage);

		//CharacterGameplayHelper.SpawnDamageFloaterAutoColor(HitPosition, HitDamage);
	}



	[PunRPC]
	protected void RpcOnAttackHitMaster(float HitDamage, Vector3 HitPosition)
	{
		photonView.RPC("RpcOnAttackHitAll", RpcTarget.All, HitDamage, HitPosition);

		Debug.Log("마스터가 All 을 호출할 당시의 HitDamage: " + HitDamage);
	}



	[PunRPC]
	protected void RpcOnAttackHitAll(float HitDamage, Vector3 HitPosition)
	{
		OnAttackHitAction(HitDamage, HitPosition);

		Debug.Log("All 로 뿌려진 HitDamage: " + HitDamage);
	}



	protected virtual void OnSkillUsed(string SkillName)
	{

	}
	#endregion



	#region 입력에 대한 행동
	public virtual void HorizontalInput(float Value)
	{
		if (Controller != null && Value == 0.0f) return;

		Vector3 RightVector = Quaternion.AngleAxis(Controller.GetControllerYaw(), Vector3.up) * Vector3.right;

		MovementComponent.AddMovementInput(RightVector, Value);
	}

	public virtual void VerticalInput(float Value)
	{
		if (Controller != null && Value == 0.0f) return;

		Vector3 ForwardVector = Quaternion.AngleAxis(Controller.GetControllerYaw(), Vector3.up) * Vector3.forward;

		MovementComponent.AddMovementInput(ForwardVector, Value);
	}

	public virtual void JumpInput(bool bIsPressed)
	{
		if (!MovementComponent || bIsUsingSkill) return;

		MovementComponent.bPressedJump = bIsPressed;
	}

	public virtual void FireInput(bool bIsPressed) { bIsFireKeyPressed = bIsPressed; }

	public virtual void InteractInput(bool bIsPressed)
	{
		foreach (Collider collider in Physics.OverlapCapsule(transform.position, 
			transform.position + GetControllerForwardVector() * InteractDistance, 
			GetComponent<CapsuleCollider>()?.radius ?? 0.5f))
		{
			IInteractableObject Interactable = collider.GetComponent<IInteractableObject>();

			Interactable?.Interact(photonView.ViewID);
		}

		Debug.DrawLine(transform.position,
			transform.position + transform.forward * InteractDistance,
			Color.red, 2.0f);
	}

	public virtual void SkillAttack1Input(bool bIsPressed) { }

	public virtual void SkillAttack2Input(bool bIsPressed) { }

	public virtual void SkillAttack3Input(bool bIsPressed) { }

	public virtual void UseItemInput(bool bIsPressed)
	{
		if (UsableItem != null && bIsPressed)
		{
			UseItem();
		}
	}

	public virtual void SprintInput(bool bIsPressed)
	{
		MovementComponent.bIsSprint = bIsPressed;
	}
	#endregion



	#region 캐릭터 무브먼트
	public virtual void OnFalling() { }

	public virtual void OnFallingEnd(bool bIsHardFalling, float CurrentFallingPower)
	{
		if (bIsHardFalling) DamageHelper.ApplyDamage(this.gameObject,
			Mathf.Abs(CurrentFallingPower * 0.5f), this.gameObject);
	}
	#endregion



	#region 컨트롤러 관련 기능들
	public virtual void OnPossessed(PlayerController NewController)
	{
		Controller = NewController;
	}


	public virtual void OnUnPossessed()
	{
		Controller = null;
	}


	public PlayerController GetController() { return Controller; }

	

	public Vector3 GetControllerForwardVector() { return Controller?.transform.forward ?? transform.forward; }

	public Vector3 GetControllerRightVector() { return Controller?.transform.right ?? transform.right; }

	public Quaternion GetControllerRotation() { return Controller.transform.rotation; }
	#endregion



	#region 피해 관련 기능들
	void IDamageable.TakeDamage(float DamageAmount, Vector3 CauserPosition, ADamageType DamageTypePrefab, float DamageTypeDamageMult)
	{
		if (!bCanBeDamaged || bIsDead) return;

		if (photonView.IsMine)
		{
			StatusComponent?.TakeDamage(DamageAmount, CauserPosition);

			if (DamageTypePrefab != null)
			{
				ADamageType AppliedDamageType = Instantiate(DamageTypePrefab, transform);

				AppliedDamageType.StartDamage(this.gameObject, DamageTypeDamageMult, 0.0f);
			}
		}
	}



	/// <summary>
	/// 체력을 회복시킵니다.
	/// </summary>
	/// <param name="HealAmount"> 회복량입니다.</param>
	public void RestoreHealth(float HealAmount)
	{
		if (!bCanBeDamaged || bIsDead) return;

		if (photonView.IsMine)
			StatusComponent?.RestoreHealth(HealAmount);
	}



	/// <summary>
	/// 캐릭터가 죽는 행동을 정의합니다.
	/// 기본 구현은 사망 상태가 되고, 조작할 수 없도록 컨트롤러에서 분리됩니다.
	/// </summary>
	public virtual void CharacterDie()
	{
		bIsFireKeyPressed = false;

		bIsDead = true;

		Controller?.SetCursorUnLocked();

		Controller?.UnPossess();

		photonView.RPC("RpcSetAnimTrigger", RpcTarget.All, "Die");

		GameManager.Instance.OnCharacterDied();

		if (photonView.IsMine)
			Hud_InGameHud.ShowUI(SungSoo_Hud.EInGameUIMode.DIE);
	}



	/// <summary>
	/// 피해를 입은 이후 행동을 정의합니다.
	/// </summary>
	/// <param name="CauserPosition"> 피해를 준 대상의 위치입니다.</param>
	public virtual void AfterTakeDamage(Vector3 CauserPosition)
	{
		
	}



	/// <summary>
	/// 체력을 회복한 이후(RestoreHealth 호출 이후) 행동을 정의합니다.
	/// </summary>
	public virtual void AfterRestoreHealth()
	{

	}
	#endregion



	#region 아이템 관련 기능들
	void UseItem()
	{
		photonView.RPC("RpcUseItem", RpcTarget.AllViaServer);
	}



	[PunRPC]
	protected void RpcUseItem()
	{
		if (Hud_SkillCooltime.bCanUseActive)
		{
			if (UsableItem.TryUseItem())
			{
				if (photonView.IsMine)
					Hud_SkillCooltime.UseActiveItem(UsableItem.GetActiveCoolDown);
			}
		}
	}



	/// <summary>
	/// 패시브 아이템(AStatusItemBase) 추가하기
	/// </summary>
	/// <param name="StatusItemToAdd"> 추가할 아이템 프리팹</param>
	public void AddStatusItem(AStatusItemBase StatusItemToAdd)
	{
		if (photonView.IsMine)
			Hud_InGameHud.AddItemAcquisitionQueue(StatusItemToAdd.GetItemName, false);

		int ListLength = StatusItemList.Count;
		for (int i = 0; i < ListLength; i++)
		{
			// 아이템 이름 비교 후, 중복 시 개수 추가하는 작업
			if (StatusItemList[i].GetItemName == StatusItemToAdd.GetItemName)
			{
				StatusItemList[i].ItemAmount++;
				StatusComponent.UpdateStatusItemStatus(StatusItemToAdd, true);

				// 게임매니저로 넘길 데이터에도 값 저장해놓기
				StatusItemNameList[i].ItemAmount++;

				return;
			}
		}

		AStatusItemBase StatusItemInstance = Instantiate(StatusItemToAdd, transform);

		StatusItemInstance.SetOwnerCharacter(this);

		StatusItemList.Add(StatusItemInstance);
		StatusComponent.UpdateStatusItemStatus(StatusItemToAdd, true);

		// 게임매니저로 넘길 데이터에도 값 저장해놓기
		ItemAmountInfo NewItemInfo = new ItemAmountInfo();
		NewItemInfo.ItemName = StatusItemInstance.GetItemName;
		NewItemInfo.ItemAmount = 1;

		StatusItemNameList.Add(NewItemInfo);

		if (photonView.IsMine)
		{
			photonView.RPC("RpcAddStatusItem", RpcTarget.AllViaServer, StatusItemToAdd.GetItemName);
		}
	}



	/// <summary>
	/// 데이터 로드 시, AStatusItem 불러오는 기능
	/// </summary>
	/// <param name="StatusItemToAdd"></param>
	void AddStatusItemOnLoadData(AStatusItemBase StatusItemToAdd)
	{
		//Hud_InGameHud.AddItemAcquisitionQueue(StatusItemToAdd.GetItemName, false);

		int ListLength = StatusItemList.Count;
		for (int i = 0; i < ListLength; i++)
		{
			// 아이템 이름 비교 후, 중복 시 개수 추가하는 작업
			if (StatusItemList[i].GetItemName == StatusItemToAdd.GetItemName)
			{
				StatusItemList[i].ItemAmount++;
				StatusComponent.UpdateStatusItemStatus(StatusItemToAdd, true);

				return;
			}
		}

		AStatusItemBase StatusItemInstance = Instantiate(StatusItemToAdd, transform);

		StatusItemInstance.SetOwnerCharacter(this);

		StatusItemList.Add(StatusItemInstance);
		StatusComponent.UpdateStatusItemStatus(StatusItemToAdd, true);

		if (photonView.IsMine)
		{
			photonView.RPC("RpcAddStatusItem", RpcTarget.AllViaServer, StatusItemToAdd.GetItemName);
		}
	}



	[PunRPC]
	protected void RpcAddStatusItem(string ItemName)
	{
		Hud_InGameHud.AddStatusItem(PlayerID, ItemName);
	}



	/// <summary>
	/// 액티브 아이템 설정하기
	/// </summary>
	/// <param name="NewActiveItemName"> 새 액티브 아이템 이름</param>
	public void SetActiveItem(string NewActiveItemName)
	{
		if (photonView.IsMine)
			photonView.RPC("RpcSetActiveItem", RpcTarget.All, NewActiveItemName);
	}



	[PunRPC]
	protected void RpcSetActiveItem(string NewActiveItemName)
	{
		if (UsableItem != null)
		{
			Destroy(UsableItem.gameObject);
		}

		if (photonView.IsMine)
			Hud_InGameHud.AddItemAcquisitionQueue(NewActiveItemName, true);

		AActiveItemBase ActiveItemInstance = Instantiate(CharacterGameplayManager.Instance.ActiveItemDictionary[NewActiveItemName], transform);

		ActiveItemInstance.SetownerCharceter(this);

		UsableItem = ActiveItemInstance;

		if (photonView.IsMine)
		{
			Hud_SkillCooltime.SetSkillIcon(0, UsableItem.GetItemSprite);
		}
	}



	/// <summary>
	/// 액티브 아이템 불러오는 기능
	/// </summary>
	/// <param name="ActiveItemName"> 아이템 이름</param>
	void LoadActiveItem(string ActiveItemName)
	{
		if (ActiveItemName == "" || !CharacterGameplayManager.Instance.ActiveItemDictionary.ContainsKey(ActiveItemName)) return;

		Debug.Log("<color=green>아이템 불러오기 중, 아이템 이름: </color>" + ActiveItemName);

		if (photonView.IsMine)
			photonView.RPC("RpcSetActiveItem", RpcTarget.All, ActiveItemName);
	}
	#endregion



	#region 데이터 관련 기능들
	public void SavePlayerData()
	{
		PlayerData MyData;
		MyData.PlayerID = PlayerID;
		MyData.CharacterName = CharacterName;
		MyData.ActiveItemName = UsableItem?.GetItemName ?? "";
		MyData.StatusItemNameList = StatusItemNameList;
		MyData.bDoAltAttack = bDoAltAttack;
		MyData.Skill1Name = SkillComponent.GetSkillNameByCurrentSlot(1);
		MyData.Skill2Name = SkillComponent.GetSkillNameByCurrentSlot(2);
		MyData.Skill3Name = SkillComponent.GetSkillNameByCurrentSlot(3);

		GameManager.Instance.AddPlayerData(MyData);

		Debug.Log("<color=green>플레이어 데이터를 GameManager에 저장했습니다!</color>");
	}



	/// <summary>
	/// 플레이어 데이터로 캐릭터 상태 초기화
	/// PlayerController에 의해 호출
	/// </summary>
	/// <param name="Data"></param>
	public void InitializeByPlayerData(PlayerData Data)
	{
		PlayerID = Data.PlayerID;

		LoadActiveItem(Data.ActiveItemName);

		StatusItemNameList = Data.StatusItemNameList;

		foreach (ItemAmountInfo Info in StatusItemNameList)
		{
			for (int i = 0; i < Info.ItemAmount; i++)
			{
				AddStatusItemOnLoadData(CharacterGameplayManager.Instance.StatusItemDictionary[Info.ItemName]);
			}
		}

		bDoAltAttack = Data.bDoAltAttack;
		SkillComponent.SetSkillName(1, Data.Skill1Name);
		SkillComponent.SetSkillName(2, Data.Skill2Name);
		SkillComponent.SetSkillName(3, Data.Skill3Name);

		SetSkillHud();

		if (photonView.IsMine)
		{
			StatusComponent.UpdateHealthHUDFull();
			Hud_InGameHud.UpdateNetworkHealth(PhotonNetwork.LocalPlayer.ActorNumber, 1.0f);
		}
	}



	public void SetNetworkingVariables(string NameToSet, int PlayerIdToSet)
	{
		photonView.RPC("RpcSetNetworkingVariables", RpcTarget.AllBuffered, NameToSet, PlayerIdToSet);
	}



	[PunRPC]
	protected void RpcSetNetworkingVariables(string NameToSet, int PlayerIdToSet)
	{
		PlayerName = NameToSet;
		PlayerNameText.text = PlayerName;

		PlayerID = PlayerIdToSet;
	}
	#endregion



	#region 돈 관련 기능들
	/// <summary>
	/// 돈을 추가합니다.
	/// </summary>
	/// <param name="Amount"> 추가할 양입니다.</param>
	public void AddMoney(float Amount)
	{
		MoneyHas += Amount;

		MoneyHas = Mathf.Round(MoneyHas);

		if (photonView.IsMine)
			Hud_InGameHud?.UpdateMoneyText(MoneyHas);
	}



	/// <summary>
	/// 돈 사용을 시도합니다.
	/// 현재 MoneyHas가 50일 때, TryUseMoney(100)이 호출된다면: false 를 반환하고 MoneyHas는 50입니다.
	/// 현재 MoneyHas가 101일 때, TryUseMoney(100)이 호출된다면: true 를 반환하고 MoneyHas는 1이 됩니다.
	/// </summary>
	/// <param name="Amount"> 사용을 시도할 양입니다.</param>
	/// <returns>사용에 성공했다면 true, 실패했다면 false 를 반환합니다.</returns>
	public bool TryUseMoney(float Amount)
	{
		if (MoneyHas < Amount) return false;

		MoneyHas -= Amount;

		if (photonView.IsMine)
			Hud_InGameHud?.UpdateMoneyText(MoneyHas);

		SoundManager.Instance.SpawnSoundAtLocation("UseMoney", transform.position, ESoundGroup.SFX, 1.0f, 0.02f);

		return true;
	}
	#endregion



	#region 애니메이션 관련 기능들
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



	#region 버프 관련 기능들
	/// <summary>
	/// 버프 프리팹을 인스턴싱하여, 버프 목록에 추가하고 능력치를 갱신합니다.
	/// </summary>
	/// <param name="BuffPrefab">버프 프리팹</param>
	public void AddBuff(ACharacterBuffBase BuffPrefab)
	{
		ACharacterBuffBase BuffInstance = Instantiate(BuffPrefab, transform);

		BuffInstance.StartBuff(this);

		BuffList.Add(BuffInstance);

		//StatusComponent?.UpdateAllStatus();
		StatusComponent?.UpdateBuffStatus(BuffInstance, true);
	}



	/// <summary>
	/// 버프를 제거합니다. 직접 호출하지 않는 것을 추천합니다.
	/// </summary>
	/// <param name="BuffInstance">버프 객체</param>
	public void RemoveBuff(ACharacterBuffBase BuffInstance)
	{
		BuffList.Remove(BuffInstance);

		//StatusComponent?.UpdateAllStatus();
		StatusComponent?.UpdateBuffStatus(BuffInstance, false);
	}
	#endregion



	#region 공격 관련 기능들
	public void SetCanAttack(bool NewActive)
	{
		bCanAttack = NewActive;
	}




	// 사실 사용처가 굉장히 애매해진 기능.. 없앨 예정
	public virtual void Attack()
	{
		
	}



	/// <summary>
	/// 투명화(은신)합니다.
	/// </summary>
	public virtual void Cloak()
	{
		if (bIsCloaking) return;

		bIsCloaking = true;

		foreach (Renderer RenderComp in GetComponentsInChildren<Renderer>())
		{
			foreach (Material Mat in RenderComp.materials)
			{
				Mat.SetFloat("_CloakingRate", CloakingRate);
			}
		}
	}



	/// <summary>
	/// 투명화(은신) 해제합니다.
	/// </summary>
	public virtual void Decloak()
	{
		if (!bIsCloaking) return;

		bIsCloaking = false;

		foreach (Renderer RenderComp in GetComponentsInChildren<Renderer>())
		{
			foreach (Material Mat in RenderComp.materials)
			{
				Mat.SetFloat("_CloakingRate", 0.0f);
			}
		}
	}



	/// <summary>
	/// 스킬을 쉽게 사용할 수 있도록 만들어진 기능입니다.
	/// </summary>
	/// <param name="SkillSlot"> 현재 장착된 스킬 슬롯이 유효하다면 해당 스킬을 사용합니다.</param>
	/// <returns>스킬 사용 성공 여부를 반환합니다.</returns>
	protected bool TryUseSkillBySlot(int SkillSlot)
	{
		if (SkillComponent == null) return false;

		if (!bIsUsingSkill)
		{
			string SkillName = SkillComponent.GetSkillNameByCurrentSlot(SkillSlot);

			if (SkillName == "UnknownSkill") return false;

			bool SkillSuccess;
			bool SkillHasAnim;

			(SkillSuccess, SkillHasAnim) = SkillComponent.TryUseSkillByName(SkillName);

			if (SkillSuccess)
			{
				TryInBattleState();

				if (SkillHasAnim)
				{
					RpcSetAnimTrigger(SkillName);

					photonView.RPC("RpcSetAnimTrigger", RpcTarget.Others, SkillName);
				}

				if (photonView.IsMine)
					Hud_SkillCooltime.UseSkill(SkillSlot);

				return true;
			}
		}

		return false;
	}
	#endregion



	#region BattleState (전투 상태) 설정
	protected void TryInBattleState()
	{
		SetIsInBattleState(true);

		LastTryBattleStateTime = Time.time;
	}



	void SetIsInBattleState(bool NewActive)
	{
		if (bIsInBattleState == NewActive) return;

		bIsInBattleState = NewActive;

		MovementComponent.bUseControllerRotationYaw = NewActive;

		RpcSetBattleState(bIsInBattleState);

		photonView.RPC("RpcSetBattleState", RpcTarget.Others, bIsInBattleState);
	}


	[PunRPC]
	protected void RpcSetBattleState(bool NewActive)
	{
		AnimComponent.GetAnimator().SetBool("InBattleState", NewActive);

		OnBattleStateChanged();
	}



	/// <summary>
	/// 전투 상태가 변경되었을 때 호출됩니다. 재정의 가능합니다.
	/// </summary>
	protected virtual void OnBattleStateChanged()
	{

	}



	void CalculateBattleStateDuration()
	{
		if (bIsInBattleState)
		{
			if (Time.time >= LastTryBattleStateTime + BattleStateDuration)
			{
				SetIsInBattleState(false);
			}
		}
	}
	#endregion



	#region 네트워킹 관련 기능들
	void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(RigidComponent.position);

			stream.SendNext(RigidComponent.rotation.eulerAngles.y);

			stream.SendNext(MovementComponent.Velocity);

			stream.SendNext(RigidComponent.velocity);
		}
		else
		{
			NetworkingPosition = (Vector3)stream.ReceiveNext();

			NetworkingRotation = Quaternion.Euler(0.0f, (float)stream.ReceiveNext(), 0.0f);

			MovementComponent.Velocity = (Vector3)stream.ReceiveNext();

			RigidComponent.velocity = (Vector3)stream.ReceiveNext();

			float NetworkingLag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));

			NetworkingPosition += RigidComponent.velocity * NetworkingLag;

			NetworkingDistance = Vector3.Distance(RigidComponent.position, NetworkingPosition);
		}
	}



	void UpdateNetworkingTransform()
	{
		if (!photonView.IsMine)
		{
			RigidComponent.position = Vector3.MoveTowards(RigidComponent.position, NetworkingPosition,
				NetworkingDistance * (1.0f / PhotonNetwork.SerializationRate));

			RigidComponent.rotation = Quaternion.Slerp(RigidComponent.rotation, NetworkingRotation,
				NetworkingRotationInterpSpeed * Time.fixedDeltaTime);
		}
	}
	#endregion



	#region HUD 관련 기능들
	void SetSkillHud()
	{
		if (photonView.IsMine)
			for (int i = 1; i <= 3; i++)
			{
				Hud_SkillCooltime.SetCooltime(i, SkillComponent.GetBaseCooltimeByIndex(i));
				Hud_SkillCooltime.SetSkillIcon(i, SkillComponent.GetSkillIconSpriteByIndex(i));
			}
	}



	void DrawCrosshair()
	{
		if (CrosshairHitAlpha > 0.0f) CrosshairHitAlpha -= Time.deltaTime * CrosshairHitDisappearMult;

		GUI.DrawTexture(new Rect((Screen.width - CrosshairSize) * 0.5f,
			(Screen.height - CrosshairSize) * 0.5f,
			CrosshairSize, CrosshairSize),
			Tex_BaseCrosshair, ScaleMode.ScaleAndCrop,
			true, 0, Color_BaseCrosshair, 0.0f, 0.0f);

		Color HitAlphaColor = Color.white;
		HitAlphaColor.a = CrosshairHitAlpha;

		GUI.DrawTexture(new Rect((Screen.width - CrosshairSize) * 0.5f,
			(Screen.height - CrosshairSize) * 0.5f,
			CrosshairSize, CrosshairSize),
			Tex_CrosshairHit, ScaleMode.ScaleAndCrop,
			true, 0, HitAlphaColor, 0.0f, 0.0f);
	}



	void OnGUI()
	{
		if (photonView.IsMine && Tex_BaseCrosshair != null)
			DrawCrosshair();
	}
	#endregion



	public void ShowClearUI()
	{
		if (photonView.IsMine)
			Hud_InGameHud.ShowUI(SungSoo_Hud.EInGameUIMode.CLEAR);
	}



	public void ShowFailUI()
	{
		if (photonView.IsMine)
			Hud_InGameHud.ShowUI(SungSoo_Hud.EInGameUIMode.FAIL);
	}
}
