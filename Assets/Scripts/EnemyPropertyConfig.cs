using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyPropertyConfig", menuName = "Enemy/Property Config")]
public class EnemyPropertyConfig : ScriptableObject
{
    [Header("基本属性")]
    [Tooltip("最大生命值")]
    public int maxHealth = 20;

    [Tooltip("攻击力")]
    public int attackPower = 3;

    [Header("移动设置")]
    [Tooltip("移动速度")]
    public float moveSpeed = 3f;

    [Tooltip("巡逻半径")]
    public float patrolRadius = 5f;

    [Header("攻击设置")]
    [Tooltip("攻击范围")]
    public float attackRange = 2f;

    [Tooltip("攻击冷却时间(秒)")]
    public float attackCooldown = 2f;

    [Header("追逐设置")]
    [Tooltip("开始追逐的距离")]
    public float chaseStartRange = 10f;

    [Tooltip("停止追逐的距离")]
    public float chaseStopRange = 15f;

    [Header("UI设置")]
    [Tooltip("UI预制体（可选，如不设置则使用全局设置）")]
    public GameObject customUIPrefab;

    [Tooltip("UI在头顶的偏移")]
    public Vector3 uiOffset = new Vector3(0, 2.5f, 0);

    [Tooltip("血条宽度")]
    public float healthBarWidth = 200f;

    [Tooltip("血条高度")]
    public float healthBarHeight = 20f;
}