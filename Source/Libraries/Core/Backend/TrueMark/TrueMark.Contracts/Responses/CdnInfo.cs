using System.Collections.Generic;

namespace TrueMark.Contracts.Responses
{
	/// <summary>
	///  Информация о CDN-площадках
	/// </summary>
	public class CdnInfo
	{
		/// <summary>
		/// Результат обработки операции
		/// </summary>
		public int Code { get; set; }

		/// <summary>
		/// Текстовое описание результата выполнения метода
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Список адресов CDN-площадок
		/// </summary>
		public IList<CdnHost> Hosts { get; set; }
	}
}
