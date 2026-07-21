using CustomerOrdersApi.Library.SiteOrdersImport.Dto;
using Vodovoz.Core.Domain.Results;

namespace CustomerOrdersApi.Library.SiteOrdersImport.Services
{
	/// <summary>
	/// Проверяет входящий пакет выгрузки заказов и брошенных корзин с сайта.
	/// </summary>
	public interface ISiteOrdersImportRequestValidator
	{
		/// <summary>
		/// Проверяет подпись пакета на указанную дату.
		/// </summary>
		/// <param name="request">Пакет выгрузки с сайта.</param>
		/// <param name="generatedSignature">Сгенерированная на нашей стороне подпись.</param>
		/// <returns><c>true</c>, если подпись корректна.</returns>
		bool ValidateSignature(OrdersImportRequest request, out string generatedSignature);

		/// <summary>
		/// Проверяет обязательные поля пакета.
		/// </summary>
		/// <param name="request">Пакет выгрузки с сайта.</param>
		/// <returns>Результат проверки.</returns>
		Result Validate(OrdersImportRequest request);

		/// <summary>
		/// Проверяет обязательные поля записи пакета.
		/// </summary>
		/// <param name="item">Запись пакета выгрузки.</param>
		/// <returns>Результат проверки.</returns>
		Result ValidateItem(OrderImportItem item);
	}
}
