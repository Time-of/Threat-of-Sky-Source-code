using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace BehaviourTree
{
	/**
	 * 작성자: 20181220 이성수
	 * 비헤이비어 트리의 시퀀스 노드입니다.
	 * 자식 노드가 실패하지 않는 한, 계속해서 평가합니다.
	 */
	public class BT_Sequence : BT_NodeBase
	{
		public BT_Sequence(BT_ATree Tree) : base(Tree) { }

		public BT_Sequence(BT_ATree Tree, List<BT_NodeBase> ChildrenNodeList) : base(Tree, ChildrenNodeList) { }



		public override ENodeState Evaluate()
		{
			bool bIsAnyChildRunning = false;

			// continue 를 사용하는 이유는 자식 노드들을 평가하기 위함, 시퀀스이므로 실패 시 실패 반환
			foreach (BT_NodeBase ChildNode in ChildrenNodes)
			{
				switch (ChildNode.Evaluate())
				{
					case ENodeState.RUNNING:
						bIsAnyChildRunning = true;
						continue;
					case ENodeState.SUCCESS:
						continue;
					case ENodeState.FAILURE:
						NodeState = ENodeState.FAILURE;
						return NodeState;
					default:
						break;
				}
			}

			NodeState = bIsAnyChildRunning ? ENodeState.RUNNING : ENodeState.SUCCESS;

			return NodeState;
		}
	}
}