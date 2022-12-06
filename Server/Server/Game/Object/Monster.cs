using Google.Protobuf.Protocol;
using Server.Data;
using Server.Data.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class Monster : GameObject
    {
        public int TemplateId { get; private set; }
        public Monster()
        {
            ObjectType = GameObjectType.Monster;
        }

        public void Init(int templateId)
        {
            TemplateId = templateId;

            MonsterData monsterData = null;
            DataManager.MonsterDict.TryGetValue(TemplateId, out monsterData);

            Stat.MergeFrom(monsterData.stat);
            Stat.Hp = monsterData.stat.MaxHp;
            State = CreatureState.Idle;
        }

        IJob _job;
        // FSM (Finite State Machine)
        public override void Update()
        {
            switch(State)
            {
                case CreatureState.Idle:
                    UpdateIdle();
                    break;
                case CreatureState.Moving:
                    UpdateMoving();
                    break;
                case CreatureState.Skill:
                    UpdateSkill();
                    break;
                case CreatureState.Dead:
                    UpdateDead();
                    break;
            }

            // 5프레임 (0.2 초마다 한번씩 Update)
            if (Room != null)
                _job = Room.PushAfter(200, Update);
        }

       
        // 일단 무식한 방법으로 초 계산
        long _nextSearchTick = 0;

        // search 범위
        int _searchCellDist = 10;
        Player _target;

        protected virtual void UpdateIdle()
        {
            // 아직 search tick이 되지 않음
            if (_nextSearchTick > Environment.TickCount64)
                return;
            _nextSearchTick = Environment.TickCount64 + 1000;// 1초마다 통과함

            Player target =  Room.FindPlayer(p =>
            {
                Vector2Int dir = p.CellPos - CellPos;
                return dir.cellDistFromZero <= _searchCellDist;
            });

            if (target == null)
                return;

            _target = target;
            State = CreatureState.Moving;
        }

        long _nextMoveTick = 0;
        // 추적 범위
        int _chaseCellDist = 20;
        // 스킬 범위
        int _skillRange = 1;
        protected virtual void UpdateMoving()
        {
            if (_nextMoveTick > Environment.TickCount64)
                return;
            int moveTick = (int)(1000 / Speed);
            _nextMoveTick = Environment.TickCount64 + moveTick;

            // 타겟이 없는 경우
            // 타겟이 나가거나 다른 지역으로 이동하는 경우
            if(_target == null || _target.Room != Room)
            {
                _target = null;
                State = CreatureState.Idle;
                // 상태 전환도 전달
                BroadcastMove();
                return;
            }

            Vector2Int dir = _target.CellPos - CellPos;
            int dist = dir.cellDistFromZero;
            if(dist == 0 || dist > _chaseCellDist)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            // 플레이어나 몬스터는 충돌 무시
            List<Vector2Int> path =  Room.Map.FindPath(CellPos, _target.CellPos,checkObject: false);
            if(path.Count < 2 || path.Count > _chaseCellDist)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            // 스킬로 넘어갈 지 체크
            // 스킬 사정거리 안이거나 직선 방향인 경우
            if(dist <= _skillRange && (dir.x == 0 || dir.y == 0))
            {
                _coolTick = 0;
                State = CreatureState.Skill;
                return;
            }

            // 이동
            Dir = GetDirFromVec(path[1] - CellPos); // 방향 vector
            Room.Map.ApplyMove(this, path[1]);

            BroadcastMove();
        }

        public void BroadcastMove()
        {
            // 플레이어들에게 알려줌
            S_Move movePacket = new S_Move();
            movePacket.ObjectId = Id;
            movePacket.PosInfo = PosInfo;
            Room.Broadcast(CellPos,movePacket);
        }

        // 스킬 쿨타임
        long _coolTick = 0;
        protected virtual void UpdateSkill()
        {
            // 바로 공격 가능
            if (_coolTick == 0)
            {
                // 유효한 타겟인지
                if (_target == null || _target.Room != Room || _target.Hp == 0)
                {
                    _target = null;
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
                }
                // 스킬이 아직 사용 가능한지
                Vector2Int dir = _target.CellPos - CellPos;
                int dist = dir.cellDistFromZero;
                bool canUseSkill = (dist <= _skillRange && (dir.x == 0 || dir.y == 0));
                if (canUseSkill == false)
                {
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
                }
                // 타게팅 방향 주시
                MoveDir lookDir = GetDirFromVec(dir);
                if (Dir != lookDir)
                {
                    Dir = lookDir;
                    BroadcastMove();
                }

                Skill skillData = null;
                // 1번 평타 스킬 추출
                DataManager.SkillDict.TryGetValue(1, out skillData);

                // 데미지 판정
                _target.OnDamaged(this, skillData.damage + TotalAttack);

                // 스킬 사용 Broadcast
                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.ObjectId = Id;
                skill.Info.SkillId = skillData.id;
                Room.Broadcast(CellPos, skill);

                // 스킬 쿨타임 적용
                int coolTick = (int)(1000 * skillData.cooldown);
                _coolTick = Environment.TickCount64 + coolTick;
            }

            // 쿨타임 아직 안 됨
            if (_coolTick > Environment.TickCount64)
                return;

            _coolTick = 0;
        }

        protected virtual void UpdateDead()
        {

        }

        public override void OnDead(GameObject attacker)
        {
            // 죽었다면 예약해둔 일감 취소
            if (_job != null)
            {
                _job.Cancel = true;
                _job = null;
            }

            base.OnDead(attacker);

            // 플레이어가 죽였을 때만 생성
            GameObject owner = attacker.GetOnwer();
            if(owner.ObjectType == GameObjectType.Player)
            {
                RewardData rewardData = GetRandomReward();
                if(rewardData != null)
                {
                    Player player = (Player)owner;

                    DbTransaction.RewardPlayer(player, rewardData, Room);
                }
            }
        }

        // 랜덤으로 하나의 아이템 보상
        RewardData GetRandomReward()
        {
            MonsterData monsterData = null;
            DataManager.MonsterDict.TryGetValue(TemplateId, out monsterData);

            // 0~100
            int rand = new Random().Next(0, 101);

            int sum = 0;
            foreach(RewardData rewardData in monsterData.rewards)
            {
                // 확률의 합이 rand를 넘기는 순간 그 아이템 반환
                sum += rewardData.probability;
                if(rand <= sum)
                {
                    return rewardData;
                }
            }
            return null;
        }
    }
}
