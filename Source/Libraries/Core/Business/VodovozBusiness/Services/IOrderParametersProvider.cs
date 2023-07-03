namespace Vodovoz.Services
{
    public interface IOrderParametersProvider
    {
        int PaymentByCardFromMobileAppId { get; }
		int PaymentByCardFromSiteId { get; }
		int PaymentByCardFromSmsId { get; }
		int PaymentByCardFromOnlineStoreId { get; }
		int PaymentFromTerminalId { get; }
		int PaymentFromSmsYuKassaId { get; }
		int OldInternalOnlineStoreId { get; }
		int GetPaymentByCardFromMarketplaceId { get; }
		int GetPaymentByCardFromFastPaymentServiceId { get; }
		int GetPaymentByCardFromAvangardId { get; }
		int GetPaymentByCardFromSiteByQrCodeId { get; }
		int GetPaymentByCardFromMobileAppByQrCodeId { get; }
		int GetPaymentByCardFromKulerSaleId { get; }
		int[] PaymentsByCardFromNotToSendSalesReceipts { get; }
		int[] PaymentsByCardFromForNorthOrganization { get; }
		int[] PaymentsByCardFromAvangard { get; }
		int GetDiscountReasonStockBottle10PercentsId { get; }
		int GetDiscountReasonStockBottle20PercentsId { get; }
    }
}
