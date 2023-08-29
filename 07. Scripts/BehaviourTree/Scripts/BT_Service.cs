using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace BehaviourTree
{
	/**
	 * 작성자: 20181220 이성수
	 * 비헤이비어 트리에서 특정 노드에 붙어 n 초마다 한 번씩 실행되는 행동을 정의합니다.
	 */
	public class BT_Service
	{
		protected BT_ATree AttachedTree = null;

		// 경과된 시간입니다.
		private float ElapsedTime = 0.0f;

		// 틱 액션 주기, ElapsedTime이 이 주기를 넘어갈 때마다 TickAction 이 실행됩니다.
		private float TickActionCycle = 1.0f;



		public BT_Service(BT_ATree Tree, float NewTickActionCycle)
		{
			AttachedTree = Tree;

			TickActionCycle = NewTickActionCycle;
		}



		public void Tick()
		{
			ElapsedTime += Time.deltaTime;

			if (ElapsedTime >= TickActionCycle)
			{
				TickAction();
				ElapsedTime -= TickActionCycle;
			}
		}



		public virtual void TickAction()
		{

		}
	}
}