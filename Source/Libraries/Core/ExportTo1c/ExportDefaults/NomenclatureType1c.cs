using QS.DomainModel.Entity;
using Vodovoz.EntityRepositories.Orders;

namespace ExportTo1c.Library.ExportDefaults
{
	/// <summary>
	/// Тип номенклатуры
	/// </summary>
	public class NomenclatureType1c : IDomainObject
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public bool IsService { get; set; }

		public static NomenclatureType1c GoodsType(Export1cMode export1CMode) => new NomenclatureType1c
		{
			Id = 1,
			Name = export1CMode == Export1cMode.ComplexAutomation ? "Товар" : "Товары",
			IsService = false
		};

		public static NomenclatureType1c ServicesType(Export1cMode export1CMode) => new NomenclatureType1c
		{
			Id = 2,
			Name = export1CMode == Export1cMode.ComplexAutomation ? "Услуга" : "Услуги",
			IsService = true
		};
	}
}
