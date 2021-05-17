using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
    public class OrganisationCashTransferDocumentCashCategoryParametersProvider : IOrganisationCashTransferDocumentCashCategoryParametersProvider
    {
        private readonly IParametersProvider parametersProvider;

        public OrganisationCashTransferDocumentCashCategoryParametersProvider(IParametersProvider parametersProvider)
        {
            this.parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
        }

        public int CashIncomeCategoryTransferId => parametersProvider.GetIntValue("cash_income_category_transfer_id");

        public int CashExpenseCategoryTransferId => parametersProvider.GetIntValue("cash_expense_category_transfer_id");
    }
}
