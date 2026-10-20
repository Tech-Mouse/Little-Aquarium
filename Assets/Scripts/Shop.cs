using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class Shop : MonoBehaviour
{

    [SerializeField] private UIDocument shop;
    [SerializeField] private Vector3 defaultSpawnPosition = new Vector3();
    private StyleLength closedLength = new Length(50, LengthUnit.Percent);
    private VisualElement shopElem;
    private VisualElement shopList;
    private Button shopHandle;

    private static bool isOpen = false;

    [SerializeField] private List<ShopItem> items = new List<ShopItem>();

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip buySound;
    [SerializeField] private AudioClip openSound;

    public static bool IsOpen { get => isOpen; } 


    [System.Serializable]
    public class ShopItem
    {
        public string itemName;
        public int itemCost;
        public GameObject prefab;
        public Material material;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnEnable()
    {
        shopElem = shop.rootVisualElement.Q<VisualElement>("VisualElement");
        shopList = shopElem.Q<VisualElement>("ShopList");
        shopHandle = shopElem.Q<Button>("Handle");
        shopElem.style.position = Position.Absolute;
        shopElem.style.left = closedLength;
        PopulateShop();
        SetupShopHandle();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void PopulateShop()
    {
        foreach(ShopItem item in items)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.SpaceEvenly;
            row.style.marginBottom = 10;

            var label = new Label($"{item.itemName}");
            var labelCost = new Label($"{item.itemCost}");
            label.style.fontSize = 25;
            label.style.color = Color.black;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.flexGrow = 0;

            labelCost.style.fontSize = 25;
            labelCost.style.color = Color.black;
            labelCost.style.unityTextAlign = TextAnchor.MiddleLeft;
            labelCost.style.flexGrow = 0;

            var buyButton = new Button(() => TryBuy(item)) { text = "Buy" };
            buyButton.style.fontSize = 25;
            buyButton.style.backgroundColor = Color.cornsilk;
            buyButton.style.borderBottomLeftRadius = 10;
            buyButton.style.borderBottomRightRadius = 10;
            buyButton.style.borderTopLeftRadius = 10;
            buyButton.style.borderTopRightRadius = 10;

            row.Add(label);
            row.Add(labelCost);
            row.Add(buyButton);
            shopList.Add(row);
        }
    }

    private void SetupShopHandle()
    {
        // ---- Wire up toggle ----
        shopHandle.clicked += ToggleShop;
    }

    private void ToggleShop()
    {
        sfxSource.Stop();
        sfxSource.clip = openSound;
        sfxSource.Play();

        isOpen = !isOpen;
        // Open: X = 0. Closed: X = 90% of parent width (top-right corner minus ~10%)
        shopElem.style.left = isOpen
            ? new StyleLength(0f)
            : closedLength;
    }

    private void TryBuy(ShopItem item)
    {
        if (! Player.HasEnoughCoins(item.itemCost))
        {
            Debug.Log($"Not enough gold for {item.itemName}!");
            return;
        }
        if (! Player.CanBuyFish())
        {
            return;
        }
        Player.DecreaseCoins(item.itemCost);

        // ---- Spawn the purchased item ----
        if (item.prefab != null)
        {
            Vector3 spawnPos = new Vector3(
                Random.Range(defaultSpawnPosition.x -1, defaultSpawnPosition.x +1), 
                Random.Range(defaultSpawnPosition.y -1, defaultSpawnPosition.y +1), 
                Random.Range(defaultSpawnPosition.z -1, defaultSpawnPosition.z +1));

            sfxSource.Stop();
            sfxSource.clip = buySound;
            sfxSource.Play();

            GameObject obj = Instantiate(item.prefab, spawnPos, Quaternion.identity);
            if (item.itemName.StartsWith("Blowfish"))
            {
                obj.GetComponent<Blowfish>().IsValid = true;
            }
            else
            {
                obj.GetComponent<Fish>().IsValid = true;
            }
            
            //obj.GetComponent<MeshRenderer>().sharedMaterial = item.material;
        }
        else
        {
            Debug.LogWarning($"ShopItem '{item.itemName}' has no prefab assigned.");
        }
    }
}
