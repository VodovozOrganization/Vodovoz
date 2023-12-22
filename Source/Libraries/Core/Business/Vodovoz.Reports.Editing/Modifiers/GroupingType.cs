using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Reports.Editing.Modifiers
{
	public enum GroupingType
	{
		[Display(Name = "Заказ")]
		Order,
		[Display(Name = "Контрагент")]
		Counterparty,
		[Display(Name = "Подразделение")]
		Subdivision,
		[Display(Name = "Дата доставки")]
		DeliveryDate,
		[Display(Name = "Маршрутный лист")]
		RouteList,
		[Display(Name = "Номенклатура")]
		Nomenclature,
		[Display(Name = "Тип номенклатуры")]
		NomenclatureType,
		[Display(Name = "Группа номенклатур")]
		NomenclatureGroup,
		[Display(Name = "Группа номенклатур 1")]
		NomenclatureGroup1,
		[Display(Name = "Группа номенклатур 2")]
		NomenclatureGroup2,
		[Display(Name = "Группа номенклатур 3")]
		NomenclatureGroup3,
		[Display(Name = "Тип контрагента")]
		CounterpartyType,
		[Display(Name = "Тип оплаты")]
		PaymentType,
		[Display(Name = "Организация")]
		Organization,
		[Display(Name = "Классификация контрагента")]
		CounterpartyClassification,
		[Display(Name = "Промонабор")]
		PromotionalSet
	}
}
