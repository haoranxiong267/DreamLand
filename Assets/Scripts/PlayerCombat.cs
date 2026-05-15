using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    [Header("攻击属性")]
    [Tooltip("玩家基础攻击力")]
    public int attackPower = 10;

    [Tooltip("攻击范围（单位：米）")]
    public float attackRange = 2f;

    [Tooltip("攻击冷却时间（单位：秒）")]
    public float attackCooldown = 0.5f;

    [Tooltip("攻击检测角度（0-180度）")]
    [Range(0, 180)]
    public float attackAngle = 90f;

    [Header("攻击输入")]
    [Tooltip("攻击按键（默认：鼠标左键）")]
    public KeyCode attackKey = KeyCode.Mouse0;

    [Tooltip("备用攻击按键（默认：J键）")]
    public KeyCode altAttackKey = KeyCode.J;

    [Header("攻击效果")]
    [Tooltip("攻击特效预制体（可选）")]
    public GameObject attackEffectPrefab;

    [Tooltip("攻击特效生成位置偏移")]
    public Vector3 effectOffset = new Vector3(0, 1f, 0.5f);

    [Tooltip("攻击特效持续时间")]
    public float effectDuration = 0.3f;

    [Header("调试选项")]
    [Tooltip("在Scene视图中显示攻击范围")]
    public bool showAttackRange = true;

    [Tooltip("攻击范围显示颜色")]
    public Color rangeColor = new Color(1f, 0f, 0f, 0.3f);

    // 私有变量
    private bool canAttack = true;
    private Animator animator;
    private Camera mainCamera;

    void Start()
    {
        // 获取组件引用
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;

        // 如果没有Animator组件，给出提示
        if (animator == null)
        {
            Debug.LogWarning("PlayerCombat: 未找到Animator组件，攻击动画将不会播放。");
        }
    }

    void Update()
    {
        // 检测攻击输入
        if (canAttack && (Input.GetKeyDown(attackKey) || Input.GetKeyDown(altAttackKey)))
        {
            StartCoroutine(PerformAttack());
        }
    }

    IEnumerator PerformAttack()
    {
        // 进入冷却状态
        canAttack = false;

        // 播放攻击动画（如果有）
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // 播放攻击音效（可选）
        // AudioManager.Instance.PlaySound("Attack");

        // 生成攻击特效（如果有）
        if (attackEffectPrefab != null)
        {
            Vector3 effectPosition = transform.position +
                                   transform.forward * effectOffset.z +
                                   transform.up * effectOffset.y +
                                   transform.right * effectOffset.x;

            GameObject effect = Instantiate(attackEffectPrefab, effectPosition, transform.rotation);
            Destroy(effect, effectDuration);
        }

        // 等待动画开始（0.1秒后执行伤害检测）
        yield return new WaitForSeconds(0.1f);

        // 执行攻击检测
        DetectAndDamageEnemies();

        // 等待冷却时间
        yield return new WaitForSeconds(attackCooldown);

        // 恢复攻击能力
        canAttack = true;
    }

    void DetectAndDamageEnemies()
    {
        // 方法1：球形检测（简单但可能检测到背后的敌人）
        // Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);

        // 方法2：扇形检测（更符合挥剑动作）
        Collider[] allColliders = Physics.OverlapSphere(transform.position, attackRange);

        int hitCount = 0;

        foreach (Collider collider in allColliders)
        {
            // 检查是否为敌人
            if (collider.CompareTag("Enemy"))
            {
                // 计算敌人方向
                Vector3 directionToEnemy = (collider.transform.position - transform.position).normalized;

                // 计算角度（忽略Y轴差异）
                Vector3 flatDirectionToEnemy = new Vector3(directionToEnemy.x, 0, directionToEnemy.z);
                Vector3 flatForward = new Vector3(transform.forward.x, 0, transform.forward.z);

                float angle = Vector3.Angle(flatForward, flatDirectionToEnemy);

                // 检查是否在攻击角度内
                if (angle <= attackAngle / 2)
                {
                    // 获取敌人AI组件
                    EnemyAI enemyAI = collider.GetComponent<EnemyAI>();

                    if (enemyAI != null)
                    {
                        // 调用敌人受伤方法
                        enemyAI.TakeDamage(attackPower);
                        hitCount++;

                        // 显示击中效果
                        Debug.Log($"击中敌人！造成 {attackPower} 点伤害");
                    }
                }
            }
        }

        if (hitCount == 0)
        {
            Debug.Log("攻击未命中任何敌人");
        }
        else
        {
            Debug.Log($"本次攻击击中了 {hitCount} 个敌人");
        }
    }

    // 在Scene视图中绘制攻击范围（仅用于调试）
    void OnDrawGizmosSelected()
    {
        if (showAttackRange)
        {
            Gizmos.color = rangeColor;

            // 绘制攻击范围球体
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // 绘制攻击角度扇形
            DrawAttackAngleGizmo();
        }
    }

    void DrawAttackAngleGizmo()
    {
        // 计算扇形边缘方向
        float halfAngle = attackAngle / 2;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-halfAngle, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(halfAngle, Vector3.up);

        Vector3 leftRayDirection = leftRayRotation * transform.forward;
        Vector3 rightRayDirection = rightRayRotation * transform.forward;

        // 绘制扇形边缘线
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, leftRayDirection * attackRange);
        Gizmos.DrawRay(transform.position, rightRayDirection * attackRange);

        // 绘制扇形弧线
        int segments = 20;
        Vector3 prevPoint = transform.position + leftRayDirection * attackRange;

        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = Mathf.Lerp(-halfAngle, halfAngle, t);
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 point = transform.position + rotation * transform.forward * attackRange;

            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }

        // 连接弧线两端
        Vector3 leftPoint = transform.position + leftRayDirection * attackRange;
        Vector3 rightPoint = transform.position + rightRayDirection * attackRange;
        Gizmos.DrawLine(leftPoint, rightPoint);
    }

    // 公共方法：用于外部修改攻击力（如拾取道具后）
    public void IncreaseAttackPower(int amount)
    {
        attackPower += amount;
        Debug.Log($"攻击力提升！当前攻击力：{attackPower}");
    }

    // 公共方法：用于外部修改攻击范围
    public void IncreaseAttackRange(float amount)
    {
        attackRange += amount;
        Debug.Log($"攻击范围提升！当前范围：{attackRange}");
    }

    // 公共方法：用于外部修改攻击冷却
    public void ReduceAttackCooldown(float amount)
    {
        attackCooldown = Mathf.Max(0.1f, attackCooldown - amount);
        Debug.Log($"攻击冷却减少！当前冷却：{attackCooldown}");
    }
}