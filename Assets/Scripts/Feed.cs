using UnityEngine;

public class Feed : MonoBehaviour
{

    private Rigidbody rb;

    [SerializeField] private float gravityPush = 0.3f;

    [SerializeField] private bool isDestroyable = true;

    [SerializeField] private float destroyTime = 20f;
    private float currentTime = 0f;
    private float headstartPush = 50f;     // extra push at spawn
    private float headstartTime = 1.5f;    // how long to blend it out

    private float extraGravity;
    private float headstartTimer;

    public bool IsDestroyable { get => IsDestroyable; set => isDestroyable = value; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.name == "Water" && c.gameObject.layer != 6)
        {
            rb.linearVelocity = new Vector3(0, Mathf.Max(0, rb.linearVelocity.y), 0);
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (! isDestroyable)
        {
            return;
        }

        if (transform.position.y > 3)
        {
            transform.position.Set(transform.position.x, 3, transform.position.z);
        }

        currentTime += Time.deltaTime;
        if (currentTime > destroyTime && isDestroyable)
        {
            Destroy(rb.gameObject);
        }

        if (headstartTimer > 0f)
        {
            headstartTimer -= Time.deltaTime;
            // smooth ease-out from headstartPush to 0
            extraGravity = headstartPush * Mathf.Clamp01(headstartTimer / headstartTime);
            extraGravity *= extraGravity; // ease-out curve (optional)
        }
        else
        {
            extraGravity = 0f;
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        extraGravity = headstartPush;
        headstartTimer = headstartTime;
    }

    void FixedUpdate()
    {
        if (! isDestroyable)
        {
            return;
        }
        rb.AddForce(Vector3.down * gravityPush, ForceMode.Acceleration);
    }

    void OnDisable()
    {
        Water.inside.Remove(rb.gameObject);
    }
}
