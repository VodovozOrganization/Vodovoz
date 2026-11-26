using Vodovoz.Core.Domain.Interfaces.TrueMark;

namespace Vodovoz.Models.TrueMark
{
	/// <summary>
	/// Парсер кода маркировки одной товарной группы.
	/// <para/>
	/// Используется фасадом TrueMarkCodeParser для перебора товарных групп.
	/// </summary>
	public interface ITrueMarkProductCodeParser
	{
		/// <summary>
		/// Безопасный разбор кода в стандартном формате (в т.ч. скобочном).
		/// </summary>
		bool TryParse(string rawCode, out ITrueMarkCode result);

		/// <summary>
		/// Безопасный разбор кода, выгруженного из 1С/CSV.
		/// </summary>
		bool TryParseCodeFrom1c(string rawCode, out ITrueMarkCode result);

		/// <summary>
		/// Нечёткий разбор для поиска и фильтрации (часть данных может отсутствовать).
		/// </summary>
		bool FuzzyParse(string input, out ITrueMarkCode result);

		/// <summary>
		/// Разбор кода, введённого вручную без разделителей. По умолчанию не поддерживается.
		/// </summary>
		bool TryParseManual(string rawCode, out ITrueMarkCode result);
	}
}
