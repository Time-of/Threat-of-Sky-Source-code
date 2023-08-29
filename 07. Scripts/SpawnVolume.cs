using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CharacterGameplay;



/**
 * 작성자: 20181220 이성수
 * 스폰 볼륨입니다.
 * 스케일을 직접 조정하지 말고, BoxBound 값을 조정합니다.
 */
public class SpawnVolume : MonoBehaviour
{
	[Header("필요 변수 설정")]

	[SerializeField]
	private Vector3 BoxBound;

	[SerializeField]
	private bool bDebugDrawBoxBound = false;

	[SerializeField]
	private Color DebugColor = Color.magenta;



	public Vector3 GetRandomPoint()
	{
		return CharacterGameplayHelper.GetRandomPointInBox(0.5f * BoxBound, transform.position, transform.rotation);
	}



	#region 디버그
	void OnDrawGizmos()
	{
		if (!bDebugDrawBoxBound) return;

		Gizmos.color = DebugColor;

		Matrix4x4 TRMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

		Gizmos.matrix = TRMatrix;

		Gizmos.DrawWireCube(Vector3.zero, BoxBound);
	}
	#endregion
}
