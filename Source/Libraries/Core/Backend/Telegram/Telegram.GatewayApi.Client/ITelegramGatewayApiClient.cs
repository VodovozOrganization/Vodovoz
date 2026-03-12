using System.Threading.Tasks;
using Telegram.Contracts.Requests;
using Telegram.Contracts.Response;

namespace Telegram.GatewayApi.Client
{
	/// <summary>
	/// Контракт работы с Telegram Gateway api
	/// </summary>
	public interface ITelegramGatewayApiClient
	{
		/// <summary>
		/// Отправка верификационного сообщения.
		/// Может содержать сгенерированный код или без него, в таком случае Telegram сгенерирует самостоятельно
		/// </summary>
		/// <param name="dto"></param>
		/// <returns></returns>
		Task<ResponseDto> SendVerificationMessage(SendVerificationMessageRequest dto);
		/// <summary>
		/// Проверка регистрации пользователя в Telegram.
		/// Если пользователь зарегистрирован и отправка верификационных кодов возможна, то при вызове SendVerificationMessage лучше передавать
		/// requestId полученный в ответе. Таким образом оплата будет взыматься только один раз,
		/// т.е. проверка авторизации и отправка кода будет стоить 0.01$
		/// </summary>
		/// <param name="dto"></param>
		/// <returns></returns>
		Task<ResponseDto> CheckSendAbility(CheckSendAbilityRequest dto);
		/// <summary>
		/// Проверка статуса введенного кода
		/// </summary>
		/// <param name="dto"></param>
		/// <returns></returns>
		Task<ResponseDto> CheckVerificationStatus(CheckVerificationStatusRequest dto);
	}
}
