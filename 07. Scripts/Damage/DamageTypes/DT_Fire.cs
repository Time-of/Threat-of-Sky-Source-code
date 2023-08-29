using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DamageFramework;



/**
 * 작성자: 20181220 이성수
 * 피해 타입 ADamageType을 확장하여 화상 피해를 구현했습니다.
 */
public class DT_Fire : ADamageType
{
	protected override void ApplyTickDamageToTarget()
	{
		base.ApplyTickDamageToTarget();

		Debug.Log("<color=red>화상 데미지!!!!</color> : " + TargetObject);
	}
}
