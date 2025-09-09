using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class PlayerChoicePanelUI : MonoBehaviour
{
    [Header("UI���")]
    public TextMeshProUGUI selectText;              // Ŀ��ѡ�����ʾ
    public Button[] playerButtons = new Button[4];  // Ŀ��ѡ��İ�ť, ��ť5Ϊȡ��
    public Button cancelButton;                     // ȡ����ť

    private PlayerCore player;

    public void SetOptions(string title, PlayerCore player, bool canSelectSelf, Action[] callbacks, int searchSteps=0)
    {
        gameObject.SetActive(true);
        this.player = player;
        selectText.text = title;
        List<PlayerCore> playersWithinSteps = new List<PlayerCore>();

        if (searchSteps > 0)
        {
            List<Tile> tilesWithPlayers = TileManager.Instance.GetTileBySteps(player.playerMove.currentTile, searchSteps);
            
            foreach (Tile tile in tilesWithPlayers)
            {
                if (tile.currentPlayers.Count != 0)
                    foreach (PlayerCore playerInRange in tile.currentPlayers)
                        if (!playersWithinSteps.Contains(playerInRange))
                            playersWithinSteps.Add(playerInRange);
            }
        }
        // ��ȡ������Ҳ�����������
        var players = TurnManager.Instance.players.OrderBy(p => p.playerData.playerIndex).ToList();
        Debug.Log($"player counts:{players.Count}");
        Debug.Log($"can select self?:{canSelectSelf}");
        for (int i = 0; i < players.Count; i++)
        {
            bool isSelf = i == player.playerData.playerIndex;
            playerButtons[i].gameObject.SetActive(i < players.Count);
            playerButtons[i].interactable = (canSelectSelf || !isSelf); // �ؼ��߼�

            if (searchSteps > 0)
                playerButtons[i].interactable = (playersWithinSteps.Contains(players[i]));

            int index = i; // ���հ�����
            playerButtons[i].onClick.RemoveAllListeners();
            playerButtons[i].onClick.AddListener(() =>
            {
                Debug.Log($"ѡ�������:{index + 1}");
                callbacks[index]?.Invoke();
            });
        }
        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() =>
        {
            Debug.Log($"ѡ����ȡ��");
            callbacks[4]?.Invoke();
        });
    }
}
