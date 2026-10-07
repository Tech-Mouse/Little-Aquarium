using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class FeedBox : MonoBehaviour
{

    private bool isTargetable = true;
    private bool isHeld = false;
    private bool canBeClicked = false;
    private bool isClicked = false;
    private Quaternion originalRotation;
    private Quaternion clickRotation;
    private Coroutine rotationAnimation = null;
    private Coroutine dipAnimation = null;
    private float dipOffset = 0f;
    [SerializeField] private float dipDepth = 0.5f;

    [SerializeField] private float liftHeight = 10;
    [SerializeField] private float maxOffsetX = 20;
    [SerializeField] private float moveSpeed = 1;
    [SerializeField] private float rotationSpeed = 360f;

    private Vector3 originalPosition;
    [SerializeField] private bool followMouseX = true;
    private Camera cam;
    private float startMouseX = 0f;

    private float targetFeedY;

    public bool IsTargetable { get => isTargetable; set => isTargetable = value; }
    public bool IsHeld { get => isHeld; set => isHeld = value; }
    public bool CanBeClicked { get => canBeClicked; set => canBeClicked = value; }
    public bool IsClicked { get => isClicked; set => isClicked = value; }

    private void Awake()
    {
        cam = Camera.main;
        originalPosition = transform.position;
        originalRotation = Quaternion.Euler(-90f, 210f, 0f);
        clickRotation = Quaternion.Euler(45f, -135f, 0f);
    }

    private void StopCoroutineIfRunning()
    {
        if (rotationAnimation != null)
        {
            StopCoroutine(rotationAnimation);
            rotationAnimation = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        if (isHeld)
        {
            if (startMouseX == 0)
            {
                startMouseX = GetMouseScreenX();
            }

            float targetY = originalPosition.y + liftHeight;
            float y = Mathf.Lerp(transform.position.y, targetY,
                Time.deltaTime * moveSpeed);
            if (followMouseX && cam != null)
            {
                float deltaFromStart = GetMouseScreenX() - startMouseX;
                float offset = (deltaFromStart / Screen.width) * GetWorldWidthAtCylinder();
                offset = Mathf.Clamp(offset, -maxOffsetX, 0);

                // Follow axis = camera's right vector, flattened to horizontal,
                // so the object moves the way "left/right" looks on screen.
                Vector3 axis = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
                transform.position = originalPosition
                    + Vector3.up * (y - originalPosition.y)
                    + axis * offset;
                if (transform.position.y >= targetY -1 && transform.position.x >= -9 && transform.position.x < 7.5)
                {
                    canBeClicked = true;
                }
                else
                {
                    canBeClicked = false;
                }
            }
            transform.position = new Vector3(transform.position.x, y + dipOffset,
                transform.position.z);
        }
        else
        {
            MouseUp();
            isTargetable = false;
            canBeClicked = false;
            // Return to the original position the shortest way:
            // a direct straight-line path back.
            transform.position = Vector3.Lerp(transform.position, originalPosition,
                Time.deltaTime * moveSpeed * 6);

            // Snap and stop once we're basically there.
            if (Vector3.Distance(transform.position, originalPosition) < 0.1f)
            {
                transform.position = originalPosition;
                isTargetable = true;
                startMouseX = 0;
            }
        }

    }

    private float GetMouseScreenX()
    {
        return Mouse.current != null
            ? Mouse.current.position.x.ReadValue()
            : startMouseX;
    }
    private float GetWorldWidthAtCylinder()
    {
        float depth = Mathf.Abs(
            cam.transform.InverseTransformPoint(originalPosition).z);
        float frustumHeight = 2f * depth * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float frustumWidth = frustumHeight * cam.aspect;
        return frustumWidth;
    }

    public void MouseDown()
    {
        StopCoroutineIfRunning();
        rotationAnimation = StartCoroutine(RotateTo(clickRotation));
        StartDip();
        Invoke(nameof(DropFeed), 0.2f);
    }

    private void StartDip()
    {
        if (dipAnimation != null) StopCoroutine(dipAnimation);
        dipAnimation = StartCoroutine(DipAnimation());
    }

    private IEnumerator DipAnimation()
    {
        float duration = 0.5f;             // total time down+up
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Min(t + Time.deltaTime / duration, 1f);
            // Sine curve: 0 -> -dipDepth at t=0.25 -> back to 0 at t=0.5.
            dipOffset = -dipDepth * Mathf.Sin(t * Mathf.PI);
            yield return null;
        }
        dipOffset = 0f;
        dipAnimation = null;
    }



    public void MouseUp()
    {
        StopCoroutineIfRunning();
        rotationAnimation = StartCoroutine(RotateTo(originalRotation));
    }


    private void DropFeed()
    {
        if (isHeld)
        {
            if (Utils.FindGameObjectsWithLayer(6).Length >= 20)
            {
                return;
            }
            int howMany = Random.Range(1, 6);

            for(int i = 0; i < howMany; i++)
            {
                int toSpawn = Random.Range(0, 2);
                GameObject feed;
                if (toSpawn == 0)
                {
                    feed = Instantiate(GameObject.Find("Feed1"));
                }
                else
                {
                    feed = Instantiate(GameObject.Find("Feed2"));
                }

                float xDifference = Random.Range(-1f, 1f);

                feed.transform.SetPositionAndRotation(new Vector3(this.transform.position.x - xDifference, this.transform.position.y - 2, this.transform.position.z), new Quaternion());
                feed.GetComponent<Feed>().IsDestroyable = true;
            }
        }
    }

    private IEnumerator RotateTo(Quaternion targetRot)
    {
        Quaternion startRot = transform.rotation;
        float angle = Quaternion.Angle(startRot, targetRot);
        float duration = angle/rotationSpeed; // seconds
        float t = 0f;

        // Animate to completion regardless of input duration.
        while (t < 1f)
        {
            t = Mathf.Min(t + Time.deltaTime / duration, 1f);
            // SmoothStep for ease-in/ease-out feel.
            transform.rotation = Quaternion.Slerp(startRot, targetRot,
                t * t * (3f - 2f * t));
            yield return null;
        }

        transform.rotation = targetRot;
        rotationAnimation = null;
        yield break;
    }
}