using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace DamageFramework
{
	/**
	 * 작성자: 20181220 이성수
	 * 대상에게 피해 입히기를 쉽게 사용할 수 있도록 만들어 놓은 클래스입니다.
	 * TakeDamage 를 직접 호출하기보단 DamageHelper.ApplyDamage 등의 기능을 사용하는 것을 추천합니다.
	 * 사용 전, using 키워드로 DamageFramework 네임스페이스를 포함해야 합니다.
	 */
	public abstract class DamageHelper : MonoBehaviour
	{
		/// <summary>
		/// IDamageable 인터페이스를 구현하는 대상에게 피해를 입힙니다. (대상의 TakeDamage 호출)
		/// </summary>
		/// <param name="DamageTarget"> 피해를 입힐 대상 게임오브젝트입니다.</param>
		/// <param name="DamageAmount"> 피해를 입힐 수치, 즉 피해량입니다.</param>
		/// <param name="DamageCauser"> 피해를 가한 주체입니다. 투사체에 의한 공격이라면, 투사체가 DamageCauser가 됩니다.</param>
		/// <param name="DamageTypePrefab"> 피해를 가하는 유형을 정의한, 인스턴싱되지 않은 프리팹입니다. 없다면 null 을 사용하면 됩니다.</param>
		/// <param name="DamageTypeDamageMult"> 피해 유형의 피해 배수입니다.</param>
		/// <returns>대상이 IDamageable을 구현한다면 true 를 반환합니다.</returns>
		public static bool ApplyDamage(GameObject DamageTarget, float DamageAmount, GameObject DamageCauser = null, ADamageType DamageTypePrefab = null, float DamageTypeDamageMult = 1.0f)
		{
			IDamageable DamageableObject = DamageTarget.GetComponent<IDamageable>();

			if (DamageableObject == null) return false;

			DamageableObject.TakeDamage(DamageAmount,
					DamageCauser?.transform.position ?? DamageTarget.transform.position,
					DamageTypePrefab, DamageTypeDamageMult);

			return true;
		}



		/// <summary>
		/// 범위 내의 IDamageable 인터페이스를 구현하는 대상들에게 피해를 입힙니다.
		/// </summary>
		/// <param name="DamagePosition"> 범위 피해를 입힐 장소입니다.</param>
		/// <param name="DamageRadius"> 범위 피해를 입힐 구의 반지름입니다.</param>
		/// <param name="DamageAmount"> 범위 피해를 입힐 수치, 즉 피해량입니다.</param>
		/// <param name="TargetLayer"> 범위 피해의 대상을 검출할 레이어입니다.</param>
		/// <param name="ApplyFullDamage"> true 인 경우, 범위 내 대상에게 모두 동일한 피해를 입힙니다. false 인 경우, 멀리 있는 대상에게 적은 피해를 줍니다.</param>
		/// <param name="DamageCauser"> 피해를 가한 주체입니다. 투사체에 의한 공격이라면, 투사체가 DamageCauser가 됩니다.</param>
		/// <param name="DamageTypePrefab"> 피해를 가하는 유형을 정의한 프리팹 원본(인스턴싱X)입니다. 없다면 null 을 사용하면 됩니다.</param>
		/// <param name="DamageTypeDamageMult"> 피해 유형의 피해 배수입니다.</param>
		/// <returns>입힌 피해 리스트와 적중 대상의 위치 리스트를 반환합니다.</returns>
		public static (List<float>, List<Vector3>) ApplyRadialDamage(Vector3 DamagePosition, float DamageRadius, float DamageAmount, string TargetLayer = "Enemy", bool ApplyFullDamage = false, GameObject DamageCauser = null, ADamageType DamageTypePrefab = null, float DamageTypeDamageMult = 1.0f)
		{
			List<float> DamageList = new List<float>();
			List<Vector3> PositionList = new List<Vector3>();

			foreach (Collider ColliderInRange in Physics.OverlapSphere(DamagePosition, DamageRadius, 1 << LayerMask.NameToLayer(TargetLayer)))
			{
				IDamageable Damageable = ColliderInRange.GetComponent<IDamageable>();

				if (Damageable != null)
				{
					float RadialDamageMult = (ApplyFullDamage ? 1 : Mathf.Lerp(1, 0, Vector3.SqrMagnitude(DamagePosition - ColliderInRange.transform.position) / (DamageRadius * DamageRadius)));

					// 멀리 있는 적에게 더 적은 피해 입히기
					Damageable.TakeDamage(DamageAmount * RadialDamageMult,
						DamagePosition,
						DamageTypePrefab, DamageTypeDamageMult);

					DamageList.Add(DamageAmount * RadialDamageMult);
					PositionList.Add(ColliderInRange.transform.position);
				}
			}

			return (DamageList, PositionList);
		}



		/// <summary>
		/// DamageZone을 스폰하고, 바로 시작시킵니다.
		/// </summary>
		/// <param name="DamageZoneToSpawn"> 스폰할 DamageZone 타입 프리팹입니다.</param>
		/// <param name="Location"> 스폰할 위치입니다.</param>
		/// <param name="Rotation"> 스폰할 로테이션입니다.</param>
		/// <param name="DamageMult"> 피해량 배수입니다.</param>
		/// <param name="AtkSpeedMult"> 공격속도 배수입니다.</param>
		/// <param name="OwnerCharacter"> 소유 캐릭터입니다.</param>
		/// <param name="Parent"> null이 아니라면 해당 트랜스폼을 부모로 설정합니다.</param>
		public static void SpawnAutoStartDamageZone(DamageZone DamageZoneToSpawn, Vector3 Location, Quaternion Rotation, float DamageMult, float AtkSpeedMult, GameObject OwnerCharacter, Transform Parent = null)
		{
			DamageZone SpawnedDamageZone = Instantiate(DamageZoneToSpawn, Location, Rotation);

			SpawnedDamageZone.StartZone(DamageMult, AtkSpeedMult, OwnerCharacter);

			if (Parent != null) SpawnedDamageZone.transform.SetParent(Parent, true);
		}



		/// <summary>
		/// DamageZone을 스폰하고, 스폰된 인스턴스를 반환합니다.
		/// StartZone 호출이 필요합니다.
		/// </summary>
		/// <param name="DamageZoneToSpawn"> 스폰할 DamageZone 타입 프리팹입니다.</param>
		/// <param name="Location"> 스폰할 위치입니다.</param>
		/// <param name="Rotation"> 스폰할 로테이션입니다.</param>
		/// <param name="Parent"> null이 아니라면 해당 트랜스폼을 부모로 설정합니다.</param>
		/// <returns>인스턴싱된 DamageZone을 반환합니다.</returns>
		public static DamageZone SpawnDamageZone(DamageZone DamageZoneToSpawn, Vector3 Location, Quaternion Rotation, Transform Parent = null)
		{
			DamageZone SpawnedDamageZone = Instantiate(DamageZoneToSpawn, Location, Rotation);

			if (Parent != null) SpawnedDamageZone.transform.SetParent(Parent, true);

			return SpawnedDamageZone;
		}
	}
}