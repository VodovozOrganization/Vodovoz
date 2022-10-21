using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Domain.Documents
{
    [Appellative(Gender = GrammaticalGender.Masculine,
        NominativePlural = "Документы прихода налички авансового отчета по юр лицу",
        Nominative = "Документ прихода налички авансового отчета по юр лицу")]
    public class AdvanceIncomeCashDistributionDocument : CashOrganisationDistributionDocument
    {
        private AdvanceReport advanceReport;
        [Display (Name = "Авансовый отчет")]
        public virtual AdvanceReport AdvanceReport
        {
            get => advanceReport;
            set => SetField(ref advanceReport, value);
        }

        public override CashOrganisationDistributionDocType Type =>
            CashOrganisationDistributionDocType.AdvanceIncomeCashDistributionDoc;
    }
}