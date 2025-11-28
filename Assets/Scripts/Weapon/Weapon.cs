using UnityEngine;

/// <summary>
/// 총의 기반 클래스
/// </summary>
public abstract class Weapon : MonoBehaviour
{
    [Header("무기 설정")]
    [SerializeField] protected Vector2 _aimDirection = Vector2.right;

    /// <summary>
    /// 플레이어가 바라보는 방향 (참조)
    /// </summary>
    public Vector2 AimDirection
    {
        get => _aimDirection;
        set => _aimDirection = value.normalized;
    }

    /// <summary>
    /// 공격 메서드 (추상)
    /// </summary>
    /// <param name="attackPower">공격력</param>
    public abstract void Attack(int attackPower);

    /// <summary>
    /// 조준 방향 업데이트
    /// </summary>
    /// <param name="direction">조준 방향</param>
    public virtual void UpdateAimDirection(Vector2 direction)
    {
        if (direction.magnitude > 0.1f)
        {
            _aimDirection = direction.normalized;
        }
    }
}


