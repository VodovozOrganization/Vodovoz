using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Core.Application.Receipts.Correction
{
	public class ReceiptCorrectionEdoTaskFactory
	{
		public ReceiptEdoTask Create(IUnitOfWork uow, Order order, ReceiptEdoTask sourceTask)
		{
			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(order == null)
			{
				throw new ArgumentNullException(nameof(order));
			}

			if(sourceTask == null)
			{
				throw new ArgumentNullException(nameof(sourceTask));
			}

			var orderEntity = uow.GetById<OrderEntity>(order.Id);
			if(orderEntity == null)
			{
				throw new InvalidOperationException($"Не найден заказ #{order.Id} для создания задачи корректировки чека.");
			}

			var sourceRequest = sourceTask.FormalEdoRequest;
			var request = new PrimaryEdoRequest
			{
				Type = sourceRequest?.Type ?? CustomerEdoRequestType.Order,
				Time = DateTime.Now,
				Source = EdoRequestSource.Manual,
				DocumentType = sourceRequest?.DocumentType ?? EdoDocumentType.UPD,
				Order = orderEntity,
				ProductCodes = new ObservableList<TrueMarkProductCode>(
					sourceRequest?.ProductCodes ?? Enumerable.Empty<TrueMarkProductCode>())
			};

			var task = new ReceiptEdoTask
			{
				Status = EdoTaskStatus.New,
				ReceiptStatus = EdoReceiptStatus.New,
				CashboxId = sourceTask.CashboxId,
				FormalEdoRequest = request
			};
			request.Task = task;

			if(sourceTask.Items != null)
			{
				foreach(var sourceItem in sourceTask.Items.Where(x => x?.ProductCode != null))
				{
					task.Items.Add(new EdoTaskItem
					{
						ProductCode = sourceItem.ProductCode,
						CustomerEdoTask = task
					});
				}
			}

			return task;
		}
	}
}
