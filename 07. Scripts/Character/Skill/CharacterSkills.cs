using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 스킬의 정보 및 기능을 모아 놓은 클래스입니다.
 * CharacterSkills의 SkillsList로 관리합니다.
 * 
 * 한계: 캐릭터에 미리 등록된 스킬만 사용 가능하며, 캐릭터는 모든 스킬을 미리 알고 있어야 합니다.
 또한, 다른 캐릭터에 스킬을 이식하는 것이 불가능합니다.
 */
namespace SkillFramework
{
	[System.Serializable]
	public class Skill
	{
		[SerializeField, Tooltip("스킬 이름입니다. 재생할 애니메이션 트리거 이름과 같도록 하는 것을 추천합니다.")]
		private string SkillName = "";

		public Sprite SkillIcon;

		[SerializeField]
		public float BaseCooldown;

		[ReadOnlyProperty]
		public float CurrentCooldown;

		[Tooltip("스킬 충전 횟수입니다.")]
		public int MaxSkillStack = 1;

		[ReadOnlyProperty]
		public int CurrentSkillStack = 1;

		[SerializeField]
		private bool bHasAnimation = true;

		public bool GetHasAnimation { get => bHasAnimation; }



		public void TryUseSkill()
		{
			if (CanUseSkill())
			{
				UseSkill();
			}
		}



		public void UseSkill()
		{
			CurrentSkillStack--;
			CurrentCooldown = BaseCooldown;
		}



		public void TryStackSkill()
		{
			if (CurrentSkillStack < MaxSkillStack)
			{
				CurrentCooldown = (CurrentSkillStack + 1 < MaxSkillStack) ? BaseCooldown : CurrentCooldown;

				CurrentSkillStack++;
			}
		}



		public string GetSkillName() { return SkillName; }

		public bool CanUseSkill() { return CurrentSkillStack >= 1; }
	}



	/**
	 * 작성자: 20181220 이성수
	 * 캐릭터의 스킬을 관리하는 클래스입니다.
	 * 애니메이션을 베이스로 작동합니다.
	 */
	public class CharacterSkills : MonoBehaviour
	{
		public delegate void OnSkillUsedCallback(string SkillName);

		private OnSkillUsedCallback OnSkillUsed = null;

		[SerializeField]
		private List<Skill> SkillsList = new List<Skill>();

		
		
		[Header("스킬 설정 (이름)")]

		[SerializeField]
		private string CurrentSkill1Name = "";

		[SerializeField]
		private string CurrentSkill2Name = "";

		[SerializeField]
		private string CurrentSkill3Name = "";



		void Update()
		{
			CalculateCooldown();
		}



		void CalculateCooldown()
		{
			foreach (Skill skill in SkillsList)
			{
				if (skill.CurrentCooldown > 0.0f) skill.CurrentCooldown -= Time.deltaTime;
				else
				{
					skill.TryStackSkill();
				}
			}
		}



		#region 사용자를 위한 기능
		public void SetupUseSkillCallback(OnSkillUsedCallback onSkillUsedCallback)
		{
			OnSkillUsed += onSkillUsedCallback;
		}



		public float GetBaseCooltimeByIndex(int SkillIndex)
		{
			string SkillName = GetSkillNameByCurrentSlot(SkillIndex);

			foreach (Skill skill in SkillsList)
			{
				if (skill.GetSkillName() == SkillName) return skill.BaseCooldown;
			}

			return -1;
		}



		public Sprite GetSkillIconSpriteByIndex(int SkillIndex)
		{
			string SkillName = GetSkillNameByCurrentSlot(SkillIndex);

			foreach (Skill skill in SkillsList)
			{
				if (skill.GetSkillName() == SkillName) return skill.SkillIcon;
			}

			return null;
		}



		/// <summary>
		/// 스킬 이름으로 스킬을 사용하려 시도하고, 성공 여부를 반환합니다.
		/// </summary>
		/// <param name="SkillName"> 스킬 이름입니다.</param>
		/// <returns>스킬 사용에 성공했는지 여부, 스킬이 애니메이션을 재생하는지 여부를 반환합니다.</returns>
		public (bool, bool) TryUseSkillByName(string SkillName)
		{
			foreach (Skill skill in SkillsList)
			{
				if (skill.GetSkillName() == SkillName)
				{
					if (skill.CanUseSkill())
					{
						skill.UseSkill();

						OnSkillUsed(SkillName);

						return (true, skill.GetHasAnimation);
					}
				}
			}

			return (false, false);
		}



		/// <summary>
		/// 현재 슬롯에 장착된 스킬의 이름을 반환합니다.
		/// </summary>
		/// <param name="SkillSlot"> 현재 장착된 스킬 슬롯의 번호를 의미합니다. 1 ~ 3의 값을 가집니다.</param>
		/// <returns>찾았다면 스킬 이름을, 찾지 못한 경우 UnknownSkill을 반환합니다.</returns>
		public string GetSkillNameByCurrentSlot(int SkillSlot)
		{
			string SkillName = "UnknownSkill";

			switch (SkillSlot)
			{
				case 1: SkillName = CurrentSkill1Name; break;
				case 2: SkillName = CurrentSkill2Name; break;
				case 3: SkillName = CurrentSkill3Name; break;
				default: break;
			}

			return SkillName;
		}



		public void SetSkillName(int SkillSlot, string NewSkillName)
		{
			switch (SkillSlot)
			{
				case 1: CurrentSkill1Name = NewSkillName; break;
				case 2: CurrentSkill2Name = NewSkillName; break;
				case 3: CurrentSkill3Name = NewSkillName; break;
				default: break;
			}
		}



		/// <summary>
		/// 스킬 이름으로 쿨다운을 반환합니다. 찾지 못한 경우 -1을 반환합니다.
		/// </summary>
		/// <param name="SkillName"></param>
		/// <returns></returns>
		public float GetCurrentCooldownByName(string SkillName)
		{
			foreach (Skill skill in SkillsList)
			{
				if (skill.GetSkillName() == SkillName) return skill.CurrentCooldown;
			}

			return -1;
		}



		/// <summary>
		/// 스킬 이름으로 사용 가능 여부를 반환합니다.
		/// </summary>
		/// <param name="SkillName"> 스킬 이름입니다.</param>
		/// <returns>찾지 못한 경우 false 를 반환합니다.</returns>
		public bool GetCanUseSkillByName(string SkillName)
		{
			foreach (Skill skill in SkillsList)
			{
				if (skill.GetSkillName() == SkillName) return skill.CanUseSkill();
			}

			return false;
		}
		#endregion
	}
}