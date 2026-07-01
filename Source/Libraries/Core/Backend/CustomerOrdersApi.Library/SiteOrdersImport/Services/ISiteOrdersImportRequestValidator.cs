using CustomerOrdersApi.Library.SiteOrdersImport.Dto;
using System;

namespace CustomerOrdersApi.Library.SiteOrdersImport.Services
{
	/// <summary>
	/// Проверяет входящий пакет выгрузки заказов и брошенных корзин с сайта.
	/// </summary>
	public interface ISiteOrdersImportRequestValidator
	{
		/// <summary>
		/// Проверяет обязательные поля пакета и токен авторизации на указанную дату.
		/// </summary>
		/// <param name="request">Пакет выгрузки с сайта.</param>
		/// <param name="date">Дата, на которую проверяется токен (используется в формате "yyyy.MM.dd").</param>
		/// <returns>Результат проверки с типом ошибки для HTTP-ответа.</returns>
		SiteOrdersImportRequestValidationResult Validate(OrdersImportRequest request, DateTime date);
	}
}
