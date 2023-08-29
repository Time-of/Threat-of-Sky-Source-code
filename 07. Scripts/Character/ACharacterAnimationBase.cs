using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CharacterGameplay;



[System.Serializable]
public class AnimEventStateInfo
{
	public string EventName;

	public AAnimationEventStateBase AnimEventStatePrefab;
}



/**
 * 작성자: 20181220 이성수
 * 캐릭터의 애니메이션을 담당해줄 베이스 추상 클래스 스크립트입니다.
 * 파생으로 확장하여 쓰면 됩니다.
 */
public abstract class ACharacterAnimationBase : MonoBehaviour
{
	protected ACharacterBase OwnerCharacter;

	protected Animator Anim;

	public Animator GetAnimator() { return Anim; }



	[Header("Animation Event State 목록")]

	[SerializeField, Tooltip("이 캐릭터가 가지고 있는 AnimEventState 리스트입니다.")]
	protected List<AnimEventStateInfo> AnimEventStateList = new List<AnimEventStateInfo>();

	protected Dictionary<string, AAnimationEventStateBase> AnimEventStateInstanceDictionary = new Dictionary<string, AAnimationEventStateBase>();



	[Header("발소리")]

	[SerializeField, ReadOnlyProperty]
	protected Transform LeftFootTransform;

	[SerializeField, ReadOnlyProperty]
	protected Transform RightFootTransform;

	[SerializeField, ReadOnlyProperty]
	private bool bCanStepLeftFoot = true;

	[SerializeField, ReadOnlyProperty]
	private bool bCanStepRightFoot = true;

	[SerializeField]
	private float FootstepVolume = 0.5f;



	protected virtual void Awake()
	{
		OwnerCharacter = GetComponentInParent<ACharacterBase>();
		Anim = GetComponent<Animator>();

		LeftFootTransform = Anim.GetBoneTransform(HumanBodyBones.LeftFoot);
		RightFootTransform = Anim.GetBoneTransform(HumanBodyBones.RightFoot);

		InitializeAnimEventStates();
	}



	protected virtual void Update()
	{
		if (OwnerCharacter == null) return;

		Vector3 VelocityXZ = OwnerCharacter.GetMovementComponent().VelocityXZ;

		Anim.SetFloat("SpeedSqr", VelocityXZ.sqrMagnitude, 10.0f * Time.deltaTime, Time.deltaTime);
		Anim.SetBool("IsInAir", OwnerCharacter.GetMovementComponent().bIsFalling);

		UpdateDirection(VelocityXZ);
	}



	/// <summary>
	/// 발소리 재생
	/// </summary>
	/// <param name="Direction"> -1: 왼쪽, 1: 오른쪽</param>
	protected void PlayFootstepSound(int Direction)
	{
		switch (Direction)
		{
			case -1:
				if (!bCanStepLeftFoot) return;

				SoundManager.Instance.SpawnSoundAtLocation(CharacterGameplayHelper.GetRandomFootstepSound(),
					LeftFootTransform.position,
					ESoundGroup.SFX, FootstepVolume, 0.05f);

				bCanStepLeftFoot = false;
				bCanStepRightFoot = true;

				break;
			case 1:
				if (!bCanStepRightFoot) return;

				SoundManager.Instance.SpawnSoundAtLocation(CharacterGameplayHelper.GetRandomFootstepSound(),
					RightFootTransform.position,
					ESoundGroup.SFX, FootstepVolume, 0.05f);

				bCanStepLeftFoot = true;
				bCanStepRightFoot = false;

				break;
			default:
				break;
		}
	}




	/// <summary>
	/// 속도와 회전값으로 캐릭터가 가는 방향을 계산합니다.
	/// </summary>
	/// <param name="Velocity"> 속도 벡터입니다.</param>
	/// <param name="CurrentRotation"> 현재 회전 값입니다.</param>
	/// <returns>방향에 따라서 -180 ~ 180이 됩니다.</returns>
	protected float CalculateDirection(Vector3 Velocity, Quaternion CurrentRotation)
	{
		if (Velocity == Vector3.zero) return 0.0f;

		float SideCheck = Vector3.Dot(Vector3.up, Vector3.Cross(transform.forward, Velocity));

		return Quaternion.Angle(Quaternion.LookRotation(Velocity, Vector3.up), CurrentRotation) * ((SideCheck >= 0.0f) ? 1 : -1);
	}



	#region AnimationEventState 관리
	void InitializeAnimEventStates()
	{
		// 딕셔너리에 인스턴싱한 결과를 추가
		foreach (AnimEventStateInfo info in AnimEventStateList)
		{
			AAnimationEventStateBase AnimEventStateInstance = Instantiate(info.AnimEventStatePrefab, transform);
			AnimEventStateInstanceDictionary.Add(info.EventName, AnimEventStateInstance);
		}



		foreach (AnimationEventHandler Handler in Anim.GetBehaviours<AnimationEventHandler>())
		{
			foreach (string EventName in Handler.ManagedEventNamesList)
			{
				if (AnimEventStateInstanceDictionary.TryGetValue(EventName, out AAnimationEventStateBase OutAnimEventState))
				{
					Handler.OnStateExitHandler +=
					_ => { OutAnimEventState.CallEventEnd(OwnerCharacter.gameObject); };
				}
			}
		}
	}



	public void BeginAnimationEventState(string EventStateName)
	{
		if (AnimEventStateInstanceDictionary.TryGetValue(EventStateName, out AAnimationEventStateBase OutAnimEventState))
		{
			OutAnimEventState.CallEventBegin(OwnerCharacter.gameObject);
		}
	}



	public void EndAnimationEventState(string EventStateName)
	{
		if (AnimEventStateInstanceDictionary.TryGetValue(EventStateName, out AAnimationEventStateBase OutAnimEventState))
		{
			OutAnimEventState.CallEventEnd(OwnerCharacter.gameObject);
		}
	}
	#endregion



	protected void OnAnimatorMove()
	{
		OwnerCharacter.RigidBody.position += Anim.deltaPosition;
		
		//OwnerCharacter.RigidBody.rotation = Anim.rootRotation;
	}



	#region 애니메이션 이벤트
	public void Event_Jump()
	{
		OwnerCharacter.GetMovementComponent().bPressedJump = true;
	}



	public void Event_SpawnSoundAtMyPosition(string SoundName)
	{
		SoundManager.Instance.SpawnSoundAtLocation(SoundName, transform.position, ESoundGroup.SFX, 1.0f, 0.05f);
	}



	public void Event_WeakCameraShake(float Duration)
	{
		OwnerCharacter.GetPlayerCameraComponent().PlayCameraShake(3.0f, Duration);
	}



	public void Event_StrongCameraShake(float Duration)
	{
		OwnerCharacter.GetPlayerCameraComponent().PlayCameraShake(25.0f, Duration);
	}
	#endregion



	#region 연산
	protected void UpdateDirection(Vector3 VelocityXZ)
	{
		Quaternion YawRotation = transform.rotation;
		YawRotation.x = 0;
		YawRotation.z = 0;

		Anim.SetFloat("Direction", CalculateDirection(VelocityXZ, YawRotation));
	}
	#endregion
}
