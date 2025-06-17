using System.Collections.Generic;

namespace Vodovoz.RobotMia.Contracts.Responses.V1
{
	/// <summary>
	/// Номенклатура
	/// </summary>
	public class NomenclatureDto
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Название
		/// </summary>
		public string ShortName { get; set; }

		/// <summary>
		/// Продажа доступна
		/// </summary>
		public bool CanSale { get; set; }

		/// <summary>
		/// Жаргонизмы
		/// </summary>
		public IEnumerable<string> SlangWords { get; set; }

		/// <summary>
		/// Цена
		/// </summary>
		public decimal Price { get; set; }
	}
}
