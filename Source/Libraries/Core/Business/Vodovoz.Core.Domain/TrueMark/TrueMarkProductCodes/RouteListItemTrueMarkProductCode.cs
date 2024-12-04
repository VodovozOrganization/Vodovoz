using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Logistics;

namespace Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes
{
	[Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "коды ЧЗ товаров адресов маршрутных листов",
			Nominative = "код ЧЗ товара адреса маршрутного листа")]
	public class RouteListItemTrueMarkProductCode : TrueMarkProductCode
	{
		private RouteListItemEntity _routeListItem;

		[Display(Name = "Адрес маршрутного листа")]
		public virtual RouteListItemEntity RouteListItem
		{
			get => _routeListItem;
			set => SetField(ref _routeListItem, value);
		}
	}
}
