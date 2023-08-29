using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace DamageFramework
{
	/**
	 * 작성자: 20181220 이성수
	 * 지속적인 범위 피해 스킬(장판기)을 구현하기 위해 만든 클래스입니다.
	 * BoxBound 내부의 TargetLayer에 해당하는 레이어를 가진 오브젝트들 중 IDamageable을 가진
	  오브젝트에 대해서 TickAction을 실행합니다.
	 * DamageHelper를 통해 사용하는 것을 추천합니다.
	 */
	public class DamageZone : MonoBehaviour
	{
		public delegate void OnHitCallback(float HitDamage, Vector3 HitPosition);

		private OnHitCallback OnHit = null;

		[SerializeField, ReadOnlyProperty]
		GameObject ZoneOwner;



		[Header("필요 변수 설정")]

		[SerializeField]
		protected Vector3 BoxBound;

		[SerializeField]
		private bool bDebugDrawBoxBound = false;

		[SerializeField, Tooltip("이 레이어에 해당하는 IDamageable만 검출합니다.")]
		protected string TargetLayer = "Enemy";

		[SerializeField]
		protected float BaseDamageRate = 1.0f;

		[SerializeField, ReadOnlyProperty]
		protected float DamageMult = 1.0f;

		[SerializeField]
		protected ADamageType DamageType = null;

		protected bool bIsStarted = false;



		[Header("지속성")]

		[SerializeField, Tooltip("true 인 경우 즉시 지속 효과가 발동됩니다. false 인 경우 SustainTick 이후부터 지속 피해를 받습니다.")]
		protected bool bImmediatelyApplyDotEffect = false;

		[SerializeField, Tooltip("범위 피해의 총 지속시간입니다."), Min(0.0f)]
		protected float DotDuration = 3.0f;

		[SerializeField, Tooltip("지속 효과가 얼마나 자주 발생할건지를 결정합니다. 0 인 경우, 즉시 종료됩니다."), Min(0.0f)]
		protected float DotTick = 0.5f;

		[SerializeField, ReadOnlyProperty]
		protected float CurrentDotDuration = 0.0f;

		[SerializeField, ReadOnlyProperty]
		protected float CurrentDotTick = 0.0f;

		[SerializeField, ReadOnlyProperty]
		protected bool bIsEnded = false;



		[Header("사운드")]

		[SerializeField]
		protected AudioClip StartSoundClip;

		[SerializeField]
		protected AudioClip EndSoundClip;

		[SerializeField]
		protected AudioClip TickSoundClip;

		[SerializeField, Tooltip("기본적으로, 적을 맞추는 데 성공했을 경우 호출됩니다.")]
		protected AudioClip TickActionSoundClip;



		#region 재정의 기능
		/// <summary>
		/// 해당 DamageZone이 유효한 경우, 시작 시 호출됩니다.
		/// </summary>
		protected virtual void OnZoneStarted()
		{
			if (StartSoundClip != null)
				SoundManager.Instance.SpawnSoundAtLocation(StartSoundClip, transform.position, ESoundGroup.SFX, 1.0f, 0.05f);
		}



		/// <summary>
		/// 해당 DamageZone이 종료되고, 삭제되기 직전 호출됩니다.
		/// </summary>
		protected virtual void OnZoneEnded()
		{
			if (EndSoundClip != null)
				SoundManager.Instance.SpawnSoundAtLocation(EndSoundClip, transform.position, ESoundGroup.SFX, 1.0f, 0.05f);
		}



		/// <summary>
		/// DotTick 마다 호출됩니다.
		/// </summary>
		protected virtual void Tick()
		{
			if (TickSoundClip != null)
				SoundManager.Instance.SpawnSoundAtLocation(TickSoundClip, transform.position, ESoundGroup.SFX, 1.0f, 0.05f);
		}



		/// <summary>
		/// 재정의 가능한 틱 기능입니다.
		/// BoxBound 내부의 IDamageable 인터페이스를 구현하는 객체들에게 호출됩니다.
		/// </summary>
		/// <param name="DamageableObject"> 피해를 입는 대상 게임오브젝트입니다.</param>
		protected virtual void TickAction(GameObject DamageableObject)
		{
			if (DamageHelper.ApplyDamage(DamageableObject, BaseDamageRate * DamageMult, this.gameObject, DamageType, DamageMult))
			{
				if (TickActionSoundClip != null)
					SoundManager.Instance.SpawnSoundAtLocation(TickActionSoundClip, transform.position, ESoundGroup.SFX, 1.0f, 0.05f);
			}
		}
		#endregion



		#region 사용자 기능
		public void SetupHitCallback(OnHitCallback NewCallback) { OnHit += NewCallback; }



		public void StartZone(float DamageMult, float AtkSpeedMult, GameObject NewZoneOwner)
		{
			ZoneOwner = NewZoneOwner;

			this.DamageMult = DamageMult;

			bIsEnded = DotDuration <= 0.0f || DotTick <= 0.0f;

			CurrentDotDuration = DotDuration / AtkSpeedMult;

			if (bImmediatelyApplyDotEffect) CurrentDotTick = DotTick / AtkSpeedMult;
			else CurrentDotTick = 0.0f;


			if (bIsEnded) Destroy(this.gameObject);
			else bIsStarted = true;

			OnZoneStarted();
		}
		#endregion



		#region 로직
		protected void Update()
		{
			if (!bIsStarted) return;

			if (CurrentDotDuration > 0.0f)
			{
				CurrentDotDuration -= Time.deltaTime;
				CurrentDotTick += Time.deltaTime;

				if (CurrentDotTick >= DotTick)
				{
					Tick();

					CurrentDotTick -= DotTick;

					foreach (Collider col in Physics.OverlapBox(transform.position, 0.5f * BoxBound,
						transform.rotation, 1 << LayerMask.NameToLayer(TargetLayer)))
					{
						IDamageable Target = col.gameObject.GetComponent<IDamageable>();

						if (Target != null && col.gameObject != null)
						{
							if (OnHit != null)
								OnHit(BaseDamageRate * DamageMult, col.transform.position);

							TickAction(col.gameObject);
						}
					}
				}
			}
			else
			{
				CurrentDotDuration = 0.0f;

				bIsEnded = true;

				OnZoneEnded();

				Destroy(this.gameObject);
			}
		}
		#endregion



		#region 디버그
		void OnDrawGizmos()
		{
			if (!bDebugDrawBoxBound) return;

			Gizmos.color = Color.magenta;

			Matrix4x4 TRMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

			Gizmos.matrix = TRMatrix;

			Gizmos.DrawWireCube(Vector3.zero, BoxBound);
		}
		#endregion
	}
}