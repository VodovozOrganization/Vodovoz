using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Goods
{
	/// <summary>
	/// Типы номенклатур
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Типы номенклатур",
		Nominative = "Тип номенклатуры",
		GenitivePlural = "Типов номенклатур")]
	public enum NomenclatureCategory
	{
		/// <summary>
		/// Вода
		/// </summary>
		[Display(Name = "Вода")]
		water,
		/// <summary>
		/// Залог
		/// </summary>
		[Display(Name = "Залог")]
		deposit,
		/// <summary>
		/// Запчасти сервисного центра
		/// </summary>
		[Display(Name = "Запчасти сервисного центра")]
		spare_parts,
		/// <summary>
		/// Оборудование
		/// </summary>
		[Display(Name = "Оборудование")]
		equipment,
		/// <summary>
		/// Товары
		/// </summary>
		[Display(Name = "Товары")]
		additional,
		/// <summary>
		/// Услуга
		/// </summary>
		[Display(Name = "Услуга")]
		service,
		/// <summary>
		/// Тара
		/// </summary>
		[Display(Name = "Тара")]
		bottle,
		/// <summary>
		/// Сырьё
		/// </summary>
		[Display(Name = "Сырьё")]
		material,
		/// <summary>
		/// Выезд мастера
		/// </summary>
		[Display(Name = "Выезд мастера")]
		master,
		/// <summary>
		/// Топливо
		/// </summary>
		[Display(Name = "Топливо")]
		fuel,
		/// <summary>
		/// Кассовое оборудование
		/// </summary>
		[Display(Name = "Кассовое оборудование")]
		CashEquipment,
		/// <summary>
		/// Автомобильные запчасти
		/// </summary>
		[Display(Name = "Автомобильные запчасти")]
		CarParts,
		/// <summary>
		/// Инструменты
		/// </summary>
		[Display(Name = "Инструменты")]
		Tools,
		/// <summary>
		/// Канцелярия
		/// </summary>
		[Display(Name = "Канцелярия")]
		Stationery,
		/// <summary>
		/// Оборудование для внутреннего использования
		/// </summary>
		[Display(Name = "Оборудование для внутреннего использования")]
		EquipmentForIndoorUse,
		/// <summary>
		/// Орг.техника
		/// </summary>
		[Display(Name = "Орг.техника")]
		OfficeEquipment,
		/// <summary>
		/// Производственное оборудование
		/// </summary>
		[Display(Name = "Производственное оборудование")]
		ProductionEquipment,
		/// <summary>
		/// Рекламная продукция
		/// </summary>
		[Display(Name = "Рекламная продукция")]
		PromotionalProducts,
		/// <summary>
		/// Спецодежда
		/// </summary>
		[Display(Name = "Спецодежда")]
		Overalls,
		/// <summary>
		/// Транспортное средство
		/// </summary>
		[Display(Name = "Транспортное средство")]
		Vehicle,
		/// <summary>
		/// Хоз.инвентарь
		/// </summary>
		[Display(Name = "Хоз.инвентарь")]
		HouseholdInventory
	}
}
