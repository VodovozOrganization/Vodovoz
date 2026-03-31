using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Application.Orders.Services
{
	/// <summary>
	/// Сервис для обработки логики переноса заказов
	/// </summary>
	public interface IOrderTransferLogicService
	{
		/// <summary>
		/// Проверяет, был ли изменен интервал или дата доставки
		/// </summary>
		bool IsDeliveryParametersChanged(Order order, DateTime? newDeliveryDate, int? newDeliveryScheduleId);

		/// <summary>
		/// Применяет перенос заказа
		/// </summary>
		Task<(bool Success, string ErrorMessage)> ApplyTransferAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			DateTime? newDeliveryDate,
			DeliverySchedule newDeliverySchedule,
			Source source,
			CancellationToken cancellationToken);
	}
}
