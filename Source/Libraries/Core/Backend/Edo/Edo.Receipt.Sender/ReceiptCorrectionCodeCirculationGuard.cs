using Edo.Common;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Organizations;

namespace Edo.Receipt.Sender
{
	public enum ReceiptCorrectionCirculationDecision
	{
		Send,
		Wait,
		Reject
	}

	public sealed class ReceiptCorrectionCirculationResult
	{
		private ReceiptCorrectionCirculationResult(
			ReceiptCorrectionCirculationDecision decision,
			string message,
			int? transferIterationId)
		{
			Decision = decision;
			Message = message;
			TransferIterationId = transferIterationId;
		}

		public ReceiptCorrectionCirculationDecision Decision { get; }

		public string Message { get; }

		public int? TransferIterationId { get; }

		public static ReceiptCorrectionCirculationResult Send()
		{
			return new ReceiptCorrectionCirculationResult(ReceiptCorrectionCirculationDecision.Send, null, null);
		}

		public static ReceiptCorrectionCirculationResult Wait(string message, int? transferIterationId = null)
		{
			return new ReceiptCorrectionCirculationResult(
				ReceiptCorrectionCirculationDecision.Wait,
				message,
				transferIterationId);
		}

		public static ReceiptCorrectionCirculationResult Reject(string message)
		{
			return new ReceiptCorrectionCirculationResult(ReceiptCorrectionCirculationDecision.Reject, message, null);
		}
	}

	/// <summary>
	/// Не даёт пробить приход чужим или уже выбывшим кодом и не даёт возврату уйти без марки.
	/// </summary>
	public class ReceiptCorrectionCodeCirculationGuard
	{
		private readonly ILogger<ReceiptCorrectionCodeCirculationGuard> _logger;
		private readonly EdoTaskItemTrueMarkStatusProviderFactory _statusProviderFactory;
		private readonly TransferRequestCreator _transferRequestCreator;

		public ReceiptCorrectionCodeCirculationGuard(
			ILogger<ReceiptCorrectionCodeCirculationGuard> logger,
			EdoTaskItemTrueMarkStatusProviderFactory statusProviderFactory,
			TransferRequestCreator transferRequestCreator)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_statusProviderFactory = statusProviderFactory
				?? throw new ArgumentNullException(nameof(statusProviderFactory));
			_transferRequestCreator = transferRequestCreator
				?? throw new ArgumentNullException(nameof(transferRequestCreator));
		}

		public string GetReturnWithoutMarkError(EdoFiscalDocument document)
		{
			if(document == null
				|| (document.DocumentType != FiscalDocumentType.Return
					&& document.DocumentType != FiscalDocumentType.SaleReturnCorrection))
			{
				return null;
			}

			var names = (document.InventPositions ?? Enumerable.Empty<FiscalInventPosition>())
				.Where(x => x?.EdoTaskItem?.ProductCode != null
					&& x.EdoTaskItem.ProductCode.ResultCode == null
					&& x.EdoTaskItem.ProductCode.SourceCode == null)
				.Select(x => x.Name)
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Distinct()
				.ToList();

			if(names.Count == 0)
			{
				return null;
			}

			return "Чек возврата не содержит код маркировки ("
				+ string.Join(", ", names)
				+ "). Честный знак не вернёт код в оборот, отправка остановлена.";
		}

		public async Task<ReceiptCorrectionCirculationResult> EnsureSaleCanBeSent(
			IUnitOfWork uow,
			EdoFiscalDocument document,
			int cashboxId,
			CancellationToken cancellationToken)
		{
			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(document == null)
			{
				throw new ArgumentNullException(nameof(document));
			}

			if(document.DocumentType != FiscalDocumentType.Sale
				&& document.DocumentType != FiscalDocumentType.SaleCorrection)
			{
				return ReceiptCorrectionCirculationResult.Send();
			}

			var task = document.ReceiptEdoTask;
			var cashboxOrganization = await uow.Session.Query<OrganizationEntity>()
				.FirstOrDefaultAsync(x => x.CashBoxId == cashboxId, cancellationToken);
			var sellerInn = cashboxOrganization?.INN?.Trim();
			if(string.IsNullOrEmpty(sellerInn))
			{
				return ReceiptCorrectionCirculationResult.Reject(
					"Не определена организация кассы. Приход с кодом маркировки не отправлен.");
			}

			var markedItems = new List<EdoTaskItem>();
			var missingMark = false;
			foreach(var position in document.InventPositions ?? Enumerable.Empty<FiscalInventPosition>())
			{
				var productCode = position?.EdoTaskItem?.ProductCode;
				if(productCode == null)
				{
					continue;
				}

				if(productCode.ResultCode == null && productCode.SourceCode == null)
				{
					missingMark = true;
					continue;
				}

				markedItems.Add(position.EdoTaskItem);
			}

			if(missingMark)
			{
				return ReceiptCorrectionCirculationResult.Reject(
					"В приходе есть маркированная позиция без кода. Продажа остановлена.");
			}

			if(markedItems.Count == 0)
			{
				return ReceiptCorrectionCirculationResult.Send();
			}

			markedItems = markedItems
				.GroupBy(x => x.Id)
				.Select(x => x.First())
				.ToList();

			if(task.TransferIterations != null
				&& task.TransferIterations.Any(x => x.Status == TransferEdoRequestIterationStatus.InProgress))
			{
				return ReceiptCorrectionCirculationResult.Wait(
					"Приход ждёт завершения трансфера кодов в Честный знак.");
			}

			var checker = _statusProviderFactory.Create(task);
			checker.UseItems(markedItems);

			IDictionary<TrueMarkWaterIdentificationCode, EdoTaskItemTrueMarkStatus> statuses;
			try
			{
				statuses = await checker.GetCodeItemsStatusesAsync(cancellationToken);
			}
			catch(EdoException ex)
			{
				_logger.LogWarning(
					ex,
					"Не удалось проверить коды документа {DocumentId} в Честном знаке.",
					document.Id);
				return ReceiptCorrectionCirculationResult.Wait(
					"Честный знак не ответил по кодам прихода. Отправка отложена.");
			}

			var needsTransfer = false;
			foreach(var item in markedItems)
			{
				var code = item.ProductCode.ResultCode ?? item.ProductCode.SourceCode;
				if(!statuses.TryGetValue(code, out var codeStatus) || codeStatus?.ProductInstanceStatus == null)
				{
					return ReceiptCorrectionCirculationResult.Reject(
						"Честный знак не вернул статус кода. Приход не отправлен.");
				}

				var instance = codeStatus.ProductInstanceStatus;
				if(instance.Status == ProductInstanceStatusEnum.Retired
					|| instance.Status == ProductInstanceStatusEnum.Withdrawn)
				{
					return ReceiptCorrectionCirculationResult.Wait(
						"Код уже выбыл из оборота. Приход не отправлен, иначе Честный знак зафиксирует повторную продажу. Сначала должен пройти чек возврата с этим кодом.");
				}

				if(instance.Status != ProductInstanceStatusEnum.Introduced)
				{
					return ReceiptCorrectionCirculationResult.Reject(
						$"Код в статусе {instance.Status}. Продажа запрещена.");
				}

				if(!string.Equals(instance.OwnerInn?.Trim(), sellerInn, StringComparison.Ordinal))
				{
					needsTransfer = true;
				}
			}

			if(!needsTransfer)
			{
				return ReceiptCorrectionCirculationResult.Send();
			}

			var orderOrganization = task.FormalEdoRequest?.Order?.Contract?.Organization;
			if(orderOrganization == null || orderOrganization.Id != cashboxOrganization.Id)
			{
				return ReceiptCorrectionCirculationResult.Reject(
					"Код принадлежит не организации кассы, а трансфер на эту кассу невозможен. Приход не отправлен.");
			}

			TransferEdoRequestIteration iteration;
			try
			{
				iteration = await _transferRequestCreator.CreateTransferRequests(
					uow,
					task,
					checker,
					cancellationToken);
			}
			catch(EdoException ex)
			{
				return ReceiptCorrectionCirculationResult.Reject(ex.Message);
			}

			if(iteration == null || !iteration.TransferEdoRequests.Any())
			{
				if(iteration != null)
				{
					await uow.DeleteAsync(iteration, cancellationToken);
				}

				return ReceiptCorrectionCirculationResult.Reject(
					"Владелец кода не совпадает с организацией кассы, трансфер не создан. Приход не отправлен.");
			}

			task.ReceiptStatus = EdoReceiptStatus.Transfering;
			await uow.SaveAsync(task, cancellationToken: cancellationToken);
			await uow.Session.FlushAsync(cancellationToken);
			if(iteration.Id <= 0)
			{
				return ReceiptCorrectionCirculationResult.Reject(
					"Не удалось сохранить трансфер. Приход не отправлен.");
			}

			_logger.LogInformation(
				"Перед приходом документа {DocumentId} создан трансфер {TransferIterationId}: владелец кода не организация кассы {SellerInn}.",
				document.Id,
				iteration.Id,
				sellerInn);

			return ReceiptCorrectionCirculationResult.Wait(
				"Приход не отправлен: код принадлежит другой организации. Запущен трансфер в Честный знак.",
				iteration.Id);
		}
	}
}
