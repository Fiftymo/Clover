using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class DirectionArrowController : MonoBehaviour
{
    public Tile linkedTile;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
            Debug.LogWarning("DirectionArrowController requires a Button component");
    }

    public void Setup(Tile tile)
    {
        linkedTile = tile;
    }

    public void OnArrowClicked()
    {
        if (linkedTile != null)
        {
            var identity = NetworkClient.localPlayer;
            var player = identity.GetComponent<PlayerCore>();
            if (player != null)
            {
                player.playerMove.CmdChooseDirection(linkedTile.netId);
            }
        }
    }
}