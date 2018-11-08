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
		[OnlineStoreGuid("554b1944-b5b4-4f6b-b3bb-65255e854ca8")]
		Color,
		[Display(Name = "Материал")]
		[OnlineStoreGuid("1453960d-69d7-453b-8e78-a4472312e621")]
		Material,
		[Display(Name = "Объем")]
		[OnlineStoreGuid("07d85e74-6f29-44ae-bbee-09b843b4d4ca")]
		Liters,
		[Display(Name = "Размеры")]
		[OnlineStoreGuid("300afb55-09a9-47b5-ab01-f2f705dd37ef")]
		Size,
		[Display(Name = "Тип упаковки")]
		[OnlineStoreGuid("9542ec79-024a-49ed-a4b6-f71832025e15")]
		Package,
		[Display(Name = "Степень обжарки")]
		[OnlineStoreGuid("19a15835-c68f-4689-94ff-dcfe3584852b")]
		DegreeOfRoast,
		[Display(Name = "Запах")]
		[OnlineStoreGuid("51b0c07e-3831-4aa1-adea-bc640c8f8994")]
		Smell,
		[Display(Name = "Вкус")]
		[OnlineStoreGuid("7a9e4236-4dd8-4359-a02d-13ee53f1aaf0")]
		Taste,
		[Display(Name = "Объем шкафчика/холодильника")]
		[OnlineStoreGuid("5a023cba-e6bc-42ea-add5-a0a43e3c0412")]
		RefrigeratorCapacity,
		[Display(Name = "Тип охлаждения")]
		[OnlineStoreGuid("91b97e82-0bc9-4a30-9980-0c5741de858a")]
		CoolingType,
		[Display(Name = "Мощность нагрева")]
		[OnlineStoreGuid("3a8ae5d7-f4c1-46fe-9403-f19ce04758aa")]
		HeatingPower,
		[Display(Name = "Мощность охлаждения")]
		[OnlineStoreGuid("d914844e-1835-4d90-a80f-872322a84562")]
		CoolingPower,
		[Display(Name = "Производительность нагрева")]
		[OnlineStoreGuid("277a714e-de31-4778-8c9c-db02cf7dbf70")]
		HeatingPerformance,
		[Display(Name = "Производительность охлаждения")]
		[OnlineStoreGuid("74c9dd68-158e-4e76-8874-b08980a4297a")]
		СoolingPerformance,
		[Display(Name = "Количество картриджей")]
		[OnlineStoreGuid("10751bcf-c490-48ba-a19b-c8b50258d01d")]
		NumberOfCartridges,
		[Display(Name = "Характеристика картриджей")]
		[OnlineStoreGuid("538bdd05-0e72-4d03-8681-8f48308a4fc3")]
		CharacteristicsOfCartridges,
		[Display(Name = "Страна происхождения")]
		[OnlineStoreGuid("b198a894-42bb-479a-9f1f-a49dd9cb05cf")]
		CountryOfOrigin,
		[Display(Name = "Количество  в упаковке")]
		[OnlineStoreGuid("1f77c36e-b9da-495a-854f-d7833912fa44")]
		AmountInAPackage
	}
}
