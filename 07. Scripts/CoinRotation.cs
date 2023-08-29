using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 회전하는 코인
 */
public class CoinRotation : MonoBehaviour
{
	[SerializeField]
	private float RotationSpeed = 3.0f;

	private float OriginPosY;

	private float ElapsedTime = 0.0f;



	private void Start()
	{
		OriginPosY = transform.position.y;
	}



	void Update()
	{
		ElapsedTime += Time.deltaTime * RotationSpeed;

		Vector3 NewPosition = transform.position;
		NewPosition.y = OriginPosY + 0.2f * Mathf.Cos(ElapsedTime);

		transform.position = NewPosition;

		transform.rotation = Quaternion.Euler(0.0f, 7.5f * ElapsedTime, 90.0f);
	}
}
