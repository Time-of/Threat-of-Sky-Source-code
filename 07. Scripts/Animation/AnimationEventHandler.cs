using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



/**
 * 작성자: 20181220 이성수
 * 애니메이션 이벤트를 관리해주는 StateMachineBehaviour 클래스입니다.
 * AAnimationEventStateBase로부터 파생된 이벤트 스테이트 객체의 EventEnd 동작을 돕습니다.
 * 즉, EventEnd가 발생하지 않을 확률을 줄여줍니다.
 */
public class AnimationEventHandler : StateMachineBehaviour
{
	/// <summary>
	/// 이 이벤트 핸들러에서 관리하는 이벤트들의 이름 리스트입니다.
	/// 이 리스트에 적힌 이벤트만 관리합니다.
	/// </summary>
	[SerializeField]
	private List<string> ManagedEventNames = new List<string>();

	public List<string> ManagedEventNamesList { get => ManagedEventNames; }

	public event UnityAction<AnimatorStateInfo> OnStateExitHandler;



	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		OnStateExitHandler?.Invoke(stateInfo);
	}
}
