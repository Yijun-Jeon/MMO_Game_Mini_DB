using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using UnityEngine;
using UnityEngine.UIElements;
using static Define;

public class CreatureController : BaseController
{
	HpBar _hpBar;

	public override StatInfo Stat
	{
		get { return base.Stat; }
		set
		{
			base.Stat = value;
			UpdateHpBar();
        }
	}

	public override int Hp
	{
		get { return Stat.Hp; }
		set
		{
			base.Hp = value;
			UpdateHpBar();
		}
	}

	// 투사체의 경우 호출하지 않음
	protected void AddHpBar()
	{
		// HpBar의 부모를 이 Creature로 설정
		GameObject go = Managers.Resource.Instantiate("UI/HpBar", transform);
		// 상대좌표 설정
		go.transform.localPosition = new Vector3(0, 0.5f, 0);
		go.name = "HpBar";
		_hpBar = go.GetComponent<HpBar>();
		UpdateHpBar();

	}

	void UpdateHpBar()
	{
		if (_hpBar == null)
			return;

		float ratio = 0.0f;
		if(Stat.MaxHp > 0)
		{
			ratio = ((float)Hp / Stat.MaxHp);
			_hpBar.SetHpBar(ratio);
		}
	}

	protected override void Init()
	{
		base.Init();
		AddHpBar();
	}

	public virtual void OnDamaged()
	{

	}

	public virtual void OnDead()
	{
		State = CreatureState.Dead;

		// 기존 Monster.OnDamaged에서 사망처리 해주던 부분
		GameObject effect = Managers.Resource.Instantiate("Effect/DieEffect");
		effect.transform.position = transform.position;
		effect.GetComponent<Animator>().Play("START");
		GameObject.Destroy(effect, 0.5f);
	}

	public virtual void UseSkill(int skillId)
    {

    }
}
