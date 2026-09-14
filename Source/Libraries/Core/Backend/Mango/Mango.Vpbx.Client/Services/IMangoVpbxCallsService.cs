using System.Threading;
using System.Threading.Tasks;

namespace Mango.Vpbx.Client.Services
{
	/// <summary>
	/// Сервис команд звонков API ВАТС Манго
	/// </summary>
	public interface IMangoVpbxCallsService
	{
		/// <summary>
		/// Отправляет команду обратного звонка: ВАТС дозванивается сотруднику
		/// с внутренним номером <paramref name="extension"/>, а после ответа
		/// соединяет его с <paramref name="toNumber"/>.
		/// Звонок выполняется по исходящей линии, указанной в карточке сотрудника ВАТС
		/// </summary>
		/// <param name="extension">Внутренний номер сотрудника ВАТС, которому звонит ВАТС</param>
		/// <param name="toNumber">Номер телефона вызываемого абонента</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		Task SendCallbackCommand(string extension, string toNumber, CancellationToken cancellationToken);
	}
}
