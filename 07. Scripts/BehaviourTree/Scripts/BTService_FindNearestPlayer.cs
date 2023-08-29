using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;



/**
 * 작성자: 20181220 이성수
 * 가장 가까운 플레이어를 찾는 비헤이비어 트리 서비스입니다.
 */
public class BTService_FindNearestPlayer : BT_Service
{
	private List<ACharacterBase> Characters;



	public BTService_FindNearestPlayer(BT_ATree Tree, List<ACharacterBase> CharacterList) : base(Tree, 1.0f)
	{
		Characters = CharacterList;

		TickAction();
	}



	public override void TickAction()
	{
		float NearestDist = 10000000000.0f;
		ACharacterBase NearestCharacter = null;

		foreach (ACharacterBase PlayerCharacter in Characters)
		{
			if (PlayerCharacter == null) continue;

			float DistSqr = (AttachedTree.Tf.position - PlayerCharacter.transform.position).sqrMagnitude;

			if (!PlayerCharacter.IsDead && NearestDist > DistSqr)
			{
				NearestDist = DistSqr;
				NearestCharacter = PlayerCharacter;
			}

			Debug.Log("<color=green>FindNearestPlayer - 찾았당~~</color> : " + NearestCharacter);
		}

		// 찾았든 찾지 못했든 저장
		AttachedTree.SetData("NearestPlayer", NearestCharacter);
		AttachedTree.SetData("DistSqrToPlayer", NearestDist);
	}
}
