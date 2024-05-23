﻿using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Goods
{
	/// <summary>
	/// Типы номенклатур
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Типы номенклатур",
		Nominative = "Тип номенклатур")]
	public enum NomenclatureCategory
	{
		[Display(Name = "Вода")]
		water,
		[Display(Name = "Залог")]
		deposit,
		[Display(Name = "Запчасти сервисного центра")]
		spare_parts,
		[Display(Name = "Оборудование")]
		equipment,
		[Display(Name = "Товары")]
		additional,
		[Display(Name = "Услуга")]
		service,
		[Display(Name = "Тара")]
		bottle,
		[Display(Name = "Сырьё")]
		material,
		[Display(Name = "Выезд мастера")]
		master,
		[Display(Name = "Топливо")]
		fuel,
		[Display(Name = "Кассовое оборудование")]
		CashEquipment,
		[Display(Name = "Автомобильные запчасти")]
		CarParts,
		[Display(Name = "Инструменты")]
		Tools,
		[Display(Name = "Канцелярия")]
		Stationery,
		[Display(Name = "Оборудование для внутреннего использования")]
		EquipmentForIndoorUse,
		[Display(Name = "Орг.техника")]
		OfficeEquipment,
		[Display(Name = "Производственное оборудование")]
		ProductionEquipment,
		[Display(Name = "Рекламная продукция")]
		PromotionalProducts,
		[Display(Name = "Спецодежда")]
		Overalls,
		[Display(Name = "Транспортное средство")]
		Vehicle,
		[Display(Name = "Хоз.инвентарь")]
		HouseholdInventory
	}
}

