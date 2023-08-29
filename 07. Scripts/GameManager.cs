using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;



// 몬스터 스폰 정보
[System.Serializable]
public struct MonsterSpawnInfo
{
	[Tooltip("해당 몬스터가 스폰될 씬 이름")]
	public string SpawnScene;

	[Tooltip("플레이어 1인 당 최대 몬스터 수")]
	public int MaxMonsterCountPerPlayer;

	[Tooltip("보스 몬스터 소환에 필요한 적 처치 수")]
	public int EnemyDeathCountToSpawnBossMonster;

	[Tooltip("스폰될 몬스터 프리팹의 모음")]
	public List<EnemyCtrl> MonsterPrefabList;

	public ABossBase BossPrefab;
}



// 저장해야 할 플레이어 데이터 목록
[System.Serializable]
public struct PlayerData
{
	// 플레이어 포톤 ID
	public int PlayerID;

	// 고른 캐릭터의 이름
	public string CharacterName;

	public string ActiveItemName;

	public List<ItemAmountInfo> StatusItemNameList;

	public bool bDoAltAttack;

	public string Skill1Name;

	public string Skill2Name;

	public string Skill3Name;
}



/**
 * 작성자: 20181220 이성수
 * 게임플레이를 총괄하는 게임 매니저 클래스입니다.
 */
public sealed class GameManager : MonoBehaviourPun
{
	private static GameManager instance = null;

	public static GameManager Instance { get => instance; }



	[SerializeField]
	private bool bDebugMode = false;



	[SerializeField, ReadOnlyProperty, Tooltip("몬스터 및 보스 몬스터 레벨 정보")]
	private int CurrentLevel = 1;

	public int GetCurrentLevel { get => CurrentLevel; }



	[Header("씬 정보")]

	[SerializeField, ReadOnlyProperty]
	private bool bIsLevelCleared;

	[SerializeField, ReadOnlyProperty]
	private string CurrentSceneName = "";

	[SerializeField, ReadOnlyProperty]
	private int PlayerDeathCount = 0;



	[Header("스폰 정보")]

	[SerializeField, ReadOnlyProperty, Tooltip("현재 씬에 있는 몬스터 수")]
	private int CurrentEnemyCountInScene = 0;

	[SerializeField, ReadOnlyProperty, Tooltip("현재 씬에 스폰될 수 있는 최대 몬스터 수")]
	private int MaxEnemyCountInScene = 0;

	[SerializeField]
	private List<MonsterSpawnInfo> MonsterInfoList;

	// Awake 에서 MonsterInfoList를 순회하며 어떤 씬에 어떤 몬스터 리스트가 있는지 딕셔너리로 저장
	private Dictionary<string, List<EnemyCtrl>> MonstersInSceneDictionary;

	[SerializeField, ReadOnlyProperty, Tooltip("현재 씬의 스폰 볼륨 중 플레이어 스폰 볼륨을 여기에 저장합니다.")]
	private SpawnVolume PlayerSpawnVolume;

	[SerializeField, ReadOnlyProperty, Tooltip("현재 씬의 스폰 볼륨 중 적 스폰 볼륨을 모두 여기에 저장합니다.")]
	private List<SpawnVolume> MonsterSpawnVolumeList = new List<SpawnVolume>();

	[SerializeField, ReadOnlyProperty, Tooltip("현재 씬의 스폰 볼륨 중 보스 스폰 볼륨을 여기에 저장합니다.")]
	private SpawnVolume BossSpawnVolume;

	[SerializeField, ReadOnlyProperty, Tooltip("현재 씬의 스폰 볼륨 중 아이템 박스 볼륨을 모두 여기에 저장합니다.")]
	private List<SpawnVolume> ItemBoxVolumeList = new List<SpawnVolume>();

	[SerializeField, Tooltip("스폰 볼륨 당 기본 몬스터 생성량")]
	private int EnemySpawnCountPerSpawnVolume = 2;

	[SerializeField, Tooltip("몬스터 생성량 랜덤 값")]
	private int EnemySpawnCountRandomizer = 1;

	[SerializeField, Tooltip("몬스터 생성 주기")]
	private float EnemySpawnTime = 10.0f;

	[SerializeField, Tooltip("몬스터 생성 주기 랜덤 값")]
	private float EnemySpawnTimeRandomizer = 2.0f;

	// 보스 몬스터 소환을 위해 필요한 적 사망 수
	[SerializeField, ReadOnlyProperty]
	private int EnemyDeathCountToSpawnBossMonster = 0;

	// 보스 소환 여부
	[SerializeField, ReadOnlyProperty]
	private bool bBossSpawned = false;

	// 현재 씬 보스 이름
	[SerializeField, ReadOnlyProperty]
	private string CurrentSceneBossName = "";



	[Header("데이터 정보")]

	[SerializeField, ReadOnlyProperty]
	private List<PlayerData> PlayerDataList = new List<PlayerData>();



	void Awake()
	{
		if (instance == null)
		{
			DontDestroyOnLoad(gameObject);
			instance = this;
		}
		else Destroy(gameObject);
		
		InitializeMonstersInSceneDictionary();
		FindAllSpawnVolumes();

		Debug.Log("Awake()는 레벨이 넘어가도 호출될까?");
	}



	void Start()
	{
		Debug.Log("Start()는 레벨이 넘어가도 호출될까?");

		if (SceneManager.GetActiveScene().name == "01. Main_Scene")
		{
			SoundManager.Instance.PlayBGM("Main", 1.0f, true);
		}
	}



	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;

		if (bDebugMode)
		{
			SetCharacterSettings("HAHA", 1, "ArcherCharacter",
			true, "SK_Moonlight", "SK_Leap", "SK_DeathRain");
			SetCharacterSettings("DefaultNamePhoton", 2, "GhostCharacter",
				false, "SK_Creation", "SK_Teleport", "SK_Annihilation");
			SetCharacterSettings("HihiMyNameIsNone", 3, "MurdererCharacter",
				false, "SK_Slash", "SK_ChaoticStep", "SK_BloodCarnival");
		}

		Debug.Log("OnEnable()은 레벨이 넘어가도 호출될까?");
	}



	private void Update()
	{
		if (!bDebugMode) return;

		// 디버그용 기능
		if (Input.GetKeyDown(KeyCode.G)) LoadNextLevel("2. SS_TestScene1");
		if (Input.GetKeyDown(KeyCode.H)) LoadNextLevel();

		// 플레이어 캐릭터들 찾아서 플레이어 데이터 저장하기 테스트
		if (Input.GetKeyDown(KeyCode.B))
		{
			ClearPlayerData();

			foreach (ACharacterBase NetworkCharacter in FindObjectsOfType<ACharacterBase>())
			{
				NetworkCharacter.SavePlayerData();
			}
		}

		if (Input.GetKeyDown(KeyCode.Y))
		{
			SpawnBossMonster();
		}
	}



	#region 초기화
	void InitializeMonstersInSceneDictionary()
	{
		MonstersInSceneDictionary = new Dictionary<string, List<EnemyCtrl>>();

		foreach (MonsterSpawnInfo info in MonsterInfoList)
		{
			MonstersInSceneDictionary.Add(info.SpawnScene, info.MonsterPrefabList);
		}
	}



	void FindAllSpawnVolumes()
	{
		MonsterSpawnVolumeList.Clear();
		ItemBoxVolumeList.Clear();

		foreach (SpawnVolume volume in FindObjectsOfType<SpawnVolume>())
		{
			if (volume.gameObject.CompareTag("Enemy"))
			{
				MonsterSpawnVolumeList.Add(volume);
			}
			else if (volume.gameObject.CompareTag("ItemBox"))
			{
				ItemBoxVolumeList.Add(volume);
			}
			else if (volume.gameObject.CompareTag("Player"))
			{
				PlayerSpawnVolume = volume;
			}
			else if (volume.gameObject.CompareTag("Boss"))
			{
				BossSpawnVolume = volume;
			}
		}
	}
	#endregion



	#region 스폰 관련 기능
	void SpawnPlayerController()
	{
		Invoke("SpawnControllerInvoke", 1.0f);
	}



	void SpawnControllerInvoke()
	{
		GameObject PlayerControllerGO = PhotonNetwork.Instantiate("PlayerController", Vector3.zero, Quaternion.identity);

		PlayerControllerGO.GetComponent<PlayerController>().SetCharacterSpawnPosition(PlayerSpawnVolume != null ?
			PlayerSpawnVolume.GetRandomPoint() : Vector3.zero);

		PlayerControllerGO.GetComponent<PlayerController>().
			SetupPlayerControllerOnPhoton(PhotonNetwork.LocalPlayer.NickName,
			PhotonNetwork.LocalPlayer.ActorNumber);
	}



	void SpawnItemBoxes()
	{
		if (!PhotonNetwork.IsMasterClient) return;

		// 볼륨 당 하나씩 ...
		foreach (SpawnVolume ItemBoxVolume in ItemBoxVolumeList)
		{
			GameObject SpawnedBox = PhotonNetwork.Instantiate("ItemBox", ItemBoxVolume.GetRandomPoint(), Quaternion.identity);

			SpawnedBox.GetComponent<ItemManager>().SetPositionToGround();
		}
	}



	IEnumerator SpawnEnemiesCoroutine()
	{
		while (!bIsLevelCleared)
		{
			yield return new WaitForSeconds(Random.Range(EnemySpawnTime - EnemySpawnTimeRandomizer,
					EnemySpawnTime + EnemySpawnTimeRandomizer));

			foreach (SpawnVolume EnemyVolume in MonsterSpawnVolumeList)
			{
				int EnemySpawnCount = Random.Range(EnemySpawnCountPerSpawnVolume - EnemySpawnCountRandomizer,
					EnemySpawnCountPerSpawnVolume + EnemySpawnCountRandomizer);

				for (int i = 0; i < EnemySpawnCount; i++)
				{
					if (CurrentEnemyCountInScene < MaxEnemyCountInScene)
					{
						// 딕셔너리에서 가져온 현재 씬의 몬스터 리스트에서 무작위로 뽑아 스폰
						GameObject SpawnedMob = PhotonNetwork.Instantiate(MonstersInSceneDictionary[CurrentSceneName][Random.Range(0, MonstersInSceneDictionary[CurrentSceneName].Count - 1)].name,
							EnemyVolume.GetRandomPoint(), Quaternion.identity);

						//Physics.Raycast(SpawnedMob.transform.position, Vector3.down, out RaycastHit HitInfo, 10.0f, 1 << LayerMask.NameToLayer("Ground"), QueryTriggerInteraction.Ignore);

						//SpawnedMob.transform.position = HitInfo.point != Vector3.zero ? HitInfo.point : SpawnedMob.transform.position;

						SpawnedMob.GetComponent<EnemyCtrl>().SetLevelDifficulty(CurrentLevel);

						CurrentEnemyCountInScene++;
					}
				}
			}
		}
	}



	void SpawnBossMonster()
	{
		bBossSpawned = true;

		GameObject SpawnedBoss = PhotonNetwork.Instantiate(CurrentSceneBossName,
							BossSpawnVolume.GetRandomPoint(), Quaternion.identity);

		//Physics.Raycast(SpawnedBoss.transform.position, Vector3.down, out RaycastHit HitInfo, 10.0f, 1 << LayerMask.NameToLayer("Ground"), QueryTriggerInteraction.Ignore);

		//SpawnedBoss.transform.position = HitInfo.point != Vector3.zero ? HitInfo.point : SpawnedBoss.transform.position;

		SpawnedBoss.GetComponent<ABossBase>().SetLevelDifficulty(CurrentLevel);
	}
	#endregion



	// 몬스터에서 사망 시 호출할 것
	public void OnEnemyDied()
	{
		CurrentEnemyCountInScene--;
		EnemyDeathCountToSpawnBossMonster--;

		if (PhotonNetwork.IsMasterClient && !bBossSpawned && EnemyDeathCountToSpawnBossMonster <= 0)
		{
			SpawnBossMonster();
		}
	}



	public void OnBossEnemyDied(Vector3 DiedPosition)
	{
		bIsLevelCleared = true;

		if (CurrentSceneName == "GameScene_GrassLand" || CurrentSceneName == "GameScene_SnowLand")
		{
			//PhotonNetwork.Instantiate("AlterObject", DiedPosition, Quaternion.identity);

			LoadNextLevel();
		}

		else if (CurrentSceneName == "GameScene_DesertLand")
		{
			OnGameCleared();
		}
	}



	public void OnCharacterDied()
	{
		PlayerDeathCount++;

		if (PlayerDeathCount >= PhotonNetwork.PlayerList.Length)
		{
			foreach (ACharacterBase aCharacter in FindObjectsOfType<ACharacterBase>())
			{
				aCharacter.ShowFailUI();
			}
		}
	}



	public void OnGameCleared()
	{
		bIsLevelCleared = true;

		foreach (ACharacterBase aCharacter in FindObjectsOfType<ACharacterBase>())
		{
			aCharacter.ShowClearUI();
		}
	}



	public void FixCharacterPosition(ACharacterBase aCharacter)
	{
		aCharacter.transform.position = PlayerSpawnVolume.GetRandomPoint();
	}



	#region 씬 관련 기능
	// UI 에서 최초 호출!
	public void LoadFirstLevel()
	{
		CurrentLevel = 1;
		EnemySpawnCountPerSpawnVolume = (PhotonNetwork.PlayerList.Length > 2) ? EnemySpawnCountPerSpawnVolume * 2 : EnemySpawnCountPerSpawnVolume;

		Random.InitState(Random.Range(100, 3000));

		if (PhotonNetwork.IsMasterClient)
			PhotonNetwork.LoadLevel("GameScene_GrassLand");
	}



	public void LoadNextLevel()
	{
		Random.InitState(Random.Range(100, 3000));

		ClearPlayerData();

		foreach (ACharacterBase NetworkCharacter in FindObjectsOfType<ACharacterBase>())
		{
			NetworkCharacter.SavePlayerData();
		}


		if (PhotonNetwork.IsMasterClient)
		{
			if (SceneManager.GetActiveScene().name == "GameScene_GrassLand")
			{
				PhotonNetwork.LoadLevel("GameScene_SnowLand");
			}
			else if (SceneManager.GetActiveScene().name == "GameScene_SnowLand")
			{
				PhotonNetwork.LoadLevel("GameScene_DesertLand");
			}
		}
	}



	public void LoadNextLevel(string LevelName)
	{
		/*ClearPlayerData();

		foreach (ACharacterBase NetworkCharacter in FindObjectsOfType<ACharacterBase>())
		{
			NetworkCharacter.SavePlayerData();
		}*/

		if (PhotonNetwork.IsMasterClient)
			PhotonNetwork.LoadLevel(LevelName);
	}



	void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
	{
		// 메인 씬
		if (loadedScene.name == "01. Main_Scene")
		{
			SoundManager.Instance.PlayBGM("Main", 1.0f, true);
		}

		if (!PhotonNetwork.IsConnected) return;

		CurrentEnemyCountInScene = 0;
		bIsLevelCleared = false;
		bool bIsEnemySpawnableLevel = false;
		PlayerDeathCount = 0;

		CurrentSceneName = SceneManager.GetActiveScene().name;

		// 맵에서 스폰 볼륨 찾아 초기화하기
		FindAllSpawnVolumes();

		StopCoroutine("SpawnEnemiesCoroutine");
		StopCoroutine("IncrementLevelByTimeCoroutine");

		// 테스트 씬
		if (loadedScene.name == "2. SS_TestScene1")
		{
			bIsEnemySpawnableLevel = true;

			SpawnPlayerController();

			SpawnItemBoxes();

			SoundManager.Instance.PlayBGM("TestStage", 1.0f, true);
		}

		// 초원 씬
		if (loadedScene.name == "GameScene_GrassLand")
		{
			bIsEnemySpawnableLevel = true;

			SpawnPlayerController();

			SpawnItemBoxes();

			SoundManager.Instance.PlayBGM("GrassLand", 1.0f, true);
		}

		// 설원 씬
		if (loadedScene.name == "GameScene_SnowLand")
		{
			bIsEnemySpawnableLevel = true;

			SpawnPlayerController();

			SpawnItemBoxes();

			SoundManager.Instance.PlayBGM("SnowLand", 1.0f, true);
		}

		// 사막 씬
		if (loadedScene.name == "GameScene_DesertLand")
		{
			bIsEnemySpawnableLevel = true;

			SpawnPlayerController();

			SpawnItemBoxes();

			SoundManager.Instance.PlayBGM("DesertLand", 1.0f, true);
		}



		if (bIsEnemySpawnableLevel && PhotonNetwork.IsMasterClient)
		{
			// 해당 씬의 최대 몬스터 수 및 보스 몬스터 스폰을 위한 처치 수 대입
			foreach (MonsterSpawnInfo Info in MonsterInfoList)
			{
				if (Info.SpawnScene == CurrentSceneName)
				{
					bBossSpawned = false;
					MaxEnemyCountInScene = Info.MaxMonsterCountPerPlayer * PhotonNetwork.PlayerList.Length;
					EnemyDeathCountToSpawnBossMonster = Info.EnemyDeathCountToSpawnBossMonster * PhotonNetwork.PlayerList.Length;
					CurrentSceneBossName = Info.BossPrefab.name;

					break;
				}
			}

			StartCoroutine(SpawnEnemiesCoroutine());
		}

		if (bIsEnemySpawnableLevel && PhotonNetwork.IsMasterClient)
			StartCoroutine(IncrementLevelByTimeCoroutine());
	}
	#endregion



	/// <summary>
	/// 시간이 지날수록 레벨 올라가는 코루틴
	/// </summary>
	IEnumerator IncrementLevelByTimeCoroutine()
	{
		while (!bIsLevelCleared)
		{
			yield return new WaitForSeconds(150.0f);

			CurrentLevel++;
			photonView.RPC("ApplyLevelToAll", RpcTarget.All, CurrentLevel);
		}
	}



	[PunRPC]
	void ApplyLevelToAll(int NewLevel)
	{
		CurrentLevel = NewLevel;
	}



	#region 플레이어 데이터 관련 기능
	// UI에서 먼저 호출
	public void ClearPlayerData()
	{
		PlayerDataList.Clear();
	}



	public void AddPlayerData(PlayerData Data)
	{
		PlayerDataList.Add(Data);
	}



	/// <summary>
	/// 저장된 플레이어 데이터가 유효한지 검사합니다.
	/// </summary>
	/// <returns>유효하다면 true 반환</returns>
	public bool IsValidPlayerData(int PlayerID)
	{
		foreach (PlayerData data in PlayerDataList)
		{
			if (data.PlayerID == PlayerID) return true;
		}

		return false;
	}



	/// <summary>
	/// 저장된 플레이어 데이터를 가져옵니다.
	/// </summary>
	/// <returns>플레이어 이름의 플레이어 데이터를 가져옵니다. 없는 경우 오류 결과를 가져옵니다.</returns>
	public PlayerData GetPlayerData(int PlayerID)
	{
		foreach (PlayerData data in PlayerDataList)
		{
			if (data.PlayerID == PlayerID) return data;
		}

		PlayerData DataError;
		DataError.PlayerID = -1;
		DataError.CharacterName = "ArcherCharacter";
		DataError.ActiveItemName = "";
		DataError.StatusItemNameList = new List<ItemAmountInfo>();
		DataError.bDoAltAttack = false;
		DataError.Skill1Name = "";
		DataError.Skill2Name = "";
		DataError.Skill3Name = "";

		return DataError;
	}



	/// <summary>
	/// 캐릭터 선택창 UI에서 호출해주기...
	/// 플레이어 데이터를 저장해줍니다.
	/// 호출 전 ClearPlayerData()를 먼저 호출!!
	/// </summary>
	public void SetCharacterSettings(string NetworkPlayerName, int PlayerID, string CharacterName, bool DoAltAtk, string Skill1Name, string Skill2Name, string Skill3Name)
	{
		// 사실 여기에서 NetworkPlayerName은 쓸모 없는 파라미터가 되어버림..

		PlayerData MyData;
		MyData.PlayerID = PlayerID;
		MyData.CharacterName = CharacterName;
		MyData.ActiveItemName = "";
		MyData.StatusItemNameList = new List<ItemAmountInfo>();
		MyData.bDoAltAttack = DoAltAtk;
		MyData.Skill1Name = Skill1Name;
		MyData.Skill2Name = Skill2Name;
		MyData.Skill3Name = Skill3Name;

		AddPlayerData(MyData);
	}
	#endregion
}
