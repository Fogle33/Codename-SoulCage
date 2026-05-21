using System;

public static class GameState
{
    public static int Souls { get; private set; }
    public static int Scrap { get; private set; }

    public static event Action OnSoulsChanged;
    public static event Action OnScrapChanged;
    public static string NextScene = "Arena2";

    public static void AddSouls(int amount)
    {
        if (amount <= 0) return;
        Souls += amount;
        OnSoulsChanged?.Invoke();
    }

    public static void AddScrap(int amount)
    {
        if (amount <= 0) return;
        Scrap += amount;
        OnScrapChanged?.Invoke();
    }

    public static bool SpendSouls(int amount)
    {
        if (amount <= 0 || Souls < amount) return false;
        Souls -= amount;
        OnSoulsChanged?.Invoke();
        return true;
    }

    public static bool SpendScrap(int amount)
    {
        if (amount <= 0 || Scrap < amount) return false;
        Scrap -= amount;
        OnScrapChanged?.Invoke();
        return true;
    }

    public static void ResetAll()
    {
        Souls = 0;
        Scrap = 0;
        OnSoulsChanged?.Invoke();
        OnScrapChanged?.Invoke();
    }
}