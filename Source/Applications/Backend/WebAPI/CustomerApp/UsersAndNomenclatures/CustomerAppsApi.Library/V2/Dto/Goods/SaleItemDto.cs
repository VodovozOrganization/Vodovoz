using System;

namespace CustomerAppsApi.Library.V2.Dto.Goods
{
	/// <summary>
	/// Продаваемая позиция в ИПЗ
	/// </summary>
	public abstract class SaleItemDto
	{
		/// <summary>
		/// Id номенклатуры в ДВ
		/// </summary>
		public int ErpId { get; set; }
		/// <summary>
		/// Тип
		/// </summary>
		public abstract SaleItemType Type { get; }
		/// <summary>
		/// Guid онлайн каталога в ИПЗ
		/// </summary>
		public Guid? OnlineCatalogGuid { get; set; }
		/// <summary>
		/// Группа товара в ИПЗ
		/// </summary>
		public string OnlineGroup { get; set; }
		/// <summary>
		/// Тип товара в ИПЗ
		/// </summary>
		public string OnlineCategory { get; set; }
		/// <summary>
		/// Наименование товара в ИПЗ
		/// </summary>
		public string OnlineName { get; set; }
	}
}
