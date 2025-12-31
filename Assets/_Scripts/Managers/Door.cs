using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    public RoomGenerator.Direction doorDirection;
    public bool startingDoor = false;
    private void Start()
    {
        if (startingDoor)
        {
            gameObject.SetActive(true);
        }
        if (!startingDoor)
        {
            bool exists = RoomGenerator.Instance.HasDoor(doorDirection);
            gameObject.SetActive(exists);
        }
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