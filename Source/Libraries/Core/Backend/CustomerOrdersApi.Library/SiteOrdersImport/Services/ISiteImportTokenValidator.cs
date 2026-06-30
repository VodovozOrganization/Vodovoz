using System;

namespace CustomerOrdersApi.Library.SiteOrdersImport.Services
{
	/// <summary>
	/// Проверка токена авторизации запроса приёма выгрузки с сайта (I-5840).
	/// </summary>
	public interface ISiteImportTokenValidator
	{
		/// <summary>
		/// Проверяет токен запроса на указанную дату.
		/// </summary>
		/// <param name="token">Токен, пришедший в запросе</param>
		/// <param name="date">Дата, на которую формировался токен (используется в формате "yyyy.MM.dd")</param>
		/// <param name="expectedToken">Сгенерированный на нашей стороне ожидаемый токен</param>
		/// <returns><c>true</c>, если токен совпал с ожидаемым</returns>
		bool Validate(string token, DateTime date, out string expectedToken);

		/// <summary>
		/// Генерирует ожидаемый токен на указанную дату по формуле контракта.
		/// </summary>
		/// <param name="date">Дата, используемая в формате "yyyy.MM.dd"</param>
		/// <returns>Ожидаемый токен в верхнем регистре</returns>
		string GenerateToken(DateTime date);
	}
}
