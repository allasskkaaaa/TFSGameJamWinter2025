using UnityEngine;

[CreateAssetMenu(fileName = "HealthBuff", menuName = "PowerUps/HealthBuff")]
public class HealthBuff : PowerupModifier
{
    public int healthIncrease;
    public override void Activate(GameObject target)
    {
        GameManager.Instance.Health += healthIncrease;
    }
}
