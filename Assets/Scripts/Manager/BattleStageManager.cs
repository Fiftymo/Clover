using TMPro;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BattleStageManager : NetworkBehaviour
{
    public static BattleStageManager Instance;

    [Header("������")]
    public Image attackerIcon;
    public TextMeshProUGUI attackerHP;
    public TextMeshProUGUI atk;
    public TextMeshProUGUI finalAtk;
    [SyncVar] private uint attackerNetId;
    private PlayerCore attackerLocal;
    [Header("���ط�")]
    public Image defenderIcon;
    public TextMeshProUGUI defenderHP;
    public TextMeshProUGUI def;
    public TextMeshProUGUI finalDef;
    [SyncVar] private uint defenderNetId;
    private PlayerCore defenderLocal;
    [Header("��������")]
    public float scaleDuration = 0.18f; // ���Ŷ�������ʱ��
    public float maxScale = 1.5f;     // ������ű���
    public Color highlightColor = Color.yellow; // ������ɫ

    // Э�����ã�����ֹͣ����
    private Coroutine atkAnimationCoroutine;
    private Coroutine defAnimationCoroutine;
    private Coroutine hpAnimationCoroutine;

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
    }

    public void SetUp(PlayerCore attacker, PlayerCore defender)
    {
        if (attacker == null)
        {
            Debug.Log("[Stage]������Ϊ��");
            return;
        }
        if (defender == null)
        {
            Debug.Log("[Stage]���ط�Ϊ��");
            return;
        }
        attackerNetId = attacker.netId;
        defenderNetId = defender.netId;

        attackerLocal = attacker;
        defenderLocal = defender;

        SetUpAtk(attacker);
        SetUpDef(defender);
    }

    private void SetUpAtk(PlayerCore attacker)
    {
        RoleData role = RoleManager.Instance.roleList.Find(r => r.roleId == attacker.playerData.playerRoleId);
        attackerIcon.sprite = role.roleImage;
        attackerHP.text = attacker.playerData.hp.ToString();
        atk.text = $"+{attacker.playerData.minAtk}~{attacker.playerData.maxAtk}";
    }

    private void SetUpDef(PlayerCore defender)
    {
        RoleData role = RoleManager.Instance.roleList.Find(r => r.roleId == defender.playerData.playerRoleId);
        defenderIcon.sprite = role.roleImage;
        defenderHP.text = defender.playerData.hp.ToString();
        def.text = $"+{defender.playerData.minDef}~{defender.playerData.maxDef}";
    }

    public void UpdateText(bool isAttacker,int min,int max,int hp)
    {
        attackerHP.text = hp.ToString();
        defenderHP.text = hp.ToString();
        if (isAttacker)
        {
            atk.text = $"+{min}~{max}";
            StartTextAnimation(atk, ref atkAnimationCoroutine);
        }
        else
        {
            def.text = $"+{min}~{max}";
            StartTextAnimation(def, ref defAnimationCoroutine);
        }
    }

    public void UpdateFinalText(bool isAttacker,int value)
    {
        if (isAttacker)
        {
            finalAtk.text = $"{value}";
        }
        else
        {
            finalDef.text = $"{value}";
        }
    }

    private void StartTextAnimation(TextMeshProUGUI text, ref Coroutine animationCoroutine)
    {
        // ֹ֮ͣǰ�Ķ���������������У�
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        // �����¶���
        animationCoroutine = StartCoroutine(ScaleTextAnimation(text));
    }

    // �����ı�����Э��
    private IEnumerator ScaleTextAnimation(TextMeshProUGUI text)
    {
        if (text == null) yield break;

        Vector3 originalScale = text.transform.localScale;
        Color originalColor = text.color;

        // ��һ�׶Σ��Ŵ�
        float timer = 0f;
        while (timer < scaleDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / scaleDuration;

            // ����
            float scale = Mathf.Lerp(1f, maxScale, progress);
            text.transform.localScale = originalScale * scale;

            yield return null;
        }

        // �ڶ��׶Σ��ָ�
        timer = 0f;
        while (timer < scaleDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / scaleDuration;

            // ����
            float scale = Mathf.Lerp(maxScale, 1f, progress);
            text.transform.localScale = originalScale * scale;

            yield return null;
        }
        // ȷ���ָ�ԭʼ״̬
        text.transform.localScale = originalScale;
        text.color = originalColor;
    }
}
