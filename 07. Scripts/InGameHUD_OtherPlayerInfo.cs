using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using CharacterGameplay;



/**
 * 작성자: 20181220 이성수
 * 인게임 HUD에서 다른 플레이어의 정보를 가지고 있는 클래스입니다.
 */
public class InGameHUD_OtherPlayerInfo : MonoBehaviour
{
	[ReadOnlyProperty]
	public int PlayerNumber = -1;

	[SerializeField, ReadOnlyProperty]
	private string PlayerName = "";

	[ReadOnlyProperty]
	public float HealthPercent = 1.0f;

	[ReadOnlyProperty]
	public List<Sprite> ItemImageList = new List<Sprite>();

	private int ImageListCount = 0;

	[SerializeField]
	private Image HealthBarImage;

	[SerializeField]
	private Text PlayerNameText;

	[SerializeField]
	private Text TabMenuPlayerNameText;

	[SerializeField]
	private GameObject ScrollbarContent;



	[Header("프리팹")]

	[SerializeField]
	private Image ItemIconImagePrefab;



	public bool IsValidInfo() { return PlayerNumber != -1; }



	public void UpdateHealthBarFillAmount(float NewPercent)
	{
		HealthPercent = NewPercent;
		HealthBarImage.fillAmount = HealthPercent;
	}



	public void UpdatePlayerNameText(string NewName, bool AlsoUpdateTabMenu = true)
	{
		PlayerName = NewName;
		PlayerNameText.text = PlayerName;

		if (AlsoUpdateTabMenu) UpdateTabMenuPlayerNameText();
	}



	void UpdateTabMenuPlayerNameText()
	{
		TabMenuPlayerNameText.text = PlayerName;
	}



	public void AddItemImage(string ItemName)
	{
		ItemImageList.Add(CharacterGameplayManager.Instance.StatusItemDictionary[ItemName].GetItemSprite);
		ImageListCount++;

		Image ImageInstance = Instantiate(ItemIconImagePrefab, ScrollbarContent.transform);

		ImageInstance.transform.localPosition = new Vector3(64 * ImageListCount, -64, 0);

		ImageInstance.sprite = CharacterGameplayManager.Instance.StatusItemDictionary[ItemName].GetItemSprite;
	}



	public void DisableHud()
	{
		HealthBarImage.enabled = false;
		PlayerNameText.enabled = false;
	}
}
