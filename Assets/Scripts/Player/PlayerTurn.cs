using UnityEngine;
using Mirror;

public class PlayerTurn : NetworkBehaviour
{
    private PlayerCore playerCore;
    private void Awake()
    {
        playerCore = GetComponent<PlayerCore>();
    }

    [Server]
    public void ResetActionState()
    {
        playerCore.playerMove.remainingSteps = 0;
        playerCore.playerMove.isTurnDone = false;
    }
    [Server]
    public void OnTurnDone()
    {
        if (playerCore.playerMove.isTurnDone) return;
        // ��������Ƿ���
        if (playerCore.playerHand.CheckHandOverflow())
        {
            playerCore.playerHand.ForceDiscard();
        }
        else
        {
            // ֱ���л�����һλ���
            playerCore.playerMove.isTurnDone = true;
            TurnManager.Instance.NextPlayer();
        }
    }
}
