using UnityEngine;

public class Feed : MonoBehaviour
{

    private Rigidbody rb;

    [SerializeField] private float gravityPush = 0.3f;

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
        
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    void FixedUpdate()
    {
        rb.AddForce(Vector3.down * gravityPush, ForceMode.Acceleration);
    }

    void OnDisable()
    {
        Water.inside.Remove(rb.gameObject);
    }
}
