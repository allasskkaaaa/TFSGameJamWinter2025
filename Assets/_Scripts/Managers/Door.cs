using UnityEngine;

public class Door : MonoBehaviour
{
    public RoomGenerator.Direction doorDirection;

    private void Start()
    {
        bool exists = RoomGenerator.Instance.HasDoor(doorDirection);
        gameObject.SetActive(exists);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player entered door to the " + doorDirection);
            RoomGenerator.Instance.Go(doorDirection);
        }
    }
}   