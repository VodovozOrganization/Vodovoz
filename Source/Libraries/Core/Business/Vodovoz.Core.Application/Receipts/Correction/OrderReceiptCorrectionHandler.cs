using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.Receipts;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Services.Receipts;

namespace Vodovoz.Core.Application.Receipts.Correction
{
	public class OrderReceiptCorrectionHandler : IOrderReceiptCorrectionHandler
	{
		private readonly ILogger<OrderReceiptCorrectionHandler> _logger;
		private readonly IReceiptCorrectionRepository _receiptCorrectionRepository;
		private readonly IFiscalOrderSnapshotBuilder _fiscalOrderSnapshotBuilder;
		private readonly IFiscalChangeDetector _fiscalChangeDetector;
		private readonly IReceiptCorrectionScenarioClassifier _scenarioClassifier;
		private readonly IReceiptCorrectionExplanatoryNoteBuilder _explanatoryNoteBuilder;
		private readonly ReceiptCorrectionFiscalDocumentBuilder _fiscalDocumentBuilder;
		private readonly ReceiptCorrectionEdoTaskFactory _edoTaskFactory;
		private readonly IOrderReceiptHandler _orderReceiptHandler;

		public OrderReceiptCorrectionHandler(
			ILogger<OrderReceiptCorrectionHandler> logger,
			IReceiptCorrectionRepository receiptCorrectionRepository,
			IFiscalOrderSnapshotBuilder fiscalOrderSnapshotBuilder,
			IFiscalChangeDetector fiscalChangeDetector,
			IReceiptCorrectionScenarioClassifier scenarioClassifier,
			IReceiptCorrectionExplanatoryNoteBuilder explanatoryNoteBuilder,
			ReceiptCorrectionFiscalDocumentBuilder fiscalDocumentBuilder,
			ReceiptCorrectionEdoTaskFactory edoTaskFactory,
			IOrderReceiptHandler orderReceiptHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_receiptCorrectionRepository = receiptCorrectionRepository ?? throw new ArgumentNullException(nameof(receiptCorrectionRepository));
			_fiscalOrderSnapshotBuilder = fiscalOrderSnapshotBuilder ?? throw new ArgumentNullException(nameof(fiscalOrderSnapshotBuilder));
			_fiscalChangeDetector = fiscalChangeDetector ?? throw new ArgumentNullException(nameof(fiscalChangeDetector));
			_scenarioClassifier = scenarioClassifier ?? throw new ArgumentNullException(nameof(scenarioClassifier));
			_explanatoryNoteBuilder = explanatoryNoteBuilder ?? throw new ArgumentNullException(nameof(explanatoryNoteBuilder));
			_fiscalDocumentBuilder = fiscalDocumentBuilder ?? throw new ArgumentNullException(nameof(fiscalDocumentBuilder));
			_edoTaskFactory = edoTaskFactory ?? throw new ArgumentNullException(nameof(edoTaskFactory));
			_orderReceiptHandler = orderReceiptHandler ?? throw new ArgumentNullException(nameof(orderReceiptHandler));
		}

		public ReceiptCorrectionPreview TryGetCorrectionPreview(IUnitOfWork uow, Order order)
		{
			var evaluation = Evaluate(uow, order);
			if(evaluation == null)
			{
				return ReceiptCorrectionPreview.None;
			}

			return new ReceiptCorrectionPreview
			{
				WillStartProcess = true,
				ScenarioType = evaluation.ScenarioType,
				PlannedDocumentTypes = evaluation.PlannedDocumentTypes.ToList()
			};
		}

		public void TryStartCorrectionProcess(IUnitOfWork uow, Order order)
		{
			if(order == null)
			{
				throw new ArgumentNullException(nameof(order));
			}

			var evaluation = Evaluate(uow, order);
			if(evaluation == null)
			{
				return;
			}

			var sourceDocument = uow.GetById<EdoFiscalDocument>(evaluation.PreviousSnapshot.SourceEdoFiscalDocumentId.Value);
			if(sourceDocument == null)
			{
				_logger.LogWarning(
					"Для заказа {OrderId} не найден исходный EdoFiscalDocument #{DocumentId} для корректировки.",
					order.Id,
					evaluation.PreviousSnapshot.SourceEdoFiscalDocumentId);
				return;
			}

			var sourceTask = sourceDocument.ReceiptEdoTask;
			if(sourceTask == null)
			{
				_logger.LogWarning(
					"Для заказа {OrderId} у исходного EdoFiscalDocument #{DocumentId} не найдена ReceiptEdoTask.",
					order.Id,
					sourceDocument.Id);
				return;
			}

			var correctionTask = _edoTaskFactory.Create(uow, order, sourceTask);
			uow.Save(correctionTask);

			var process = new ReceiptCorrectionProcess
			{
				OrderId = order.Id,
				SourceEdoFiscalDocumentId = evaluation.PreviousSnapshot.SourceEdoFiscalDocumentId,
				ReceiptEdoTaskId = correctionTask.Id,
				BaselineFiscalDocumentNumber = evaluation.PreviousSnapshot.FiscalDocumentNumber,
				BaselineFiscalDocumentDate = evaluation.PreviousSnapshot.FiscalDocumentDate,
				BaselineSum = evaluation.PreviousSnapshot.Sum,
				ScenarioType = evaluation.ScenarioType,
				Status = ReceiptCorrectionProcessStatus.Pending,
				ChangeFingerprint = evaluation.ChangeFingerprint,
				CreatedDate = DateTime.Now,
				OrganizationId = order.Contract?.Organization?.Id
			};

			foreach(var documentType in evaluation.PlannedDocumentTypes)
			{
				process.Documents.Add(new ReceiptCorrectionProcessDocument
				{
					Process = process,
					PlannedDocumentType = documentType,
					DocumentGuid = Guid.NewGuid(),
					Status = ReceiptCorrectionProcessStatus.Pending
				});
			}

			var templateType = _scenarioClassifier.GetExplanatoryNoteTemplate(evaluation.ScenarioType);
			var previousOrganizationId = evaluation.PreviousSnapshot.OrganizationId;
			var currentOrganizationId = process.OrganizationId;
			var previousSigner = ResolveCashierSigner(uow, previousOrganizationId ?? currentOrganizationId);
			var currentSigner = ResolveCashierSigner(uow, currentOrganizationId);
			var previousContext = ResolveBuildContext(uow, previousOrganizationId ?? currentOrganizationId, order.Id);
			var currentContext = ResolveBuildContext(uow, currentOrganizationId, order.Id);

			process.ExplanatoryNotes.Add(_explanatoryNoteBuilder.Build(
				process,
				templateType,
				evaluation.ChangeSet,
				previousSigner.FullName,
				previousOrganizationId ?? currentOrganizationId,
				previousSigner.SignatureId,
				previousContext));

			if(evaluation.ScenarioType == ReceiptCorrectionScenarioType.OrganizationChange
				|| evaluation.ScenarioType == ReceiptCorrectionScenarioType.ClientChange)
			{
				process.ExplanatoryNotes.Add(_explanatoryNoteBuilder.Build(
					process,
					templateType,
					evaluation.ChangeSet,
					currentSigner.FullName,
					currentOrganizationId,
					currentSigner.SignatureId,
					currentContext));
			}

			uow.Save(process);

			foreach(var processDocument in process.Documents)
			{
				var edoFiscalDocument = _fiscalDocumentBuilder.CreateFromSource(
					sourceDocument,
					correctionTask,
					process,
					processDocument);
				uow.Save(edoFiscalDocument);
				processDocument.EdoFiscalDocumentId = edoFiscalDocument.Id;
				uow.Save(processDocument);
			}

			_logger.LogInformation(
				"Создана пачка корректировки чека {ProcessId} для заказа {OrderId}, сценарий {ScenarioType} (исходный Edo-документ {SourceEdoFiscalDocumentId}, задача ЭДО {ReceiptEdoTaskId}).",
				process.Id,
				order.Id,
				evaluation.ScenarioType,
				process.SourceEdoFiscalDocumentId,
				process.ReceiptEdoTaskId);
		}

		private CorrectionEvaluation Evaluate(IUnitOfWork uow, Order order)
		{
			if(order == null)
			{
				throw new ArgumentNullException(nameof(order));
			}

			if(!IsEligibleOrderStatus(order))
			{
				return null;
			}

			if(order.Id == 0 || !_orderReceiptHandler.HasNeededReceipt(order.Id))
			{
				return null;
			}

			var previousSnapshot = _fiscalOrderSnapshotBuilder.BuildPreviousSnapshot(uow, order.Id);
			if(previousSnapshot?.SourceEdoFiscalDocumentId == null)
			{
				_logger.LogInformation("Для заказа {OrderId} не найден baseline Edo-документ для корректировки.", order.Id);
				return null;
			}

			var currentSnapshot = _fiscalOrderSnapshotBuilder.BuildFromOrder(order);
			var isFullCancellation = order.OrderStatus == OrderStatus.Canceled
				|| order.OrderStatus == OrderStatus.DeliveryCanceled;

			var changeSet = _fiscalChangeDetector.DetectChanges(previousSnapshot, currentSnapshot, isFullCancellation);
			if(!changeSet.HasChanges)
			{
				return null;
			}

			var fingerprint = changeSet.BuildFingerprint();
			if(_receiptCorrectionRepository.ProcessExistsByFingerprint(uow, order.Id, fingerprint))
			{
				_logger.LogInformation(
					"Пачка корректировки чека для заказа {OrderId} с отпечатком {Fingerprint} уже существует.",
					order.Id,
					fingerprint);
				return null;
			}

			var scenarioType = _scenarioClassifier.Classify(changeSet);
			if(scenarioType == ReceiptCorrectionScenarioType.None)
			{
				return null;
			}

			var plannedDocumentTypes = _scenarioClassifier.GetPlannedDocumentTypes(scenarioType);
			if(plannedDocumentTypes == null || !plannedDocumentTypes.Any())
			{
				return null;
			}

			return new CorrectionEvaluation
			{
				PreviousSnapshot = previousSnapshot,
				ChangeSet = changeSet,
				ChangeFingerprint = fingerprint,
				ScenarioType = scenarioType,
				PlannedDocumentTypes = plannedDocumentTypes
			};
		}

		private static bool IsEligibleOrderStatus(Order order)
		{
			return order.OrderStatus == OrderStatus.Shipped
				|| order.OrderStatus == OrderStatus.Closed
				|| order.OrderStatus == OrderStatus.Canceled
				|| order.OrderStatus == OrderStatus.DeliveryCanceled;
		}

		private static CashierSignerInfo ResolveCashierSigner(IUnitOfWork uow, int? organizationId)
		{
			if(uow == null || !organizationId.HasValue)
			{
				return CashierSignerInfo.Empty;
			}

			var organization = uow.GetById<OrganizationEntity>(organizationId.Value);
			var version = organization?.ActiveOrganizationVersion;
			if(version == null)
			{
				return CashierSignerInfo.Empty;
			}

			var cashier = version.Cashier ?? version.Leader;
			var signature = version.SignatureCashier ?? version.SignatureLeader;

			return new CashierSignerInfo
			{
				FullName = cashier?.FullName,
				SignatureId = signature?.Id > 0 ? signature.Id : (int?)null
			};
		}

		private static ExplanatoryNoteBuildContext ResolveBuildContext(IUnitOfWork uow, int? organizationId, int orderId)
		{
			var context = new ExplanatoryNoteBuildContext
			{
				OrderId = orderId
			};

			if(uow == null || !organizationId.HasValue)
			{
				return context;
			}

			var organization = uow.GetById<OrganizationEntity>(organizationId.Value);
			if(organization == null)
			{
				return context;
			}

			var version = organization.ActiveOrganizationVersion;
			context.OrganizationName = string.IsNullOrWhiteSpace(organization.FullName)
				? organization.Name
				: organization.FullName;
			context.LeaderFullName = version?.Leader?.FullName;
			context.IsIndividualEntrepreneur = !string.IsNullOrWhiteSpace(organization.INN)
				&& organization.INN.Length == 12;

			return context;
		}

		private sealed class CashierSignerInfo
		{
			public static CashierSignerInfo Empty { get; } = new CashierSignerInfo();

			public string FullName { get; set; }

			public int? SignatureId { get; set; }
		}

		private sealed class CorrectionEvaluation
		{
			public FiscalOrderSnapshot PreviousSnapshot { get; set; }

			public FiscalChangeSet ChangeSet { get; set; }

			public string ChangeFingerprint { get; set; }

			public ReceiptCorrectionScenarioType ScenarioType { get; set; }

			public IList<FiscalDocumentType> PlannedDocumentTypes { get; set; }
		}
	}
}
