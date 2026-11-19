using System;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Services.Orders
{
	public interface IOnlineOrderStateHandler
	{
		/// <summary>
		/// Выполнение действий при только созданном онлайн заказе
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="onlineOrder">Онлайн заказ</param>
		/// <param name="isAutoOrderEnabled">Подключен автозаказ</param>
		void New(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			bool isAutoOrderEnabled
		);

		/// <summary>
		/// Перевод онлайн заказа в состояние настройки шаблона автозаказа
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="onlineOrder">Онлайн заказ</param>
		/// <param name="timeToSetupInSeconds">Время для действия</param>
		/// <param name="onlineOrderTimestampType">Тип метки</param>
		/// <param name="startTime">Начало действия</param>
		void ToSetupTemplate(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			int timeToSetupInSeconds,
			OnlineOrderTimestampType onlineOrderTimestampType,
			DateTime? startTime = null
		);

		/// <summary>
		/// Перевод онлайн заказа в состояние оплаты автозаказа и шаблона
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="onlineOrder">Онлайн заказ</param>
		/// <param name="timeToPayInSeconds">Время для действия</param>
		/// <param name="onlineOrderTimestampType">Тип метки</param>
		void ToPayTemplateByCustomer(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			int timeToPayInSeconds,
			OnlineOrderTimestampType onlineOrderTimestampType);

		/// <summary>
		/// Перевод онлайн заказа в состояние ручной обработки автозаказа
		/// </summary>
		/// <param name="onlineOrder">Онлайн заказ</param>
		void AutoOrderToManualProcessing(
			OnlineOrder onlineOrder
		);

		/// <summary>
		/// Отмена онайн заказа
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="onlineOrder">Онлайн заказ</param>
		/// <param name="cancellationToken">Токен отмены</param>
		Task CancelAsync(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			CancellationToken cancellationToken
		);
	}
}
