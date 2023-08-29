using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 캐릭터 버프 테스트용 클래스 입니다.
 */
public class TestCharacterBuff : ACharacterBuffBase
{
	protected override void OnBuffAdded()
	{
		Debug.Log("버프 추가됨!");
	}



	protected override void OnBuffRemoved()
	{
		Debug.Log("버프 제거됨!");
	}



	protected override void ApplyTickBuffToTarget()
	{
		Debug.Log("버프 틱!");
	}
}
