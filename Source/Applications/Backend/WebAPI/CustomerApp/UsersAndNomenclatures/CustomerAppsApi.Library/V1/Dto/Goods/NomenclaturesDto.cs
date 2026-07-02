using System.Collections.Generic;

namespace CustomerAppsApi.Library.V1.Dto.Goods
{
	/// <summary>
	/// Номенклатуры
	/// </summary>
	public class NomenclaturesDto
	{
		/// <summary>
		/// Сообщение об ошибке
		/// </summary>
		public string ErrorMessage { get; set; }
		/// <summary>
		/// Список номенклатур
		/// </summary>
		public IEnumerable<OnlineNomenclatureDto> OnlineNomenclatures { get; set; }
	}
}
