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