using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace CharacterGameplay
{
	/**
	 * 작성자: 20181220 이성수
	 * 특정 위치와 회전으로 이동하는 액션입니다.
	 */
	public sealed class MoveToWorldAction : MonoBehaviour
	{
		bool bStartedAction = false;

		ACharacterBase CharacterToMove;

		float TotalTime = 0.0f;

		float ElapsedTime = 0.0f;

		Vector3 SourcePosition;

		Vector3 TargetPosition;

		bool EaseIn = false;

		bool EaseOut = false;



		/// <summary>
		/// 캐릭터를 대상 위치로 이동시킵니다.
		/// </summary>
		/// <param name="CharacterToMove"> 이동시킬 캐릭터입니다.</param>
		/// <param name="Position"> 이동시킬 위치입니다.</param>
		/// <param name="Duration"> 이동시킬 지속시간입니다.</param>
		public void StartAction(ACharacterBase CharacterToMove, Vector3 Position, float Duration, bool bEaseIn, bool bEaseOut)
		{
			this.CharacterToMove = CharacterToMove;

			SourcePosition = CharacterToMove.RigidBody.position;
			TargetPosition = Position;

			TotalTime = Duration;

			EaseIn = bEaseIn;
			EaseOut = bEaseOut;

			CharacterToMove.GetMovementComponent().bCannotControlled = true;

			bStartedAction = true;
		}



		void FixedUpdate()
		{
			if (!bStartedAction) return;

			if (ElapsedTime < TotalTime)
			{
				ElapsedTime += Time.fixedDeltaTime;

				float DurationPercent = ElapsedTime / TotalTime;
				float TargetAlpha;

				if (EaseIn)
				{
					if (EaseOut)
					{
						TargetAlpha = CharacterGameplayHelper.LerpEaseInOut(0.0f, 1.0f, DurationPercent);
					}
					else
					{
						TargetAlpha = Mathf.Lerp(0.0f, 1.0f, DurationPercent * DurationPercent);
					}
				}
				else
				{
					if (EaseOut)
					{
						TargetAlpha = Mathf.Lerp(0.0f, 1.0f, Mathf.Pow(DurationPercent, 0.5f));
					}
					else
					{
						TargetAlpha = Mathf.Lerp(0.0f, 1.0f, DurationPercent);
					}
				}

				CharacterGameplayHelper.SetCharacterLocation(CharacterToMove, (ElapsedTime >= TotalTime) ?
					TargetPosition : Vector3.Lerp(SourcePosition, TargetPosition, TargetAlpha), true);
			}
			else
			{
				bStartedAction = false;

				CharacterToMove.GetMovementComponent().bCannotControlled = false;

				CharacterToMove.GetMovementComponent().ResetGravity();

				Destroy(this.gameObject, 1.0f);
			}
		}



		/*

		Vector3 TargetVelocity;

		float DistanceToTarget;

		/// <summary>
		/// 캐릭터를 대상 위치로 이동시킵니다.
		/// </summary>
		/// <param name="CharacterToMove"> 이동시킬 캐릭터입니다.</param>
		/// <param name="Position"> 이동시킬 위치입니다.</param>
		/// <param name="Duration"> 이동시킬 지속시간입니다.</param>
		/// <param name="SinTransition"> true 인 경우, Sin 곡선 보간을 사용하여 이동합니다. false 인 경우, 선형으로 이동합니다.</param>
		public void StartAction(ACharacterBase CharacterToMove, Vector3 Position, float Duration, bool SinTransition = false)
		{
			this.CharacterToMove = CharacterToMove;
			SourcePosition = CharacterToMove.RigidBody.position;

			TargetPosition = Position;
			TotalTime = Duration;

			CharacterToMove.GetMovementComponent().bCannotControlled = true;

			TargetVelocity = 1 / TotalTime * (TargetPosition - SourcePosition);

			if (!SinTransition)
				StartCoroutine(LinearMoveToWorldCoroutine());
			else
			{
				DistanceToTarget = TargetVelocity.magnitude;

				bStartedAction = true;
			}
		}



		IEnumerator LinearMoveToWorldCoroutine()
		{
			CharacterToMove.GetMovementComponent().ForceMovementVector = TargetVelocity;

			yield return new WaitForSeconds(TotalTime);

			CharacterToMove.GetMovementComponent().bCannotControlled = false;
			CharacterToMove.GetMovementComponent().ForceMovementVector = Vector3.zero;
			CharacterToMove.GetMovementComponent().ResetGravity();

			Destroy(this.gameObject, 1.0f);
		}



		void FixedUpdate()
		{
			if (!bStartedAction) return;

			if (ElapsedTime < TotalTime)
			{
				ElapsedTime += Time.fixedDeltaTime;

				// -2 파이를 곱해 진폭이 1인 사인 함수를 만들고, 거기에 곱하기
				CharacterToMove.GetMovementComponent().ForceMovementVector =
					(DistanceToTarget * 0.5f * Mathf.Sin(-2 * Mathf.PI * ElapsedTime / TotalTime) + DistanceToTarget) *
					TargetVelocity / DistanceToTarget;
			}
			else
			{
				CharacterToMove.GetMovementComponent().ResetGravity();
				CharacterToMove.GetMovementComponent().bCannotControlled = false;
				CharacterToMove.GetMovementComponent().ForceMovementVector = Vector3.zero;

				bStartedAction = false;

				Destroy(this.gameObject, 1.0f);
			}
		}*/
	}
}