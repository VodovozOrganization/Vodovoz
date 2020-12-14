using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Domain.Documents
{
    [Appellative(Gender = GrammaticalGender.Masculine,
        NominativePlural = "Документы расхода налички авансового отчета по юр лицу",
        Nominative = "Документ расхода налички авансового отчета по юр лицу")]
    public class AdvanceExpenseCashDistributionDocument : CashOrganisationDistributionDocument
    {
        private AdvanceReport advanceReport;
        [Display (Name = "Авансовый отчет")]
        public virtual AdvanceReport AdvanceReport
        {
            get => advanceReport;
            set => SetField(ref advanceReport, value);
        }

        public override CashOrganisationDistributionDocType Type =>
            CashOrganisationDistributionDocType.AdvanceExpenseCashDistributionDoc;
    }
}