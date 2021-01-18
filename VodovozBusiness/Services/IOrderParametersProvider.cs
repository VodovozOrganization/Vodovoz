namespace Vodovoz.Services
{
    public interface IOrderParametersProvider
    {
        int PaymentByCardFromMobileAppId { get; }
        
        int PaymentByCardFromSiteId { get; }
        
        int PaymentByCardFromSmsId { get; }
        
        int PaymentByCardFromOnlineStoreId { get; }
        
        int OldInternalOnlineStoreId { get; }
        
        int[] PaymentsByCardFromNotToSendSalesReceipts { get; }
    }
}
