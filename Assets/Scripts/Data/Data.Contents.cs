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

// ======================= Game Data (Read-only) =======================

#region MonsterGameData
/// <summary>
/// 몬스터(적)의 기본 스탯 정보를 정의합니다.
/// </summary>
[Serializable]
public class MonsterGameData : IDataId
{
    public int id;
    public string name;
    public string prefabPath; // Addressable Key (예: "Enemy_Basic")

    // 기본 전투 스탯
    public int hp;
    public int attack;
    public float moveSpeed;
    public float attackRange;
    public float attackSpeed;

    public int ID => id;
}
#endregion

#region WaveGameData
[Serializable]
public class WaveGameData : IDataId
{
    public int id; // Wave Level (1, 2, 3...)
    public List<SpawnEventData> events; // 스폰 이벤트 목록

    public int ID => id;
}

[Serializable]
public class SpawnEventData
{
    public float time;       // 웨이브 시작 후 경과 시간 (초)
    public int spawnerIdx;   // 스폰 포인트 인덱스 (0 ~ 15)
    public int monsterId;    // 소환할 몬스터 ID
}
#endregion

#region RewardGameData
[Serializable]
public class RewardGameData : IDataId
{
    public int id;
    public string targetType; // "Individual" or "Team"
    public string effectType; // "MaxHp", "Atk", "AtkSpeed", "MoveSpeed", "Heal" etc.
    public float value;       // 적용 수치
    public string description;

    public int ID => id;
}
#endregion

// ======================= User Data (Read-Write) =======================

[Serializable]
public class UserDataModel
{
    // 추후 보상으로 얻은 강화 수치 등을 저장할 공간입니다.
    // 현재는 비워두거나, 최소한의 정보만 남깁니다.
    public ReactiveProperty<int> Gold { get; set; } = new ReactiveProperty<int>(0);

    // 예시: 팀 전체 강화 레벨 (보상으로 획득)
    public ReactiveProperty<int> TeamAttackLevel { get; set; } = new ReactiveProperty<int>(0);
    public ReactiveProperty<int> TeamHpLevel { get; set; } = new ReactiveProperty<int>(0);
}