
using UnityEngine;

[CreateAssetMenu(fileName = "ArmourBuff", menuName = "PowerUps/ArmourBuff")]
public class ArmourBuff : TimedPowerupModifier
{
    [Tooltip("Temporary increase to MaxHealth.")]
    public int bonusMaxHealth = 20;

    public override void Activate(GameObject target)
    {
        GameManager.Instance.MaxHealth += bonusMaxHealth;

        // optional to give the player that bonus immediately
        GameManager.Instance.Health = Mathf.Min(
            GameManager.Instance.Health + bonusMaxHealth,
            GameManager.Instance.MaxHealth
        );
    }

    public override void Deactivate(GameObject target)
    {
        // reduce MaxHealth back and clamp current Health
        GameManager.Instance.MaxHealth = Mathf.Max(GameManager.Instance.MaxHealth - bonusMaxHealth, 1);
        GameManager.Instance.Health = Mathf.Min(GameManager.Instance.Health, GameManager.Instance.MaxHealth);
    }
}
