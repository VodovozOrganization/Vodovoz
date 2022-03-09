namespace Vodovoz.Services
{
    public interface IOrderParametersProvider
    {
        int PaymentByCardFromMobileAppId { get; }
		int PaymentByCardFromSiteId { get; }
		int PaymentByCardFromSmsId { get; }
		int PaymentByCardFromOnlineStoreId { get; }
		int PaymentFromTerminalId { get; }
		int OldInternalOnlineStoreId { get; }
		int GetPaymentByCardFromMarketplaceId { get; }
		int GetPaymentByCardFromFastPaymentServiceId { get; }
		int[] PaymentsByCardFromNotToSendSalesReceipts { get; }
		int[] PaymentsByCardFromForNorthOrganization { get; }
		int GetDiscountReasonStockBottle10PercentsId { get; }
		int GetDiscountReasonStockBottle20PercentsId { get; }
    }
}
