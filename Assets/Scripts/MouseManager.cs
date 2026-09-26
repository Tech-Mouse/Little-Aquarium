using UnityEngine;
using UnityEngine.InputSystem;
using Riten.Native.Cursors;

public class MouseManager : MonoBehaviour
{
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero;
    private Camera cam;
    [SerializeField] private LayerMask mask;

    private int currentCursorID;

    protected void Awake() => cam = Camera.main;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentCursorID = CursorStack.Push(NTCursors.Default);
    }

    // Update is called once per frame
    void Update()
    {
        var hovered = RaycastFish();

        // --- hover feedback ---
        if (hovered != null && CursorStack.Peek().id != currentCursorID)
        {
            currentCursorID = CursorStack.Push(NTCursors.Link);
        }
        else if (hovered == null)
        {
            CursorStack.Pop(currentCursorID);   // restore default
        }

        // --- click to remove ---
        if (hovered != null
            && Mouse.current != null
            && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Destroy(hovered.gameObject);
        }
    }

    private Fish RaycastFish()
    {
        if (Mouse.current == null) return null;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, mask))
        {
            Fish s = hit.collider.GetComponentInParent<Fish>();
            if (s != null && s.IsDead) return s;
        }
        return null;
    }
}
