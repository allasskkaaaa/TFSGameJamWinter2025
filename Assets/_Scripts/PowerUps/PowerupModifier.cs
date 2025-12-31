using UnityEngine;

[CreateAssetMenu(fileName = "PowerupModifier", menuName = "Scriptable Objects/PowerupModifier")]
public abstract class PowerupModifier : ScriptableObject
{
    public abstract void Activate(GameObject target);

}
