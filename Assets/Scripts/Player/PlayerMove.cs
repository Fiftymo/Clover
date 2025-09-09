using TMPro;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerMove : NetworkBehaviour
{
    private PlayerCore playerCore;

    [Header("移动状态")]
    [SyncVar] public Tile currentTile;    // 当前所在格子索引
    [SyncVar] public Tile previousTile;   // 来时方向格子索引
    private Vector3 startPosition;
    private Vector3 targetPosition;
    public bool isMoving = false;
    private List<Vector3> movePath = new List<Vector3>();
    private Queue<Vector3> pathQueue = new Queue<Vector3>();
    [SyncVar] public bool stepDone = false;          // 单步移动完成
    [SyncVar] public bool isTurnDone = false;        // 是否结束回合
    [SyncVar] public int rollCounts = 1;             // 剩余移动次数
    [SyncVar] public bool canDoubleRoll = false;     // 能否双倍移动
    [SyncVar] public bool canFreeRoll = false;       // 能否自由移动
    [SyncVar] public int remainingSteps = 0;         // 剩余移动步数
    [SyncVar] public int extraSteps = 0;             // 额外移动步数
    [SyncVar] public bool isStepWaiting = false;    // 暂停移动处理

    [Header("棋子配件")]
    public Transform directionArrow;
    public TextMeshPro index;

    [Header("方向选择")]
    public List<Tile> pendingPaths;  // 待选择的路径列表
    [SyncVar] public bool isPathSelected;
    private Coroutine moveCoroutine;

    [Header("方向箭头")]
    public Transform arrowSpawnParent;
    public GameObject arrowPrefab;
    public float arrowOffset = 0.5f; // 控制箭头生成距离
    public float arrowHeightOffset = 0.3f;

    [System.Serializable]
    public class DirectionArrow
    {
        public string name;
        public Button button;
        public Tile linkedTile;
    }

    [Header("移动设置")]
    public float moveDuration = 0f;
    public float moveSpeed = 5f; // 可调的移动速度

    private void Awake()
    {
        playerCore = GetComponent<PlayerCore>();
    }

    // 方向选择箭头初始化
    private void Start()
    {
        pendingPaths = new List<Tile>();
    }
    private void Update()
    {
        if (isMoving)
        {
            // 更新方向箭头指向移动方向
            Vector3 horizontalDirection = new Vector3(
            targetPosition.x - transform.position.x,
            0,
            targetPosition.z - transform.position.z
        ).normalized;
            if (horizontalDirection != Vector3.zero)
            {
                // 计算Y轴旋转角度
                float targetAngle = Mathf.Atan2(horizontalDirection.x, horizontalDirection.z) * Mathf.Rad2Deg;

                // 保持箭头原有X/Z旋转（平放状态），仅改变Y轴旋转
                directionArrow.rotation = Quaternion.Euler(
                    directionArrow.transform.eulerAngles.x, // 保持原有X轴旋转
                    targetAngle,                            // 仅更新Y轴旋转
                    directionArrow.transform.eulerAngles.z  // 保持原有Z轴旋转
                );
            }
            Vector3 direction = (targetPosition - startPosition).normalized;
            float distancePlayerCoreFrame = moveSpeed * Time.deltaTime;
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

            if (distancePlayerCoreFrame >= distanceToTarget)
            {
                transform.position = targetPosition;
                isMoving = false;
                MoveToNextTile();
            }
            else
            {
                transform.position += direction * distancePlayerCoreFrame;
            }
        }
    }
    private void LateUpdate()
    {
        if (Camera.main != null)
        {
            // 使文本始终面向相机，但保持竖直方向
            index.transform.rotation = Quaternion.LookRotation(
                index.transform.position - Camera.main.transform.position,
                Vector3.up
            );
            // 锁定X和Z轴旋转，仅保留Y轴旋转
            Vector3 euler = index.transform.eulerAngles;
            index.transform.eulerAngles = new Vector3(0, euler.y, 0);
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        index.text = $"P{playerCore.playerData.playerIndex + 1}";
    }
    // 请求移动
    [Command]
    public void CmdMovePlayer(int steps)
    {
        remainingSteps = steps;
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(ServerMove());
    }
    // 服务器处理移动请求(计算路径)
    [Server]
    private IEnumerator ServerMove()
    {
        while (remainingSteps > 0)
        {
            var nextOptions = currentTile.GetAvailablePaths(previousTile);
            if (nextOptions.Count > 1)
            {
                pendingPaths = null;
                currentTile.OnPlayerExit(playerCore);
                previousTile = currentTile;
                RpcShowDirectionArrows(nextOptions);
                yield return new WaitUntil(() => pendingPaths != null && pendingPaths.Count > 0);
                currentTile = pendingPaths[0];
                currentTile.OnPlayerEnter(playerCore);
            }
            else if (nextOptions.Count == 1)
            {
                currentTile.OnPlayerExit(playerCore);
                previousTile = currentTile;
                currentTile = nextOptions[0];
                currentTile.OnPlayerEnter(playerCore);
            }
            else break;

            stepDone = false;
            RpcStartMove(new Vector3[] { currentTile.transform.position });
            yield return new WaitUntil(() => stepDone);
            CheckPlayersInTile(currentTile);
            yield return new WaitUntil(() => !isStepWaiting);
            CheckImmediateEffect(currentTile);
            yield return new WaitUntil(() => !isStepWaiting);
            remainingSteps--;
            //yield return new WaitForSeconds(0.3f);
        }
        OnMoveFinished();
    }
    // 服务器处理移动请求(批量移动)
    [ClientRpc]
    private void RpcStartMove(Vector3[] pathSegment)
    {
        pathQueue = new Queue<Vector3>(pathSegment);
        MoveToNextTile();
        // 添加移动完成回调
        StartCoroutine(WaitForMoveComplete());
    }
    private IEnumerator WaitForMoveComplete()
    {
        yield return new WaitUntil(() => pathQueue.Count == 0 && !isMoving);
        CmdNotifyStepComplete();
    }
    [Command]
    private void CmdNotifyStepComplete()
    {
        // 在服务器端把标志置 true
        stepDone = true;
    }
    // 移动到下一个目标点(客户端)
    private void MoveToNextTile()
    {
        if (pathQueue.Count > 0)
        {
            previousTile = currentTile;
            Vector3 nextPos = pathQueue.Dequeue();
            Tile nextTile = TileManager.Instance.GetTileAtPosition(nextPos);
            currentTile = nextTile;

            startPosition = transform.position;
            targetPosition = nextPos;

            isMoving = true;
        }
        else
        {
            if (isServer)
            {
                // 直接在服务器端调用
                if (remainingSteps == 0)
                    OnMoveFinished();
            }
            else if (isClient)
            {
                // 客户端通过 Command 通知服务器
                if (remainingSteps == 0)
                    CmdMoveTurn();
            }
        }
    }
    // --------------方向选择相关--------------------------------------------------
    [ClientRpc]
    private void RpcShowDirectionArrows(List<Tile> availablePaths)
    {
        if (!isLocalPlayer) return;// 仅当前玩家操作
                                   // 初始化
        foreach (Transform child in arrowSpawnParent)
            Destroy(child.gameObject);
        // 动态绑定新路径
        foreach (var tile in availablePaths)
        {
            if (tile == previousTile) continue;
            Debug.Log($"生成{currentTile.name}到{tile.name}的箭头,当前prev:{previousTile}");
            Vector3 dir = (tile.transform.position - currentTile.transform.position).normalized;
            Vector3 spawnPos = transform.position
                   + dir * arrowOffset
                   + Vector3.up * arrowHeightOffset;

            // 实例化箭头
            GameObject arrowObj = Instantiate(arrowPrefab, spawnPos, Quaternion.identity, arrowSpawnParent);
            arrowObj.transform.rotation = Quaternion.LookRotation(dir, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
            var arrow = arrowObj.GetComponent<DirectionArrowController>();
            arrow.Setup(tile);
        }
    }
    // 箭头点击事件
    [Command]
    public void CmdChooseDirection(uint pathNetId)
    {
        Debug.Log("Choose a direction");
        if (NetworkServer.spawned.TryGetValue(pathNetId, out NetworkIdentity identity))
        {
            pendingPaths = new List<Tile> { identity.GetComponent<Tile>() };
            isPathSelected = true;
            RpcHideArrows();
        }
    }
    [ClientRpc]
    private void RpcHideArrows()
    {
        HideAllArrows();
    }
    private void HideAllArrows()
    {
        Debug.Log("Hide all arrows");
        foreach (Transform child in arrowSpawnParent)
            Destroy(child.gameObject);
    }
    //--------------------------------------------------------------------------------
    // 请求服务器处理移动(结束回合)
    [Command]
    public void CmdMoveTurn()
    {
        OnMoveFinished();
    }
    // 服务器处理停留请求(结束回合)
    [Server]
    public void OnMoveFinished()
    {
        currentTile.HandleSpecialTile(playerCore);
        if (isTurnDone)
            OnTurnFinished();
    }
    public void OnTurnFinished()
    {
        TurnManager.Instance.NextPlayer();
    }
    [Server]
    public void Teleport(Tile targetTile)
    {
        currentTile.OnPlayerExit(playerCore);
        previousTile = currentTile;
        currentTile = targetTile;
        currentTile.OnPlayerEnter(playerCore);
        transform.position = targetTile.transform.position;
        // 强制同步位置到所有客户端
        RpcSyncPosition(previousTile, currentTile);
        // 清空移动路径队列
        pathQueue.Clear();
    }
    [ClientRpc]
    private void RpcSyncPosition(Tile pre, Tile cur)
    {
        previousTile = pre;
        currentTile = cur;
        transform.position = cur.transform.position;
    }
    [Server]// 回合开始
    public void ResetActionState()
    {
        // 重置行动状态（如可移动次数）
        isMoving = false;
        isTurnDone = false;
        rollCounts = -1;
        rollCounts = 1;
    }
    [Server]
    private void CheckImmediateEffect(Tile tile)
    {
        // 检查即时效果
        isStepWaiting = true;
        switch (tile.tileType)
        {
            case Tile.TileType.Shop:
                TileManager.Instance.OnShop(playerCore);
                Debug.Log($"player{playerCore.playerData.playerIndex + 1} enter shop");
                break;
            case Tile.TileType.Home:
                if (tile.homeIndex == playerCore.playerData.playerIndex)
                    Debug.Log("self home");
                break;
            default:
                break;
        }
        isStepWaiting = false;
    }
    [Server]
    private void CheckPlayersInTile(Tile tile)
    {
        // 检查玩家同格
        isStepWaiting = true;
        if (tile.currentPlayers.Count != 1)
        {
            foreach (var otherPlayer in currentTile.currentPlayers)
            {
                if (otherPlayer != playerCore)
                {
                    Debug.Log($"玩家 {playerCore.playerData.playerName} 经过 {otherPlayer.playerData.playerName}，询问是否战斗");
                    BattleManager.Instance.RequestBattle(playerCore, otherPlayer);
                    return;
                }
            }
        }
        else isStepWaiting = false;
    }
}
