namespace Vodovoz.Services
{
    public interface IOrganizationCashTransferDocumentParametersProvider
    {
        int CashIncomeCategoryTransferId { get; }
        int CashExpenseCategoryTransferId { get; }
    }
}
