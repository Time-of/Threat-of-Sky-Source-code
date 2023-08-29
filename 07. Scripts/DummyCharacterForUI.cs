using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * UI를 위한 더미 캐릭터입니다.
 * SetReady()를 호출하면 레디 애니메이션을 재생할 수 있습니다.
 */
public class DummyCharacterForUI : MonoBehaviour
{
	private Animator AnimComponent = null;



	private void Awake()
	{
		AnimComponent = GetComponent<Animator>();
	}



	public void SetReady()
	{
		AnimComponent.SetTrigger("Ready");
	}
}
