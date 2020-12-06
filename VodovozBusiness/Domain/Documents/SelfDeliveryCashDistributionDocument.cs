using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Documents
{
    public class SelfDeliveryCashDistributionDocument : CashOrganisationDistributionDocument
    {
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