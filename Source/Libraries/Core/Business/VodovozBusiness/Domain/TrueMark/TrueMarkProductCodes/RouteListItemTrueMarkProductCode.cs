using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Logistic;

namespace VodovozBusiness.Domain.TrueMark.TrueMarkProductCodes
{
	[Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "коды ЧЗ товаров адресов маршрутных листов",
			Nominative = "код ЧЗ товара адреса маршрутного листа")]
	public class RouteListItemTrueMarkProductCode : TrueMarkProductCode
	{
		private RouteListItem _routeListItem;

		[Display(Name = "Адрес маршрутного листа")]
		public virtual RouteListItem RouteListItem
		{
			get => _routeListItem;
			set => SetField(ref _routeListItem, value);
		}
	}
}
