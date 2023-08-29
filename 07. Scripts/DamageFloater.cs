using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;



/**
 * 작성자: 20181220 이성수
 * 피해량을 띄우는 기능입니다.
 */
public class DamageFloater : MonoBehaviour
{
	[SerializeField]
	private TMP_Text DamageTextPrefab;

	private TMP_Text DamageTextInstance;

	private Animator AnimComp;

	bool bStarted = false;



	private void Awake()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}



	void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
	{
		MainCanvasUI mainCanvasUI = FindObjectOfType<MainCanvasUI>();

		if (mainCanvasUI != null && DamageTextInstance == null)
		{
			DamageTextInstance = Instantiate(DamageTextPrefab, mainCanvasUI.transform);

			AnimComp = DamageTextInstance.GetComponent<Animator>();
		}
	}



	public void StartDamageFloating(float DamageAmount, Color DamageTextColor)
	{
		DamageTextInstance.text = Mathf.Ceil(DamageAmount).ToString();
		DamageTextInstance.color = DamageTextColor;

		StartCoroutine(StartDamageFloatingCoroutine());

		AnimComp.SetTrigger("MoveUp");

		bStarted = true;
	}



	private void LateUpdate()
	{
		if (!bStarted) return;

		Camera CurrentActiveCamera = Camera.allCameras[0];

		if (CurrentActiveCamera != null)
			DamageTextInstance.transform.position = CurrentActiveCamera.WorldToScreenPoint(transform.position);
	}



	IEnumerator StartDamageFloatingCoroutine()
	{
		/*float alpha = 1.0f;

		float ElapsedTime = 0.0f;
		float EntirePercent;
		float BlendPercent;

		while (ElapsedTime <= 2.0f)
		{
			//alpha -= Time.deltaTime * 2.0f;

			ElapsedTime += Time.deltaTime;
			EntirePercent = ElapsedTime / 2.0f;

			BlendPercent = Mathf.Lerp(0.0f, 1.0f, EntirePercent * EntirePercent);

			alpha = Mathf.Lerp(1.0f, -0.1f, BlendPercent);

			DamageTextColor.a = alpha;
			DamageText.color = DamageTextColor;

			yield return null;
		}*/

		yield return new WaitForSeconds(2.0f);

		bStarted = false;
		CharacterGameplay.CharacterGameplayManager.Instance.EnqueueDamageFloaterPool(this);
	}
}
