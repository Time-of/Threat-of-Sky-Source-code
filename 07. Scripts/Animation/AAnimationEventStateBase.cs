using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 애니메이션 이벤트를 효율적으로 관리하기 위해 만든 클래스입니다.
 * 파생 클래스에서 EventBegin과 EventEnd를 재정의하여 사용하면 됩니다.
 * CallEventEnd는 OnStateExit에서도 실행됩니다.
 */
public abstract class AAnimationEventStateBase : MonoBehaviour
{
	public bool bIsEnded = false;



	#region 이벤트 호출
	public void CallEventBegin(GameObject EventCaller)
	{
		bIsEnded = false;

		EventBegin(EventCaller);
	}

	public void CallEventEnd(GameObject EventCaller)
	{
		if (!bIsEnded)
		{
			bIsEnded = true;

			EventEnd(EventCaller);
		}
	}
	#endregion



	#region 이벤트 정의
	public virtual void EventBegin(GameObject EventCaller)
	{

	}

	public virtual void EventEnd(GameObject EventCaller)
	{
		
	}
	#endregion
}
