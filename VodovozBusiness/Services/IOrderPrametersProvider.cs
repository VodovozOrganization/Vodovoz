namespace Vodovoz.Services
{
    public interface IOrderPrametersProvider
    {
        int PaymentByCardFromMobileAppId { get; }
        
        int PaymentByCardFromSiteId { get; }
    }
}