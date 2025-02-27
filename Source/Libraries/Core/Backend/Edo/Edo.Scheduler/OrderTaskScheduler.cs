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
			var task = new DocumentEdoTask
			{
				DocumentType = edoRequest.DocumentType,
				FromOrganization = edoRequest.Order.Contract.Organization.Id,
				ToCustomer = edoRequest.Order.Client.Id,
				Status = EdoTaskStatus.New,
				Stage = DocumentEdoTaskStage.New
			};

			task.Items = new ObservableList<EdoTaskItem>(
				edoRequest.ProductCodes.Select(x => 
					new EdoTaskItem
					{
						ProductCode = x,
						CustomerEdoTask = task
					})
			);

			edoRequest.Task = task;

			return task;
		}

		private EdoTask CreateBulkAccountingEdoTask(OrderEdoRequest edoRequest)
		{
			var task = new BulkAccountingEdoTask
			{
				Status = EdoTaskStatus.New
			};

			edoRequest.Task = task;

			return task;
		}

		private EdoTask CreateReceiptTask(OrderEdoRequest edoRequest)
		{
			var task = new ReceiptEdoTask
			{
				Status = EdoTaskStatus.New
			};

			task.Items = new ObservableList<EdoTaskItem>(
				edoRequest.ProductCodes.Select(x =>
					new EdoTaskItem
					{
						ProductCode = x,
						CustomerEdoTask = task
					})
			);

			edoRequest.Task = task;

			return task;
		}

		private EdoTask CreateSaveCodeTask(OrderEdoRequest edoRequest)
		{
			var task = new SaveCodesEdoTask
			{
				Status = EdoTaskStatus.New
			};

			task.Items = new ObservableList<EdoTaskItem>(
				edoRequest.ProductCodes.Select(x =>
					new EdoTaskItem
					{
						ProductCode = x,
						CustomerEdoTask = task
					})
			);

			edoRequest.Task = task;

			return task;
		}
	}
}
