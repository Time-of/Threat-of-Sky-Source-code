using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 드래곤 보스의 애니메이션입니다.
 */
public class BossAnim_FinalDragon : ABossAnimationBase
{
	BossEnemy_FinalDragon Dragon;



	protected override void Awake()
	{
		base.Awake();

		Dragon = OwnerBossEnemy.GetComponent<BossEnemy_FinalDragon>();
	}



	#region 이벤트 모음
	public void Event_BiteAttack()
	{
		Dragon.BiteAttack();
	}



	public void Event_Breath()
	{
		Dragon.SpawnBreathZone();
	}



	public void Event_TailAtk()
	{
		Dragon.TailAttack();
	}



	public void Event_StartPhasingAndSetColDisable()
	{
		Dragon.StartPhasing();
		Dragon.SetCollisionEnabled(false);
	}



	public void Event_EndPhasing()
	{
		Dragon.EndPhasing();
	}



	public void Event_SetCollisionEnable()
	{
		Dragon.SetCollisionEnabled(true);
	}



	public void Event_LandAttack()
	{
		SoundManager.Instance.SpawnSoundAtLocation("DragonLand", transform.position, ESoundGroup.SFX, 0.8f, 0.05f, AudioRolloffMode.Linear);
		Dragon.LandAttack();
	}



	public void Event_Roar()
	{
		Dragon.Roar();
	}



	public void Event_OnDied()
	{
		Dragon.OnDied();
	}
	#endregion
}
