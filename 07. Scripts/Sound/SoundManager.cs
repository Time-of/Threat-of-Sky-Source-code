using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SoundFramework;



public enum ESoundGroup : int
{
	BGM, SFX
}



/**
 * 작성자: 20181220 이성수
 * 사운드 매니저
 * 게임 내의 모든 사운드들을 관리합니다.
 * 3D 사운드, 2D 사운드 모두 사용 가능
 * 3D 사운드가 필요하다면 SpawnSoundAtLocation 호출
 * 2D(UI) 사운드가 필요하다면 SpawnSound2D 호출
 * 사운드 매니저에 등록된 사운드라면, 사운드 이름으로 호출 가능
 * 사운드 매니저에 등록되지 않은 사운드라면, AudioClip 타입으로 호출 가능
 */
public class SoundManager : MonoBehaviour
{
	private static SoundManager instance;

	public static SoundManager Instance { get => instance; }

	private AudioSource AudioComponent;



	[Header("BGM Storage")]

	[SerializeField]
	private List<AudioClip> BgmList = new List<AudioClip>();

	private Dictionary<string, AudioClip> BgmDictionary = new Dictionary<string, AudioClip>();

	[SerializeField, ReadOnlyProperty]
	private string BgmNowPlaying = "";



	[Header("Sound Storage")]

	[SerializeField]
	private List<AudioClip> SoundList = new List<AudioClip>();

	private Dictionary<string, AudioClip> SoundDictionary = new Dictionary<string, AudioClip>();



	[Header("Sound Pool")]

	[SerializeField]
	private SoundObject DefaultSoundObject;

	[SerializeField, ReadOnlyProperty]
	private Queue<SoundObject> SoundPool = new Queue<SoundObject>();

	[SerializeField]
	private int PoolingAmount = 20;



	void Awake()
	{
		if (instance == null)
		{
			DontDestroyOnLoad(gameObject);
			instance = this;
		}
		else Destroy(gameObject);

		AudioComponent = GetComponent<AudioSource>();

		transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

		Initialize();
		CreatePoolingObjects();
	}



	#region 초기화
	void Initialize()
	{
		foreach (AudioClip Clip in BgmList)
		{
			BgmDictionary.Add(Clip.name, Clip);
		}

		foreach (AudioClip Clip in SoundList)
		{
			SoundDictionary.Add(Clip.name, Clip);
		}
	}



	void CreatePoolingObjects()
	{
		if (DefaultSoundObject == null) Debug.LogError("<color=red>기본 사운드 오브젝트 없음!!</color>");

		for (int i = 0; i < PoolingAmount; i++)
		{
			SoundObject SoundInstance = Instantiate(DefaultSoundObject, transform);
			SoundInstance.gameObject.SetActive(false);
			SoundPool.Enqueue(SoundInstance);
		}
	}
	#endregion



	#region 사용자를 위한 기능
	/// <summary>
	/// 위치에 3D 사운드를 클립으로 재생합니다.
	/// </summary>
	/// <param name="ClipToPlay"> 재생할 AudioClip입니다.</param>
	/// <param name="Location"> 재생할 위치입니다.</param>
	/// <param name="SoundGroup"> 재생할 사운드의 분류 그룹입니다.</param>
	/// <param name="Volume"> 재생할 사운드의 볼륨입니다.</param>
	/// <param name="PitchRandomize"> 재생할 사운드의 Pitch 랜덤 조정 값입니다. 1에서 +-로 사용됩니다.</param>
	/// <param name="RolloffMode"> 롤오프 모드를 선택합니다. 기본은 Logarithmic 롤오프입니다.</param>
	public void SpawnSoundAtLocation(AudioClip ClipToPlay, Vector3 Location, ESoundGroup SoundGroup, float Volume = 1.0f, float PitchRandomize = 0.0f, AudioRolloffMode RolloffMode = AudioRolloffMode.Logarithmic)
	{
		if (ClipToPlay == null)
		{
			Debug.LogWarning("<color=yellow>재생할 사운드가 유효하지 않습니다!</color>");

			return;
		}

		SoundObject PooledSound = GetSound();

		PooledSound.gameObject.SetActive(true);

		PooledSound.transform.position = Location;

		

		PooledSound.PlaySound(ClipToPlay, SoundGroup, Volume, PitchRandomize, false, RolloffMode);
	}



	/// <summary>
	/// 위치에 3D 사운드를 이름으로 재생합니다.
	/// 사운드 매니저에 등록된 사운드만 가능합니다.
	/// </summary>
	/// <param name="ClipName"> 재생할 사운드의 이름입니다.</param>
	/// <param name="Location"> 재생할 위치입니다.</param>
	/// <param name="SoundGroup"> 재생할 사운드의 분류 그룹입니다.</param>
	/// <param name="Volume"> 재생할 사운드의 볼륨입니다.</param>
	/// <param name="PitchRandomize"> 재생할 사운드의 Pitch 랜덤 조정 값입니다. 1에서 +-로 사용됩니다.</param>
	/// <param name="RolloffMode"> 롤오프 모드를 선택합니다. 기본은 Logarithmic 롤오프입니다.</param>
	public void SpawnSoundAtLocation(string ClipName, Vector3 Location, ESoundGroup SoundGroup, float Volume = 1.0f, float PitchRandomize = 0.0f, AudioRolloffMode RolloffMode = AudioRolloffMode.Logarithmic)
	{
		AudioClip ClipToPlay = GetSoundClipByName(ClipName);

		if (ClipToPlay == null)
		{
			Debug.LogWarning("<color=yellow>재생할 사운드가 사운드 매니저에 등록되어 있지 않습니다!</color>");

			return;
		}

		SoundObject PooledSound = GetSound();

		PooledSound.gameObject.SetActive(true);

		PooledSound.transform.position = Location;

		

		PooledSound.PlaySound(ClipToPlay, SoundGroup, Volume, PitchRandomize, false, RolloffMode);
	}



	/// <summary>
	/// 루프 기능이 있는 사운드를 위치에 재생합니다.
	/// 자동으로 사운드 풀로 반환하지 않으므로, 반드시 직접 ReturnToPool()을 사용해 반환해야 합니다.
	/// </summary>
	/// <param name="ClipToPlay"> 재생할 사운드입니다.</param>
	/// <param name="Location"> 재생할 위치입니다.</param>
	/// <param name="Volume"> 볼륨의 크기입니다.</param>
	/// <returns>해당 사운드를 재생하는 사운드 오브젝트를 반환합니다.</returns>
	public SoundObject Spawn3DLoopSound(AudioClip ClipToPlay, Vector3 Location, float Volume = 1.0f)
	{
		if (ClipToPlay == null)
		{
			Debug.LogWarning("<color=yellow>재생할 사운드가 유효하지 않습니다!</color>");

			return null;
		}

		SoundObject PooledSound = GetSound();

		PooledSound.gameObject.SetActive(true);

		PooledSound.transform.position = Location;



		PooledSound.PlayLoopSound(ClipToPlay, ESoundGroup.SFX, Volume, 0.0f, false);

		return PooledSound;
	}



	/// <summary>
	/// 어디서든 들을 수 있는 사운드를 재생합니다.
	/// </summary>
	/// <param name="ClipToPlay"> 재생할 AudioClip입니다.</param>
	/// <param name="SoundGroup"> 재생할 사운드의 분류 그룹입니다.</param>
	/// <param name="Volume"> 재생할 사운드의 볼륨입니다.</param>
	/// <param name="PitchRandomize"> 재생할 사운드의 Pitch 랜덤 조정 값입니다. 1에서 +-로 사용됩니다.</param>
	public void SpawnSound2D(AudioClip ClipToPlay, ESoundGroup SoundGroup, float Volume = 1.0f, float PitchRandomize = 0.0f)
	{
		if (ClipToPlay == null)
		{
			Debug.LogWarning("<color=yellow>재생할 사운드가 유효하지 않습니다!</color>");

			return;
		}

		SoundObject PooledSound = GetSound();

		PooledSound.gameObject.SetActive(true);

		PooledSound.PlaySound(ClipToPlay, SoundGroup, Volume, PitchRandomize, true, AudioRolloffMode.Logarithmic);
	}



	/// <summary>
	/// 어디서든 들을 수 있는 사운드를 이름으로 재생합니다.
	/// 사운드 매니저에 등록된 사운드만 가능합니다.
	/// </summary>
	/// <param name="ClipName"> 재생할 사운드의 이름입니다.</param>
	/// <param name="SoundGroup"> 재생할 사운드의 분류 그룹입니다.</param>
	/// <param name="Volume"> 재생할 사운드의 볼륨입니다.</param>
	/// <param name="PitchRandomize"> 재생할 사운드의 Pitch 랜덤 조정 값입니다. 1에서 +-로 사용됩니다.</param>
	public void SpawnSound2D(string ClipName, ESoundGroup SoundGroup, float Volume = 1.0f, float PitchRandomize = 0.0f)
	{
		AudioClip ClipToPlay = GetSoundClipByName(ClipName);

		if (ClipToPlay == null)
		{
			Debug.LogWarning("<color=yellow>재생할 사운드가 사운드 매니저에 등록되어 있지 않습니다!</color>");

			return;
		}

		SoundObject PooledSound = GetSound();

		PooledSound.gameObject.SetActive(true);

		PooledSound.PlaySound(ClipToPlay, SoundGroup, Volume, PitchRandomize, true, AudioRolloffMode.Logarithmic);
	}



	/// <summary>
	/// 배경음을 재생합니다.
	/// </summary>
	/// <param name="BgmName"> BgmList에 등록된 배경음의 이름입니다.</param>
	/// <param name="Volume"> 볼륨입니다.</param>
	/// <param name="Loop"> 반복 여부입니다.</param>
	public void PlayBGM(string BgmName, float Volume = 1.0f, bool Loop = true)
	{
		AudioClip Clip = GetBgmClipByName(BgmName);

		if (Clip == null)
		{
			Debug.LogWarning("<color=yellow>재생할 배경음이 사운드 매니저에 등록되어 있지 않습니다!</color>");

			return;
		}

		AudioComponent.Stop();

		BgmNowPlaying = BgmName;
		AudioComponent.clip = Clip;
		AudioComponent.volume = Volume;
		AudioComponent.loop = Loop;
		AudioComponent.Play();
	}



	public string GetBgmNowPlayingName()
	{
		return BgmNowPlaying;
	}
	#endregion



	#region 사운드 풀링
	SoundObject GetSound()
	{
		if (SoundPool.Count > 0)
		{
			return SoundPool.Dequeue();
		}
		else
		{
			SoundObject SoundInstance = Instantiate(DefaultSoundObject, transform);

			Debug.Log("<color=yellow>사운드 풀이 부족하여 새로 사운드를 생성했습니다.</color> : " + SoundInstance);

			return SoundInstance;
		}
	}



	public void ReturnToPool(SoundObject Sound)
	{
		Sound.StopSound();

		Sound.gameObject.SetActive(false);

		Sound.transform.position = Vector3.zero;

		SoundPool.Enqueue(Sound);
	}
	#endregion



	#region 사운드 탐색
	AudioClip GetBgmClipByName(string ClipName)
	{
		return BgmDictionary[ClipName] ? BgmDictionary[ClipName] : null;
	}



	AudioClip GetSoundClipByName(string ClipName)
	{
		return SoundDictionary[ClipName] ? SoundDictionary[ClipName] : null;
	}
	#endregion
}
