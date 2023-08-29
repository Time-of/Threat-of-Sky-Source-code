using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DamageFramework;



/**
 * 작성자: 20181220 이성수
 * 투사체 기본이 되는 추상 클래스입니다.
 * 투사체를 스폰하고, StartFire()를 호출하세요.
 * OnProjectileHitSuccessed, OnProjectileHitFailed 및 OnProjectileLateEnterTrigger를 재정의하여 적절한 동작을 만들 수 있습니다.
 */
public abstract class AProjectileBase : MonoBehaviour
{
	public delegate void OnHitCallback(float DirectHitDamage, Vector3 HitPosition);

	public delegate void OnSplashHitCallback(float HitDamage, Vector3 HitPosition);

	private OnHitCallback OnHit = null;

	private OnSplashHitCallback OnSplashHit = null;



	[Header("Projectile Property")]

	[SerializeField]
	protected bool bCanApplyDamage = true;

	[SerializeField]
	protected float InitialSpeed;

	[SerializeField, Tooltip("중력을 얼마만큼 받는지 결정하는 변수입니다.")]
	protected float GravityAmount = 1.0f;

	[SerializeField, Tooltip("투사체 직격 피해량 배수입니다.")]
	protected float DamageRate = 1.0f;

	[SerializeField, ReadOnlyProperty]
	protected float FinalDamage = 0.0f;

	[SerializeField, Tooltip("이 레이어에 속한 대상을 공격하려고 시도합니다.")]
	protected string DamageTargetLayer = "Enemy";

	[SerializeField, Tooltip("이 레이어에 속한 대상은 콜리전 이벤트를 일으키지 않습니다.")]
	protected string CollisionExceptionLayer = "Player";

	[SerializeField, Tooltip("이 시간이 지나면 자동으로 소멸합니다. 0인 경우, 자동으로 소멸하지 않습니다."), Min(0.0f)]
	protected float DestroyTimer = 3.0f;

	[SerializeField, Tooltip("피해 타입 프리팹입니다. 비워도 무방합니다.")]
	protected ADamageType DamageTypePrefab = null;

	[ReadOnlyProperty]
	protected GameObject ProjectileOwner;

	protected bool bIsStarted = false;



	[Header("Splash Damage")]

	[SerializeField, Tooltip("체크하면, 범위 피해를 줍니다.")]
	protected bool bCanBeSplash;

	[SerializeField, Tooltip("범위 피해량 계수입니다.")]
	protected float SplashDamageRate = 1.0f;

	[SerializeField, ReadOnlyProperty]
	protected float FinalSplashDamage = 0.0f;

	[SerializeField]
	protected float SplashRadius;

	[SerializeField, Tooltip("체크하면, 범위 피해를 줄 때 직접 맞은 대상에게 직격 피해를 줍니다.")]
	protected bool bCanDirectHitWhenSplash;

	[SerializeField]
	protected bool bSplashDoFullDamage;



	#region 사용자 기능
	public void SetupHitCallback(OnHitCallback NewCallback) { OnHit += NewCallback; }

	public void SetupSplashHitCallback(OnSplashHitCallback NewCallback) { OnSplashHit += NewCallback; }

	/// <summary>
	/// 스폰 후에 호출해야 하는 함수입니다.
	/// </summary>
	/// <param name="NewOwner"> 이 투사체를 발사한 게임오브젝트 객체입니다.</param>
	/// <param name="NewDamageMult"> 피해량 배수입니다.</param>
	/// <param name="NewSplashDamageMult"> 스플래시 피해량 배수입니다.</param>
	public void StartFire(GameObject NewOwner, float NewDamageMult, float NewSplashDamageMult = 0.0f)
	{
		ProjectileOwner = NewOwner;

		FinalDamage = NewDamageMult * DamageRate;
		FinalSplashDamage = NewSplashDamageMult * SplashDamageRate;

		bIsStarted = true;
	}
	#endregion



	#region 투사체 기능
	/// <summary>
	/// bCanApplyDamage인 경우, 투사체가 IDamageable을 맞춘 경우 호출됩니다.
	/// </summary>
	/// <param name="Other"></param>
	protected virtual void OnProjectileHitSuccessed(Collider Other)
	{

	}



	/// <summary>
	/// bCanApplyDamage인 경우, 투사체가 IDamageable을 맞추지 못한 경우 호출됩니다.
	/// </summary>
	/// <param name="Other"></param>
	protected virtual void OnProjectileHitFailed(Collider Other)
	{

	}



	/// <summary>
	/// OnCollisionEnter 로직이 모두 끝나고 가장 나중에 호출됩니다.
	/// </summary>
	/// <param name="Other"></param>
	protected virtual void OnProjectileLateEnterTrigger(Collider Other)
	{
		Destroy(gameObject);
	}
	#endregion



	#region 투사체 로직
	protected virtual void OnEnable()
	{
		if (GetComponent<Rigidbody>() != null)
			GetComponent<Rigidbody>().useGravity = false;

		if (DestroyTimer > 0.0f)
			Destroy(gameObject, DestroyTimer);
	}



	protected virtual void Update()
	{
		if (bIsStarted)
		{
			transform.Translate(Vector3.forward * InitialSpeed * Time.deltaTime);
			transform.LookAt(transform.position + transform.forward * InitialSpeed + Vector3.down * GravityAmount / 9.8f, Vector3.up);
		}
	}



	protected virtual void OnTriggerEnter(Collider other)
	{
		if (!bIsStarted ||
			other.gameObject == ProjectileOwner ||
			other.gameObject.layer == LayerMask.NameToLayer(CollisionExceptionLayer) ||
			other.gameObject.layer == LayerMask.NameToLayer("Projectile") ||
			other.gameObject.layer == LayerMask.NameToLayer("Item") ||
			other.gameObject.layer == LayerMask.NameToLayer("Drone")) return;

		bool bDirectHitSuccessed = false;
		bool bSplashHitSuccessed = false;

		if (other.gameObject.layer == LayerMask.NameToLayer(DamageTargetLayer) && bCanApplyDamage)
		{
			if (bCanDirectHitWhenSplash || !bCanBeSplash)
			{
				bDirectHitSuccessed = DamageHelper.ApplyDamage(other.gameObject,
					FinalDamage, this.gameObject,
					DamageTypePrefab, FinalDamage / DamageRate);

				// 콜백 호출
				if (bDirectHitSuccessed && OnHit != null) OnHit(FinalDamage, transform.position);
			}
		}

		if (bCanBeSplash)
		{
			List<float> DamageList;
			List<Vector3> PositionList;

			(DamageList, PositionList) = DamageHelper.ApplyRadialDamage(transform.position, SplashRadius,
				FinalSplashDamage, DamageTargetLayer, bSplashDoFullDamage, this.gameObject,
				DamageTypePrefab, FinalSplashDamage / SplashDamageRate);

			int SplashHitCount = DamageList.Count;

			if (OnSplashHit != null)
			{
				// 스플래시 당한 만큼 콜백 호출
				for (int i = 0; i < SplashHitCount; i++)
				{
					OnSplashHit(DamageList[i], PositionList[i]);
				}
			}

			bSplashHitSuccessed = SplashHitCount > 0;
		}

		if (bDirectHitSuccessed || bSplashHitSuccessed) OnProjectileHitSuccessed(other);
		else OnProjectileHitFailed(other);

		OnProjectileLateEnterTrigger(other);
	}
	#endregion
}
