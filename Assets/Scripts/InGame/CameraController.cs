using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.EventSystems;

public class CameraController : NetworkBehaviour
{
    public static CameraController Instance;
    [Header("��������")]
    public float smoothTime = 0.3f;      // �ƶ�ƽ��ʱ��
    public float smoothSpeed = 5f; // �ƶ�ƽ���ٶ�
    public Vector3 offset = new Vector3(0, 6, -5); // ���ƫ�� (Y��߶�, Z�����)
    public float lookAngle = 45f;        // �̶����ӽǶ�
    public bool isAutoFocus = true;
    public bool isManualFocus = false;

    public Transform target;            // ��ǰ����Ŀ��
    private Vector3 velocity = Vector3.zero;

    [Header("�����ӽ�")]
    public float dragSpeed = 6f;          // �϶��ٶ�
    public Vector2 boundaryX = new(-4, 5); // X���ƶ��߽�
    public Vector2 boundaryZ = new(-11, -2); // Z���ƶ��߽�
    private Vector3 dragOrigin;            // �϶���ʼ��
    private bool isDragging = false;       // �Ƿ������϶�

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
        HandleDragInput(); // ÿ֡����϶�����
    }
    private void LateUpdate()
    {
        // �Զ�ģʽ�����浱ǰ�غ����
        if (isAutoFocus && !isDragging && TurnManager.Instance != null)
        {
            PlayerCore currentPlayer = TurnManager.Instance.GetCurrentPlayer();
            if (currentPlayer != null)
            {
                target = currentPlayer.transform;
            }
        }

        // ���������λ��
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
    // �Զ�����
    public void ResetToAutoFocus()
    {
        if (isManualFocus) return;
        isAutoFocus = true;
    }
    // �ֶ��۽�
    public void FocusOnPlayer(Transform playerTransform)
    {
        if (playerTransform == null) return;
        target = playerTransform;
        isManualFocus = true;
        isAutoFocus = false; // �ر��Զ�����
        Debug.Log($"��������ֶ��۽��� {target.name}");
    }
    // ȡ������
    public void UnlockFocous()
    {
        isManualFocus = false;
        isAutoFocus = true;
    }
    // �����ӽ�
    private void HandleDragInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // ����Ƿ�����UI��
            if (EventSystem.current.IsPointerOverGameObject()) return;

            dragOrigin = Input.mousePosition;
            isDragging = true;
            isAutoFocus = false;
            isManualFocus = false; // �϶�ʱ����һ���Զ�����
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
    // ����������ƶ��߽�
    private void ClampCameraPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, boundaryX.x, boundaryX.y);
        pos.z = Mathf.Clamp(pos.z, boundaryZ.x, boundaryZ.y);
        transform.position = pos;
    }
}