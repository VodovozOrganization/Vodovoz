using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Services;

namespace Vodovoz.Domain.Cash
{
    public class FuelCashOrganisationDistributor
    {
        private readonly ICashDistributionCommonOrganisationProvider cashDistributionCommonOrganisationProvider;

        public FuelCashOrganisationDistributor(ICashDistributionCommonOrganisationProvider cashDistributionCommonOrganisationProvider)
        {
            this.cashDistributionCommonOrganisationProvider =
                cashDistributionCommonOrganisationProvider ?? throw new ArgumentNullException(nameof(cashDistributionCommonOrganisationProvider));
        }

        public void DistributeCash(IUnitOfWork uow, FuelDocument fuelDoc)
        {
            var org = cashDistributionCommonOrganisationProvider.GetCommonOrganisation(uow);
            
            var operation = new OrganisationCashMovementOperation
            {
                Organisation = org,
                OperationTime = fuelDoc.Date,
                Amount = -fuelDoc.PayedForFuel.Value
            };
            
            var fuelCashDistributionDoc = new FuelExpenseCashDistributionDocument
            {
                Author = fuelDoc.Author,
                CashExpenseCategory = fuelDoc.FuelCashExpense.ExpenseCategory,
                CreationDate = fuelDoc.Date,
                Organisation = org,
                FuelDocument = fuelDoc,
                LastEditor = fuelDoc.LastEditor,
                LastEditedTime = fuelDoc.LastEditDate,
                OrganisationCashMovementOperation = operation,
                Amount = operation.Amount
            };
            
            uow.Save(operation);
            uow.Save(fuelCashDistributionDoc);
        }
    }
}