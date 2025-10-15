using BitrixNotificationsSend.Contracts.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;

namespace BitrixNotificationsSend.Client
{
	/// <summary>
	/// Отправляет уведомления в Битрикс24
	/// </summary>
	public interface IBitrixNotificationsSendClient
	{
		/// <summary>
		/// Отправка уведомления в Битрикс24 о долгах по безналу клиентов
		/// </summary>
		/// <param name="counterpartiesCashlessDebts">Список с данными по долгам</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат отправки</returns>
		Task<Result> SendCounterpartiesCashlessDebtsNotification(IEnumerable<CounterpartyCashlessDebtDto> counterpartiesCashlessDebts, CancellationToken cancellationToken);
	}
}
