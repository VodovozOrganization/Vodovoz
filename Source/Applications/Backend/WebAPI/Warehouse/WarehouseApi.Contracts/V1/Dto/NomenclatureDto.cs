using System.Collections.Generic;

namespace WarehouseApi.Contracts.V1.Dto
{
	/// <summary>
	/// Номенклатура
	/// </summary>
	public class NomenclatureDto
	{
		/// <summary>
		/// Id номенклатуры
		/// </summary>
		public int NomenclatureId { get; set; }

		/// <summary>
		/// Название номенклатуры
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Номера товарной продукции GTIN
		/// </summary>
		public IEnumerable<string> Gtin { get; set; }

		/// <summary>
		/// Номера группы товарной продукции GTIN
		/// </summary>
		public IEnumerable<GroupGtinDto> GroupGtins { get; set; }

		/// <summary>
		/// Количество
		/// </summary>
		public int Quantity { get; set; }

		/// <summary>
		/// Коды маркировки ЧЗ
		/// </summary>
		public IEnumerable<TrueMarkCodeDto> Codes { get; set; }
	}
}
