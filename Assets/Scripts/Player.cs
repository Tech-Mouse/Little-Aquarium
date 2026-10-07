using UnityEngine;

public class Player : MonoBehaviour
{

    [SerializeField] private static int coins = 5;

    public static void AddCoins(int howMuch)
    {
        coins += howMuch;
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
}
