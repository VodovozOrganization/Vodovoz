namespace Vodovoz.Services
{
    public interface IOrganisationCashTransferDocumentCashCategoryParametersProvider
    {
        int CashIncomeCategoryTransferId { get; }
        int CashExpenseCategoryTransferId { get; }
    }
}
