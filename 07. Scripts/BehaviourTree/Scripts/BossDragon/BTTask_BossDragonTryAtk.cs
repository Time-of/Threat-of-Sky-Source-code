using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;


/**
 * 작성자: 20181220 이성수
 * 드래곤 보스가 공격을 시도하는 비헤이비어 트리 태스크입니다.
 */
public class BTTask_BossDragonTryAtk : BT_NodeBase
{
	public BTTask_BossDragonTryAtk(BT_ATree Tree) : base(Tree) { }



	public override ENodeState Evaluate()
	{
		object DistData = AttachedTree.GetData("DistSqrToPlayer");
		object IsInActionData = AttachedTree.GetData("IsInAction");

		float DistSqr = (float)DistData;
		bool bCanAtk = !(bool)IsInActionData;

		if (DistSqr <= 52.0f)
		{
			if (bCanAtk)
				AttachedTree.GetComponent<ABossBase>().TryAttack(0);

			Debug.Log("TryAtk 평가 중: SUCCESS");

			NodeState = ENodeState.SUCCESS;

			return NodeState;
		}
		else if (DistSqr <= 100.0f)
		{
			if (bCanAtk)
				AttachedTree.GetComponent<ABossBase>().TryAttack(1);

			Debug.Log("TryAtk 평가 중: SUCCESS");

			NodeState = ENodeState.SUCCESS;

			return NodeState;
		}



		Debug.Log("TryAtk 평가 중: FAILURE");

		NodeState = ENodeState.FAILURE;

		return NodeState;
	}
}
