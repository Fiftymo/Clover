using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    public static SpawnManager Instance;

    [Header("出生点列表（按索引0-3顺序配置）")]
    public List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    private void Awake()
    {
        Instance = this;
    }

    // 根据玩家索引获取对应出生点
    [Server]
    public SpawnPoint GetSpawnPoint(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < spawnPoints.Count)
            return spawnPoints[playerIndex];
        return null;
    }
}