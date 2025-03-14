using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Logistic
{
	public partial class RouteListsOnDayViewModel
	{
		/// <summary>
		/// Тип дополнительного параметра фильтра адресов доставки
		/// </summary>
		[Appellative(
			Nominative = "дополнительные параметры фильтра адресов доставки",
			NominativePlural = "дополнительный параметр фильтра адреса доставки")]
		public enum AddressAdditionalParameterType
		{
			/// <summary>
			/// Доставка за час
			/// </summary>
			[Display(Name = "Доставка за час")]
			FastDelivery,

			/// <summary>
			/// Требует скан на складе
			/// </summary>
			[Display(Name = "Требует скан на складе")]
			CodesScanInWarehouseRequired
		}
	}
}
