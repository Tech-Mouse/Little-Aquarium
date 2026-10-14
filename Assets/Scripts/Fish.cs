using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

public class Fish : MonoBehaviour
{
    [SerializeField] protected bool isValid = true;

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
    [SerializeField] protected float hunger = 50;
    [SerializeField] protected float happiness = 50;
    [SerializeField] protected float happinessPettingDeltaPerSec = 2f;
    [SerializeField] protected float happinessDeltaPerSec = 0.2f;
    [SerializeField] protected float hungerDeltaPerSec = 0.2f;
    [SerializeField] protected int coinsPerSec = 2;

    [SerializeField] protected float maxY = 3;
    [SerializeField] protected float speed = 1;

    [SerializeField] protected float wallCheckDistance = 1f;
    [SerializeField] protected float foodSenseRadius = 5f;

    protected float uiHeightOffset = 3;

    protected bool isDead = false;
    protected bool stopSwim = false;

    protected float hungerTimer = 0;

    public bool IsDead { get => isDead; set => isDead = value; }
    public float Happiness { get => happiness; }
    public float MaxHappiness { get => maxHappiness; }
    public float Hunger { get => hunger; }
    public float MaxHunger { get => maxHunger; }
    public int CoinsPerSec { get => coinsPerSec; }
    public int Health { get => health; }
    public int MaxHealth { get => maxHealth;  }
    public bool IsValid { get => isValid; set => isValid = value; }
    public float UiHeightOffset { get => uiHeightOffset; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (! isValid)
        {
            return;
        }
        animation = gameObject.GetComponent<Animation>();
        animation["Idle"].blendMode = AnimationBlendMode.Blend;
        animation["Swim"].blendMode = AnimationBlendMode.Blend;
        StartCoroutine(Swim());
    }

    // Update is called once per frame
    void Update()
    {
        if (! isValid)
        {
            return;
        }

        if (health <= minHealth)
        {
            Die();
            return;
        }
        if (hunger < maxHunger -10)
        {
            LookForFood();
        }

        hungerTimer += Time.deltaTime;

        if (hungerTimer >= 1f)
        {
            hungerTimer -= 1f;
            if (hunger <= minHunger)
            {
                health--;
            }
            if (hunger >= maxHunger / 2)
            {
                if (health < maxHealth)
                {
                    health++;
                }
            }
            DecreaseHunger(hungerDeltaPerSec);
            DecreaseHappiness(happinessDeltaPerSec);

            if (happiness >= maxHappiness / 2)
            {
                Player.AddCoins(coinsPerSec);
            }
        }
        
        
    }

    protected void LookForFood()
    {
        GameObject best = null;
        float bestDist = foodSenseRadius;

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
                if (IsDead || stopSwim)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    StopCoroutine(Swim());
                    yield break;
                }
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
        IncreaseHappiness(2);
        IncreaseHunger(5);
        
        Destroy(target);
        target = null;
    }

    protected IEnumerator DieAnimation()
    {
        Quaternion upsideDown = Quaternion.Euler(0f, transform.eulerAngles.y, 180f);

        while (true)
        {
            float distToSurface = maxY - rb.position.y;

            // float up slowly, ease out near the surface
            float riseSpeed = Mathf.Clamp(distToSurface * 0.5f, 0.05f, 0.3f);
            Vector3 v = rb.linearVelocity;
            v.y = riseSpeed;
            rb.linearVelocity = v;

            // smoothly roll belly-up while rising
            rb.MoveRotation(Quaternion.Slerp(
                rb.rotation, upsideDown, Time.deltaTime * 2f));

            // gentle horizontal drift damping — dead fish doesn't steer
            v = rb.linearVelocity;
            v.x *= 0.98f; v.z *= 0.98f;
            rb.linearVelocity = v;

            if (distToSurface < 0.05f)
            {
                // arrived at surface: stop everything, lock the state
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.freezeRotation = true;      // nothing can knock it around anymore
                yield break;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    protected void Die()
    {
        IsDead = true;
        animation.Stop();
        StartCoroutine(DieAnimation());
    }

    void OnDestroy()
    {
        Water.inside.Remove(this.gameObject);
    }

    protected void IncreaseHunger(float howMuch)
    {
        hunger += howMuch;
        if (hunger > maxHunger)
        {
            hunger = maxHunger;
        }
    }

    protected void DecreaseHunger(float howMuch)
    {
        hunger -= howMuch;
        if (hunger < minHunger)
        {
            hunger = minHunger;
        }
    }

    protected void IncreaseHappiness(float howMuch)
    {
        happiness += howMuch;
        if (happiness > maxHappiness)
        {
            happiness = maxHappiness;
        }
    }

    protected void DecreaseHappiness(float howMuch)
    {
        happiness -= howMuch;
        if (happiness < minHappiness)
        {
            happiness = minHappiness;
        }
    }

    public void Pet()
    {
        IncreaseHappiness(happinessPettingDeltaPerSec);
    }

    public void StopSwimming()
    {
        stopSwim = true;
    }

    public void StartSwimming()
    {
        stopSwim = false;
        StartCoroutine(Swim());
    }
}
