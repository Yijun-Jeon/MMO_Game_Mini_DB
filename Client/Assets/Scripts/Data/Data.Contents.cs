using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{ 
    #region
    [Serializable]
    public class Skill
    {
        public int id;
        public string name;
        public float cooldown;
        public int damagae;
        public SkillType skillType;
        public ProjecttileInfo projecttile;
    }

    // 투사체 정보 클래스
    public class ProjecttileInfo
    {
        public string name;
        public float speed;
        // 갈 수 있는 최대 거리
        public int range;
        // prefab 경로
        public string prefab;

    }

    // Skill 로더
    [Serializable]
    public class SkillData : ILoader<int, Skill>
    {
        public List<Skill> skills = new List<Skill>();

        public Dictionary<int, Skill> MakeDict()
        {
            Dictionary<int, Skill> dict = new Dictionary<int, Skill>();
            foreach (Skill skill in skills)
                dict.Add(skill.id, skill);
            return dict;
        }
    }
    #endregion
}