using UnityEngine;
using System.Collections;

public class Fish : MonoBehaviour
{
    protected Rigidbody rb;
    protected Animation animation;

    [SerializeField] protected int maxHunger = 100;
    [SerializeField] protected int minHunger = 0;

    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int minHealth = 0;

    [SerializeField] protected int maxHappiness = 100;
    [SerializeField] protected int minHappiness = 0;


    [SerializeField] protected string fishName;
    [SerializeField] protected int health = 100;
    [SerializeField] protected int hunger = 50;
    [SerializeField] protected int happiness = 50;

    [SerializeField] protected float maxY = 3;
    [SerializeField] protected float speed = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animation = gameObject.GetComponent<Animation>();
        animation["Idle"].blendMode = AnimationBlendMode.Blend;
        animation["Swim"].blendMode = AnimationBlendMode.Blend;
        StartCoroutine(Swim());
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= minHealth)
        {
            Die();
        }
        if (hunger < minHunger)
        {
            health--;
        }
        else if (hunger > maxHunger / 2)
        {
            //give coins!
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Water")
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    // Allow for backwards swim of some species?
    protected virtual IEnumerator Swim()
    {
        while (true)
        {
            float waitFor = Random.Range(0, 5.0f);
            // pick a new heading
            Vector3 euler = transform.rotation.eulerAngles;
            Quaternion targetRotation = Quaternion.Euler(
                euler.x + Random.Range(-30f, 30f),
                euler.y + Random.Range(-30f, 30f),
                euler.z + Random.Range(-30f, 30f));

            float swimTime = Random.Range(0.5f, 2.0f);
            float t = 0f;

            while (t < swimTime)
            {
                animation.Stop("Idle");
                PlayAnimation("Swim");
                t += Time.deltaTime;
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * speed * 10));
                rb.linearVelocity = speed * transform.forward;

                if (rb.position.y > maxY)
                {
                    Vector3 pos = rb.position;
                    pos.y = maxY;
                    rb.position = pos;
                    Vector3 v = rb.linearVelocity;
                    if (v.y > 0) { v.y = 0; rb.linearVelocity = v; }
                }
                yield return new WaitForFixedUpdate();
            }
            animation.Play("Idle");
            rb.linearVelocity = Utils.Random(Vector3.zero, rb.linearVelocity / 2);
            yield return new WaitForSeconds(waitFor);
            
        }
        
    }

    protected void PlayAnimation(string name)
    {
        if (! animation.isPlaying)
        {
            print("animating " + name);
            animation.Play(name);
        }
        
    }

    protected void Die()
    {
        Destroy(this);
    }
}
