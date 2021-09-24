namespace Vodovoz.Services
{
	public interface ISalesReceiptsServiceSettings
	{
		int MaxUnsendedCashReceiptsForWorkingService { get; }
		int DefaultSalesReceiptCashierId { get; }
	}
}
