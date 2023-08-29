using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;



/**
 * 작성자: 20181220 이성수
 * 사운드 오브젝트
 * SoundManager에 의해 관리되므로, 직접 스폰하지 말 것을 권장합니다.
 * SoundManager의 SpawnSoundAtLocation 또는 SpawnSound2D 기능을 사용해 스폰하세요.
 */
namespace SoundFramework
{
	[RequireComponent(typeof(AudioSource))]
	public class SoundObject : MonoBehaviour
	{
		private AudioSource AudioComponent;



		[Header("Channels")]

		[SerializeField]
		private AudioMixerGroup BgmGroup;

		[SerializeField]
		private AudioMixerGroup SfxGroup;



		void Awake()
		{
			AudioComponent = GetComponent<AudioSource>();

			AudioComponent.playOnAwake = false;
		}



		public void PlaySound(AudioClip ClipToPlay, ESoundGroup SoundGroup, float Volume, float PitchRandomize, bool Is2DSound, AudioRolloffMode RolloffMode = AudioRolloffMode.Logarithmic)
		{
			AudioComponent.Stop();

			AudioMixerGroup SoundChannel = null;

			switch (SoundGroup)
			{
				case ESoundGroup.BGM:
					SoundChannel = BgmGroup;
					break;
				default:
				case ESoundGroup.SFX:
					SoundChannel = SfxGroup;
					break;
			}

			AudioComponent.outputAudioMixerGroup = SoundChannel;

			AudioComponent.clip = ClipToPlay;
			AudioComponent.volume = Volume;
			AudioComponent.pitch = (PitchRandomize != 1) ? Random.Range(1 - PitchRandomize, 1 + PitchRandomize) : 1;
			AudioComponent.spatialBlend = Is2DSound ? 0 : 1;
			AudioComponent.rolloffMode = RolloffMode;
			AudioComponent.loop = false;

			AudioComponent.Play();

			Invoke("ReturnToPool", ClipToPlay.length + 0.5f);
		}



		/// <summary>
		/// 사운드를 루프로 재생합니다.
		/// 자동으로 사운드 오브젝트 풀로 돌려보내지 않습니다.
		/// 사용을 완료했다면, 반드시 풀로 돌려보내야합니다.
		/// </summary>
		public void PlayLoopSound(AudioClip ClipToPlay, ESoundGroup SoundGroup, float Volume, float PitchRandomize, bool Is2DSound, AudioRolloffMode RolloffMode = AudioRolloffMode.Logarithmic)
		{
			AudioComponent.Stop();

			AudioMixerGroup SoundChannel = null;

			switch (SoundGroup)
			{
				case ESoundGroup.BGM:
					SoundChannel = BgmGroup;
					break;
				default:
				case ESoundGroup.SFX:
					SoundChannel = SfxGroup;
					break;
			}

			AudioComponent.outputAudioMixerGroup = SoundChannel;

			AudioComponent.clip = ClipToPlay;
			AudioComponent.volume = Volume;
			AudioComponent.pitch = (PitchRandomize != 1) ? Random.Range(1 - PitchRandomize, 1 + PitchRandomize) : 1;
			AudioComponent.spatialBlend = Is2DSound ? 0 : 1;
			AudioComponent.rolloffMode = RolloffMode;
			AudioComponent.loop = true;

			AudioComponent.Play();
		}



		public void StopSound()
		{
			AudioComponent.Stop();
		}



		void ReturnToPool()
		{
			SoundManager.Instance.ReturnToPool(this);
		}
	}
}