using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace DamageFramework
{
	/**
	 * 작성자: 20181220 이성수
	 * 피해 프레임워크의 피해 유형을 결정짓는 추상 클래스입니다.
	 * using 키워드로 DamageFramework 네임스페이스를 포함해야 합니다.
	 * 이 클래스를 상속하여 화염 피해, 독성 피해, 출혈 피해, 빙결 피해 등을 정의할 수 있습니다.
	 */
	public abstract class ADamageType : MonoBehaviour
	{
		[Header("피해 타입 속성")]

		[SerializeField, Tooltip("피해 타입의 이름입니다.")]
		protected string DamageName = "DefaultDamageType";

		[SerializeField, Tooltip("기본 피해량 계수입니다.")]
		protected float BaseDamageRate = 1.0f;

		[Tooltip("BaseDamage(기본 피해량 계수)에 곱하는 배율입니다.")]
		protected float DamageMult = 1.0f;

		[Tooltip("피해에 추가적으로 입히는 피해입니다. DamageMult의 영향을 받지 않습니다.")]
		protected float AdditionalDamage = 0.0f;

		[Tooltip("최종 피해량입니다.")]
		protected float FinalDamage = 0.0f;

		[SerializeField, Tooltip("지속 피해인지의 여부입니다.")]
		protected bool bIsDotDamage = false;

		[SerializeField, Tooltip("true 인 경우 즉시 지속 피해를 입습니다. false 인 경우 DotTick 이후부터 지속 피해를 받습니다.")]
		protected bool bImmediatelyApplyDotDamage = false;

		[SerializeField, Tooltip("지속 피해의 총 지속시간입니다."), Min(0.0f)]
		protected float DotDuration = 3.0f;

		[SerializeField, Tooltip("지속 피해가 얼마나 자주 발생할건지를 결정합니다."), Min(0.0f)]
		protected float DotTick = 0.5f;

		[SerializeField, ReadOnlyProperty]
		protected float CurrentDotDuration = 0.0f;

		[SerializeField, ReadOnlyProperty]
		protected float CurrentDotTick = 0.0f;

		[SerializeField, ReadOnlyProperty]
		protected bool bIsEnded = false;



		[Header("피해 타입 대상")]

		[SerializeField, ReadOnlyProperty]
		protected GameObject TargetObject;



		#region 사용자 기능 모음
		/// <summary>
		/// 피해 주기를 시작합니다.
		/// 지속 피해가 아니라면, 즉시 피해를 줍니다.
		/// </summary>
		/// <param name="NewTarget"> 피해의 대상이 될 게임오브젝트입니다. 일반적으로, 이 프리팹을 TakeDamage 등에서 인스턴싱한 대상입니다.</param>
		/// <param name="DamageMult"> 피해 배율입니다. 추가 피해를 더한 후, 이 변수만큼 곱해집니다.</param>
		/// <param name="AdditionalDamage"> 추가 피해량입니다. (합연산)</param>
		public void StartDamage(GameObject NewTarget, float DamageMult = 1.0f, float AdditionalDamage = 0.0f)
		{
			TargetObject = NewTarget;

			this.DamageMult = DamageMult;

			this.AdditionalDamage = AdditionalDamage;

			

			bIsEnded = !bIsDotDamage || DotDuration <= 0.0f || DotTick <= 0.0f;


			if (bIsDotDamage)
			{
				CurrentDotDuration = DotDuration;

				CurrentDotTick = 0.0f;

				if (bImmediatelyApplyDotDamage) ApplyTickDamageToTarget();
			}


			if (bIsEnded)
			{
				ApplyDamageToTarget();

				Destroy(gameObject);
			}
		}
		#endregion



		#region 재정의 기능들
		/// <summary>
		/// 재정의 가능한 즉발 피해 기능입니다. '빙결' 등의 상태이상을 사용하기에 적합합니다.
		/// </summary>
		protected virtual void ApplyDamageToTarget()
		{
			DamageHelper.ApplyDamage(TargetObject, CalculateFinalDamage(), TargetObject, null);
		}



		/// <summary>
		/// 재정의 가능한 틱 피해 기능입니다. '화상', '출혈' 등의 피해를 주기에 적합합니다.
		/// </summary>
		protected virtual void ApplyTickDamageToTarget()
		{
			DamageHelper.ApplyDamage(TargetObject, CalculateFinalDamage(), TargetObject, null);
		}
		#endregion



		#region 작동
		protected void Update()
		{
			if (bIsDotDamage)
			{
				if (CurrentDotDuration > 0.0f)
				{
					CurrentDotDuration -= Time.deltaTime;
					CurrentDotTick += Time.deltaTime;

					if (CurrentDotTick >= DotTick)
					{
						CurrentDotTick -= DotTick;

						ApplyTickDamageToTarget();
					}
				}
				else
				{
					CurrentDotDuration = 0.0f;

					bIsEnded = true;

					Destroy(this.gameObject);
				}
			}
		}



		float CalculateFinalDamage()
		{
			FinalDamage = Mathf.Max(0.0f, BaseDamageRate * DamageMult + AdditionalDamage);

			return FinalDamage;
		}
		#endregion
	}
}