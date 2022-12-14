using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Define;

public class MyPlayerController : PlayerController
{
    bool _moveKeyPressed = false;

    public int WeaponDamage { get; private set; }
    public int ArmorDefence { get; private set; }

    protected override void Init()
    {
        base.Init();

        RefreshAdditionalStat();
    }
    
    protected override void UpdateController()
    {
        GetUIKeyInput();

        switch (State)
        {
            case CreatureState.Idle:
                GetDirInput();
                break;
            case CreatureState.Moving:
                GetDirInput();
                break;
        }

        base.UpdateController();
    }

    void GetUIKeyInput()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_Inventory invenUI = gameSceneUI.InvenUI;

            // 인벤토리가 켜져 있음
            if(invenUI.gameObject.activeSelf)
            {
                invenUI.gameObject.SetActive(false);
            }
            // 인벤토리가 꺼져 있음
            else
            {
                invenUI.gameObject.SetActive(true);
                invenUI.RefreshUI();
            }
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_Stat statUI = gameSceneUI.StatUI;

            // 인벤토리가 켜져 있음
            if (statUI.gameObject.activeSelf)
            {
                statUI.gameObject.SetActive(false);
            }
            // 인벤토리가 꺼져 있음
            else
            {
                statUI.gameObject.SetActive(true);
                statUI.RefreshUI();
            }
        }
    }
    void GetDirInput()
    {
        _moveKeyPressed = true;
        if (Input.GetKey(KeyCode.W))
        {
            Dir = MoveDir.Up;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Dir = MoveDir.Down;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Dir = MoveDir.Left;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Dir = MoveDir.Right;
        }
        else
        {
            _moveKeyPressed = false;
        }
    }

    void LateUpdate()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    protected override void UpdateIdle()
    {
        if (_moveKeyPressed)
        {
            State = CreatureState.Moving;
            return;
        }

        if(_coSkillCoolTime == null && Input.GetKey(KeyCode.Space))
        {
            C_Skill skill = new C_Skill() { Info = new SkillInfo() };
            skill.Info.SkillId = 2;
            Managers.Network.Send(skill);

            _coSkillCoolTime = StartCoroutine("CoInputCooltime",0.2f);
        }
    }

    Coroutine _coSkillCoolTime;
    IEnumerator CoInputCooltime(float time)
    {
        yield return new WaitForSeconds(time);
        _coSkillCoolTime = null;
    }

    protected override void MoveToNextPos()
    {
        if (_moveKeyPressed == false)
        {
            State = CreatureState.Idle;
            CheckUpdatedFlag();
            return;
        }

        Vector3Int destPos = CellPos;

        switch (Dir)
        {
            case MoveDir.Up:
                destPos += Vector3Int.up;
                break;
            case MoveDir.Down:
                destPos += Vector3Int.down;
                break;
            case MoveDir.Left:
                destPos += Vector3Int.left;
                break;
            case MoveDir.Right:
                destPos += Vector3Int.right;
                break;
        }

        if (Managers.Map.CanGo(destPos))
        {
            if (Managers.Object.FindCreature(destPos) == null)
            {
                CellPos = destPos;
            }
        }

        CheckUpdatedFlag();
    }

    protected override void CheckUpdatedFlag()
    {
        if(_updated)
        {
            C_Move movePacket = new C_Move();
            movePacket.PosInfo = PosInfo;
            Managers.Network.Send(movePacket);
            _updated = false;
        }
    }

    public void RefreshAdditionalStat()
    {
        WeaponDamage = 0;
        ArmorDefence = 0;

        foreach (Item item in Managers.Inven.Items.Values)
        {
            if (item.Equipped == false)
                continue;

            switch (item.ItemType)
            {
                case ItemType.Weapon:
                    WeaponDamage += ((Weapon)item).Damage;
                    break;
                case ItemType.Armor:
                    ArmorDefence += ((Armor)item).Defence;
                    break;
            }
        }
    }
}
