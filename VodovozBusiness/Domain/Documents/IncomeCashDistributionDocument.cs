using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Domain.Documents
{
    [Appellative(Gender = GrammaticalGender.Masculine,
        NominativePlural = "Документы прихода налички по юр лицу",
        Nominative = "Документ прихода налички по юр лицу")]
    public class IncomeCashDistributionDocument : CashOrganisationDistributionDocument
    {
        public virtual string Title => $"Документ прихода налички по юр лицу №{Id}";
        
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