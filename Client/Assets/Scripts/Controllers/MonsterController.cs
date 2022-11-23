using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
	Coroutine _coSkill;


	protected override void Init()
	{
		base.Init();
	}

	protected override void UpdateIdle()
	{
		base.UpdateIdle();
	}

	public override void OnDamaged()
	{
        /*Managers.Object.Remove(Id);
        Managers.Resource.Destroy(gameObject);*/
    }

	public override void UseSkill(int skillId)
	{
		// 펀치
		if (skillId == 1)
		{
			// 서버에서 관리하므로 코루틴도 필요없어짐
			State = CreatureState.Skill;
		}
	}
}
