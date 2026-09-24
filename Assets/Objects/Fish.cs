using UnityEngine;

public class Fish : MonoBehaviour
{
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        swim();
    }

    // Allow for backwards swim of some species?
    protected virtual void swim()
    {
        
    }
}
