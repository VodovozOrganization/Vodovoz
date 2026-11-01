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
		/// <returns></returns>
		Result PauseTemplate(
			IUnitOfWork uow,
			OnlineOrderTemplate template,
			int? customerId,
			int? employeeId);
	}
}
