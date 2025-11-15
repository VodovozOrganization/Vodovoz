using Mailganer.Api.Client.Dto;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;

namespace EmailSendWorker.Services
{
	/// <summary>
	/// Сервис для отправки и проверки email через почтовый сервис
	/// </summary>
	public interface IEmailSendService
	{
		/// <summary>
		/// Код ошибки для email, находящегося в стоп-листе
		/// </summary>
		string EmaiInStopListErrorCodeString { get; }

		/// <summary>
		/// Отправляет email через почтовый сервис
		/// </summary>
		/// <param name="email">Письмо</param>
		/// <returns>Результат выполнения</returns>
		Task<Result> SendEmail(EmailMessage email);

		/// <summary>
		/// Проверяет email на наличие в стоп-листе и удаляет его оттуда, если он там есть и помечен как спам
		/// </summary>
		/// <param name="emailTo">Адрес получателя</param>
		/// <param name="emailFrom">Адрес отправителя (нашего почтового ящика)</param>
		/// <returns>Результат выполнения</returns>
		Task<Result> CheckAndRemoveSpamEmailFromStopList(string emailTo, string emailFrom);
	}
}
