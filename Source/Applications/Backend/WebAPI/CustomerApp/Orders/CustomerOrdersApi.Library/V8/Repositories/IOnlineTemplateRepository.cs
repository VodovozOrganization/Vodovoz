using System.Linq;
using CustomerApp.Contracts.Sale.Templates;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Sale;

namespace CustomerOrdersApi.Library.V8.Repositories
{
	public interface IOnlineTemplateRepository
	{
		/// <summary>
		/// Получение интервалов доставки по дням
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="onlineOrderId">Идентификатор онлайн заказа</param>
		/// <returns></returns>
		ILookup<WeekDayName, DeliveryScheduleDto> GetWeeklyDeliverySchedules(IUnitOfWork uow, int onlineOrderId);
	}
}
