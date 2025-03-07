using QS.Extensions.Observable.Collections.List;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
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
					return CreateEdoTaskForCashless(edoRequest);
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
			
			return CreateSaveCodeTask(edoRequest);
		}

		private EdoTask CreateEdoTaskForCashless(OrderEdoRequest edoRequest)
		{
			return edoRequest.Order.Client.IsNewEdoProcessing
				? CreateInstanceAccountingEdoTask(edoRequest)
				: CreateBulkAccountingEdoTask(edoRequest);
		}
		
		private EdoTask CreateInstanceAccountingEdoTask(OrderEdoRequest edoRequest)
		{
			
			if(edoRequest.Order.Client.ConsentForEdoStatus == ConsentForEdoStatus.Agree)
			{
				// Если есть согласение на ЭДО
				// создаем задачу формирования документа ЭДО
				return CreateDocumentEdoTask(edoRequest);
			}
			else
			{
				// Иначе
				// создаем задачу вывода из оборота через честный знак
				return CreateWithdrawalEdoTask(edoRequest);
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

		private EdoTask CreateWithdrawalEdoTask(OrderEdoRequest edoRequest)
		{
			var task = new WithdrawalEdoTask
			{
				Status = EdoTaskStatus.New,
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
			OrderEdoTask task = null;
			
			if(edoRequest.Order.Client.IsNewEdoProcessing)
			{
				task = new ReceiptEdoTask();
			}
			else
			{
				task = new BulkAccountingEdoTask();
			}

			task.Status = EdoTaskStatus.New;
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
