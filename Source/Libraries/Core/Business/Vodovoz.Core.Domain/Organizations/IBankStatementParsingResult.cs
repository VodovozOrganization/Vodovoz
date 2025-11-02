namespace Vodovoz.Core.Domain.Organizations
{
	/// <summary>
	/// Результат парсинга бвнковской выписки
	/// </summary>
	public interface IBankStatementParsingResult
	{
		/// <summary>
		/// Баланс
		/// </summary>
		decimal? Total { get; set; }
		/// <summary>
		/// Наименование
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Наименование банка
		/// </summary>
		string Bank { get; }
		/// <summary>
		/// Номер расчетного счета
		/// </summary>
		string AccountNumber { get; }
	}
}
