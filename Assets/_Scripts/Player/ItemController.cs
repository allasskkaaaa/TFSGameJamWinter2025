using UnityEngine;
using System.Collections;

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
        if (Input.GetMouseButton(0) && grabbedItem != null)
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

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            rayPoint.position,
            rayDistance,
            grabMask
        );

        if (hits.Length == 0) return;

        Collider2D best = null;
        int bestPriority = -1;
        float bestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (!hit.attachedRigidbody) continue;

            int priority = GetPriority(hit.gameObject);
            float dist = Vector2.Distance(rayPoint.position, hit.transform.position);

            bool isBetter =
                priority > bestPriority ||
                (priority == bestPriority && dist < bestDist);

            if (isBetter)
            {
                bestPriority = priority;
                bestDist = dist;
                best = hit;
            }
        }

        if (best != null)
        {
            facingDir = (best.transform.position - rayPoint.position).normalized;
            facingDir = SnapToCardinal(facingDir);
            Grab(best.gameObject);
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
        
        StartCoroutine(IgnorePlayerMomentarily(rb, col));

        if (col) col.isTrigger = false;

        if (rb)
        {
            rb.freezeRotation = false;
            rb.bodyType = RigidbodyType2D.Dynamic;
            //rb.gravityScale = 1f; // might reset later

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;

            Vector2 throwDirection = ((Vector2)mouseWorldPos - (Vector2)grabPoint.position).normalized;

            // applying impulse tot hrow in the direction of where mouse is 
            // NOTE TO PUT A LITTLE CROSSHAIR THING FOR THIS
            Vector2 arc = new Vector2(0, 1f); // parabola effect ig
            rb.linearVelocity = throwDirection * throwImpulse; // simpler than AddForce

            StartCoroutine(SlowStop(rb, 0.5f));


        }

        grabbedItem.transform.SetParent(null);
        grabbedItem = null;
    }

    IEnumerator SlowStop(Rigidbody2D r, float duration)
    {
        Vector2 initialVel = r.linearVelocity;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            r.linearVelocity = Vector2.Lerp(initialVel, Vector2.zero, t / duration);
            yield return null;
        }
        r.linearVelocity = Vector2.zero;
    }


    IEnumerator IgnorePlayerMomentarily(Rigidbody2D rb, Collider2D col)
    {
        col.excludeLayers = LayerMask.GetMask("Player");
        rb.excludeLayers = LayerMask.GetMask("Player"); 

        yield return new WaitForSeconds(0.5f);

        col.includeLayers = LayerMask.GetMask("Player");
        rb.excludeLayers = LayerMask.GetMask("Player");

    }


    private int GetPriority(GameObject obj)
    {
        if (obj.CompareTag("Bomb")) return 2;
        if (obj.CompareTag("Gift")) return 1;
        return 0;
    }

}
