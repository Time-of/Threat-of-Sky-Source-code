using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace BehaviourTree
{
	public enum ENodeState
	{
		RUNNING, SUCCESS, FAILURE
	}



	/**
	 * 작성자: 20181220 이성수
	 * 비헤이비어 트리의 노드 베이스입니다.
	 */
	public class BT_NodeBase
	{
		protected ENodeState NodeState;

		protected BT_ATree AttachedTree;

		public BT_NodeBase ParentNode = null;

		protected List<BT_NodeBase> ChildrenNodes = new List<BT_NodeBase>();

		private List<BT_Service> Services = new List<BT_Service>();



		// 노드 평가하기
		public virtual ENodeState Evaluate() { return ENodeState.FAILURE; }



		// 서비스 틱 실행
		public void TickServices()
		{
			if (NodeState == ENodeState.FAILURE) return;

			foreach (BT_Service ServiceTask in Services)
			{
				ServiceTask.Tick();
			}

			foreach (BT_NodeBase ChildNode in ChildrenNodes)
			{
				ChildNode.TickServices();
			}
		}



		// 노드에 서비스 붙이기
		public BT_NodeBase AttachServices(List<BT_Service> ServiceList)
		{
			Services = ServiceList;

			return this;
		}



		public BT_NodeBase(BT_ATree Tree)
		{
			AttachedTree = Tree;
		}



		public BT_NodeBase(BT_ATree Tree, List<BT_NodeBase> ChildrenNodeList)
		{
			AttachedTree = Tree;

			foreach (BT_NodeBase ChildNode in ChildrenNodeList)
			{
				AttachNode(ChildNode);
			}
		}



		void AttachNode(BT_NodeBase NodeToAttach)
		{
			NodeToAttach.ParentNode = this;
			ChildrenNodes.Add(NodeToAttach);
		}
	}
}