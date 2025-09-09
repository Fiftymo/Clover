using Mirror;
using UnityEngine;

public class ServerLauncher : MonoBehaviour
{
#if UNITY_SERVER
    void Start()
    {
        Debug.Log("[Server] 启动中...");
        NetworkManager.singleton.StartServer();
    }
#endif
}
