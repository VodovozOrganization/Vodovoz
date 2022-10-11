using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Documents
{
    [Appellative(Gender = GrammaticalGender.Masculine,
        NominativePlural = "Документы распределения самовывоза налички по юр лицу",
        Nominative = "Документ распределения налички самовывоза по юр лицу")]
    public class SelfDeliveryCashDistributionDocument : CashOrganisationDistributionDocument
    {
        public virtual string Title => $"Документ распределения налички самовывоза по юр лицу №{Id}";
        
        private Order order;
        [Display (Name = "Заказ")]
        public virtual Order Order
        {
            get => order;
            set => SetField(ref order, value);
        }

        public override CashOrganisationDistributionDocType Type =>
            CashOrganisationDistributionDocType.SelfDeliveryCashDistributionDoc;
    }
}