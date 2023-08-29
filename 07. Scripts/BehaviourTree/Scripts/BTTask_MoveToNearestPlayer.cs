using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviourTree;
using UnityEngine.AI;



/**
 * 작성자: 20181220 이성수
 * 가장 가까운 플레이어가 있다면, 그 플레이어에게 이동하는 비헤이비어 트리 태스크입니다.
 */
public class BTTask_MoveToNearestPlayer : BT_NodeBase
{
	private float ElapsedTime = 0.0f;

	private float TickActionCycle = 1.0f;



	public BTTask_MoveToNearestPlayer(BT_ATree Tree) : base(Tree) {	}



	public override ENodeState Evaluate()
	{
		ElapsedTime += Time.deltaTime;

		if (ElapsedTime >= TickActionCycle)
		{
			ElapsedTime -= TickActionCycle;

			object CharacterData = AttachedTree.GetData("NearestPlayer");

			if (CharacterData != null)
			{
				ACharacterBase NearestCharacter = (ACharacterBase)CharacterData;
				object IsInActionData = AttachedTree.GetData("IsInAction");

				bool bIsInAction = (bool)IsInActionData;

				if (NearestCharacter != null && !bIsInAction)
				{
					NavMeshAgent Agent = AttachedTree.GetComponent<NavMeshAgent>();

					Agent.SetDestination(NearestCharacter.transform.position);
				}
			}
			// 캐릭터가 없는 경우
			else
			{
				AttachedTree.GetComponent<NavMeshAgent>().isStopped = true;
			}
		}

		NodeState = ENodeState.RUNNING;

		return NodeState;
	}
}
