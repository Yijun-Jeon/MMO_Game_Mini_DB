using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : CreatureController
{
	protected Coroutine	 _coSkill;
    protected bool _rangedSkill = false;

	protected override void Init()
	{
		base.Init();
	}

	protected override void UpdateAnimation()
	{
		if (_animator == null || _sprite == null)
			return;
		if (State == CreatureState.Idle)
		{
			switch (Dir)
			{
				case MoveDir.Up:
					_animator.Play("IDLE_BA,CK");
					_sprite.flipX = false;
					break;
				case MoveDir.Down:
					_animator.Play("IDLE_FRONT");
					_sprite.flipX = false;
					break;
				case MoveDir.Left:
					_animator.Play("IDLE_RIGHT");
					_sprite.flipX = true;
					break;
				case MoveDir.Right:
					_animator.Play("IDLE_RIGHT");
					_sprite.flipX = false;
					break;
			}
		}
		else if (State == CreatureState.Moving)
		{
			switch (Dir)
			{
				case MoveDir.Up:
					_animator.Play("WALK_BACK");
					_sprite.flipX = false;
					break;
				case MoveDir.Down:
					_animator.Play("WALK_FRONT");
					_sprite.flipX = false;
					break;
				case MoveDir.Left:
					_animator.Play("WALK_RIGHT");
					_sprite.flipX = true;
					break;
				case MoveDir.Right:
					_animator.Play("WALK_RIGHT");
					_sprite.flipX = false;
					break;
			}
		}
		else if (State == CreatureState.Skill)
		{
			switch (Dir)
			{
				case MoveDir.Up:
					_animator.Play(_rangedSkill ? "ATTACK_WEAPON_BACK" : "ATTACK_BACK");
					_sprite.flipX = false;
					break;
				case MoveDir.Down:
					_animator.Play(_rangedSkill ? "ATTACK_WEAPON_FRONT" : "ATTACK_FRONT");
					_sprite.flipX = false;
					break;
				case MoveDir.Left:
					_animator.Play(_rangedSkill ? "ATTACK_WEAPON_RIGHT" : "ATTACK_RIGHT");
					_sprite.flipX = true;
					break;
				case MoveDir.Right:
					_animator.Play(_rangedSkill ? "ATTACK_WEAPON_RIGHT" : "ATTACK_RIGHT");
					_sprite.flipX = false;
					break;
			}
		}
		else
		{

		}
	}

	protected override void UpdateController()
	{
		base.UpdateController();
	}

	public override void UseSkill(int skillId)
	{
		// 펀치
		if(skillId == 1)
		{
			_coSkill = StartCoroutine("CoStartPunch");
		}
		else if(skillId == 2)
		{
			_coSkill = StartCoroutine("CoStartShootArrow");
		}
	}

	protected virtual void CheckUpdatedFlag()
	{

	}


    IEnumerator CoStartPunch()
	{
		// 피격 판정 부분 삭제 -> 서버

		// 대기 시간
		_rangedSkill = false;
		State = CreatureState.Skill;
		// 스킬의 쿨타임 체크는 클라 & 서버 둘 다 해야함
		yield return new WaitForSeconds(0.5f);
		State = CreatureState.Idle;
		_coSkill = null;
        // 서버에서는 자동으로 Idle 상태로 돌아가는 코드가 없음
        // 임시 방편으로 Dirty Flag로 movePacket을 전송하던 방식 사용
        CheckUpdatedFlag();
    }

	IEnumerator CoStartShootArrow()
	{ 
		// 대기 시간
		_rangedSkill = true;
        State = CreatureState.Skill;
        yield return new WaitForSeconds(0.3f);
		State = CreatureState.Idle;
		_coSkill = null;
        CheckUpdatedFlag();
    }

	public override void OnDamaged()
	{
		Debug.Log("Player HIT !");
	}
}
