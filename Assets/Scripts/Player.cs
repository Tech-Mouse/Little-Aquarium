using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{

    [SerializeField] private static int coins = 5;
    [SerializeField] private UIDocument coinIndicator;
    private VisualElement coinIndicatorElem;
    private Label coinlabel;
    private Label numOfFishLabel;
    private Label maxNumOfFishLabel;

    private static int numOfFish = 0;
    private static int maxNumOfFish = 10;

    public static int MaxNumOfFish {get => maxNumOfFish; }
    public static int Coins { get => coins; }

    private void Awake()
    {
        coinIndicatorElem = coinIndicator.rootVisualElement.Q<VisualElement>("VisualElement");
        coinlabel = coinIndicatorElem.Q<Label>("Coins");
        numOfFishLabel = coinIndicatorElem.Q<Label>("CurrentFish");
        maxNumOfFishLabel = coinIndicatorElem.Q<Label>("MaxFish");
    }

    private void Update()
    {
        UpdateNumOfFish();
        coinlabel.text = coins.ToString();
        numOfFishLabel.text = numOfFish.ToString();
        maxNumOfFishLabel.text = maxNumOfFish.ToString();

    }

    public static void AddCoins(int howMuch)
    {
        if (coins < 10000)
        {
            coins += howMuch;
        }
        else
        {
            coins = 10000;
        }
        
    }

    public static bool HasEnoughCoins(int compareTo)
    {
        return coins >= compareTo;
    }

    public static bool DecreaseCoins(int howMuch)
    {
        if (HasEnoughCoins(howMuch))
        {
            coins -= howMuch;
            return true;
        }
        return false;
        
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        coins = 5;
    }

    private static void UpdateNumOfFish()
    {
        numOfFish = Water.inside.FindAll(obj => obj.layer == 7).Count;
    }

    public static bool CanBuyFish()
    {
        return numOfFish < maxNumOfFish;
    }
}
