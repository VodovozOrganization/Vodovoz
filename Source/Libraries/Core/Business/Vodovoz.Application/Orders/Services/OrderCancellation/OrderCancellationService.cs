using Core.Infrastructure;
using Edo.Contracts.Messages.Events;
using MassTransit;
using QS.Dialog;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Application.Orders.Services.OrderCancellation
{
	/// <summary>
	/// Сервис проверяет возможности отмены заказа
	/// так же уведомляет пользователя и запрашивает подтверждение
	/// </summary>
	public class OrderCancellationService
	{
		private readonly IInteractiveService _interactiveService;
		private readonly IEdoRepository _edoRepository;
		private readonly IBus _messageBus;

		public OrderCancellationService(
			IInteractiveService interactiveService,
			IEdoRepository edoRepository,
			IBus messageBus
			)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		public OrderCancellationPermit CanCancelOrder(
			IUnitOfWork uow,
			Order order, 
			OrderCancellationPermit inputPermit = null
			)
		{
			var permit = inputPermit ?? new OrderCancellationPermit();

			var edoTasks = _edoRepository.GetEdoTaskByOrderAsync(uow, order.Id);

			var hasMarkedProducts = order.OrderItems.Any(x => x.Nomenclature.IsAccountableInTrueMark);
			if(!hasMarkedProducts)
			{
				permit.Type = OrderCancellationPermitType.AllowCancelOrder;
				return permit;
			}

			var hasNoEdoTasks = !edoTasks.Any();
			if(hasNoEdoTasks)
			{
				permit.Type = OrderCancellationPermitType.AllowCancelOrder;
				return permit;
			}

			bool hasCodes = edoTasks.Any(x => x.OrderEdoRequest.ProductCodes.Any());
			if(!hasCodes)
			{
				// разрешено отменять заказ так как еще не сканировали коды маркировки
				permit.Type = OrderCancellationPermitType.AllowCancelOrder;
				return permit;
			}


			var edoTasksWithCodes = edoTasks
				.Where(x => x.OrderEdoRequest.ProductCodes.Any(c => c.SourceCodeStatus.IsIn(
					SourceProductCodeStatus.Accepted,
					SourceProductCodeStatus.Changed
				)));

			if(edoTasksWithCodes.Count() > 1)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Error,
					"По текущему заказу невозможно автоматически определить правильный документооборот.\n" +
					"Обратитесь за технической поддержкой для помощи в отмене заказа"
				);

				permit.Type = OrderCancellationPermitType.Deny;
				return permit;
			}

			var edoTask = edoTasksWithCodes.SingleOrDefault();

			if(edoTask == null || edoTask.Status == EdoTaskStatus.Cancelled)
			{
				var lastCancelledEdoTask = edoTask;

				if(edoTask == null)
				{
					lastCancelledEdoTask = edoTasks
						.Where(x => x.OrderEdoRequest.ProductCodes.Any(c => c.SourceCodeStatus == SourceProductCodeStatus.Rejected))
						.Where(x => x.Status == EdoTaskStatus.Cancelled)
						.OrderBy(x => x.CreationTime)
						.LastOrDefault();
				}

				if(lastCancelledEdoTask == null)
				{
					permit.Type = OrderCancellationPermitType.AllowCancelOrder;
					return permit;
				}

				bool updCancellationOfferAcceptedByClient =  lastCancelledEdoTask.Status == EdoTaskStatus.Cancelled;

				if(!updCancellationOfferAcceptedByClient)
				{
					_interactiveService.ShowMessage(
						ImportanceLevel.Warning,
						"Для отмены заказа необходимо аннулировать документооброт с клиентом.\n" +
						"Предложение об аннулировании уже было отправлено, необходимо дождаться завершения процесса.\n" +
						"Проверить состояние документооборота можно проверить в журнале Маркировка - Документооборот с клиентом.\n" +
						"Если клиент еще не принял предложение, ему необходимо сообщить об этом."
					);

					permit.Type = OrderCancellationPermitType.Deny;
					return permit;
				}

				permit.Type = OrderCancellationPermitType.AllowCancelOrder;
				return permit;
			}

			// Есть ли уже отправленный чек
			if(edoTask.TaskType == EdoTaskType.Receipt)
			{
				var receiptTask = edoTask.As<ReceiptEdoTask>();
				if(receiptTask.ReceiptStatus != EdoReceiptStatus.Transfering)
				{
					_interactiveService.ShowMessage(
						ImportanceLevel.Warning,
						"По данному заказу уже оформлен и отправлен чек клиенту и отменить его нельзя"
					);

					// запрещено отменять заказ так как уже есть в работе задача на отправку чека
					permit.Type = OrderCancellationPermitType.Deny;
					return permit;
				}

				if(!permit.DocflowCancellationOfferConfirmation && !ConfirmOrderCancellation())
				{
					permit.Type = OrderCancellationPermitType.Deny;
					return permit;
				}
				permit.DocflowCancellationOfferConfirmation = true;

				permit.Type = OrderCancellationPermitType.AllowCancelOrder;
				permit.EdoTaskToCancellationId = edoTask.Id;
				return permit;
			}

			// Есть ли уже отправленный УПД
			if(edoTask.TaskType == EdoTaskType.Document)
			{
				if(!permit.OrderCancellationConfirmation && !ConfirmOrderCancellation())
				{
					permit.Type = OrderCancellationPermitType.Deny;
					return permit;
				}
				permit.OrderCancellationConfirmation = true;

				var documentTask = edoTask.As<DocumentEdoTask>();

				bool updSentToClient = documentTask.Stage.IsIn(
					DocumentEdoTaskStage.Sending,
					DocumentEdoTaskStage.Sent,
					DocumentEdoTaskStage.Completed
				);

				if(updSentToClient)
				{
					var buttonYes = "Аннулировать УПД";
					var buttonNo = "Отмена";
					var answer = _interactiveService.Question(
						new[] { buttonYes, buttonNo },
						"В текущем состоянии отменить заказ невозможно, так как УПД уже отправлено клиенту.\n" +
						"Для отмены заказа необходимо отправить предложение об аннулировании УПД.\n" +
						"После полной отмены документооброта с клиентом будет доступна отмена заказа.\n" +
						"Если продолжите, предложение об аннулировании будет отправлено автоматически, но нужно будет связаться с клиентом для подтверждением аннулирования.\n" +
						"Уверены что хотите отправить предложение об аннулировании УПД клиенту?"
					);
					if(answer == buttonNo)
					{
						permit.Type = OrderCancellationPermitType.Deny;
						return permit;
					}
					permit.DocflowCancellationOfferConfirmation = true;

					permit.Type = OrderCancellationPermitType.AllowCancelDocflow;
					permit.EdoTaskToCancellationId = documentTask.Id;
					return permit;
				}
				else
				{
					permit.Type = OrderCancellationPermitType.AllowCancelOrder;
					permit.EdoTaskToCancellationId = documentTask.Id;
					return permit;
				}
			}

			// Все прочие типы ЭДО задач нельзя отменять
			_interactiveService.ShowMessage(
				ImportanceLevel.Warning,
				"Для заказов с маркированной продукцией предусмотрена отмена только заказов с чеком или УПД.\n" +
				"Для отмены остальных заказов обратитесь за технической поддержкой."
			);
			permit.Type = OrderCancellationPermitType.Deny;
			return permit;
		}

		private bool ConfirmOrderCancellation()
		{
			var buttonYes = "Отменить заказ";
			var buttonNo = "Не отменять";
			var answer = _interactiveService.Question(
				new[] { buttonYes, buttonNo },
				"В заказе есть маркированная продукция по которой уже отсканированы и получены коды маркировки.\n" +
				"Заказ не получиться вернуть обратно в работу, необходимо будет создать новый заказ и сканировать коды заново.\n" +
				"Уверены что хотите отменить заказ?"
			);
			return answer == buttonYes;
		}

		public void CancelDocflowByUser(Order order, int edoTaskId)
		{
			CancelDocflow(order, edoTaskId);

			_interactiveService.ShowMessage(
				ImportanceLevel.Warning,
				"Процесс аннулирования документооборота с клиентом запущен.\n" +
				"Проверить состояние можно\n" +
				"в журнале Маркировка -  Документооборот с клиентом,\n" +
				"а также в личном кабинете оператора ЭДО.\n" +
				"Отменить заказ можно будет после подтверждения клиентом."
			);
		}

		public void AutomaticCancelDocflow(IUnitOfWork uow, Order order, int edoTaskId)
		{
			var edoTask = uow.GetById<EdoTask>(edoTaskId);
			if(edoTask.Status.IsIn(
				EdoTaskStatus.InCancellation,
				EdoTaskStatus.Cancelled
			))
			{
				return;
			}
			CancelDocflow(order,edoTaskId);
		}

		private void CancelDocflow(Order order, int edoTaskId)
		{
			if(edoTaskId == 0)
			{
				throw new ArgumentException("Не указана задача ЭДО для отмены");
			}

			_messageBus.Publish(new RequestTaskCancellationEvent
			{
				TaskId = edoTaskId,
				Reason = $"Отмена заказа №{order.Id}"
			});
		}
	}
}
