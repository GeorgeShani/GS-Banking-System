namespace Banking_System.Communication.Exchange;

public static class StaticExchangeRateManager
{
    public static Dictionary<string, decimal> GetExchangeRates()
    {
        // Static exchange rates relative to GEL (base currency).
        // These rates are hardcoded and should be updated manually as needed.
        // Last updated on 29.01.2025.
        
        return new Dictionary<string, decimal>
        {
            { "GEL", 1m },    // Georgian Lari ₾
            { "USD", 2.88m }, // US Dollar $
            { "EUR", 2.99m }, // Euro €
            { "GBP", 3.58m }, // British Pound £
        };
    }
}