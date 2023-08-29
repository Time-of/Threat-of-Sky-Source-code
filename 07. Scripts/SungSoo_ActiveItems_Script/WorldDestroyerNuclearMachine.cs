using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using DamageFramework;



/**
 * 작성자: 20181220 이성수
 * 액티브 아이템 '세계 파괴자'의 역할을 보조하는 설치물입니다.
 * 10초 후 핵폭탄을 투하합니다.
 */
public class WorldDestroyerNuclearMachine : MonoBehaviour
{
	[SerializeField]
	private TMP_Text CountdownText;

	[SerializeField]
	private DamageZone DZ_AfterNuclear;

	[SerializeField]
	private ParticleSystem Vfx_NuclearBomb;

	[SerializeField]
	private AudioClip Sound_NuclearBomb;

	[SerializeField]
	private AudioClip Sound_NuclearAfterBomb;

	[SerializeField]
	private List<AudioClip> SoundList_NuclearCountdown;

	private int CountRemaining;

	private WaitForSeconds OneSeconds = new WaitForSeconds(1.0f);

	private float DamageMult;



	public void StartNuclearCount(float DamageMult)
	{
		CountRemaining = 10;

		this.DamageMult = DamageMult;

		StartCoroutine(NuclearCountCoroutine());
	}



	IEnumerator NuclearCountCoroutine()
	{
		while (CountRemaining > 0)
		{
			CountRemaining--;

			CountdownText.text = (CountRemaining + 1).ToString();

			SoundManager.Instance.SpawnSoundAtLocation(SoundList_NuclearCountdown[CountRemaining], transform.position,
				ESoundGroup.SFX, 1.0f, 0.0f, AudioRolloffMode.Linear);

			Debug.Log("<color=red>핵폭탄 투하 준비 남은 시간: </color>" + CountRemaining);

			yield return OneSeconds;
		}

		CountdownText.text = "0";

		SoundManager.Instance.SpawnSoundAtLocation(Sound_NuclearBomb, transform.position,
				ESoundGroup.SFX, 1.0f, 0.0f, AudioRolloffMode.Linear);

		SoundManager.Instance.SpawnSoundAtLocation(Sound_NuclearAfterBomb, transform.position,
				ESoundGroup.SFX, 0.8f, 0.0f, AudioRolloffMode.Linear);

		CharacterGameplay.CharacterGameplayHelper.PlayVfx(Vfx_NuclearBomb, transform.position, Quaternion.identity, null);

		DamageHelper.ApplyRadialDamage(transform.position, 50.0f, 50.0f * DamageMult, "Enemy", false);
		DamageHelper.ApplyRadialDamage(transform.position, 50.0f, 5.0f * DamageMult, "Player", false);

		yield return OneSeconds;

		DamageHelper.SpawnAutoStartDamageZone(DZ_AfterNuclear, transform.position, Quaternion.identity, DamageMult, 1.0f, null);

		yield return OneSeconds;

		Destroy(gameObject);
	}
}
