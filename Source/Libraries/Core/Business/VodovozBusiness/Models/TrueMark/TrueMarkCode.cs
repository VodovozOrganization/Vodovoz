using TrueMark.Contracts;
using Vodovoz.Core.Domain.Interfaces.TrueMark;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Models.TrueMark
{
	/// <summary>
	/// Распарсенный код маркировки «Честный знак» (вода, корма для животных и другие товарные группы).
	/// </summary>
	public class TrueMarkCode : ITrueMarkCode
	{
		/// <summary>
		/// Исходный нормализованный код маркировки
		/// </summary>
		public string SourceCode { get; set; }

		/// <summary>
		/// Код GTIN
		/// </summary>
		public string Gtin { get; set; }

		/// <summary>
		/// Серийный номер экземпляра
		/// </summary>
		public string SerialNumber { get; set; }

		/// <summary>
		/// Код проверки (формат зависит от товарной группы)
		/// </summary>
		public string CheckCode { get; set; }

		/// <summary>
		/// Товарная группа маркировки
		/// </summary>
		public TrueMarkProductGroup ProductGroup { get; set; }
	}
}
