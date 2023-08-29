using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**
 * 작성자: 20181220 이성수
 * 아이템 중 약점 분석기 아이템입니다.
 */
public class SS_StatusItem_FatalDetector : AStatusItemBase
{
	protected override void OnItemAdded()
	{
		// 캐릭터가 향상된 치명타를 가할 수 있도록 만듬
		OwnerCharacter.GetStatusComponent().SetCanDealImprovedCritical(true);
	}
}
