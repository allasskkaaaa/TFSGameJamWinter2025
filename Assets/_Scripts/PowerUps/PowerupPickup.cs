using System.Diagnostics;
using System.Runtime.Serialization.Formatters;
using UnityEngine;

public class PowerupPickup : MonoBehaviour
{
    public PowerupModifier powerupModifier;
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            PlayerController pc = col.gameObject.GetComponent<PlayerController>();
            ActivatePowerup(pc);
        }
    }

    void ActivatePowerup(PlayerController playerController)
    {
        UnityEngine.Debug.Log("Pickup worked");

        // give an effect to player
        playerController.ApplyPowerupModifier(powerupModifier);


        //destroys the pickup
        Destroy(gameObject);
    }
}
