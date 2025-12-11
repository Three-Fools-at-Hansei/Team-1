using System.Collections.Generic;
using UI;
using UnityEngine;

public class UI_GameStatusHUD : UI_View
{
    [Header("Pre-placed Slots")]
    [SerializeField] private UI_EntityHp[] _playerSlots; // 플레이어용 슬롯 2개
    [SerializeField] private UI_EntityHp _coreSlot;      // 코어용 슬롯 1개

    private GameStatusViewModel _viewModel;

    public override void SetViewModel(IViewModel viewModel)
    {
        _viewModel = viewModel as GameStatusViewModel;
        base.SetViewModel(viewModel);
    }

    protected override void OnStateChanged()
    {
        if (_viewModel == null) return;

        // 1. 플레이어 슬롯 갱신
        var playerVMs = _viewModel.Players;
        for (int i = 0; i < _playerSlots.Length; i++)
        {
            if (i < playerVMs.Count)
            {
                // 데이터가 있으면 활성화 및 연결
                _playerSlots[i].gameObject.SetActive(true);
                _playerSlots[i].SetViewModel(playerVMs[i]);
            }
            else
            {
                // 데이터가 없으면 비활성화
                _playerSlots[i].gameObject.SetActive(false);
            }
        }

        // 2. 코어 슬롯 갱신
        if (_viewModel.Core != null)
        {
            _coreSlot.gameObject.SetActive(true);
            _coreSlot.SetViewModel(_viewModel.Core);
        }
        else
        {
            _coreSlot.gameObject.SetActive(false);
        }
    }
}