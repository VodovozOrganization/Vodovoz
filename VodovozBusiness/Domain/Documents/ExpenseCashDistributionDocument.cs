using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Domain.Documents
{
    [Appellative(Gender = GrammaticalGender.Masculine,
        NominativePlural = "Документы расхода налички по юр лицу",
        Nominative = "Документ расхода налички по юр лицу")]
    public class ExpenseCashDistributionDocument : CashOrganisationDistributionDocument
    {
        public virtual string Title => $"Документ расхода налички по юр лицу №{Id}";

        public override CashOrganisationDistributionDocType Type =>
            CashOrganisationDistributionDocType.ExpenseCashDistributionDoc;
    }
}