using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;



/**
 * 작성자: 20181220 이성수
 * Final 드래곤 보스 몬스터의 로어 스킬입니다.
 */
public class BTTask_BossFinalDragonSkill_Roar : BT_NodeBase
{
	public BTTask_BossFinalDragonSkill_Roar(BT_ATree Tree) : base(Tree) { }



	public override ENodeState Evaluate()
	{
		bool bIsInAction = (bool)AttachedTree.GetData("IsInAction");
		bool bCanUseThisSkill = (bool)AttachedTree.GetData("CanUseRoar");

		if (bIsInAction || !bCanUseThisSkill)
		{
			NodeState = ENodeState.FAILURE;

			return NodeState;
		}


		AttachedTree.SetData("CanUseRoar", false);
		AttachedTree.GetComponent<BossEnemy_FinalDragon>().TrySkill_Roar();


		NodeState = ENodeState.SUCCESS;

		return NodeState;
	}
}
