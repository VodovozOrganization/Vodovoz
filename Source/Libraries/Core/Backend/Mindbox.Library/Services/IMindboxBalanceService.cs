using System.Threading;
using System.Threading.Tasks;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Получение баланса клиента из Mindbox
	/// </summary>
	public interface IMindboxBalanceService
	{
		/// <summary>
		/// Запрашивает баланс клиента в Mindbox
		/// </summary>
		/// <param name="erpCounterpartyId">Идентификатор контрагента ERP</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Баланс клиента или признак недоступности Mindbox</returns>
		Task<MindboxBalanceInfo> GetAsync(int erpCounterpartyId, CancellationToken cancellationToken);
	}
}
