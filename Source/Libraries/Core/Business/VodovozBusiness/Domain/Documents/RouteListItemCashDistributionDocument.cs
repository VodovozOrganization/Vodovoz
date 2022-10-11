using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Documents
{
    [Appellative(Gender = GrammaticalGender.Masculine,
        NominativePlural = "Документы распределения налички на адрес МЛ по юр лицу",
        Nominative = "Документ распределения налички на адрес МЛ по юр лицу")]
    public class RouteListItemCashDistributionDocument : CashOrganisationDistributionDocument
    {
        public virtual string Title => $"Документ распределения налички на адрес МЛ по юр лицу №{Id}";

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