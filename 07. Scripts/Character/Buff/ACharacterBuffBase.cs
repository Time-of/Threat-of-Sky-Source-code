using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// 합연산, 곱연산을 정의합니다.
[System.Serializable]
public enum EApplyStatusType : int
{
	ADDITIVE, MULT
}



[System.Serializable]
public struct CharacterBuffInfo
{
	public ECharacterStatus StatusToBuff;
	public float Amount;
	public EApplyStatusType ApplyType;
}



/**
 * 작성자: 20181220 이성수
 * 캐릭터에게 적용되는 버프 베이스 추상 클래스입니다.
 * 인스턴싱 후에 StartBuff를 호출하면 됩니다.
 * 절대 직접 캐릭터에 버프를 추가하지 마세요!
 */
public abstract class ACharacterBuffBase : MonoBehaviour
{
	[Header("버프 정보")]

	[SerializeField]
	protected List<CharacterBuffInfo> BuffInfoList = new List<CharacterBuffInfo>();

	public List<CharacterBuffInfo> GetBuffInfoList { get => BuffInfoList; }



	[Header("버프 지속성")]

	[SerializeField, Tooltip("true 인 경우 즉시 지속 효과가 발동됩니다. false 인 경우 DotTick 이후부터 지속 효과가 적용됩니다.")]
	protected bool bImmediatelyApplyDotEffect = false;

	[SerializeField, Tooltip("버프의 총 지속시간입니다."), Min(0.0f)]
	protected float DotDuration = 3.0f;

	[SerializeField, Tooltip("버프 지속 효과가 얼마나 자주 발생할건지를 결정합니다. 0 인 경우, 지속 효과가 발생하지 않습니다."), Min(0.0f)]
	protected float DotTick = 0.5f;

	[SerializeField, ReadOnlyProperty]
	protected float CurrentDotDuration = 0.0f;

	[SerializeField, ReadOnlyProperty]
	protected float CurrentDotTick = 0.0f;

	[SerializeField, ReadOnlyProperty]
	protected bool bIsEnded = false;



	[Header("버프 대상")]

	[SerializeField, ReadOnlyProperty]
	protected ACharacterBase TargetCharacter;



	#region 사용자 기능 모음
	/// <summary>
	/// 버프 효과를 시작합니다.
	/// 참고: DotTick이 0.0f 이하인 버프의 경우, 자동으로 999999.0f로 설정됩니다.
	/// 참고: DotDuration이 0.0f 이하인 버프의 경우, 추가되지 않고 바로 삭제됩니다.
	/// </summary>
	/// <param name="NewTarget"> NewTarget 버프 대상이 될 캐릭터입니다.</param>
	public void StartBuff(ACharacterBase NewTarget)
	{
		TargetCharacter = NewTarget;

		bIsEnded = DotDuration <= 0.0f;

		DotTick = DotTick <= 0.0f ? 999999.0f : DotTick;

		CurrentDotDuration = DotDuration;

		CurrentDotTick = 0.0f;

		OnBuffAdded();

		if (bImmediatelyApplyDotEffect) ApplyTickBuffToTarget();
		
		if (bIsEnded)
		{
			Destroy(this.gameObject);
		}
	}



	public void SetBuffAmount(int index, float NewAmount)
	{
		CharacterBuffInfo TempInfo = BuffInfoList[index];
		TempInfo.Amount = NewAmount;

		BuffInfoList[index] = TempInfo;
	}



	public void SetBuffDuration(float NewDuration)
	{
		DotDuration = NewDuration;
		CurrentDotDuration = DotDuration;
	}
	#endregion



	#region 재정의 기능
	/// <summary>
	/// 재정의 가능한 틱 기능입니다.
	/// </summary>
	protected virtual void ApplyTickBuffToTarget()
	{
		
	}



	/// <summary>
	/// 버프가 추가될 때 호출됩니다.
	/// </summary>
	protected virtual void OnBuffAdded()
	{

	}



	/// <summary>
	/// 버프가 만료될 때 호출됩니다. 호출 이후 바로 오브젝트가 삭제됩니다.
	/// </summary>
	protected virtual void OnBuffRemoved()
	{

	}
	#endregion



	#region 작동
	protected void Update()
	{
		if (CurrentDotDuration > 0.0f)
		{
			CurrentDotDuration -= Time.deltaTime;
			CurrentDotTick += Time.deltaTime;

			if (CurrentDotTick >= DotTick)
			{
				CurrentDotTick -= DotTick;

				ApplyTickBuffToTarget();
			}
		}
		else
		{
			CurrentDotDuration = 0.0f;

			bIsEnded = true;

			RemoveBuffFromList();

			OnBuffRemoved();

			Destroy(this.gameObject);
		}
	}



	void RemoveBuffFromList()
	{
		TargetCharacter.RemoveBuff(this);
	}
	#endregion
}
