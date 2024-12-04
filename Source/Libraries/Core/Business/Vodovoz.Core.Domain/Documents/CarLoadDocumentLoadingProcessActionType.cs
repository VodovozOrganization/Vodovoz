using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Documents
{
	public enum CarLoadDocumentLoadingProcessActionType
	{
		[Display(Name = "Начало погрузки")]
		StartLoad,
		[Display(Name = "Добавление кода ЧЗ")]
		AddTrueMarkCode,
		[Display(Name = "Замена кода ЧЗ")]
		ChangeTrueMarkCode,
		[Display(Name = "Завершение погрузки")]
		EndLoad,
		[Display(Name = "Запрос данных заказа")]
		OrderDataRequest
	}
}
