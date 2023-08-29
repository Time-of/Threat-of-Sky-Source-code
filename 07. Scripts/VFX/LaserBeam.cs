using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 레이저 빔 이펙트를 사용하기 위한 클래스입니다.
 */
public class LaserBeam : MonoBehaviour
{
	[SerializeField]
	private LineRenderer LaserBeamLine;

	[SerializeField]
	private float LaserBeamLength = 10.0f;



	void Start()
	{
		LaserBeamLine.SetPosition(0, transform.position);
		LaserBeamLine.SetPosition(1, transform.position + transform.forward * LaserBeamLength);
	}



	void Update()
	{
		LaserBeamLine.SetPosition(0, transform.position);
		LaserBeamLine.SetPosition(1, transform.position + transform.forward * LaserBeamLength);
	}
}
