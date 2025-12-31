using UnityEngine;
using System.Collections;


[CreateAssetMenu(fileName = "TimedPowerup", menuName = "Scriptable Objects/TimedPowerup")]
public abstract class TimedPowerupModifier : PowerupModifier
{
    public float powerupTimeInSeconds;

    public abstract void Deactivate(GameObject target);

    //run coroutine for powerup duration
    public IEnumerator StartPowerupCountdown(GameObject target)
    {
        yield return new WaitForSeconds(powerupTimeInSeconds);
        
        //remove the powerup effect
        Deactivate(target);
    }
}
