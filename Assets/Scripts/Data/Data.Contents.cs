using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 GameDB 데이터가 고유 ID를 갖도록 강제하는 인터페이스입니다.
/// </summary>
public interface IDataId
{
    int ID { get; }
}

// ======================= Game Data (읽기 전용) =======================

#region NikkeGameData
/// <summary>
/// 캐릭터의 모든 고정된 기본 정보를 정의합니다.
/// </summary>
[Serializable]
public class NikkeGameData : IDataId
{
    public int id;
    public string name;
    public string nikkeClass;
    public int burstLevel;
    public string element;
    public string rarity;
    public int hp;
    public int attack;
    public int defense;
    public Color color;

    /// <summary>
    /// 무기 정보를 담는 중첩 클래스입니다.
    /// </summary>
    public WeaponData weapon;

    /// <summary>
    /// 스킬 정보 리스트를 담는 중첩 클래스입니다.
    /// </summary>
    public List<SkillData> skills;

    public int ID => id;
}

/// <summary>
/// 무기의 고정된 정보를 정의합니다.
/// </summary>
[Serializable]
public class WeaponData
{
    public string weaponClass;
    public int maxAmmo;
    public float reloadTime;
    public string controlType;
    public string description;
    public float damagePercent;
}


[Serializable]
public class SkillData
{
    public int skillID;

    public int burstStage;
    public string name;
    public string description;
    public string skillTypeName; // 스킬 타입 이름 (예: "Passive", "Active")
    public float cooldown; // 초 단위
    public string skillIconPath;

    public List<SkillValueData> values;
}

[Serializable]
public class SkillValueData
{
    public string type; // 문자열 매핑용 타입
    public string minValue;
    public string maxValue;
}
#endregion

#region ItemGameData
[Serializable]
public class ItemGameData : IDataId
{
    public int id;
    public string name;
    public string desc;
    public string iconPath;

    public int ID => id;
}
#endregion

#region MissionGameData
[Serializable]
public class MissionGameData : IDataId
{
    public int id;
    public string title;
    public string description;
    public eMissionType missionType;
    public int targetCount;

    public int rewardItemID;
    public int rewardItemCount;
    public int ID => id;
}
#endregion

// ======================= User Data (읽기/쓰기) =======================

[Serializable]
public class UserDataModel
{
    public Dictionary<int, UserNikkeData> Nikkes { get; set; } = new Dictionary<int, UserNikkeData>();
    public Dictionary<int, UserItemData> Items { get; set; } = new Dictionary<int, UserItemData>();
    public Dictionary<int, UserSquadData> Squads { get; set; } = new Dictionary<int, UserSquadData>();
    public Dictionary<int, UserMissionData> Missions { get; set; } = new Dictionary<int, UserMissionData>();
}

[Serializable]
public class UserNikkeData
{
    public int id; // 캐릭터 고유 번호
    public ReactiveProperty<int> level;
    public ReactiveProperty<int> skill1Level;
    public ReactiveProperty<int> skill2Level;
    public ReactiveProperty<int> skill3Level;

    public UserNikkeData() { }
    public UserNikkeData(int id, int level = 1)
    {
        this.id = id;
        this.level = new ReactiveProperty<int>(level);
        this.skill1Level = new ReactiveProperty<int>(1);
        this.skill2Level = new ReactiveProperty<int>(1);
        this.skill3Level = new ReactiveProperty<int>(1);
    }
}

[Serializable]
public class UserItemData
{
    public int id; // 아이템 고유 번호
    public ReactiveProperty<int> count;

    public UserItemData() { }
    public UserItemData(int id, int count = 0)
    {
        this.id = id;
        this.count = new ReactiveProperty<int>(count);
    }
}

[Serializable]
public class UserSquadData
{
    public int id; // 스쿼드 고유 번호
    public List<int> slot; // 5개 슬롯에 배치된 캐릭터 ID 목록

    public UserSquadData() { }
    public UserSquadData(int id)
    {
        this.id = id;
        // 5개의 빈 슬롯으로 초기화
        this.slot = new List<int>(5) { -1, -1, -1, -1, -1 }; // -1을 빈 슬롯으로 가정
    }
}

[Serializable]
public class UserMissionData
{
    public int id;
    public ReactiveProperty<eMissionState> state; // 미션 진행도
    public ReactiveProperty<int> currentCount; // 현재 개수

    public UserMissionData() { }
    public UserMissionData(int id)
    {
        this.id = id;
        this.state = new ReactiveProperty<eMissionState>(eMissionState.InProgress);
        this.currentCount = new ReactiveProperty<int>(0);
    }
}
