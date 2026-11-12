using System;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Core.Domain.Sale
{
	public interface IOnlineOrderTemplateStatusHandler
	{
		/// <summary>
		/// Приостановка шаблона автозаказа
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="template">Шаблон автозаказа</param>
		/// <param name="customerId">Идентификатор клиента, если действие совершил клиент. Иначе null</param>
		/// <param name="employeeId">Идентификатор сотрудника, если действие совершил сотрудник. Иначе null</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result> PauseTemplateAsync(
			IUnitOfWork uow,
			OnlineOrderTemplate template,
			int? customerId,
			int? employeeId,
			CancellationToken cancellationToken
			);

		/// <summary>
		/// Возобновление шаблона автозаказа
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="template">Шаблон автозаказа</param>
		/// <param name="nextDeliveryDate">Новая первая дата доставки</param>
		/// <param name="customerId">Идентификатор клиента, если действие совершил клиент. Иначе null</param>
		/// <param name="employeeId">Идентификатор сотрудника, если действие совершил сотрудник. Иначе null</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result> ResumeTemplateAsync(
			IUnitOfWork uow,
			OnlineOrderTemplate template,
			DateTime nextDeliveryDate,
			int? customerId,
			int? employeeId,
			CancellationToken cancellationToken
			);

		/// <summary>
		/// Архивация шаблона
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="template">Шаблон автозаказа</param>
		/// <param name="customerId">Идентификатор клиента, если действие совершил клиент. Иначе null</param>
		/// <param name="employeeId">Идентификатор сотрудника, если действие совершил сотрудник. Иначе null</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<Result> ArchiveTemplateAsync(
			IUnitOfWork uow,
			OnlineOrderTemplate template,
			int? customerId,
			int? employeeId,
			CancellationToken cancellationToken
			);
	}
}
