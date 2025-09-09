using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class PlayerChoicePanelUI : MonoBehaviour
{
    [Header("UI组件")]
    public TextMeshProUGUI selectText;              // 目标选择的提示
    public Button[] playerButtons = new Button[4];  // 目标选择的按钮, 按钮5为取消
    public Button cancelButton;                     // 取消按钮

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
        // 获取所有玩家并按索引排序
        var players = TurnManager.Instance.players.OrderBy(p => p.playerData.playerIndex).ToList();
        Debug.Log($"player counts:{players.Count}");
        Debug.Log($"can select self?:{canSelectSelf}");
        for (int i = 0; i < players.Count; i++)
        {
            bool isSelf = i == player.playerData.playerIndex;
            playerButtons[i].gameObject.SetActive(i < players.Count);
            playerButtons[i].interactable = (canSelectSelf || !isSelf); // 关键逻辑

            if (searchSteps > 0)
                playerButtons[i].interactable = (playersWithinSteps.Contains(players[i]));

            int index = i; // 防闭包问题
            playerButtons[i].onClick.RemoveAllListeners();
            playerButtons[i].onClick.AddListener(() =>
            {
                Debug.Log($"选择了玩家:{index + 1}");
                callbacks[index]?.Invoke();
            });
        }
        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() =>
        {
            Debug.Log($"选择了取消");
            callbacks[4]?.Invoke();
        });
    }
}
