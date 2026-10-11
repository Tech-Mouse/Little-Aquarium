using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] private float degreesPerSec = 2;

    private float fastThreshold = 180f; // angle where it speeds up
    private float stopAngle = 365f;     // final angle

    private float currentAngle = 5f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float speed = currentAngle < fastThreshold ? degreesPerSec : 20;

        // Advance the angle
        currentAngle += speed * Time.deltaTime;

        // Clamp at the end and stop
        if (currentAngle >= stopAngle)
        {
            currentAngle = 5f;
        }

        // Apply rotation around the local X axis
        transform.rotation = Quaternion.Euler(currentAngle, -30f, 0f);
    }



}
