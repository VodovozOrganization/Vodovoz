using System.Threading.Tasks;
using SecureCodeSender.Contracts.Requests;
using Vodovoz.Core.Domain.Results;

namespace SecureCodeSenderApi.Services
{
	/// <summary>
	/// Интерфейс обработчика авторизационных кодов
	/// </summary>
	public interface ISecureCodeHandler
	{
		/// <summary>
		/// Создание и отправка кода авторизации
		/// </summary>
		/// <param name="sendSecureCodeDto">Данные для отправки <see cref="SendSecureCodeDto"/></param>
		/// <returns><c>Code</c> - код ответа для ИПЗ, <c>TimeForNextCode</c> - время до следующего запроса</returns>
		Task<Result<(string Code, int TimeForNextCode)>> GenerateAndSendSecureCode(SendSecureCodeDto sendSecureCodeDto);
		/// <summary>
		/// Генерация кода авторизации
		/// </summary>
		/// <param name="sendSecureCodeDto">Данные для генерации <see cref="SendSecureCodeDto"/></param>
		/// <returns><c>Code</c> - код ответа для ИПЗ, <c>TimeForNextCode</c> - время до следующего запроса</returns>
		(string Code, int TimeForNextCode) GenerateSecureCode(SendSecureCodeDto sendSecureCodeDto);
		/// <summary>
		/// Проверка Кода авторизации
		/// </summary>
		/// <param name="checkSecureCodeDto">Данные для проверки <see cref="CheckSecureCodeDto"/></param>
		/// <returns><c>Response</c> - код ответа для ИПЗ, <c>Message</c> - сообщение</returns>
		(int Response, string Message) CheckSecureCode(CheckSecureCodeDto checkSecureCodeDto);
	}
}
