using UnityEngine;
using UnityEngine.InputSystem;
using Riten.Native.Cursors;
using System;

public class MouseManager : MonoBehaviour
{
    private Camera cam;
    [SerializeField] private LayerMask fishMask;
    [SerializeField] private LayerMask feedBoxMask;

    private FeedBox lastHeldBox = null;

    private int currentCursorID;
    private Fish pettingTarget = null;
    private float petTimer = 0f;
    private float feedBoxTimer = 0f;

    private float feedBoxClickTimer = 0f;

    void Awake() => cam = Camera.main;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentCursorID = CursorStack.Push(NTCursors.Default);
    }

    // Update is called once per frame
    void Update()
    {
        Fish hoveredFish = RaycastFish();
        FeedBox hoveredFeedBox = null;

        if (hoveredFish == null)
        {
            hoveredFeedBox = RaycastFeedbox();
        }

        bool isLMBPressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool isLMBHeld = Mouse.current != null && Mouse.current.leftButton.isPressed;

        HandleFishMouseEvents(hoveredFish, isLMBPressed, isLMBHeld);
        HandleFeedBoxMouseEvents(hoveredFeedBox, isLMBPressed, isLMBHeld);

        if ((hoveredFeedBox == null && lastHeldBox == null) && hoveredFish == null)
        {
            feedBoxTimer = 0f;
            CursorStack.Pop(currentCursorID);
        }
        
    }

    private void HandleFeedBoxMouseEvents(FeedBox hoveredFeedBox, bool isLMBPressed, bool isLMBHeld)
    {
        if (hoveredFeedBox == null)
        {
            if (lastHeldBox != null)
            {
                feedBoxTimer += Time.deltaTime;
                if (feedBoxTimer >= 1f)
                {
                    lastHeldBox.IsHeld = false;
                    lastHeldBox = null;
                }
            }
            return;
        }
        else
        {
            if (hoveredFeedBox != null && CursorStack.Peek().cursor != NTCursors.OpenHand)
            {
                currentCursorID = CursorStack.Push(NTCursors.OpenHand);
            }
            if (hoveredFeedBox != null && lastHeldBox != null && isLMBPressed && hoveredFeedBox.CanBeClicked)
            {
                if (! hoveredFeedBox.IsClicked)
                {
                    hoveredFeedBox.IsClicked = true;
                }
                hoveredFeedBox.MouseDown();
                feedBoxClickTimer = 0;

            }
            if (hoveredFeedBox != null && lastHeldBox != null && ! isLMBPressed )
            {
                feedBoxClickTimer += Time.deltaTime;
                if (hoveredFeedBox.IsClicked && feedBoxClickTimer >= 1f)
                {
                    hoveredFeedBox.IsClicked = false;
                    hoveredFeedBox.MouseUp();
                    feedBoxClickTimer = 0;
                }
                
            }
            if (hoveredFeedBox != null && isLMBHeld && hoveredFeedBox.IsTargetable)
            {
                hoveredFeedBox.IsHeld = true;
                lastHeldBox = hoveredFeedBox;
            }
            
        }
    }

    private void HandleFishMouseEvents(Fish hoveredFish, bool isLMBPressed, bool isLMBHeld)
    {

        if (hoveredFish == null)
        {
            return;
        }
        else
        {
            // --- hover on dead fish ---
            if (hoveredFish != null && hoveredFish.IsDead && CursorStack.Peek().cursor != NTCursors.Link)
            {
                currentCursorID = CursorStack.Push(NTCursors.Link);
            }
            // --- hover on undead fish ---
            else if (hoveredFish != null && !hoveredFish.IsDead && CursorStack.Peek().cursor != NTCursors.OpenHand)
            {
                currentCursorID = CursorStack.Push(NTCursors.OpenHand);
            }
        }

        // --- pet the fish ---
        if (hoveredFish != null && !hoveredFish.IsDead && isLMBHeld)
        {

            if (pettingTarget == null)
            {
                pettingTarget = hoveredFish;
                pettingTarget.StopSwimming();
            }
        }

        if (pettingTarget != null)
        {
            bool stillValid = isLMBHeld && hoveredFish == pettingTarget && !pettingTarget.IsDead;

            if (stillValid)
            {
                petTimer += Time.deltaTime;

                if (petTimer >= 1f)
                {
                    petTimer -= 1f;
                    hoveredFish.Pet();
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
        if (hoveredFish != null && hoveredFish.IsDead && isLMBPressed)
        {
            Destroy(hoveredFish.gameObject);
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

    private FeedBox RaycastFeedbox()
    {
        if (Mouse.current == null) return null;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, feedBoxMask))
        {
            FeedBox s = hit.collider.GetComponentInParent<FeedBox>();
            if (s != null) return s;
        }
        return null;
    }
}
