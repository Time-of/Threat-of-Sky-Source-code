using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 플레이어 카메라에 들어가는 스크립트입니다.
 * 주요 기능은 카메라 흔들기입니다.
 * 위치 적용은 CameraArm이 담당합니다.
 */
public class PlayerCamera : MonoBehaviour
{
	[ReadOnlyProperty]
	public Vector3 ShakeVector;

	private float ShakeForce;

	private float ShakeDuration;

	Vector3 CenterWorldLocation;



	public Vector3 GetCenterWorldLocation()
	{
		LayerMask ExceptPlayer = ~(1 << LayerMask.NameToLayer("Player"));
		Physics.Raycast(transform.position, transform.forward, out RaycastHit RayHit, 1000.0f, ExceptPlayer, QueryTriggerInteraction.Ignore);
		CenterWorldLocation = RayHit.point != Vector3.zero ? RayHit.point : transform.position + transform.forward * 1000.0f;

		return CenterWorldLocation;
	}



	public void PlayCameraShake(float Force, float Duration)
	{
		ShakeForce = Force * 0.01f;
		ShakeDuration = Duration;

		StopAllCoroutines();
		StartCoroutine(ShakeCoroutine());
	}


	
	IEnumerator ShakeCoroutine()
	{
		while (ShakeDuration > 0.0f)
		{
			ShakeVector = new Vector3(Random.Range(-ShakeForce, ShakeForce), Random.Range(-ShakeForce, ShakeForce), 0.0f);

			ShakeDuration -= Time.deltaTime;

			yield return null;
		}

		yield return null;

		ShakeVector = Vector3.zero;
	}
}
