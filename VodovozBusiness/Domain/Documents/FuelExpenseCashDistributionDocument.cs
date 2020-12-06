using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Documents
{
    public class FuelExpenseCashDistributionDocument : CashOrganisationDistributionDocument
    {
        private FuelDocument fuelDocument;
        [Display (Name = "Документ выдачи топлива")]
        public virtual FuelDocument FuelDocument
        {
            get => fuelDocument;
            set => SetField(ref fuelDocument, value);
        }

        public override CashOrganisationDistributionDocType Type =>
            CashOrganisationDistributionDocType.FuelExpenseCashOrgDistributionDoc;
    }
}