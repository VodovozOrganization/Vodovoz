using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Domain.Documents
{
    public class ExpenseCashDistributionDocument : CashOrganisationDistributionDocument
    {
        private Expense expense;
        [Display (Name = "Расход")]
        public virtual Expense Expense
        {
            get => expense;
            set => SetField(ref expense, value);
        }

        public override CashOrganisationDistributionDocType Type =>
            CashOrganisationDistributionDocType.ExpenseCashDistributionDoc;
    }
}