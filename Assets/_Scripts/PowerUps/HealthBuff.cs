using UnityEngine;

[CreateAssetMenu(fileName = "HealthBuff", menuName = "PowerUps/HealthBuff")]
public class HealthBuff : PowerupModifier
{
    public float healthIncrease;
    public override void Activate(GameObject target)
    {
        //without exceeding MaxHealth
        GameManager.Instance.Heal(healthIncrease);
    }
}
