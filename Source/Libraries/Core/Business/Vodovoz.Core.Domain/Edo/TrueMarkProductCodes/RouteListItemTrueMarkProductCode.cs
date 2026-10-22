using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes
{
	[Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "коды ЧЗ товаров адресов маршрутных листов",
			Nominative = "код ЧЗ товара адреса маршрутного листа")]
	public class RouteListItemTrueMarkProductCode : TrueMarkProductCode
	{
		private int? _routeListItemId;

		[Display(Name = "Id адреса маршрутного листа")]
		public virtual int? RouteListItemId
		{
			get => _routeListItemId;
			set => SetField(ref _routeListItemId, value);
		}
	}
}
