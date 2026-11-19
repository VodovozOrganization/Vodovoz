using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings.OnlineOrders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	public class OnlineOrderStateHandler : IOnlineOrderStateHandler
	{
		private readonly IOnlineOrderCancellationReasonSettings _onlineOrderCancellationReasonSettings;

		public OnlineOrderStateHandler(
			IOnlineOrderCancellationReasonSettings onlineOrderCancellationReasonSettings
			)
		{
			_onlineOrderCancellationReasonSettings =
				onlineOrderCancellationReasonSettings ?? throw new ArgumentNullException(nameof(onlineOrderCancellationReasonSettings));
		}
		
		public void New(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			bool isAutoOrderEnabled
		)
		{
			if(isAutoOrderEnabled)
			{
				var onlineOrderTimers = uow.GetAll<OnlineOrderTimers>()
					.FirstOrDefault();

				if(onlineOrderTimers is null)
				{
					throw new InvalidOperationException($"Не найдены временные метки для онлайн заказа, проверьте {nameof(OnlineOrderTimers)}");
				}

				ToSetupTemplate(
					uow,
					onlineOrder,
					(int)onlineOrderTimers.AutoOrderSettingsTime.TotalSeconds,
					OnlineOrderTimestampType.AutoOrderSettingsTime,
					onlineOrder.Created
				);
			}
			else
			{
				if(onlineOrder.OnlineOrderPaymentStatus == OnlineOrderPaymentStatus.UnPaid
					&& onlineOrder.OnlineOrderPaymentType == OnlineOrderPaymentType.PaidOnline)
				{
					onlineOrder.OnlineOrderStatus = OnlineOrderStatus.WaitingForPayment;
				}
				else
				{
					onlineOrder.OnlineOrderStatus = OnlineOrderStatus.New;
				}
			}
		}
		
		public void ToSetupTemplate(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			int timeToSetupInSeconds,
			OnlineOrderTimestampType onlineOrderTimestampType,
			DateTime? startTime = null
			)
		{
			//TODO-6115 сделать различные проверки, чтобы нельзя было перевести в настройку не в тех состояниях
			onlineOrder.OnlineOrderStatus = OnlineOrderStatus.PendingAutoOrderSettings;
			onlineOrder.TemplateSetupTimestamp =
				OnlineOrderTimestamp.Create(startTime ?? DateTime.Now, timeToSetupInSeconds, onlineOrderTimestampType);
			
			uow.Save(onlineOrder.TemplateSetupTimestamp);
		}
		
		public virtual void ToPayTemplateByCustomer(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			int timeToPayInSeconds,
			OnlineOrderTimestampType onlineOrderTimestampType)
		{
			var template = onlineOrder.Template;
			
			if(template is null)
			{
				throw new InvalidOperationException("Нельзя использовать этот метод для обработки онлайн заказа без шаблона!");
			}

			template.Status = OnlineOrderTemplateStatus.AwaitingPayment;
			
			//TODO-6115 сделать различные проверки, чтобы нельзя было перевести на оплату не в тех состояниях
			onlineOrder.OnlineOrderStatus = OnlineOrderStatus.WaitingForPayment;
			onlineOrder.TemplateSetupTimestamp = null;
			onlineOrder.TemplatePayTimestamp = OnlineOrderTimestamp.Create(DateTime.Now, timeToPayInSeconds, onlineOrderTimestampType);
			
			uow.Save(onlineOrder.TemplatePayTimestamp);
		}
		
		public void AutoOrderToManualProcessing(
			OnlineOrder onlineOrder
			)
		{
			var template = onlineOrder.Template;

			if(template is null)
			{
				throw new InvalidOperationException("Нельзя использовать этот метод для обработки онлайн заказа без шаблона!");
			}
			
			template.Status = OnlineOrderTemplateStatus.ManualProcessing;
			onlineOrder.OnlineOrderStatus = OnlineOrderStatus.New;
		}

		public async Task CancelAsync(IUnitOfWork uow, OnlineOrder onlineOrder, CancellationToken cancellationToken)
		{
			onlineOrder.OnlineOrderStatus = OnlineOrderStatus.Canceled;

			var cancellationReasonId = _onlineOrderCancellationReasonSettings.GetDuplicateOnlineOrderCancellationReasonId;
			onlineOrder.OnlineOrderCancellationReason = await uow.Session
				.GetAsync<OnlineOrderCancellationReason>(cancellationReasonId, cancellationToken);
		}
	}
}
