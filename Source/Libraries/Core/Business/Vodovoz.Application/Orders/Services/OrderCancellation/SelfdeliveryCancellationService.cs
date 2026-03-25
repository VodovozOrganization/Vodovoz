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
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Application.Orders.Services.OrderCancellation
{
	/// <summary>
	/// Сервис проверяет возможности отмены отпуска самовывоза
	/// так же уведомляет пользователя и запрашивает подтверждение
	/// </summary>
	public class SelfdeliveryCancellationService : OrderCancellationService
	{
		private readonly IInteractiveService _interactiveService;

		public SelfdeliveryCancellationService(
			IInteractiveService interactiveService,
			IEdoRepository edoRepository, 
			IBus messageBus
		) : base(interactiveService, edoRepository, messageBus)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}

		public OrderCancellationPermit CanDeleteSelfdeliveryDocument(
			IUnitOfWork uow,
			SelfDeliveryDocument selfDeliveryDocument,
			OrderCancellationPermit inputPermit = null
			)
		{
			var permit = CanCancelOrder(
				uow,
				selfDeliveryDocument.Order,
				inputPermit
			);
			return permit;
		}

		protected override void WaitDocflowCancellationMessage()
		{
			_interactiveService.ShowMessage(
				ImportanceLevel.Warning,
				"Для удаления документа отпуска самовывоза необходимо аннулировать документооброт с клиентом.\n" +
				"Предложение об аннулировании уже было отправлено, необходимо дождаться завершения процесса.\n" +
				"Проверить состояние документооборота можно в журнале Маркировка - Документооборот с клиентом.\n" +
				"Если клиент еще не принял предложение, ему необходимо сообщить об этом."
			);
		}

		protected override bool UpdCancellationConfirmationQuestion()
		{
			var buttonYes = "Аннулировать УПД";
			var buttonNo = "Отмена";
			var answer = _interactiveService.Question(
				new[] { buttonYes, buttonNo },
				"В текущем состоянии удалить документ отпуска самовывоза невозможно, так как УПД уже отправлен клиенту.\n" +
				"Чтобы была возможность удалить, необходимо отправить предложение об аннулировании УПД.\n" +
				"Только после полной отмены документооброта с клиентом будет доступна возможность удаления.\n" +
				"Если продолжите, предложение об аннулировании будет отправлено автоматически, но нужно будет связаться с клиентом для подтверждением аннулирования.\n" +
				"Уверены что хотите отправить предложение об аннулировании УПД клиенту?"
			);
			return answer == buttonYes;
		}
		protected override void CantDetermineSingleDocflowMessage()
		{
			_interactiveService.ShowMessage(
				ImportanceLevel.Error,
				"По текущему документу отпуска самовывоза невозможно автоматически определить правильный документооборот.\n" +
				"Обратитесь за технической поддержкой"
			);
		}

		protected override bool ConfirmOrderCancellationQuestion()
		{
			var buttonYes = "Удалить отпуск самовывоза";
			var buttonNo = "Не удалять";
			var answer = _interactiveService.Question(
				new[] { buttonYes, buttonNo },
				"В документе отпуска самовывоза есть маркированная продукция по которой уже отсканированы и получены коды маркировки.\n" +
				"Уверены что хотите удалить документ?"
			);
			return answer == buttonYes;
		}

		protected override void CantCancelOtherDocflowsMessage()
		{
			_interactiveService.ShowMessage(
				ImportanceLevel.Warning,
				"Для документа отпуска самовывоза с маркированной продукцией предусмотрено удаление только если был чек или УПД.\n" +
				"Для удаления остальных документов обратитесь за технической поддержкой."
			);
		}

		protected override void CantCancelReceiptDocflowMessage()
		{
			_interactiveService.ShowMessage(
				ImportanceLevel.Warning,
				"По данному документу отпуска самовывоза уже оформлен и отправлен чек клиенту и удалить его нельзя"
			);
		}


		protected override void DocflowCancellationStartedMessage()
		{
			_interactiveService.ShowMessage(
				ImportanceLevel.Warning,
				"Процесс аннулирования документооборота с клиентом запущен.\n" +
				"Проверить состояние можно\n" +
				"в журнале Маркировка -  Документооборот с клиентом,\n" +
				"а также в личном кабинете оператора ЭДО.\n" +
				"Удалить отпуск самовывоза можно будет после подтверждения клиентом."
			);
		}
	}
}
