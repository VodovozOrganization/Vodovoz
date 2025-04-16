using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.ReportsParameters.Logistic
{
	
	[Appellative(
		Nominative = "Тип принадлежности МЛ",
		NominativePlural = "Типы принадлежности МЛ")]
	public enum RouteListOwnType
	{
		[Display(Name = "Доставка")]
		Delivery,
		[Display(Name = "Сетевой магазин")]
		ChainStore,
		[Display(Name = "СЦ")]
		ServiceCenter,
		[Display(Name = "Фуры")]
		Trucks,
		[Display(Name = "Складская логистика")]
		StorageLogistics
	}
}
