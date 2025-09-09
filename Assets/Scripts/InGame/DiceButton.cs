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
    public TextMeshProUGUI diceResult;//���ӽ��

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
        // ����ǰ�غϵı�����ҿɵ����ť
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
            player.playerMove.rollCounts--; // ����Ͷ������
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
                diceResult.text = $"���ӽ��: {steps}";
                yield return new WaitForSeconds(0.1f);
            }
            if (player.playerMove.canDoubleRoll) // ˫��Ͷ��
            {
                steps = Random.Range(1, 7) + Random.Range(1, 7) + player.playerMove.extraSteps;
                
            }
            else
                steps = Random.Range(1, 7) + player.playerMove.extraSteps;
            //steps = 2;  // ������
            diceResult.text = $"���ӽ��: {steps}";
            isRolling = false;
            // ֪ͨ�������ƶ����
            PlayerCore localPlayer = NetworkClient.localPlayer.GetComponent<PlayerCore>();
            localPlayer.playerMove.CmdMovePlayer(steps);
        }
        // ���������ƶ�״̬
        player.playerMove.canFreeRoll = false;
        player.playerMove.canDoubleRoll = false;
    }
}