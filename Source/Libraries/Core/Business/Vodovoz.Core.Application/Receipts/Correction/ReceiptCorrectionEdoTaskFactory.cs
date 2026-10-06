using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;

namespace Vodovoz.Core.Application.Receipts.Correction
{
	public class ReceiptCorrectionEdoTaskFactory
	{
		private readonly IEmployeeRepository _employeeRepository;

		public ReceiptCorrectionEdoTaskFactory(IEmployeeRepository employeeRepository)
		{
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
		}

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
				Author = _employeeRepository.GetEmployeeForCurrentUser(uow),
				ProductCodes = new ObservableList<TrueMarkProductCode>()
			};

			var task = new ReceiptEdoTask
			{
				Status = EdoTaskStatus.New,
				ReceiptStatus = EdoReceiptStatus.New,
				CashboxId = sourceTask.CashboxId,
				FormalEdoRequest = request
			};
			request.Task = task;

			return task;
		}
	}
}
