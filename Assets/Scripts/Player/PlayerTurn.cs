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
        // 检查手牌是否超限
        if (playerCore.playerHand.CheckHandOverflow())
        {
            playerCore.playerHand.ForceDiscard();
        }
        else
        {
            // 直接切换到下一位玩家
            playerCore.playerMove.isTurnDone = true;
            TurnManager.Instance.NextPlayer();
        }
    }
}
