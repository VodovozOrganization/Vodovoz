using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Тип действия при сборке талона погрузки автомобиля
	/// </summary>
	public enum CarLoadDocumentLoadingProcessActionType
	{
		/// <summary>
		/// Начало сборки талона
		/// </summary>
		[Display(Name = "Начало погрузки")]
		StartLoad,
		/// <summary>
		/// Добавление кода ЧЗ
		/// </summary>
		[Display(Name = "Добавление кода ЧЗ")]
		AddTrueMarkCode,
		/// <summary>
		/// Замена кода ЧЗ
		/// </summary>
		[Display(Name = "Замена кода ЧЗ")]
		ChangeTrueMarkCode,
		/// <summary>
		/// Завершение сборки талона
		/// </summary>
		[Display(Name = "Завершение погрузки")]
		EndLoad,
		/// <summary>
		/// Запрос данных заказа
		/// </summary>
		[Display(Name = "Запрос данных заказа")]
		OrderDataRequest
	}
}
