using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;



/**
 * 작성자: 20181220 이성수
 * 드래곤 보스의 비헤이비어 트리입니다.
 */
public class BTTree_BossDragon : BT_ATree
{
	[SerializeField, ReadOnlyProperty]
	private List<ACharacterBase> Characters;



	protected override void SetupBlackboardData()
	{
		SetData("IsInAction", false);

		SetData("CanUseRoar", true);
		SetData("CanUseLiftLandSkill", true);
		
		SetData("NearestPlayer", null);
		SetData("DistSqrToPlayer", 10000000.0f);
	}



	protected override BT_NodeBase SetupRootNode()
	{
		foreach (ACharacterBase PlayerCharacter in FindObjectsOfType<ACharacterBase>())
		{
			Characters.Add(PlayerCharacter);
		}

		BT_NodeBase NewRoot = new BT_Selector(this, new List<BT_NodeBase>
		{
			new BTTask_BossDragonSkill_Roar(this),
			new BTTask_BossDragonSkill_LiftLand(this),
			new BTTask_BossDragonTryAtk(this),
			new BTTask_MoveToNearestPlayer(this)
		}).AttachServices(new List<BT_Service>
		{
			new BTService_FindNearestPlayer(this, Characters)
		});

		return NewRoot;
	}
}
