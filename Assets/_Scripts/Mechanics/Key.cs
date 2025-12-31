using UnityEngine;

public class Key : MonoBehaviour
{
    private string currentRoom;

    private void Start()
    {
        currentRoom = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // Check if this room already had its key collected so it doesnt appear again
        if (RoomGenerator.Instance.RoomHasKeyBeenCollected(currentRoom))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance.Keys++;
            Debug.Log($"Keys collected: {GameManager.Instance.Keys}");

            //  marking the key collected
            RoomGenerator.Instance.MarkKeyCollected(currentRoom);

            gameObject.SetActive(false);
        }
    }
}
