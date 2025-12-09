using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

public class RewardPopupViewModel : ViewModelBase
{
    public override event Action OnStateChanged;

    // View에서 생성할 보상 아이템의 ViewModel 목록
    public List<RewardItemViewModel> RewardItems { get; private set; } = new();

    public RewardPopupViewModel()
    {
        GenerateRandomRewards();
    }

    /// <summary>
    /// 로컬 클라이언트에서 표시할 보상 3개를 무작위 추첨
    /// </summary>
    private void GenerateRandomRewards()
    {
        var allRewards = Managers.Data.GetTable<RewardGameData>();
        if (allRewards == null) return;

        var teamRewards = allRewards.Values.Where(r => r.targetType == "Team").ToList();
        var individualRewards = allRewards.Values.Where(r => r.targetType == "Individual").ToList();

        // 팀 보상 1개 + 개인 보상 2개 (랜덤)
        if (teamRewards.Count > 0)
        {
            var r = teamRewards[UnityEngine.Random.Range(0, teamRewards.Count)];
            RewardItems.Add(new RewardItemViewModel(r, OnSelectReward));
        }

        if (individualRewards.Count > 0)
        {
            // 중복 방지 셔플
            var shuffled = individualRewards.OrderBy(x => UnityEngine.Random.value).Take(2);
            foreach (var r in shuffled)
            {
                RewardItems.Add(new RewardItemViewModel(r, OnSelectReward));
            }
        }
    }

    private void OnSelectReward(int rewardId)
    {
        // 서버에 선택 알림
        if (CombatGameManager.Instance != null)
        {
            CombatGameManager.Instance.SelectRewardServerRpc(rewardId);
        }

        // 팝업 닫기 요청 (View가 이벤트를 처리하도록 하거나, 
        // 여기서 직접 Close를 호출하기 위해 이벤트를 추가할 수도 있음)
        // 여기서는 간단히 View가 ViewModel의 상태를 보고 닫는 게 아니라, 
        // 선택 즉시 닫아야 하므로 Action을 View에 전달하는 방식 대신,
        // Popup 내부에서 클릭 시 Close()를 호출하도록 View 구현
    }
}
