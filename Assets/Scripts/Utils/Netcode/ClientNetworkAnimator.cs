using Unity.Netcode.Components;
using UnityEngine;

/// <summary>
/// Owner(클라이언트)가 애니메이션 권한을 갖는 NetworkAnimator입니다.
/// </summary>
[DisallowMultipleComponent]
public class ClientNetworkAnimator : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}