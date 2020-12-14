using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Documents
{
    [Appellative(Gender = GrammaticalGender.Masculine,
        NominativePlural = "Документы расхода налички на топливо по юр лицу",
        Nominative = "Документ расхода налички на топливо по юр лицу")]
    public class FuelExpenseCashDistributionDocument : CashOrganisationDistributionDocument
    {
        public virtual string Title => $"Документ расхода налички на топливо по юр лицу №{Id}";
        
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