using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



namespace CharacterGameplay
{
	/**
	 * 작성자: 20181220 이성수
	 * 캐릭터 게임플레이를 위해 사용되는 액션 객체들을 관리하는 싱글턴 객체입니다.
	 * 또한 아이템 정보를 저장하고 있습니다.
	 */
	public class CharacterGameplayManager : MonoBehaviour
	{
		public MoveToWorldAction MoveToWorldPrefab;



		public List<AudioClip> FootstepSoundClipList;



		public List<AStatusItemBase> AllStatusItemList;

		public List<AActiveItemBase> AllActiveItemList;

		public Dictionary<string, AStatusItemBase> StatusItemDictionary = new Dictionary<string, AStatusItemBase>();

		[ReadOnlyProperty]
		public List<string> StatusItem_COMMON = new List<string>();

		[ReadOnlyProperty]
		public List<string> StatusItem_ADVANCED = new List<string>();

		[ReadOnlyProperty]
		public List<string> StatusItem_SPECIAL = new List<string>();

		[ReadOnlyProperty]
		public List<string> StatusItem_UNIQUE = new List<string>();

		[ReadOnlyProperty]
		public List<string> StatusItem_UNREAL = new List<string>();

		[ReadOnlyProperty]
		public List<string> StatusItem_ULTRON = new List<string>();

		[ReadOnlyProperty]
		public List<string> ActiveItemNameList = new List<string>();



		public Dictionary<string, AActiveItemBase> ActiveItemDictionary = new Dictionary<string, AActiveItemBase>();



		private Queue<DamageFloater> DamageFloaterPool = new Queue<DamageFloater>();

		[SerializeField]
		private DamageFloater DamageFloaterPrefab;

		[SerializeField]
		private int DefaultDamageFloaterPoolCount = 30;



		[SerializeField]
		private List<ParticleSystem> ItemBoxItemSpawnVfxList = new List<ParticleSystem>();

		public ParticleSystem GetItemBoxVfx(int Index) { return ItemBoxItemSpawnVfxList[Index]; }



		private static CharacterGameplayManager instance;

		public static CharacterGameplayManager Instance { get => instance; }

		public static CharacterGameplayManager TryGetInstance()
		{
			if (instance == null)
			{
				instance = FindObjectOfType<CharacterGameplayManager>();
			}

			return instance;
		}



		void Awake()
		{
			if (instance == null)
			{
				DontDestroyOnLoad(gameObject);
				instance = this;
			}
			else Destroy(gameObject);

			InitializeStatusItemDictionary();
			InitializeActiveItemDictionary();
			InitializeDamageFloaterPool();
		}



		void InitializeStatusItemDictionary()
		{
			foreach (AStatusItemBase Item in AllStatusItemList)
			{
				StatusItemDictionary.Add(Item.GetItemName, Item);

				switch (Item.GetItemRating)
				{
					case ItemRating.COMMON:
						StatusItem_COMMON.Add(Item.GetItemName);
						break;
					case ItemRating.ADVANCED:
						StatusItem_ADVANCED.Add(Item.GetItemName);
						break;
					case ItemRating.SPECIAL:
						StatusItem_SPECIAL.Add(Item.GetItemName);
						break;
					case ItemRating.UNIQUE:
						StatusItem_UNIQUE.Add(Item.GetItemName);
						break;
					case ItemRating.UNREAL:
						StatusItem_UNREAL.Add(Item.GetItemName);
						break;
					case ItemRating.ULTRON:
						StatusItem_ULTRON.Add(Item.GetItemName);
						break;
					default:
						break;
				}
			}
		}



		void InitializeActiveItemDictionary()
		{
			foreach (AActiveItemBase Item in AllActiveItemList)
			{
				ActiveItemDictionary.Add(Item.GetItemName, Item);
				ActiveItemNameList.Add(Item.GetItemName);
			}
		}



		#region 피해량 HUD 관련 기능들
		void InitializeDamageFloaterPool()
		{
			for (int i = 0; i < DefaultDamageFloaterPoolCount; i++)
			{
				DamageFloater DamageFloaterInstance = Instantiate(DamageFloaterPrefab, transform);

				DamageFloaterInstance.gameObject.SetActive(false);

				DamageFloaterPool.Enqueue(DamageFloaterInstance);
			}
		}



		public DamageFloater GetDamageFloaterFromQueue(Vector3 NewLocation)
		{
			if (DamageFloaterPool.TryDequeue(out DamageFloater OutDamageFloater))
			{
				OutDamageFloater.gameObject.transform.position = NewLocation;

				OutDamageFloater.gameObject.SetActive(true);

				return OutDamageFloater;
			}
			else
			{
				DamageFloater DamageFloaterInstance = Instantiate(DamageFloaterPrefab, transform);

				DamageFloaterInstance.gameObject.transform.position = NewLocation;

				return DamageFloaterInstance;
			}
		}



		public void EnqueueDamageFloaterPool(DamageFloater DamageFloaterToReturn)
		{
			DamageFloaterToReturn.StopAllCoroutines();

			DamageFloaterToReturn.gameObject.SetActive(false);

			DamageFloaterPool.Enqueue(DamageFloaterToReturn);
		}
		#endregion
	}
}