public static class PlayerUpgrades
{
    public static float DamageMultiplier = 1f;
    public static float SpeedBonus = 0f;
    public static bool HealOnNextSpawn = false;

    public static void Reset()
    {
        DamageMultiplier = 1f;
        SpeedBonus = 0f;
        HealOnNextSpawn = false;
    }
}