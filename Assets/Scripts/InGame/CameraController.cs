using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.EventSystems;

public class CameraController : NetworkBehaviour
{
    public static CameraController Instance;
    [Header("跟随设置")]
    public float smoothTime = 0.3f;      // 移动平滑时间
    public float smoothSpeed = 5f; // 移动平滑速度
    public Vector3 offset = new Vector3(0, 6, -5); // 相机偏移 (Y轴高度, Z轴距离)
    public float lookAngle = 45f;        // 固定俯视角度
    public bool isAutoFocus = true;
    public bool isManualFocus = false;

    public Transform target;            // 当前跟随目标
    private Vector3 velocity = Vector3.zero;

    [Header("自由视角")]
    public float dragSpeed = 6f;          // 拖动速度
    public Vector2 boundaryX = new(-4, 5); // X轴移动边界
    public Vector2 boundaryZ = new(-11, -2); // Z轴移动边界
    private Vector3 dragOrigin;            // 拖动起始点
    private bool isDragging = false;       // 是否正在拖动

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        isAutoFocus = true;
    }
    void Update()
    {
        HandleDragInput(); // 每帧检测拖动输入
    }
    private void LateUpdate()
    {
        // 自动模式：跟随当前回合玩家
        if (isAutoFocus && !isDragging && TurnManager.Instance != null)
        {
            PlayerCore currentPlayer = TurnManager.Instance.GetCurrentPlayer();
            if (currentPlayer != null)
            {
                target = currentPlayer.transform;
            }
        }

        // 更新摄像机位置
        if (target != null && (isAutoFocus || isManualFocus))
        {
            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                smoothSpeed * Time.deltaTime
            );
        }
        else if (isDragging)
        {
            ClampCameraPosition();
        }
    }
    // 自动跟随
    public void ResetToAutoFocus()
    {
        if (isManualFocus) return;
        isAutoFocus = true;
    }
    // 手动聚焦
    public void FocusOnPlayer(Transform playerTransform)
    {
        if (playerTransform == null) return;
        target = playerTransform;
        isManualFocus = true;
        isAutoFocus = false; // 关闭自动跟随
        Debug.Log($"摄像机已手动聚焦到 {target.name}");
    }
    // 取消锁定
    public void UnlockFocous()
    {
        isManualFocus = false;
        isAutoFocus = true;
    }
    // 自由视角
    private void HandleDragInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 检查是否点击在UI上
            if (EventSystem.current.IsPointerOverGameObject()) return;

            dragOrigin = Input.mousePosition;
            isDragging = true;
            isAutoFocus = false;
            isManualFocus = false; // 拖动时禁用一切自动跟随
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
            Vector3 move = new Vector3(pos.x * dragSpeed, 0, pos.y * dragSpeed);
            transform.Translate(-move, Space.World);
            dragOrigin = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            //isAutoFocus = true; 
        }
    }
    // 限制摄像机移动边界
    private void ClampCameraPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, boundaryX.x, boundaryX.y);
        pos.z = Mathf.Clamp(pos.z, boundaryZ.x, boundaryZ.y);
        transform.position = pos;
    }
}