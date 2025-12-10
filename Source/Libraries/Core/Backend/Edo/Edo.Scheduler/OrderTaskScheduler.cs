using System;
using QS.Extensions.Observable.Collections.List;
using System.Linq;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Controllers;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Domain.Client;

namespace Edo.Scheduler.Service
{
	public class OrderTaskScheduler
	{
		private readonly ICounterpartyEdoAccountEntityController _edoAccountEntityController;

		public OrderTaskScheduler(ICounterpartyEdoAccountEntityController edoAccountEntityController)
		{
			_edoAccountEntityController = edoAccountEntityController ?? throw new ArgumentNullException(nameof(edoAccountEntityController));
		}
		
		public EdoTask CreateTask(FormalEdoRequest edoRequest)
		{
			if(edoRequest.Order.Client.ReasonForLeaving == ReasonForLeaving.Tender)
			{
				return CreateTenderEdoTask(edoRequest);
			}
			
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

		private EdoTask CreatePaidOnlineTask(FormalEdoRequest edoRequest)
		{
			if(edoRequest.Order.PaymentByCardFrom.ReceiptRequired)
			{
				return CreateReceiptTask(edoRequest);
			}
			
			return CreateSaveCodeTask(edoRequest);
		}

		private EdoTask CreateEdoTaskForCashless(FormalEdoRequest edoRequest)
		{
			return edoRequest.Order.Client.IsNewEdoProcessing
				? CreateInstanceAccountingEdoTask(edoRequest)
				: CreateBulkAccountingEdoTask(edoRequest);
		}
		
		private EdoTask CreateInstanceAccountingEdoTask(FormalEdoRequest edoRequest)
		{
			var edoAccount = _edoAccountEntityController.GetDefaultCounterpartyEdoAccountByOrganizationId(
				edoRequest.Order.Client, edoRequest.Order.Contract.Organization.Id);

			if(edoAccount.ConsentForEdoStatus == ConsentForEdoStatus.Agree)
			{
				// Если есть согласие на ЭДО
				// создаем задачу формирования документа ЭДО
				return CreateDocumentEdoTask(edoRequest);
			}

			return CreateSaveCodeTask(edoRequest);
		}

		private EdoTask CreateDocumentEdoTask(FormalEdoRequest edoRequest)
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

		private EdoTask CreateWithdrawalEdoTask(FormalEdoRequest edoRequest)
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


		private EdoTask CreateBulkAccountingEdoTask(FormalEdoRequest edoRequest)
		{
			var task = new BulkAccountingEdoTask
			{
				Status = EdoTaskStatus.New
			};

			edoRequest.Task = task;

			return task;
		}

		private EdoTask CreateReceiptTask(FormalEdoRequest edoRequest)
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

		private EdoTask CreateSaveCodeTask(FormalEdoRequest edoRequest)
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
		
		private EdoTask CreateTenderEdoTask(FormalEdoRequest edoRequest)
		{
			var task = new TenderEdoTask
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
