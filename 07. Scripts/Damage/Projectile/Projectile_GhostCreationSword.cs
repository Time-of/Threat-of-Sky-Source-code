using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 고스트 캐릭터의 조형 스킬로 만들어내는 검 투사체입니다.
 */
public class Projectile_GhostCreationSword : ProjectileArrow
{
	[Header("조형 스킬 속성")]

	[SerializeField]
	private float LaunchDelay = 1.5f;

	[SerializeField]
	private float FindEnemyRange = 10.0f;

	[SerializeField, Tooltip("공전 거리입니다.")]
	private float OrbitDistance = 1.5f;

	[SerializeField, Tooltip("공전 속도입니다.")]
	private float OrbitSpeed = 5.0f;

	// 활성화되기 전까지는 주변을 돌다가, 활성화되면 가장 가까운 적에게 발사합니다.
	private bool bLaunchReady = false;

	// 0번부터 시작하는 이 투사체의 번호(순서)입니다.
	private int OrderOfSwords = 0;

	private float ElapsedTime = 0.0f;



	public void SetOrderOfSwords(int NewOrder)
	{
		OrderOfSwords = NewOrder;

		float CalculatedAngle = 0.8966f * OrderOfSwords;

		transform.localPosition = new Vector3(OrbitDistance * Mathf.Cos(CalculatedAngle),
			0, OrbitDistance * Mathf.Sin(CalculatedAngle));

		SpawnParticle(StartFx, true);

		LaunchDelay = LaunchDelay + NewOrder * 0.12f;

		SoundManager.Instance.SpawnSoundAtLocation(ShotSound, transform.position, ESoundGroup.SFX, 1.0f, 0.05f);
	}



	protected override void OnEnable()
	{
		if (GetComponent<Rigidbody>() != null)
			GetComponent<Rigidbody>().useGravity = false;

		if (DestroyTimer > 0.0f)
			Destroy(gameObject, DestroyTimer);
	}



	protected override void Update()
	{
		if (!bLaunchReady)
		{
			ElapsedTime += Time.deltaTime * OrbitSpeed;

			// 투사체 번호마다 51.4도씩..
			float CalculatedAngle = 0.8966f * OrderOfSwords;

			transform.localPosition = new Vector3(OrbitDistance * Mathf.Cos(ElapsedTime + CalculatedAngle),
				0, OrbitDistance * Mathf.Sin(ElapsedTime + CalculatedAngle));

			if (ElapsedTime >= LaunchDelay * OrbitSpeed)
			{
				FindNearestEnemy();
				bLaunchReady = true;
			}
		}
		else
		{
			base.Update();
		}
	}



	void FindNearestEnemy()
	{
		float NearestDistSqr = FindEnemyRange * FindEnemyRange;
		Collider NearestCollider = null;

		foreach (Collider ColliderInRange in Physics.OverlapSphere(transform.position, FindEnemyRange, 1 << LayerMask.NameToLayer("Enemy")))
		{
			float DistSqr = (transform.position - ColliderInRange.transform.position).sqrMagnitude;

			if (NearestDistSqr > DistSqr)
			{
				NearestDistSqr = DistSqr;
				NearestCollider = ColliderInRange;
			}
		}

		if (NearestCollider != null)
		{
			transform.SetParent(null);
			transform.LookAt(NearestCollider.transform);
		}
		else
		{
			transform.SetParent(null);

			ACharacterBase OwnerCharacter = ProjectileOwner.GetComponent<ACharacterBase>();

			if (OwnerCharacter != null)
			{
				transform.LookAt(transform.position + OwnerCharacter.GetControllerForwardVector() * 3.0f);
			}
		}
	}



	protected override void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Ground")) return;

		base.OnTriggerEnter(other);
	}
}
