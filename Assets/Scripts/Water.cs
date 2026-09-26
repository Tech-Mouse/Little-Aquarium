using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{

    public static List<GameObject> inside = new();

    void OnTriggerEnter(Collider c) => inside.Add(c.gameObject);
    void OnTriggerExit(Collider c) => inside.Remove(c.gameObject);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (inside.Contains(null) )
        {
            inside.Remove(null);
        }
    }
}
