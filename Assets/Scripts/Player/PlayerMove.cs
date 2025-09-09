using TMPro;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerMove : NetworkBehaviour
{
    private PlayerCore playerCore;

    [Header("�ƶ�״̬")]
    [SyncVar] public Tile currentTile;    // ��ǰ���ڸ�������
    [SyncVar] public Tile previousTile;   // ��ʱ�����������
    private Vector3 startPosition;
    private Vector3 targetPosition;
    public bool isMoving = false;
    private List<Vector3> movePath = new List<Vector3>();
    private Queue<Vector3> pathQueue = new Queue<Vector3>();
    [SyncVar] public bool stepDone = false;          // �����ƶ����
    [SyncVar] public bool isTurnDone = false;        // �Ƿ�����غ�
    [SyncVar] public int rollCounts = 1;             // ʣ���ƶ�����
    [SyncVar] public bool canDoubleRoll = false;     // �ܷ�˫���ƶ�
    [SyncVar] public bool canFreeRoll = false;       // �ܷ������ƶ�
    [SyncVar] public int remainingSteps = 0;         // ʣ���ƶ�����
    [SyncVar] public int extraSteps = 0;             // �����ƶ�����
    [SyncVar] public bool isStepWaiting = false;    // ��ͣ�ƶ�����

    [Header("�������")]
    public Transform directionArrow;
    public TextMeshPro index;

    [Header("����ѡ��")]
    public List<Tile> pendingPaths;  // ��ѡ���·���б�
    [SyncVar] public bool isPathSelected;
    private Coroutine moveCoroutine;

    [Header("�����ͷ")]
    public Transform arrowSpawnParent;
    public GameObject arrowPrefab;
    public float arrowOffset = 0.5f; // ���Ƽ�ͷ���ɾ���
    public float arrowHeightOffset = 0.3f;

    [System.Serializable]
    public class DirectionArrow
    {
        public string name;
        public Button button;
        public Tile linkedTile;
    }

    [Header("�ƶ�����")]
    public float moveDuration = 0f;
    public float moveSpeed = 5f; // �ɵ����ƶ��ٶ�

    private void Awake()
    {
        playerCore = GetComponent<PlayerCore>();
    }

    // ����ѡ���ͷ��ʼ��
    private void Start()
    {
        pendingPaths = new List<Tile>();
    }
    private void Update()
    {
        if (isMoving)
        {
            // ���·����ͷָ���ƶ�����
            Vector3 horizontalDirection = new Vector3(
            targetPosition.x - transform.position.x,
            0,
            targetPosition.z - transform.position.z
        ).normalized;
            if (horizontalDirection != Vector3.zero)
            {
                // ����Y����ת�Ƕ�
                float targetAngle = Mathf.Atan2(horizontalDirection.x, horizontalDirection.z) * Mathf.Rad2Deg;

                // ���ּ�ͷԭ��X/Z��ת��ƽ��״̬�������ı�Y����ת
                directionArrow.rotation = Quaternion.Euler(
                    directionArrow.transform.eulerAngles.x, // ����ԭ��X����ת
                    targetAngle,                            // ������Y����ת
                    directionArrow.transform.eulerAngles.z  // ����ԭ��Z����ת
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
            // ʹ�ı�ʼ�������������������ֱ����
            index.transform.rotation = Quaternion.LookRotation(
                index.transform.position - Camera.main.transform.position,
                Vector3.up
            );
            // ����X��Z����ת��������Y����ת
            Vector3 euler = index.transform.eulerAngles;
            index.transform.eulerAngles = new Vector3(0, euler.y, 0);
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        index.text = $"P{playerCore.playerData.playerIndex + 1}";
    }
    // �����ƶ�
    [Command]
    public void CmdMovePlayer(int steps)
    {
        remainingSteps = steps;
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(ServerMove());
    }
    // �����������ƶ�����(����·��)
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
    // �����������ƶ�����(�����ƶ�)
    [ClientRpc]
    private void RpcStartMove(Vector3[] pathSegment)
    {
        pathQueue = new Queue<Vector3>(pathSegment);
        MoveToNextTile();
        // ����ƶ���ɻص�
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
        // �ڷ������˰ѱ�־�� true
        stepDone = true;
    }
    // �ƶ�����һ��Ŀ���(�ͻ���)
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
                // ֱ���ڷ������˵���
                if (remainingSteps == 0)
                    OnMoveFinished();
            }
            else if (isClient)
            {
                // �ͻ���ͨ�� Command ֪ͨ������
                if (remainingSteps == 0)
                    CmdMoveTurn();
            }
        }
    }
    // --------------����ѡ�����--------------------------------------------------
    [ClientRpc]
    private void RpcShowDirectionArrows(List<Tile> availablePaths)
    {
        if (!isLocalPlayer) return;// ����ǰ��Ҳ���
                                   // ��ʼ��
        foreach (Transform child in arrowSpawnParent)
            Destroy(child.gameObject);
        // ��̬����·��
        foreach (var tile in availablePaths)
        {
            if (tile == previousTile) continue;
            Debug.Log($"����{currentTile.name}��{tile.name}�ļ�ͷ,��ǰprev:{previousTile}");
            Vector3 dir = (tile.transform.position - currentTile.transform.position).normalized;
            Vector3 spawnPos = transform.position
                   + dir * arrowOffset
                   + Vector3.up * arrowHeightOffset;

            // ʵ������ͷ
            GameObject arrowObj = Instantiate(arrowPrefab, spawnPos, Quaternion.identity, arrowSpawnParent);
            arrowObj.transform.rotation = Quaternion.LookRotation(dir, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
            var arrow = arrowObj.GetComponent<DirectionArrowController>();
            arrow.Setup(tile);
        }
    }
    // ��ͷ����¼�
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
    // ��������������ƶ�(�����غ�)
    [Command]
    public void CmdMoveTurn()
    {
        OnMoveFinished();
    }
    // ����������ͣ������(�����غ�)
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
        // ǿ��ͬ��λ�õ����пͻ���
        RpcSyncPosition(previousTile, currentTile);
        // ����ƶ�·������
        pathQueue.Clear();
    }
    [ClientRpc]
    private void RpcSyncPosition(Tile pre, Tile cur)
    {
        previousTile = pre;
        currentTile = cur;
        transform.position = cur.transform.position;
    }
    [Server]// �غϿ�ʼ
    public void ResetActionState()
    {
        // �����ж�״̬������ƶ�������
        isMoving = false;
        isTurnDone = false;
        rollCounts = -1;
        rollCounts = 1;
    }
    [Server]
    private void CheckImmediateEffect(Tile tile)
    {
        // ��鼴ʱЧ��
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
        // ������ͬ��
        isStepWaiting = true;
        if (tile.currentPlayers.Count != 1)
        {
            foreach (var otherPlayer in currentTile.currentPlayers)
            {
                if (otherPlayer != playerCore)
                {
                    Debug.Log($"��� {playerCore.playerData.playerName} ���� {otherPlayer.playerData.playerName}��ѯ���Ƿ�ս��");
                    BattleManager.Instance.RequestBattle(playerCore, otherPlayer);
                    return;
                }
            }
        }
        else isStepWaiting = false;
    }
}
