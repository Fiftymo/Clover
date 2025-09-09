using TMPro;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DiceButton : MonoBehaviour
{
    public static DiceButton Instance;
    public Button button;
    public TextMeshProUGUI diceResult;//骰子结果

    private bool isRolling;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        button.onClick.AddListener(OnClick);
    }
    private void Update()
    {
        // 仅当前回合的本地玩家可点击按钮
        if (TurnManager.Instance != null)
        {
            bool isCurrentPlayer =
                NetworkClient.localPlayer != null &&
                NetworkClient.localPlayer.GetComponent<PlayerCore>().playerData.playerIndex == TurnManager.Instance.currentPlayerIndex;

            button.interactable = isCurrentPlayer;
        }
    }

    private void OnClick()
    {
        PlayerCore player = NetworkClient.localPlayer.GetComponent<PlayerCore>();
        if (!isRolling && player.playerMove.rollCounts != 0)
        {
            player.playerMove.rollCounts--; // 消耗投掷机会
            StartCoroutine(RollDiceAnimation(player));
        }
    }
    private IEnumerator RollDiceAnimation(PlayerCore player)
    {
        isRolling = true;
        int steps = 0;

        if (player.playerMove.canFreeRoll)
        {
            player.playerUI.selectStepsUI.FreeChoiceStep(player);
        }
        else
        {
            for (int i = 0; i < 10; i++)
            {
                steps = Random.Range(1, 7);
                diceResult.text = $"骰子结果: {steps}";
                yield return new WaitForSeconds(0.1f);
            }
            if (player.playerMove.canDoubleRoll) // 双次投掷
            {
                steps = Random.Range(1, 7) + Random.Range(1, 7) + player.playerMove.extraSteps;
                
            }
            else
                steps = Random.Range(1, 7) + player.playerMove.extraSteps;
            //steps = 2;  // 测试用
            diceResult.text = $"骰子结果: {steps}";
            isRolling = false;
            // 通知服务器移动玩家
            PlayerCore localPlayer = NetworkClient.localPlayer.GetComponent<PlayerCore>();
            localPlayer.playerMove.CmdMovePlayer(steps);
        }
        // 重置所有移动状态
        player.playerMove.canFreeRoll = false;
        player.playerMove.canDoubleRoll = false;
    }
}