public static class PlayerUpgrades
{
    public static float DamageMultiplier = 1f;
    public static float SpeedBonus = 0f;
    public static bool HealOnNextSpawn = false;
    public static float HealAmount = 0.5f;

    public static int HealPurchases = 0;
    public static int DamagePurchases = 0;
    public static int SpeedPurchases = 0;

    public static float HealMultiplier = 0.01f; // базовый 1%

    public static void Reset()
    {
        DamageMultiplier = 1f;
        SpeedBonus = 0f;
        HealMultiplier = 0.01f;
        HealPurchases = 0;
        DamagePurchases = 0;
        SpeedPurchases = 0;
    }
}