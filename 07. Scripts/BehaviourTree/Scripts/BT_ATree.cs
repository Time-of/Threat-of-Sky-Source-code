using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;



namespace BehaviourTree
{
	/**
	 * 작성자: 20181220 이성수
	 * 비헤이비어 트리의 트리 부분 추상 클래스입니다.
	 */
	public abstract class BT_ATree : MonoBehaviourPun
	{
		private BT_NodeBase RootNode = null;

		private Dictionary<string, object> DataBlackboard = new Dictionary<string, object>();

		[ReadOnlyProperty]
		public Transform Tf;



		void Awake()
		{
			Tf = GetComponent<Transform>();
		}



		protected void Start()
		{
			SetupBlackboardData();
			RootNode = SetupRootNode();
		}



		void Update()
		{
			if (RootNode != null && PhotonNetwork.IsMasterClient)
			{
				RootNode.Evaluate();
				RootNode.TickServices();
			}
		}



		protected abstract BT_NodeBase SetupRootNode();

		protected abstract void SetupBlackboardData();



		// 데이터 설정
		public void SetData(string Key, object Value)
		{
			DataBlackboard[Key] = Value;
		}



		// 데이터 가져오기
		public object GetData(string Key)
		{
			DataBlackboard.TryGetValue(Key, out object OutValue);

			return (OutValue != null) ? DataBlackboard[Key] : null;
		}



		// 데이터 지우기
		public bool ClearData(string Key)
		{
			if (DataBlackboard.ContainsKey(Key))
			{
				DataBlackboard.Remove(Key);

				return true;
			}

			return false;
		}
	}
}