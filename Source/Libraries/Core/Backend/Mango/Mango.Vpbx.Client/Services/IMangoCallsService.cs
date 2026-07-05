using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mango.Vpbx.Client.Services
{
	/// <summary>
	/// Сервис для звонков в Mango
	/// </summary>
	public interface IMangoCallsService
	{
		/// <summary>
		/// Отправляет команду на звонок через Mango с использованием вебхука
		/// </summary>
		/// <param name="extension">Дополнительный номер сотрудника</param>
		/// <param name="toNumber">Номер телефона получателя</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>Значение id команды на звонок</returns>
		Task MakeWebhookCall(string extension, string toNumber, CancellationToken cancellationToken);
	}
}
