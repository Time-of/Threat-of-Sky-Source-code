using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;



/**
 * 작성자: 20181220 이성수
 * 드래곤 보스가 공격 사거리가 유효한지 체크하는 비헤이비어 트리 서비스입니다.
 */
/// 폐기된 클래스: 이 서비스의 기능은 BTService_FindNearestPlayer로 통합하였습니다.
public abstract class BTService_BossDragonCheckAtkRange : BT_Service
{
	public BTService_BossDragonCheckAtkRange(BT_ATree Tree) : base(Tree, 1.5f) { }



	public override void TickAction()
	{
		object CharacterData = AttachedTree.GetData("NearestPlayer");

		if (CharacterData != null)
		{
			ACharacterBase NearestCharacter = (ACharacterBase)CharacterData;

			if (NearestCharacter != null)
			{
				float DistSqr = (AttachedTree.Tf.position - NearestCharacter.transform.position).sqrMagnitude;

				AttachedTree.SetData("DistSqrToPlayer", DistSqr);

				Debug.Log("<color=green>BossDragonCheckAtkRange</color> : " + DistSqr);
			}
		}
		// 찾지 못한 경우
		else
		{
			Debug.Log("<color=orange>드래곤 보스: 플레이어 찾지 못함</color>");
			AttachedTree.SetData("DistSqrToPlayer", 100000.0f);
		}

		Debug.Log("BossDragonCheckAtkRange 서비스 틱 작동");
	}
}
