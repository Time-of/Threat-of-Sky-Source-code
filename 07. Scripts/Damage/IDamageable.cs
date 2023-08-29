using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DamageFramework;



/**
 * 작성자: 20181220 이성수
 * IDamageable 은 피해를 입을 수 있는 오브젝트를 확장하는 인터페이스입니다.
 * DamageHelper에 의해 호출하는 것을 권장합니다. (DamageHelper.ApplyDamage 등)
 * Example_HowToUse_IDamageable 클래스를 살펴보는 것을 권장합니다. (내가 생각해도 확장하기 참 어려운 인터페이스인듯)
 */
public interface IDamageable
{
	/// <summary>
	/// 피해를 입는 기능입니다.
	/// RPC로 구현하는 것을 추천합니다.
	/// </summary>
	/// <param name="DamageAmount"> 피해량을 의미합니다.</param>
	/// <param name="CauserPosition"> 피해를 입힌 대상의 위치 정보입니다.</param>
	/// <param name="DamageTypePrefab"> 피해 유형 프리팹 오브젝트 정보입니다. null 이 될 수 있습니다.</param>
	/// <param name="DamageTypeDamageMult">피해 유형 프리팹의 피해 배율 정보입니다.</param>
	public void TakeDamage(float DamageAmount, Vector3 CauserPosition, ADamageType DamageTypePrefab, float DamageTypeDamageMult);
}
