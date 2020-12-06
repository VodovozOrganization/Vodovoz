using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Domain.Documents
{
    public class IncomeCashDistributionDocument : CashOrganisationDistributionDocument
    {
        private Income income;
        [Display (Name = "Приход")]
        public virtual Income Income
        {
            get => income;
            set => SetField(ref income, value);
        }

        public override CashOrganisationDistributionDocType Type =>
            CashOrganisationDistributionDocType.IncomeCashDistributionDoc;
    }
}