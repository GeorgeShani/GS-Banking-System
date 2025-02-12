namespace Banking_System.Communication.Exchange;

public static class StaticExchangeRateManager
{
    public static Dictionary<string, decimal> GetExchangeRates()
    {
        // Static exchange rates relative to GEL (base currency).
        // These rates are hardcoded and should be updated manually as needed.
        // Last updated on 12.02.2025.
        
        return new Dictionary<string, decimal>
        {
            { "GEL", 1m },    // Georgian Lari ₾
            { "USD", 2.81m }, // US Dollar $
            { "EUR", 2.92m }, // Euro €
            { "GBP", 3.50m }, // British Pound £
        };
    }
}