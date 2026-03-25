using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Application.BankStatements
{
	/// <summary>
	/// Результаты обработки банковской выписки
	/// </summary>
	public enum BankStatementProcessState
	{
		[Display(Name = "Пустая папка")]
		EmptyDirectory,
		[Display(Name = "Успех")]
		Success,
		[Display(Name = "Дубликат выписки")]
		BankStatementDuplicate,
		[Display(Name = "Ошибка парсинга файла")]
		ErrorParsingFile,
		[Display(Name = "Неподдерживаемый формат данных")]
		UnsupportedFileExtension,
		[Display(Name = "Не найден номер расчетного счета")]
		EmptyAccountNumber,
		[Display(Name = "Не найден исходящий остаток")]
		EmptyBalance,
		[Display(Name = "Не найдена дата выписки")]
		EmptyDate,
		[Display(Name = "Неверная дата выписки")]
		WrongBankStatementDate
	}
}
