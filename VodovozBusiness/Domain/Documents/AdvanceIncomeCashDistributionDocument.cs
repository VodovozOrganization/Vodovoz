using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Domain.Documents
{
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