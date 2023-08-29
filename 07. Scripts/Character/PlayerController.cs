using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;



/**
 * 작정자: 20181220 이성수
 * 플레이어 캐릭터를 조종할, 컨트롤러입니다.
 * 플레이어 캐릭터는 기능만을 보유하고, 조작은 모두 컨트롤러에서 담당합니다.
 * 플레이어 생성 위치는 게임매니저에 의해 플레이어 스폰 볼륨 구역 내 무작위 위치로 결정됩니다.
 */
public class PlayerController : MonoBehaviourPun
{
	delegate void BoolInputDelegate(bool Value);



	[Header("네트워크")]

	[SerializeField, ReadOnlyProperty]
	private string PlayerName = "DefaultName";

	[SerializeField, ReadOnlyProperty]
	private int PlayerID = -1;

	private PlayerData Data;



	[Header("컨트롤러 세팅")]

	[SerializeField]
	private string CharacterToControl = "ArcherCharacter";

	[SerializeField, ReadOnlyProperty]
	private ACharacterBase ControlledCharacter;

	private Vector3 CharacterSpawnPosition;

	private Camera MainCamera;



	[Header("컨트롤러 회전")]

	[SerializeField, ReadOnlyProperty]
	private bool bUpdateRotation = true;

	[SerializeField]
	private float MaxControllerPitch = 87.5f;

	[SerializeField]
	private float MinControllerPitch = -87.5f;

	private float ControllerPitch = 0.0f;

	private float ControllerYaw = 0.0f;

	[SerializeField]
	private float ControllerRotationSpeed = 750.0f;



	[Header("마우스 커서")]

	[SerializeField, ReadOnlyProperty(true)]
	private bool bCursorLocked = true;

	[SerializeField]
	private Canvas SettingsCanvas;



	void Awake()
	{
		MainCamera = Camera.main?.GetComponent<Camera>();
	}



	void Start()
	{
		if (photonView.IsMine)
		{
			Cursor.lockState = bCursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}



	void Update()
	{
		if (photonView.IsMine)
		{
			HandleInput();
		}
	}



	void FixedUpdate()
	{
		if (photonView.IsMine)
		{
			FixedHandleInput();
			UpdateRotations();
		}
	}



	// 컨트롤 민감도, 0 ~ 1 사이
	public void SetControlSensitivity(float NewSensitivityRate)
	{
		ControllerRotationSpeed = NewSensitivityRate * 1000.0f;

		PlayerPrefs.SetFloat("ControlerSensitivity", ControllerRotationSpeed);
	}



	void SpawnCharacterAndPossess()
	{
		GameObject SpawnedCharacter = PhotonNetwork.Instantiate(CharacterToControl,
				CharacterSpawnPosition, Quaternion.identity);

		Possess(SpawnedCharacter.GetComponent<PhotonView>().ViewID);
	}



	public void SetupPlayerControllerOnPhoton(string PlayerNetworkNickName, int PlayerNetworkID)
	{
		LoadPlayerDataAndSet(PlayerNetworkNickName, PlayerNetworkID);
	}



	public void SetCharacterSpawnPosition(Vector3 NewLocation)
	{
		CharacterSpawnPosition = NewLocation;
	}



	#region 플레이어 데이터 관련
	void LoadPlayerDataAndSet(string PlayerNetworkNickName, int PlayerNetworkID)
	{
		if (photonView.IsMine)
		{
			photonView.RPC("RpcLoadPlayerData", RpcTarget.All, PlayerNetworkNickName, PlayerNetworkID);

			ControllerRotationSpeed = PlayerPrefs.GetFloat("ControlerSensitivity", 750.0f);
		}
	}



	[PunRPC]
	void RpcLoadPlayerData(string PlayerNetworkNickName, int PlayerNetworkID)
	{
		PlayerName = PlayerNetworkNickName;

		PlayerID = PlayerNetworkID;

		if (!GameManager.Instance.IsValidPlayerData(PlayerID))
		{
			Debug.LogWarning("<color=red>PlayerData 로드 실패! 데이터가 유효하지 않습니다.</color> : " + PlayerID);
		}
		else
		{
			Data = GameManager.Instance.GetPlayerData(PlayerID);

			if (photonView.IsMine)
			{
				Invoke("SpawnPlayer", 1.0f);
			}
		}
	}



	void SpawnPlayer()
	{
		CharacterToControl = Data.CharacterName;

		GameObject SpawnedCharacter = PhotonNetwork.Instantiate(CharacterToControl,
		CharacterSpawnPosition, Quaternion.identity);

		Possess(SpawnedCharacter.GetComponent<PhotonView>().ViewID);

		ControlledCharacter.SetNetworkingVariables(PlayerName, PlayerID);

		ControlledCharacter.InitializeByPlayerData(Data);

		Debug.Log("플레이어 데이터 로드! : " + PlayerName + ", " + PlayerID);
	}
	#endregion



	#region 입력 관리
	void HandleInput()
	{
		AddYawInput(Input.GetAxis("Mouse X") * ControllerRotationSpeed * Time.fixedDeltaTime * (bUpdateRotation ? 1.0f : 0.0f));
		AddPitchInput(-Input.GetAxis("Mouse Y") * ControllerRotationSpeed * Time.fixedDeltaTime * (bUpdateRotation ? 1.0f : 0.0f));
		
		if (ControlledCharacter != null)
		{
			InputBool("Jump", ControlledCharacter.JumpInput);
			InputBool("Sprint", ControlledCharacter.SprintInput);

			InputBool("Fire1", ControlledCharacter.FireInput);
			InputBool("Interact", ControlledCharacter.InteractInput);

			InputBool("SkillAttack1", ControlledCharacter.SkillAttack1Input);
			InputBool("SkillAttack2", ControlledCharacter.SkillAttack2Input);
			InputBool("SkillAttack3", ControlledCharacter.SkillAttack3Input);

			InputBool("UseActiveItem", ControlledCharacter.UseItemInput);

			if (Input.GetKeyDown(KeyCode.Escape)) ToggleCursorLock();

			// 테스트용!
			//if (Input.GetKeyDown(KeyCode.C)) ControlledCharacter.AddMoney(100.0f);
		}
	}



	void FixedHandleInput()
	{
		if (ControlledCharacter != null)
		{
			ControlledCharacter.HorizontalInput(Input.GetAxis("Horizontal"));
			ControlledCharacter.VerticalInput(Input.GetAxis("Vertical"));
		}
	}




	void InputBool(string ButtonName, BoolInputDelegate Callback, bool bOnlyOnPressed = false)
	{
		if (Input.GetButtonDown(ButtonName)) Callback(true);
		else if (Input.GetButtonUp(ButtonName) && !bOnlyOnPressed) Callback(false);
	}



	public void AddPitchInput(float Value)
	{
		ControllerPitch = Mathf.Clamp(ControllerPitch + Value, MinControllerPitch, MaxControllerPitch);
	}



	public void AddYawInput(float Value)
	{
		ControllerYaw += Value;
		ControllerYaw = ControllerYaw >= 360 ? ControllerYaw - 360 : ControllerYaw;
		ControllerYaw = ControllerYaw <= -360 ? ControllerYaw + 360 : ControllerYaw;
	}



	public float GetControllerPitch() { return ControllerPitch; }

	public float GetControllerYaw() { return ControllerYaw; }



	public float GetControllerPitchAxisInput()
	{
		return -Input.GetAxis("Mouse Y") * ControllerRotationSpeed * Time.fixedDeltaTime;
	}



	public float GetControllerYawAxisInput()
	{
		return Input.GetAxis("Mouse X") * ControllerRotationSpeed * Time.fixedDeltaTime;
	}
	#endregion



	void ToggleCursorLock()
	{
		bCursorLocked = !bCursorLocked;

		Cursor.lockState = bCursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
		bUpdateRotation = bCursorLocked;

		if (photonView.IsMine)
		{
			SettingsCanvas.gameObject.SetActive(!bCursorLocked);
		}
	}



	public void SetCursorUnLocked()
	{
		bCursorLocked = false;

		Cursor.lockState = bCursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
		bUpdateRotation = bCursorLocked;
	}



	#region 컨트롤러 회전
	/// <summary>
	/// 회전값을 모두 업데이트
	/// </summary>
	void UpdateRotations()
	{
		/**
		 * 컨트롤러 회전 업데이트
		 * ControllerRotationSpeed 값이 크면 끊기는 현상이 있으므로, Slerp 로 보간
		 */
		transform.rotation = Quaternion.Slerp(transform.rotation,
				Quaternion.Euler(ControllerPitch, ControllerYaw, 0.0f), 20.0f * Time.fixedDeltaTime);

		if (ControlledCharacter != null)
		{
			// 플레이어 회전 업데이트
			if (ControlledCharacter.GetMovementComponent() != null)
				ControlledCharacter.GetMovementComponent().TargetRotation = transform.rotation;
		}
	}
	#endregion



	#region 빙의 및 빙의 해제
	public void Possess(int PhotonCharacterViewID)
	{
		var TempCharacter = PhotonView.Find(PhotonCharacterViewID).gameObject;
		ACharacterBase NewCharacter = TempCharacter.GetComponent<ACharacterBase>();

		/** 컨트롤러가 있는 캐릭터는 실패 */
		if (NewCharacter.GetController() != null)
		{
			Debug.LogWarning("현재 누군가가 컨트롤 중인 캐릭터에는 빙의할 수 없습니다!");
			return;
		}

		UnPossess();

		photonView.RPC("PossessRPC", RpcTarget.AllBuffered, PhotonCharacterViewID);
	}



	[PunRPC]
	void PossessRPC(int PhotonCharacterViewID, PhotonMessageInfo info)
	{
		var TempCharacter = PhotonView.Find(PhotonCharacterViewID).gameObject;

		ACharacterBase NewCharacter = TempCharacter.GetComponent<ACharacterBase>();

		// 소유권 이전
		TempCharacter.GetPhotonView().TransferOwnership(photonView.Owner);

		if (NewCharacter != null)
		{
			ControlledCharacter = NewCharacter;
			ControlledCharacter.OnPossessed(this);

			Debug.Log("빙의!: " + info.Sender + ", " + info.photonView);

			if (photonView.IsMine)
			{
				ControlledCharacter.GetCameraComponent().enabled = true;

				ControlledCharacter.GetComponent<AudioListener>().enabled = true;

				MainCamera.transform.parent = ControlledCharacter.GetCameraComponent().transform;
				MainCamera.transform.localPosition = Vector3.zero;
				MainCamera.transform.localRotation = Quaternion.identity;
				MainCamera.GetComponent<AudioListener>().enabled = false;
				MainCamera.GetComponent<Camera>().enabled = false;
			}
		}
	}



	public void UnPossess()
	{
		photonView.RPC("UnPossessRPC", RpcTarget.AllBuffered);
	}



	[PunRPC]
	void UnPossessRPC(PhotonMessageInfo info)
	{
		if (ControlledCharacter != null)
		{
			if (photonView.IsMine)
			{
				ControlledCharacter.GetCameraComponent().enabled = false;

				ControlledCharacter.GetComponent<AudioListener>().enabled = false;

				MainCamera.GetComponent<Camera>().enabled = true;
				MainCamera.GetComponent<AudioListener>().enabled = true;
				MainCamera.transform.SetParent(null, true);
			}

			ControlledCharacter.OnUnPossessed();
			ControlledCharacter = null;

			Debug.Log("빙의 해제!: " + info.Sender + ", " + info.photonView);
		}
	}
	#endregion
}
