using UnityEngine;
using System.Collections;

public class Fish : MonoBehaviour
{
    protected Rigidbody rb;
    protected Animation animation;
    [SerializeField] protected GameObject target;

    [SerializeField] protected float maxSwimAngle = 0.2f;

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

    [SerializeField] protected float wallCheckDistance = 1f;
    [SerializeField] protected float foodSenseRadius = 5f;

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
        if (hunger < maxHunger -10)
        {
            LookForFood();
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

    protected void LookForFood()
    {
        GameObject best = null;
        float bestDist = foodSenseRadius;

        print(Water.inside.ToString());
        foreach (GameObject c in Water.inside)
        {
            if (c.layer != 6) continue;
            float d = Vector3.Distance(rb.position, c.transform.position);
            if (d < bestDist) 
            { 
                bestDist = d; 
                best = c; 
            }
        }
        target = best;
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
        if (collision.gameObject.layer == 6) // "Food"
        {
            Eat();
        }
    }

    protected Quaternion PickRotation()
    {
        Vector3 dir;
        Vector3 origin = rb.position;

        if (target != null)
        {
            dir = (target.transform.position - rb.position).normalized;
        }
        else
        {
            // pick a random heading
            dir = (transform.forward + Random.insideUnitSphere).normalized;
            dir.y = Mathf.Clamp(dir.y, -maxSwimAngle, maxSwimAngle);
            dir = dir.normalized;
        }

        if (Physics.Raycast(origin, dir, out RaycastHit hit, wallCheckDistance))
        {
            if (hit.collider.gameObject.name == "Water")
            {
                dir = Vector3.Reflect(dir, hit.normal).normalized;
            }
        }

        return Quaternion.LookRotation(dir, Vector3.up);
    }

    // Allow for backwards swim of some species?
    protected virtual IEnumerator Swim()
    {
        while (true)
        {
            float waitFor = Random.Range(0, 5.0f);
            float swimTime = Random.Range(0.5f, 2.0f);
            float t = 0f;

            while (t < swimTime)
            {
                waitFor = (target == null) ? Random.Range(0, 5.0f) : 0;
                Quaternion targetRotation = PickRotation();
                float multiplyFactor = (target == null) ? 2 : 1;
                animation.Stop("Idle"); 
                PlayAnimation("Swim");
                t += Time.deltaTime;
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * speed * multiplyFactor));
                rb.linearVelocity = Random.Range(speed/2, speed * 1.5f) * transform.forward;

                if (rb.position.y > maxY)
                {
                    Vector3 pos = rb.position;
                    pos.y = maxY;
                    rb.position = pos;
                    Vector3 v = rb.linearVelocity;
                    if (v.y > 0) 
                    { 
                        v.y = 0; 
                        rb.linearVelocity = v; 
                    }
                }
                yield return new WaitForFixedUpdate();
            }
            animation.Play("Idle");
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Utils.Random(Vector3.zero, rb.linearVelocity / 2);
            yield return new WaitForSeconds(waitFor);
        }
    }

    protected void PlayAnimation(string name)
    {
        if (! animation.isPlaying)
        {
            animation.Play(name);
        }
    }

    protected void Eat()
    {
        hunger += 5;
        if (hunger > maxHunger)
        {
            hunger = maxHunger;
        }
        Destroy(target);
        target = null;
    }

    protected void Die()
    {
        Destroy(this);
    }
}
