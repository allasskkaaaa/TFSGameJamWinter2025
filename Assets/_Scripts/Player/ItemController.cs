using UnityEngine;

public class ItemController : MonoBehaviour
{
    [SerializeField] private Transform grabPoint;
    [SerializeField] private Transform rayPoint;
    [SerializeField] private float rayDistance = 10f;

    private GameObject grabbedItem;
    private int layerIndex;

    void Start()
    {
        layerIndex = LayerMask.NameToLayer("Objects");
    }

    void Update()
    {
        Vector2 direction = transform.right;

        RaycastHit2D hitInfo = Physics2D.Raycast(rayPoint.position, direction, rayDistance);
        Debug.DrawLine(rayPoint.position, rayPoint.position + (Vector3)direction * rayDistance, Color.red);

        // GRAB
        if (hitInfo.collider != null &&
            hitInfo.collider.gameObject.layer == layerIndex &&
            Input.GetKeyDown(KeyCode.E) &&
            grabbedItem == null)
        {
            Debug.Log("Grabbed " + hitInfo.collider.name);

            grabbedItem = hitInfo.collider.gameObject;

            grabbedItem.transform.SetParent(grabPoint);
            grabbedItem.transform.position = grabPoint.position;

            Rigidbody2D rb = grabbedItem.GetComponent<Rigidbody2D>();
            Collider2D col = grabbedItem.GetComponent<Collider2D>();

            if (col) col.isTrigger = true;

            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

        // DROP / THROW - Throw will require physics -- throwing it in a parabolic arc
        if (Input.GetKeyDown(KeyCode.Space) && grabbedItem != null)
        {
            Debug.Log("Dropped item");

            Rigidbody2D rb = grabbedItem.GetComponent<Rigidbody2D>();
            Collider2D col = grabbedItem.GetComponent<Collider2D>();

            if (col) col.isTrigger = false;

            rb.bodyType = RigidbodyType2D.Dynamic;

            grabbedItem.transform.SetParent(null);
            grabbedItem = null;
        }
    }
}
