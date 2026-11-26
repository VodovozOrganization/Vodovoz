using TrueMark.Contracts;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Domain.Interfaces.TrueMark
{
	/// <summary>
	/// Общий интерфейс распарсенного кода маркировки «Честный знак».
	/// Реализуется кодами разных товарных групп (вода, корма для животных).
	/// </summary>
	public interface ITrueMarkCode
	{
		/// <summary>
		/// Исходный (нормализованный) код маркировки
		/// </summary>
		string SourceCode { get; set; }

		/// <summary>
		/// Код GTIN товара
		/// </summary>
		string Gtin { get; set; }

		/// <summary>
		/// Серийный номер экземпляра
		/// </summary>
		string SerialNumber { get; set; }

		/// <summary>
		/// Код проверки (включая идентификаторы применения и разделители,
		/// формат зависит от товарной группы)
		/// </summary>
		string CheckCode { get; set; }

		/// <summary>
		/// Товарная группа, к которой относится код
		/// </summary>
		TrueMarkProductGroup ProductGroup { get; }
	}
}
