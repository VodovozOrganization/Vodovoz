using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;

namespace DriverAPI.Library.V6.Services
{
	/// <summary>
	/// Сервис для работы со звонками
	/// </summary>
	public interface ICallsService
	{
		/// <summary>
		/// Отправляет запрос на совершение звонка через вебхук
		/// </summary>
		/// <param name="extension">Добавочный номер сотрудника</param>
		/// <param name="toNumber">Номер телефона, на который нужно позвонить</param>
		/// <param name="lineNumber">Номер линии</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>Результат операции со значением id команды на звонок</returns>
		Task<Result<Guid>> MakeWebhookCall(string extension, string toNumber, string lineNumber, CancellationToken cancellationToken);
	}
}
