using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    public static SpawnManager Instance;

    [Header("�������б�������0-3˳�����ã�")]
    public List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    private void Awake()
    {
        Instance = this;
    }

    // �������������ȡ��Ӧ������
    [Server]
    public SpawnPoint GetSpawnPoint(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < spawnPoints.Count)
            return spawnPoints[playerIndex];
        return null;
    }
}