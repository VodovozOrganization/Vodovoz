using System.Collections.Generic;

namespace CustomerAppsApi.Library.V2.Dto.Goods
{
	/// <summary>
	/// Продаваемые позиции
	/// </summary>
	public class SaleItemsDto
	{
		/// <summary>
		/// Сообщение об ошибке
		/// </summary>
		public string ErrorMessage { get; set; }
		/// <summary>
		/// Список продаваемых позиций
		/// </summary>
		public IEnumerable<object> SaleItems { get; set; }
	}
}
