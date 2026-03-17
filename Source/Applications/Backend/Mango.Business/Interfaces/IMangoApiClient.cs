using System;
using System.Threading;
using System.Threading.Tasks;
using Mango.Contracts.V1.Response;

namespace Mango.Business.Interfaces
{
	public interface IMangoApiClient
	{
		/// <summary>
		/// Запросить группы манго
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns>Ответ с группами</returns>
		Task<GroupsResponse> GetGroupsAsync(
			CancellationToken cancellationToken);

		/// <summary>
		/// Запросить статистику с результатами по ключу
		/// </summary>
		/// <param name="key">Ключ для запроса статистики</param>
		/// <param name="cancellationToken"></param>
		/// <returns>Ответ с статистикой</returns>
		Task<CallsResponse> GetCallsAsync(
			string key,
			CancellationToken cancellationToken);

		/// <summary>
		/// Запросить статистику по звонка в выбранном промежутке дат (не более 1 мес)
		/// </summary>
		/// <param name="fromDate">С даты</param>
		/// <param name="toDate">По дату</param>
		/// <param name="cancellationToken"></param>
		/// <returns>Ответ с ключом для запроса статистики</returns>
		Task<CallsStatResponse> GetCallsStatAsync(
			DateTime fromDate,
			DateTime toDate,
			CancellationToken cancellationToken);
	}
}
