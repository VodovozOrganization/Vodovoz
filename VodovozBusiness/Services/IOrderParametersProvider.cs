namespace Vodovoz.Services
{
    public interface IOrderParametersProvider
    {
        int PaymentByCardFromMobileAppId { get; }
        
        int PaymentByCardFromSiteId { get; }
        
        int OldInternalOnlineStoreId { get; }
    }
}