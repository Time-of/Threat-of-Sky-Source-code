using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * AAnimationEventStateBase를 테스트하기 위해 존재하는 클래스
 */
public class TestAnimEventState : AAnimationEventStateBase
{
	public override void EventBegin(GameObject EventCaller)
	{
		Debug.Log("<color=green>TestAnimEventState EventBegin!</color>");
	}

	public override void EventEnd(GameObject EventCaller)
	{
		Debug.Log("<color=green>TestAnimEventState EventEnd!</color>");
	}
}
