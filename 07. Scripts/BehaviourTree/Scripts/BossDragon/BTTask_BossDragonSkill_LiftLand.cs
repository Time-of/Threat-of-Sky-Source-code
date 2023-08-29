using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;



/**
 * 작성자: 20181220 이성수
 * 드래곤 보스 몬스터의 이륙-착륙 공격 스킬입니다.
 */
public class BTTask_BossDragonSkill_LiftLand : BT_NodeBase
{
	public BTTask_BossDragonSkill_LiftLand(BT_ATree Tree) : base(Tree) { }



	public override ENodeState Evaluate()
	{
		bool bIsInAction = (bool)AttachedTree.GetData("IsInAction");
		bool bCanUseThisSkill = (bool)AttachedTree.GetData("CanUseLiftLandSkill");

		if (bIsInAction || !bCanUseThisSkill)
		{
			Debug.Log("Skill_LiftLand 평가 중 : FAILURE");

			NodeState = ENodeState.FAILURE;

			return NodeState;
		}


		AttachedTree.SetData("CanUseLiftLandSkill", false);
		AttachedTree.GetComponent<BossEnemy_Dragon>().TrySkill_LiftLand();


		Debug.Log("Skill_LiftLand 평가 중 : SUCCESS");

		NodeState = ENodeState.SUCCESS;

		return NodeState;
	}
}
