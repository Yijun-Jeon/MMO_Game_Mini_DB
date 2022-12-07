using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {

        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;

            // 실질적인 정보 수정은 한 쓰레드만 실행되도록
            PositionInfo movePosInfo = movePacket.PosInfo;
            ObjectInfo info = player.Info;

            // 다른 좌표로 이동하려는 경우, 갈 수 있는지 체크
            if(movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
            {
                if (Map.CanGo(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false)
                    return;
            }

            info.PosInfo.State = movePosInfo.State;
            info.PosInfo.MoveDir = movePosInfo.MoveDir;
            // Map의 _players 갱신
            Map.ApplyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY));

            // 다른 플레이어한테도 알려줌
            S_Move resMovePacket = new S_Move();
            resMovePacket.ObjectId = player.Info.ObjectId;
            resMovePacket.PosInfo = movePacket.PosInfo;

            Broadcast(player.CellPos,resMovePacket);
        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null)
                return;

            ObjectInfo info = player.Info;
            // 스킬을 쓸 수 없는 상태
            if (info.PosInfo.State != CreatureState.Idle)
                return;

            // TODO : 스킬 사용 가능 여부 체크 - 쿨타임 등

            // 통과
            info.PosInfo.State = CreatureState.Skill;
            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectId = player.Info.ObjectId;
            skill.Info.SkillId = skillPacket.Info.SkillId;
            Broadcast(player.CellPos,skill);

            Data.Skill skillData = null;
            if (DataManager.SkillDict.TryGetValue(skillPacket.Info.SkillId, out skillData) == false)
                return;
            switch (skillData.skillType)
            {
                case SkillType.SkillAuto:
                    {
                        // TODO : 데미지 판정
                        Vector2Int skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir);
                        // 해당 위치에 있는 플레이어 반환
                        GameObject target = Map.Find(skillPos);
                        if (target != null)
                        {
                            Console.WriteLine("Hit GameObject!");
                        }
                    }
                    break;
                case SkillType.SkillProjecttile:
                    {
                        Arrow arrow = ObjectManager.Instance.Add<Arrow>();
                        if (arrow == null)
                            return;

                        arrow.Owner = player;
                        arrow.Data = skillData;
                        arrow.PosInfo.State = CreatureState.Moving;
                        arrow.PosInfo.MoveDir = player.PosInfo.MoveDir;
                        arrow.PosInfo.PosX = player.PosInfo.PosX;
                        arrow.PosInfo.PosY = player.PosInfo.PosY;
                        arrow.Speed = skillData.projecttile.speed;
                        // 화살도 서버에서 관리하여 클라로 전송
                        //EnterGame(arrow);
                        Push(EnterGame, arrow,false);
                    }
                    break;
            }
        }
    }
}

