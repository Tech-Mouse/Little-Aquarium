using UnityEngine;
using UnityEngine.InputSystem;
using Riten.Native.Cursors;

public class MouseManager : MonoBehaviour
{
    private Camera cam;
    [SerializeField] private LayerMask fishMask;

    private int currentCursorID;
    private Fish pettingTarget = null;
    private float petTimer = 0f;

    void Awake() => cam = Camera.main;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentCursorID = CursorStack.Push(NTCursors.Default);
    }

    // Update is called once per frame
    void Update()
    {
        Fish hovered = RaycastFish();

        bool isLMBPressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool isLMBHeld = Mouse.current != null && Mouse.current.leftButton.isPressed;

        // --- hover on dead fish ---
        if (hovered != null && hovered.IsDead && CursorStack.Peek().id != currentCursorID)
        {
            currentCursorID = CursorStack.Push(NTCursors.Link);
        }
        // --- hover on undead fish ---
        else if (hovered != null && !hovered.IsDead && CursorStack.Peek().id != currentCursorID)
        {
            currentCursorID = CursorStack.Push(NTCursors.OpenHand);
        }
        else if (hovered == null)
        {
            CursorStack.Pop(currentCursorID);   // restore default
        }

        // --- pet the fish ---
        if (hovered != null && !hovered.IsDead && isLMBHeld)
        {

            if (pettingTarget == null)
            {
                pettingTarget = hovered;
                pettingTarget.StopSwimming();
            }
        }

        if (pettingTarget != null)
        {
            bool stillValid = isLMBHeld && hovered == pettingTarget && !pettingTarget.IsDead;

            if (stillValid)
            {
                petTimer += Time.deltaTime;

                if (petTimer >= 1f)
                {
                    petTimer -= 1f;
                    hovered.Pet();
                }
            }
            else
            {
                pettingTarget.StartSwimming();
                pettingTarget = null;
                petTimer = 0f;
                CursorStack.Pop(currentCursorID);
                
            }
        }

        // --- click to remove dead fish ---
        if (hovered != null && hovered.IsDead && isLMBPressed)
        {
            Destroy(hovered.gameObject);
        }
    }

    private Fish RaycastFish()
    {
        if (Mouse.current == null) return null;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, fishMask))
        {
            Fish s = hit.collider.GetComponentInParent<Fish>();
            if (s != null) return s;
        }
        return null;
    }
}
