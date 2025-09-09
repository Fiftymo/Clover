using UnityEngine;
using UnityEngine.UI;

public class CameraChangePlayer : MonoBehaviour
{
    [SerializeField] private int playerIndex; // ���ö�Ӧ���������1-4��

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnAvatarClicked);
    }

    private void OnAvatarClicked()
    {
        PlayerCore targetPlayer = TurnManager.Instance.GetPlayerByIndex(playerIndex - 1);
        if (targetPlayer != null)
        {
            CameraController.Instance.FocusOnPlayer(targetPlayer.transform);
        }
    }
}
