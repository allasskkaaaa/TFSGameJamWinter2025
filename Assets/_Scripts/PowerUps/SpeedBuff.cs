using UnityEngine;

[CreateAssetMenu(fileName = "SpeedBuff", menuName = "PowerUps/SpeedBuff")]
public class SpeedBuff : TimedPowerupModifier
{
    public float speedBuff;
    public override void Activate(GameObject target)
    {
        var playerController = target.GetComponent<PlayerController>();
        playerController.speed += speedBuff;
    }

    public override void Deactivate(GameObject target)
    {
        var playerController = target.GetComponent<PlayerController>();
        //subtract the speed buff when the powerup duration ends
        playerController.speed -= speedBuff;
    }
}
