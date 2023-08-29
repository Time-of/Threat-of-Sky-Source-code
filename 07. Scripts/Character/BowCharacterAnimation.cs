using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * ACharacterAniamtionBase를 확장한 활 캐릭터 애니메이션 스크립트입니다.
 */
public class BowCharacterAnimation : ACharacterAnimationBase
{
	private BowCharacter Archer;

	[Header("Spine Bone")]

	[ReadOnlyProperty]
	public bool bUpdateSpineLookAtForward = false;

	[SerializeField]
	private float UpperBodyLookDistance = 10.0f;

	[SerializeField]
	private Vector3 UpperBodyRotationFix;

	[SerializeField]
	private Vector3 HeadRotationFix;

	private Transform UpperBodyBoneTransform;

	private Transform HeadBoneTransform;



	protected override void Awake()
	{
		base.Awake();

		Archer = OwnerCharacter.GetComponent<BowCharacter>();

		UpperBodyBoneTransform = Anim.GetBoneTransform(HumanBodyBones.UpperChest);
		HeadBoneTransform = Anim.GetBoneTransform(HumanBodyBones.Head);
	}



	protected override void Update()
	{
		base.Update();

		UpdateSpineRotation();
	}



	void UpdateSpineRotation()
	{
		if (bUpdateSpineLookAtForward)
		{
			Vector3 LookAtPosition = transform.position + OwnerCharacter.GetControllerForwardVector() * UpperBodyLookDistance;


			UpperBodyBoneTransform.LookAt(LookAtPosition);

			UpperBodyBoneTransform.rotation = UpperBodyBoneTransform.rotation * Quaternion.Euler(UpperBodyRotationFix);


			HeadBoneTransform.LookAt(LookAtPosition);

			HeadBoneTransform.rotation = HeadBoneTransform.rotation * Quaternion.Euler(HeadRotationFix);
		}
	}



	#region 애니메이션 이벤트
	public void Event_Die()
	{
		bUpdateSpineLookAtForward = false;
	}



	public void Event_SpawnArrow()
	{
		OwnerCharacter.Attack();
	}



	public void Event_Attack_Shadow()
	{
		Archer.Attack_Shadow();
	}



	public void Event_SK_Moonlight()
	{
		Archer.SkillMoonlightAttack();
	}



	public void Event_SK_Leap()
	{
		OwnerCharacter.GetMovementComponent().bUseControllerRotationYaw = true;

		CharacterGameplay.CharacterGameplayHelper.MoveToWorld(OwnerCharacter,
					OwnerCharacter.transform.position + OwnerCharacter.GetControllerForwardVector() * 12.0f,
					0.45f / OwnerCharacter.GetStatusComponent().GetAtkSpeedMult, true, true);
	}



	public void Event_SK_WeaknessDetection()
	{
		Archer.SkillWeaknessDetection();
	}



	public void Event_SK_Spray()
	{
		Archer.SkillSprayAttack();
	}



	public void Event_SK_Stealth()
	{
		Archer.Cloak();
	}



	public void Event_SK_DeathRain()
	{
		Archer.SkillSpawnDeathRain();
	}
	#endregion
}
