using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 머더러 캐릭터의 애니메이션입니다.
 */
public class MurdererCharacterAnimation : ACharacterAnimationBase
{
	private MurdererCharacter Murderer;



	protected override void Awake()
	{
		base.Awake();

		Murderer = OwnerCharacter.GetComponent<MurdererCharacter>();
	}



	#region 애니메이션 이벤트
	public void Event_HorizontalAttack()
	{
		Murderer.Attack_Horizontal();
	}



	public void Event_VerticalAttack()
	{
		Murderer.Attack_Vertical();
	}



	public void Event_SlashWalk()
	{
		Murderer.SlashWalk();
	}



	public void Event_Slash()
	{
		Murderer.Skill_Slash();
	}



	public void Event_Sadist()
	{
		Murderer.Skill_Sadist();
	}
	#endregion
}
