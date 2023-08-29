using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace CharacterGameplay
{
	/**
	 * 작성자: 20181220 이성수
	 * 캐릭터의 게임플레이에 있어 필요한 기능을 쉽게 사용하도록 해 주는 클래스입니다.
	 */
	public abstract class CharacterGameplayHelper : MonoBehaviour
	{
		/// <summary>
		/// 캐릭터를 대상 위치로 지속시간 동안 강제 이동합니다.
		/// </summary>
		/// <param name="CharacterObject"> 이동시킬 캐릭터입니다.</param>
		/// <param name="Position"> 이동시킬 목표 위치입니다.</param>
		/// <param name="Duration"> 이동의 지속시간입니다.</param>
		/// <param name="bEaseIn"> Ease In 보간을 사용합니다.</param>
		/// <param name="bEaseOut"> Ease Out 보간을 사용합니다.</param>
		public static void MoveToWorld(ACharacterBase CharacterObject, Vector3 Position, float Duration, bool bEaseIn, bool bEaseOut)
		{
			MoveToWorldAction MoveToWorldPrefab = CharacterGameplayManager.Instance.MoveToWorldPrefab;

			MoveToWorldAction SpawnedAction = Instantiate(MoveToWorldPrefab, Vector3.zero, Quaternion.identity);

			SpawnedAction.StartAction(CharacterObject, Position, Duration, bEaseIn, bEaseOut);
		}



		/// <summary>
		/// 벡터를 회전값으로 변환합니다.
		/// 정규화되지 않은 경우, 정규화하여 사용합니다.
		/// </summary>
		/// <param name="Vector"> 회전시킬 벡터입니다.</param>
		/// <returns>회전된 쿼터니언 값</returns>
		public static Quaternion VectorToRotation(Vector3 Vector)
		{
			if (Vector.sqrMagnitude > 1.0f)
			{
				return Quaternion.LookRotation(Vector.normalized);
			}
			else
			{
				return Quaternion.LookRotation(Vector);
			}
		}



		/// <summary>
		/// 캐릭터의 위치를 설정합니다.
		/// bSweep 옵션을 사용하면 대상 위치 쪽으로 이동한다고 가정할 때 걸리는 경우 걸린 위치로 이동하게 됩니다.
		/// 매우 좁은 거리를 이동하려 한다면, 무시됩니다. (0.001 이하 무시)
		/// </summary>
		/// <param name="CharacterObject"> 이동시킬 캐릭터입니다.</param>
		/// <param name="NewLocation"> 이동시킬 위치입니다.</param>
		/// <param name="bSweep"> 스윕 여부입니다.</param>
		/// <returns>스윕했을 때, 충돌 시 false 를 반환합니다.</returns>
		public static bool SetCharacterLocation(ACharacterBase CharacterObject, Vector3 NewLocation, bool bSweep)
		{
			if (bSweep)
			{
				Vector3 LocationDelta = NewLocation - CharacterObject.RigidBody.position;
				float DistanceDelta = LocationDelta.magnitude;
				Vector3 DirectionToLocation = LocationDelta / DistanceDelta;

				Debug.DrawRay(CharacterObject.RigidBody.position, DirectionToLocation *
					(CharacterObject.CapsuleComp.height + 0.25f), Color.red, 1.0f);

				// 먼저 레이캐스트를 선 수행
				if (Physics.Raycast(CharacterObject.RigidBody.position, DirectionToLocation,
					out RaycastHit RayHit,
					CharacterObject.CapsuleComp.height + 0.25f, -1, QueryTriggerInteraction.Ignore))
				{
					CharacterObject.RigidBody.position += DirectionToLocation *
						(RayHit.distance - 0.5f * CharacterObject.CapsuleComp.height) *
						((RayHit.distance <= 0.001f) ? 0 : 1);
				}
				// 레이캐스트에 걸리지 않은 경우 스윕테스트
				else
				{
					if (CharacterObject.RigidBody.SweepTest(DirectionToLocation, out RaycastHit HitInfo, DistanceDelta, QueryTriggerInteraction.Ignore))
					{
						CharacterObject.RigidBody.position += DirectionToLocation *
							((HitInfo.distance > 0.005f) ? HitInfo.distance : 0.0f);

						return false;
					}
					else
					{
						CharacterObject.RigidBody.position = NewLocation;
					}
				}
			}
			else
			{
				CharacterObject.RigidBody.position = NewLocation;
			}

			return true;
		}



		/// <summary>
		/// Ease In / Out 트랜지션을 함께 사용하는 보간입니다.
		/// </summary>
		public static float LerpEaseInOut(float x, float y, float alpha)
		{
			return Mathf.Lerp(x, y,
				(alpha < 0.5f) ?
				Mathf.Lerp(0.0f, 1.0f, 4 * alpha * alpha) * 0.5f :
				Mathf.Lerp(0.0f, 1.0f, Mathf.Pow(2 * alpha - 1, 0.5f)) * 0.5f + 0.5f);
		}



		/// <summary>
		/// 파티클 시스템을 생성하여 재생하고 파괴합니다.
		/// </summary>
		/// <param name="VfxToSpawn"> 스폰할 파티클 프리팹입니다.</param>
		/// <param name="SpawnPos"> 스폰할 위치입니다.</param>
		/// <param name="SpawnRot"> 스폰할 회전값입니다.</param>
		/// <param name="Parent"> null 이 아닌 경우 부모로 설정할 트랜스폼입니다.</param>
		public static void PlayVfx(ParticleSystem VfxToSpawn, Vector3 SpawnPos, Quaternion SpawnRot, Transform Parent = null)
		{
			if (VfxToSpawn == null)
			{
				Debug.LogWarning("<color=yellow>재생할 파티클 시스템이 없습니다!</color>");
				return;
			}

			ParticleSystem VfxInstance = Instantiate(VfxToSpawn, SpawnPos, SpawnRot);

			if (Parent != null) VfxInstance.gameObject.transform.SetParent(Parent);

			VfxInstance.Play();

			Destroy(VfxInstance.gameObject, VfxInstance.main.duration + 3.0f);
		}



		/// <summary>
		/// 박스 범위 내에서 무작위 지점을 가져옵니다.
		/// </summary>
		/// <param name="HalfExtents"> 박스의 절반 크기입니다.</param>
		/// <param name="OriginPosition"> 박스의 위치입니다.</param>
		/// <param name="BoxRotation"> 박스의 회전값입니다.</param>
		/// <returns>박스 안의 무작위 월드 위치를 반환합니다.</returns>
		public static Vector3 GetRandomPointInBox(Vector3 HalfExtents, Vector3 OriginPosition, Quaternion BoxRotation)
		{
			return BoxRotation * (new Vector3(Random.Range(-HalfExtents.x, HalfExtents.x),
				Random.Range(-HalfExtents.y, HalfExtents.y),
				Random.Range(-HalfExtents.z, HalfExtents.z))) + OriginPosition;

			/**
			 * return BoxRotation * (new Vector3(OriginPosition.x + Random.Range(-HalfExtents.x, HalfExtents.x),
				OriginPosition.y + Random.Range(-HalfExtents.y, HalfExtents.y),
				OriginPosition.z + Random.Range(-HalfExtents.z, HalfExtents.z)));
			 */
		}



		/// <summary>
		/// 리스트에서 무작위로 AudioClip을 가져옵니다.
		/// 리스트의 길이가 0 인 경우 null 을 반환합니다.
		/// </summary>
		/// <returns>리스트 내 무작위 AudioClip 을 반환합니다.</returns>
		public static AudioClip GetRandomSoundInList(List<AudioClip> AudioList)
		{
			int Length = AudioList.Count;
			if (Length <= 0) return null;

			return AudioList[Random.Range(0, Length - 1)];
		}



		/// <summary>
		/// 무작위 발소리를 가져옵니다.
		/// </summary>
		public static AudioClip GetRandomFootstepSound()
		{
			return GetRandomSoundInList(CharacterGameplayManager.Instance.FootstepSoundClipList);
		}



		/// <summary>
		/// 피해량 HUD를 띄웁니다.
		/// </summary>
		/// <param name="NewLocation"> 띄울 위치</param>
		/// <param name="DamageAmount"> 피해량</param>
		/// <param name="TextColor"> 색상</param>
		public static void SpawnDamageFloater(Vector3 NewLocation, float DamageAmount, Color TextColor)
		{
			CharacterGameplayManager.Instance.GetDamageFloaterFromQueue(NewLocation).StartDamageFloating(DamageAmount, TextColor);
		}



		/// <summary>
		/// 피해량 수치에 따라 색상을 자동으로 조정하는 피해량 HUD를 띄웁니다.
		/// </summary>
		/// <param name="NewLocation"> 띄울 위치</param>
		/// <param name="DamageAmount"> 피해량</param>
		public static void SpawnDamageFloaterAutoColor(Vector3 NewLocation, float DamageAmount)
		{
			if (DamageAmount <= 0.0f) return;

			Color TextColor = Color.white;

			if (DamageAmount < 1000.0f) TextColor = Color.white;
			else if (DamageAmount < 3000.0f) TextColor = Color.yellow;
			else if (DamageAmount < 10000.0f) TextColor = new Color(1.0f, 0.68f, 1.0f);
			else TextColor = new Color(1.0f, 0.2f, 0.2f);

			CharacterGameplayManager.Instance.GetDamageFloaterFromQueue(NewLocation).StartDamageFloating(DamageAmount, TextColor);
		}



		public static string GetRandomActiveItemName()
		{
			return GetRandomItemInStringList(CharacterGameplayManager.Instance.ActiveItemNameList);
		}



		public static string GetRandomItemNameByRating(ItemRating Rating)
		{
			switch (Rating)
			{
				case ItemRating.COMMON:
					return GetRandomItemInStringList(CharacterGameplayManager.Instance.StatusItem_COMMON);
				case ItemRating.ADVANCED:
					return GetRandomItemInStringList(CharacterGameplayManager.Instance.StatusItem_ADVANCED);
				case ItemRating.SPECIAL:
					return GetRandomItemInStringList(CharacterGameplayManager.Instance.StatusItem_SPECIAL);
				case ItemRating.UNIQUE:
					return GetRandomItemInStringList(CharacterGameplayManager.Instance.StatusItem_UNIQUE);
				case ItemRating.UNREAL:
					return GetRandomItemInStringList(CharacterGameplayManager.Instance.StatusItem_UNREAL);
				case ItemRating.ULTRON:
					return GetRandomItemInStringList(CharacterGameplayManager.Instance.StatusItem_ULTRON);
				default:
					break;
			}

			return "";
		}



		static string GetRandomItemInStringList(List<string> StringList)
		{
			int ListLength = StringList.Count;

			if (ListLength <= 0) return "";

			return StringList[Random.Range(0, ListLength)];
		}



		public static ParticleSystem GetItemBoxItemSpawnVfx(int Rating)
		{
			return CharacterGameplayManager.Instance.GetItemBoxVfx(Rating);
		}
	}
}