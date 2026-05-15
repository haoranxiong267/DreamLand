using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerBuff", menuName = "Game/Player Buff Config")]
public class PlayerBuff : ScriptableObject
{
    [Header("Basic Info")]
    public string buffName; // Buff name, e.g., "Attack Boost"
    [TextArea]
    public string buffDescription; // Buff description
    public Sprite buffIcon; // Buff icon (for UI)

    [Header("Effect")]
    public BuffType buffType; // Type of buff
    public float value; // Numerical value of the buff

    public enum BuffType
    {
        IncreaseMaxHP,      // Increase MaxHealth
        IncreaseAttack,     // Increase attackPower
        IncreaseAttackRange,// Increase attackRange
        ReduceAttackCooldown,// Decrease attackCooldown
        IncreaseMoveSpeed   // Increase moveSpeed
    }
}