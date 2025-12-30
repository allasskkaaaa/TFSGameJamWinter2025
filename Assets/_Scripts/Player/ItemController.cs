using UnityEngine;

// public class ItemController : MonoBehaviour
// {
//     [SerializeField] private Transform grabPoint;
//     [SerializeField] private Transform rayPoint;
//     [SerializeField] private float rayDistance = 10f;

//     private GameObject grabbedItem;
//     private int layerIndex;

//     void Start()
//     {
//         layerIndex = LayerMask.NameToLayer("Objects");
//     }

//     void Update()
//     {
//         Vector2 direction = transform.right;

//         RaycastHit2D hitInfo = Physics2D.Raycast(rayPoint.position, direction, rayDistance);
//         Debug.DrawLine(rayPoint.position, rayPoint.position + (Vector3)direction * rayDistance, Color.red);

//         // GRAB
//         if (hitInfo.collider != null &&
//             hitInfo.collider.gameObject.layer == layerIndex &&
//             Input.GetKeyDown(KeyCode.E) &&
//             grabbedItem == null)
//         {
//             Debug.Log("Grabbed " + hitInfo.collider.name);

//             grabbedItem = hitInfo.collider.gameObject;

//             grabbedItem.transform.SetParent(grabPoint);
//             grabbedItem.transform.position = grabPoint.position;

//             Rigidbody2D rb = grabbedItem.GetComponent<Rigidbody2D>();
//             Collider2D col = grabbedItem.GetComponent<Collider2D>();

//             if (col) col.isTrigger = true;

//             rb.bodyType = RigidbodyType2D.Kinematic;
//             rb.linearVelocity = Vector2.zero;
//         }

//         // DROP / THROW - Throw will require physics -- throwing it in a parabolic arc
//         if (Input.GetKeyDown(KeyCode.Space) && grabbedItem != null)
//         {
//             Debug.Log("Dropped item");

//             Rigidbody2D rb = grabbedItem.GetComponent<Rigidbody2D>();
//             Collider2D col = grabbedItem.GetComponent<Collider2D>();

//             if (col) col.isTrigger = false;

//             rb.bodyType = RigidbodyType2D.Dynamic;

//             grabbedItem.transform.SetParent(null);
//             grabbedItem = null;
//         }
//     }
// }


//second attempt prayge
public class ItemController : MonoBehaviour
{

    public enum GrabMode { FacingOnly, AllDirections }  //lowkey hate facing only not necessary imma edit it later


    //grabbing settings
    [SerializeField] private Transform grabPoint;   //  where item stays
    [SerializeField] private Transform rayPoint;    // where raycasts start (dont make near player center lol)
    [SerializeField] private float rayDistance = 1.5f;
    [SerializeField] private LayerMask grabMask;    // sets to objects layer in inspector (me thinks)
    [SerializeField] private GrabMode grabMode = GrabMode.FacingOnly;
    
    // debug yipee
    [SerializeField] private bool showDebugRays = true; 


    // throwing impulse variable
    [SerializeField] private float throwImpulse = 3f; // 

    private GameObject grabbedItem;
    private Vector2 facingDir = Vector2.right; // default facing right can change later

    void Update()
    {
        UpdateFacingDirection();

        // debug rays all directions
        if (showDebugRays && rayPoint)
        {
            Debug.DrawLine(rayPoint.position, rayPoint.position + (Vector3)Vector2.right * rayDistance, Color.red);
            Debug.DrawLine(rayPoint.position, rayPoint.position + (Vector3)Vector2.left * rayDistance, Color.red);
            Debug.DrawLine(rayPoint.position, rayPoint.position + (Vector3)Vector2.up * rayDistance, Color.red);
            Debug.DrawLine(rayPoint.position, rayPoint.position + (Vector3)Vector2.down * rayDistance, Color.red);
        }

        // GRAB 
        if (Input.GetKeyDown(KeyCode.E) && grabbedItem == null)
        {
            TryGrab();
        }

        // DROP / THROW (tired of ts, leaving it to rae)
        if (Input.GetKeyDown(KeyCode.Space) && grabbedItem != null)
        {
            DropOrThrow();
        }

        // keeps item aligned ???
        if (grabbedItem != null && grabPoint != null)
        {
            grabbedItem.transform.position = grabPoint.position;
        }
    }



    //based on which direction player is pressing (lowkey kinda hard)
    private void UpdateFacingDirection()
    {
        
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (input.sqrMagnitude > 0.01f)
        {
            // snap to cardinal direction (i hate maths)
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                facingDir = new Vector2(Mathf.Sign(input.x), 0f);
            else
                facingDir = new Vector2(0f, Mathf.Sign(input.y));
        }
    }

    private void TryGrab()
    {
        if (!rayPoint) rayPoint = transform;

        if (grabMode == GrabMode.FacingOnly)
        {
            // Single ray in facing direction
            RaycastHit2D hit = Physics2D.Raycast(rayPoint.position, facingDir, rayDistance, grabMask);
            if (hit.collider != null)
            {
                Grab(hit.collider.gameObject);
            }
        }
        else // AllDirections
        {
            // Cast in all four cardinal directions and pick the closest hit
            Vector2[] dirs = { Vector2.right, Vector2.left, Vector2.up, Vector2.down };
            RaycastHit2D bestHit = default;
            float bestDist = float.MaxValue;

            foreach (var dir in dirs)
            {
                var hit = Physics2D.Raycast(rayPoint.position, dir, rayDistance, grabMask);
                if (hit.collider != null && hit.distance < bestDist)
                {
                    bestDist = hit.distance;
                    bestHit = hit;
                }
            }

            if (bestHit.collider != null)
            {
                // Update facing to the direction we actually grabbed
                facingDir = (bestHit.point - (Vector2)rayPoint.position).normalized;
                facingDir = SnapToCardinal(facingDir);
                Grab(bestHit.collider.gameObject);
            }
        }
    }

    private Vector2 SnapToCardinal(Vector2 v)
    {
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
            return new Vector2(Mathf.Sign(v.x), 0f);
        else
            return new Vector2(0f, Mathf.Sign(v.y));
    }

    private void Grab(GameObject item)
    {
        grabbedItem = item;
        grabbedItem.transform.SetParent(grabPoint);
        grabbedItem.transform.position = grabPoint.position;

        var rb = grabbedItem.GetComponent<Rigidbody2D>();
        var col = grabbedItem.GetComponent<Collider2D>();

        if (col) col.isTrigger = true;

        if (rb)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;       
            rb.angularVelocity = 0f;
            rb.freezeRotation = true;  //item wont wobble
            rb.gravityScale = 0f;    // item steady (can also change in gui)
        }
    }


    //throw or drop 
    private void DropOrThrow()
    {
        var rb = grabbedItem.GetComponent<Rigidbody2D>();
        var col = grabbedItem.GetComponent<Collider2D>();

        if (col) col.isTrigger = false;

        if (rb)
        {
            rb.freezeRotation = false;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f; // might reset later

            // adds impulse in facing direction
            if (throwImpulse > 10f)
            {
                rb.AddForce(facingDir.normalized * throwImpulse, ForceMode2D.Impulse);
            }
        }

        grabbedItem.transform.SetParent(null);
        grabbedItem = null;
    }
}
