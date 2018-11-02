using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods
{
	/// <summary>
	/// Список дополнительных свойств номенклатуры. 
	/// Внимание для работы виджета отображения свойств необходимо чтобы название перечисления соответвстовало
	/// названию свойства в объекте номенклатуры.
	/// </summary>
	public enum NomenclatureProperties
	{
		[Display(Name = "Цвет")]
		Color,
		[Display(Name = "Материал")]
		Material,
		[Display(Name = "Объем")]
		Liters,
		[Display(Name = "Размеры")]
		Size,
		[Display(Name = "Тип упаковки")]
		Package,
		[Display(Name = "Степень обжарки")]
		DegreeOfRoast,
		[Display(Name = "Запах")]
		Smell,
		[Display(Name = "Вкус")]
		Taste,
		[Display(Name = "Объем шкафчика/холодильника")]
		RefrigeratorCapacity,
		[Display(Name = "Тип охлаждения")]
		CoolingType,
		[Display(Name = "Мощность нагрева")]
		HeatingPower,
		[Display(Name = "Мощность охлаждения")]
		CoolingPower,
		[Display(Name = "Производительность нагрева")]
		HeatingPerformance,
		[Display(Name = "Производительность охлаждения")]
		СoolingPerformance,
		[Display(Name = "Количество картриджей")]
		NumberOfCartridges,
		[Display(Name = "Характеристика картриджей")]
		CharacteristicsOfCartridges,
		[Display(Name = "Страна происхождения")]
		CountryOfOrigin,
		[Display(Name = "Количество  в упаковке")]
		AmountInAPackage
	}
}
