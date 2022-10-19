using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Documents
{
    [Appellative(Gender = GrammaticalGender.Masculine,
        NominativePlural = "Документы прихода налички по юр лицу",
        Nominative = "Документ прихода налички по юр лицу")]
    public class IncomeCashDistributionDocument : CashOrganisationDistributionDocument
    {
        public virtual string Title => $"Документ прихода налички по юр лицу №{Id}";

        public override CashOrganisationDistributionDocType Type =>
            CashOrganisationDistributionDocType.IncomeCashDistributionDoc;
    }
}