using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Documents
{
    public class RouteListItemCashDistributionDocument : CashOrganisationDistributionDocument
    {
        private RouteListItem routeListItem;
        [Display (Name = "Строка маршрутного листа")]
        public virtual RouteListItem RouteListItem
        {
            get => routeListItem;
            set => SetField(ref routeListItem, value);
        }

        public override CashOrganisationDistributionDocType Type =>
            CashOrganisationDistributionDocType.RouteListItemCashDistributionDoc;
    }
}