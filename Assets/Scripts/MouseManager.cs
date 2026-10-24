using UnityEngine;
using UnityEngine.InputSystem;
using Riten.Native.Cursors;
using System;
using UnityEngine.UIElements;

public class MouseManager : MonoBehaviour
{
    private Camera cam;
    [SerializeField] private LayerMask fishMask;
    [SerializeField] private LayerMask feedBoxMask;

    private FeedBox lastHeldBox = null;

    private int currentCursorID;
    private Fish pettingTarget = null;
    private Fish lastHoveredFish = null;
    private float petTimer = 0f;
    private float feedBoxTimer = 0f;

    private float feedBoxClickTimer = 0f;

    [SerializeField] private UIDocument fishBannerUiDocument;
    private VisualElement fishBanner;
    private Label healthLabel;
    private Label happinessLabel;
    private Label hungerLabel;

    private VisualElement healthBar;
    private VisualElement happinessBar;
    private VisualElement hungerBar;



    void Awake()
    {
        cam = Camera.main;
        fishBanner = fishBannerUiDocument.rootVisualElement.Q<VisualElement>("Background");
        healthLabel = fishBanner.Q<Label>("HealthLabel");
        happinessLabel = fishBanner.Q<Label>("HappinessLabel");
        hungerLabel = fishBanner.Q<Label>("HungerLabel");
        healthBar = fishBanner.Q<VisualElement>("HealthBar");
        happinessBar = fishBanner.Q<VisualElement>("HappinessBar");
        hungerBar = fishBanner.Q<VisualElement>("HungerBar");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentCursorID = CursorStack.Push(NTCursors.Default);
    }

    // Update is called once per frame
    void Update()
    {
        if (Shop.IsOpen)
        {
            return;
        }
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
            //CursorStack.Pop(currentCursorID);
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
            //if (hoveredFeedBox != null && CursorStack.Peek().cursor != NTCursors.OpenHand)
            //{
                //currentCursorID = CursorStack.Push(NTCursors.OpenHand);
            //}
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
            if (lastHoveredFish != null)
            {
                fishBanner.style.display = DisplayStyle.None;
                lastHoveredFish = null;
            }
            return;
        }
        else
        {
            fishBanner.style.display = DisplayStyle.Flex;
            lastHoveredFish = hoveredFish;
            FillBannerInfo(hoveredFish);

            // --- hover on dead fish ---
            //if (hoveredFish != null && hoveredFish.IsDead && CursorStack.Peek().cursor != NTCursors.Link)
            //{
                //currentCursorID = CursorStack.Push(NTCursors.Link);
            //}
            // --- hover on undead fish ---
            //else if (hoveredFish != null && !hoveredFish.IsDead && CursorStack.Peek().cursor != NTCursors.OpenHand)
            //{
                //currentCursorID = CursorStack.Push(NTCursors.OpenHand);
            //}
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
                //CursorStack.Pop(currentCursorID);

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



    private void FillBannerInfo(Fish fish)
    {
        hungerLabel.text = fish.Hunger.ToString(".0#");
        healthLabel.text = fish.Health.ToString();
        happinessLabel.text = fish.Happiness.ToString(".0#");

        FillBarInfo(fish);

        Vector3 worldPos = fish.transform.position + Vector3.up * fish.UiHeightOffset;
        Vector2 panelPos = WorldToPanel(worldPos);
        fishBanner.style.left = panelPos.x;
        fishBanner.style.top = panelPos.y;
    }

    private Vector2 WorldToPanel(Vector3 worldPos)
    {
        Vector2 screenPos = cam.WorldToScreenPoint(worldPos);

        // Screen space is bottom-left origin, panel space is top-left, Y flipped
        screenPos.y = Screen.height - screenPos.y;

        // Convert physical pixels to panel coordinates (handles Scale With Screen Size)
        var panelSettings = fishBannerUiDocument.panelSettings;
        return new Vector2(
            screenPos.x * (panelSettings.referenceResolution.x / (float)Screen.width) / panelSettings.scale,
            screenPos.y * (panelSettings.referenceResolution.y / (float)Screen.height) / panelSettings.scale);
    }

    private void FillBarInfo(Fish fish)
    {
        for(int i = 0; i < 3; i++)
        {
            VisualElement bar;
            float value;
            float max;

            if (i == 0)
            {
                bar = hungerBar;
                value = fish.Hunger;
                max = fish.MaxHunger;
            }
            else if (i == 1)
            {
                bar = happinessBar;
                value = fish.Happiness;
                max = fish.MaxHappiness;
            }
            else
            {
                bar = healthBar;
                value = fish.Health;
                max = fish.MaxHealth;
            }

            float clamped = Mathf.Clamp(value, 0f, max);
            float percent = max <= 0f ? 0f : clamped / max * 100f;

            bar.style.width = new Length(percent, LengthUnit.Percent);
            // remove flex-grow so the width isn't overridden by layout
            bar.style.flexGrow = 0f;
        }
        
    }
}
