using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace BehaviourTree
{
	/**
	 * 작성자: 20181220 이성수
	 * 비헤이비어 트리의 셀렉터 노드입니다.
	 * 모든 자식 노드가 실패할 때까지 평가합니다.
	 */
	public class BT_Selector : BT_NodeBase
	{
		public BT_Selector(BT_ATree Tree) : base(Tree) { }

		public BT_Selector(BT_ATree Tree, List<BT_NodeBase> ChildrenNodeList) : base(Tree, ChildrenNodeList) { }



		public override ENodeState Evaluate()
		{
			// 자식 노드가 실패를 반환하고 있다면 계속해서 평가, 성공이나 실행 중이라면 평가 중지하고 반환
			foreach (BT_NodeBase ChildNode in ChildrenNodes)
			{
				switch (ChildNode.Evaluate())
				{
					case ENodeState.RUNNING:
						NodeState = ENodeState.RUNNING;
						return NodeState;
					case ENodeState.SUCCESS:
						NodeState = ENodeState.SUCCESS;
						return NodeState;
					case ENodeState.FAILURE:
						continue;
					default:
						break;
				}
			}

			NodeState = ENodeState.FAILURE;

			return NodeState;
		}
	}
}