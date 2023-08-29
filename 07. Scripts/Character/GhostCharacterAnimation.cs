using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 고스트 캐릭터의 애니메이션 스크립트입니다.
 */
public class GhostCharacterAnimation : ACharacterAnimationBase
{
	private GhostCharacter Ghost;



	[Header("공격")]

	[SerializeField, ReadOnlyProperty, Tooltip("기본 공격 애니메이션에 쓰이는 변수입니다. true 인 경우 왼손에서 발사합니다. false 인 경우 오른손입니다.")]
	private bool bIsNormalAttackLeftHanded = false;

	[SerializeField, ReadOnlyProperty]
	private int AnnihilationLoopCount = 0;



	[Header("사운드")]

	[SerializeField]
	private AudioClip AnnihilationLoopSoundClip;

	[SerializeField, ReadOnlyProperty]
	private SoundFramework.SoundObject AnnihilationLoopSoundObject;



	protected override void Awake()
	{
		base.Awake();

		Ghost = OwnerCharacter.GetComponent<GhostCharacter>();
	}



	#region 애니메이션 이벤트 모음
	public void Event_PlayAnnihilationSound()
	{
		AnnihilationLoopSoundObject = SoundManager.Instance.
			Spawn3DLoopSound(AnnihilationLoopSoundClip, transform.position, 1.0f);

		AnnihilationLoopSoundObject.transform.position = Vector3.zero;
		AnnihilationLoopSoundObject.transform.SetParent(transform, false);
	}



	public void Event_StopAnnihilationSound()
	{
		Event_SpawnSoundAtMyPosition("BeamEnd");

		AnnihilationLoopSoundObject.transform.SetParent(SoundManager.Instance.transform);

		SoundManager.Instance.ReturnToPool(AnnihilationLoopSoundObject);

		AnnihilationLoopSoundObject = null;
	}



	public void Event_NormalAttack()
	{
		Ghost.Attack_PulseSphere(bIsNormalAttackLeftHanded);

		bIsNormalAttackLeftHanded = !bIsNormalAttackLeftHanded;
		Anim.SetBool("IsNormalAttackLeftHanded", bIsNormalAttackLeftHanded);
	}



	public void Event_FireballAttack()
	{
		Ghost.Attack_Fireball();
	}



	public void Event_Breakdown()
	{
		Ghost.Skill_Breakdown();
	}



	public void Event_BreakdownEnd()
	{
		Ghost.Skill_BreakdownEnd();
	}



	public void Event_BreakthroughStart()
	{
		Ghost.Skill_BreakthroughStart();
	}



	public void Event_Breakthrough()
	{
		Ghost.Skill_Breakthrough();
		Event_SpawnSoundAtMyPosition("BodyWhoosh");
	}



	public void Event_Annihilation()
	{
		Ghost.Skill_Annihilation();
		Anim.SetInteger("AnnihilationLoopCount", ++AnnihilationLoopCount);
	}



	public void Event_InitAnnihilationCount()
	{
		AnnihilationLoopCount = 0;
	}



	public void Event_Creation()
	{
		Ghost.Skill_Creation();
	}



	public void Event_Teleport()
	{
		Ghost.Skill_Teleport();
	}



	public void Event_TeleportEnd()
	{
		Ghost.Decloak();
	}



	public void Event_Combustion()
	{
		Ghost.Skill_Combustion();
	}
	#endregion
}
