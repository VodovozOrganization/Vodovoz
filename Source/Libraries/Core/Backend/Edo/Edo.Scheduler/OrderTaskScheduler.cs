using QS.Extensions.Observable.Collections.List;
using System.Linq;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Domain.Client;

namespace Edo.Scheduler.Service
{
	public class OrderTaskScheduler
	{
		public EdoTask CreateTask(OrderEdoRequest edoRequest)
		{
			switch(edoRequest.Order.PaymentType)
			{
				case PaymentType.Cashless:
					return CreateDocumentEdoTask(edoRequest);
				case PaymentType.Cash:
				case PaymentType.Terminal:
				case PaymentType.DriverApplicationQR:
				case PaymentType.SmsQR:
					return CreateReceiptTask(edoRequest);
				case PaymentType.PaidOnline:
					return CreatePaidOnlineTask(edoRequest);
				case PaymentType.Barter:
				case PaymentType.ContractDocumentation:
				default:
					return CreateSaveCodeTask(edoRequest);
			}
		}

		private EdoTask CreatePaidOnlineTask(OrderEdoRequest edoRequest)
		{
			if(edoRequest.Order.PaymentByCardFrom.ReceiptRequired)
			{
				return CreateReceiptTask(edoRequest);
			}
			else
			{
				return CreateSaveCodeTask(edoRequest);
			}
		}

		private EdoTask CreateDocumentEdoTask(OrderEdoRequest edoRequest)
		{
			// Клиентов с объемным учетом обслуживание по старому
			/*
			if(edoRequest.Order.Client.HasBulkAccounting)
			{
				return CreateBulkAccountingEdoTask(edoRequest);
			}
			*/

			var task = new DocumentEdoTask
			{
				CustomerEdoRequest = edoRequest,
				DocumentType = edoRequest.DocumentType,

				//Перенести контракт
				//FromOrganization = edoRequest.Order.IsContractCloser.Organization.Id,

				//Перенести клиента
				//ToClient = edoRequest.Order.Client.Id,

				Status = EdoTaskStatus.New
			};

			task.Items = new ObservableList<EdoTaskItem>(
				edoRequest.ProductCodes.Select(x => 
					new EdoTaskItem
					{
						ProductCode = x,
						EdoTask = task
					})
			);

			return task;
		}

		private EdoTask CreateBulkAccountingEdoTask(OrderEdoRequest edoRequest)
		{
			var task = new BulkAccountingEdoTask
			{
				CustomerEdoRequest = edoRequest,
				Status = EdoTaskStatus.New
			};
			return task;
		}

		private EdoTask CreateReceiptTask(OrderEdoRequest edoRequest)
		{
			var task = new ReceiptEdoTask
			{
				CustomerEdoRequest = edoRequest,
				Status = EdoTaskStatus.New
			};

			task.Items = new ObservableList<EdoTaskItem>(
				edoRequest.ProductCodes.Select(x =>
					new EdoTaskItem
					{
						ProductCode = x,
						EdoTask = task
					})
			);

			return task;
		}

		private EdoTask CreateSaveCodeTask(OrderEdoRequest edoRequest)
		{
			var task = new SaveCodesEdoTask
			{
				CustomerEdoRequest = edoRequest,
				Status = EdoTaskStatus.New
			};

			task.Items = new ObservableList<EdoTaskItem>(
				edoRequest.ProductCodes.Select(x =>
					new EdoTaskItem
					{
						ProductCode = x,
						EdoTask = task
					})
			);

			return task;
		}
	}
}
