using Edo.Common;
using Edo.Common.Services;
using Edo.Contracts.Messages.Events;
using Edo.Documents.Services;
using Edo.Problems;
using Edo.Problems.Custom.Sources;
using Grpc.Core.Logging;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Renci.SshNet.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Edo.Documents
{
	public class ForOwnNeedDocumentEdoTaskHandler : IDisposable
	{
		private readonly IUnitOfWork _uow;
		private readonly ILogger<ForOwnNeedDocumentEdoTaskHandler> _logger;
		private readonly ITrueMarkCodesValidator _trueMarkTaskCodesValidator;
		private readonly ITrueMarkCodeRepository _trueMarkCodeRepository;
		private readonly TransferRequestCreator _transferRequestCreator;
		private readonly ITrueMarkCodesPool _trueMarkCodesPool;
		private readonly ITrueMarkCodesPoolCodeProvider _trueMarkCodesPoolCodeProvider;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;
		private readonly IUpdDocumentBuilder _updDocumentBuilder;
		private readonly EdoProblemRegistrar _edoProblemRegistrar;
		private readonly IBus _messageBus;

		public ForOwnNeedDocumentEdoTaskHandler(
			IUnitOfWork uow,
			ILogger<ForOwnNeedDocumentEdoTaskHandler> logger,
			ITrueMarkCodesValidator trueMarkTaskCodesValidator,
			ITrueMarkCodeRepository trueMarkCodeRepository,
			TransferRequestCreator transferRequestCreator,
			ITrueMarkCodesPool trueMarkCodesPool,
			ITrueMarkCodesPoolCodeProvider trueMarkCodesPoolCodeProvider,
			IUpdDocumentBuilder updDocumentBuilder,
			ITrueMarkWaterCodeService trueMarkWaterCodeService,
			EdoProblemRegistrar edoProblemRegistrar,
			IBus messageBus
			)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_trueMarkTaskCodesValidator = trueMarkTaskCodesValidator ?? throw new ArgumentNullException(nameof(trueMarkTaskCodesValidator));
			_trueMarkCodeRepository = trueMarkCodeRepository ?? throw new ArgumentNullException(nameof(trueMarkCodeRepository));
			_transferRequestCreator = transferRequestCreator ?? throw new ArgumentNullException(nameof(transferRequestCreator));
			_trueMarkCodesPool = trueMarkCodesPool ?? throw new ArgumentNullException(nameof(trueMarkCodesPool));
			_trueMarkCodesPoolCodeProvider = trueMarkCodesPoolCodeProvider ?? throw new ArgumentNullException(nameof(trueMarkCodesPoolCodeProvider));
			_trueMarkWaterCodeService = trueMarkWaterCodeService ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
			_updDocumentBuilder = updDocumentBuilder ?? throw new ArgumentNullException(nameof(updDocumentBuilder));
			_edoProblemRegistrar = edoProblemRegistrar ?? throw new ArgumentNullException(nameof(edoProblemRegistrar));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		public async Task HandleNewForOwnNeedsFormalDocument(
			DocumentEdoTask documentEdoTask,
			EdoTaskItemTrueMarkStatusProvider trueMarkCodesChecker,
			CancellationToken cancellationToken)
		{
			if(!IsFormalDocument(documentEdoTask))
			{
				return;
			}

			object message = null;

			var order = documentEdoTask.FormalEdoRequest.Order;
			var reasonForLeaving = order.Client.ReasonForLeaving;

			if(reasonForLeaving is ReasonForLeaving.Resale)
			{
				throw new InvalidOperationException("Ошибочный вызов подготовки документа для собственных нужд " +
					"для заказа с причиной выбытия товара: перепродажа. Необходимо проверить алгоритм подготовки документов");
			}

			bool isAllValid = true;
			int attempts = 5;
			TrueMarkTaskValidationResult taskValidationResult;

			do
			{
				if(!isAllValid)
				{
					attempts--;
				}

				documentEdoTask.UpdInventPositions.Clear();
				await _updDocumentBuilder.BuildUpdDocumentAsync(documentEdoTask, cancellationToken);

				// проверить коды в ЧЗ, не валидные снова заменить кодами из пула
				trueMarkCodesChecker.ClearCache();
				taskValidationResult = await _trueMarkTaskCodesValidator.ValidateAsync(
					documentEdoTask,
					trueMarkCodesChecker,
					cancellationToken
				);

				isAllValid = taskValidationResult.IsAllValid;

				if(!isAllValid)
				{
					var hasGroupInvalidCodes = false;
					foreach(var codeResult in taskValidationResult.CodeResults)
					{
						if(codeResult.IsValid)
						{
							continue;
						}

						var isGroupCode = codeResult.EdoTaskItem.ProductCode.ResultCode?.ParentWaterGroupCodeId != null;

						if(isGroupCode)
						{
							codeResult.EdoTaskItem.ProductCode.SourceCode = null;
							codeResult.EdoTaskItem.ProductCode.ResultCode = null;
							codeResult.EdoTaskItem.ProductCode.SourceCodeStatus = SourceProductCodeStatus.New;
							codeResult.EdoTaskItem.ProductCode.Problem = ProductCodeProblem.Unscanned;
							hasGroupInvalidCodes = true;
						}
						else
						{
							var gtin = (
									from gtinEntity in _uow.Session.Query<GtinEntity>()
									where gtinEntity.GtinNumber == codeResult.EdoTaskItem.ProductCode.ResultCode.Gtin
									select gtinEntity
								)
								.FirstOrDefault();

							var newCode = await LoadCodeFromPool(gtin, GetOrderOrganizationInn(documentEdoTask), cancellationToken);
							codeResult.EdoTaskItem.ProductCode.ResultCode = newCode;
							codeResult.EdoTaskItem.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Changed;
						}
					}

					if(hasGroupInvalidCodes)
					{
						documentEdoTask.UpdInventPositions.Clear();
					}
				}
			} while(!isAllValid && attempts > 0);

			if(!isAllValid)
			{
				// регистрировать проблему
				throw new InvalidOperationException("Не удалось назначить коды");
			}

			if(taskValidationResult.ReadyToSell)
			{
				var customerDocument = await SendDocument(documentEdoTask, cancellationToken);
				documentEdoTask.Status = EdoTaskStatus.InProgress;
				documentEdoTask.Stage = DocumentEdoTaskStage.Sending;
				message = new OrderDocumentSendEvent { OrderDocumentId = customerDocument.Id };
			}
			else
			{
				// создать трансфер
				var iteration = await _transferRequestCreator.CreateTransferRequests(
					_uow,
					documentEdoTask,
					trueMarkCodesChecker,
					cancellationToken
				);
				documentEdoTask.Status = EdoTaskStatus.InProgress;
				documentEdoTask.Stage = DocumentEdoTaskStage.Transfering;
				message = new TransferRequestCreatedEvent { TransferIterationId = iteration.Id };
			}

			await _uow.SaveAsync(documentEdoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);

			await _messageBus.Publish(message, cancellationToken);
		}

		public async Task HandleTransferedForOwnNeedsFormalDocument(
			DocumentEdoTask documentEdoTask,
			EdoTaskItemTrueMarkStatusProvider trueMarkCodesChecker,
			CancellationToken cancellationToken
			)
		{
			// проверить коды в ЧЗ, не валидные снова заменить кодами из пула
			trueMarkCodesChecker.ClearCache();
			var taskValidationResult = await _trueMarkTaskCodesValidator.ValidateAsync(
				documentEdoTask,
				trueMarkCodesChecker,
				cancellationToken
			);

			if(!taskValidationResult.IsAllValid)
			{
				await _updDocumentBuilder.BuildUpdDocumentAsync(documentEdoTask, cancellationToken);
				return;
			}

			if(!taskValidationResult.ReadyToSell)
			{
				var notReadyTaskItems = taskValidationResult.CodeResults.Where(x => !x.ReadyToSell)
					.Select(x => x.EdoTaskItem);
				await _edoProblemRegistrar.RegisterCustomProblem<HasNotTransferedCodesOnTransferComplete>(
					documentEdoTask,
					notReadyTaskItems,
					cancellationToken
				);
				return;
			}

			var customerDocument = await SendDocument(documentEdoTask, cancellationToken);
			documentEdoTask.Status = EdoTaskStatus.InProgress;
			documentEdoTask.Stage = DocumentEdoTaskStage.Sending;
			var message = new OrderDocumentSendEvent { OrderDocumentId = customerDocument.Id };

			await _uow.SaveAsync(documentEdoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);

			await _messageBus.Publish(message, cancellationToken);
		}

		private async Task<TrueMarkWaterIdentificationCode> LoadCodeFromPool(
			GtinEntity gtin,
			string organizationInn,
			CancellationToken cancellationToken)
		{
			try
			{
				return await _trueMarkCodesPoolCodeProvider.TakeValidCodeAsync(
					_trueMarkCodesPool,
					gtin,
					organizationInn,
					cancellationToken);
			}
			catch(EdoCodePoolMissingCodeException ex)
			{
				throw new EdoProblemException(ex, new[]
				{
					new EdoProblemGtinItem
					{
						Gtin = gtin
					}
				});
			}
		}
		private static string GetOrderOrganizationInn(DocumentEdoTask documentEdoTask)
		{
			return documentEdoTask.FormalEdoRequest.Order.Contract.Organization.INN;
		}

		private bool IsFormalDocument(DocumentEdoTask edoTask)
		{
			switch(edoTask.DocumentType)
			{
				case EdoDocumentType.UPD:
					return true;
				case EdoDocumentType.Bill:
					return false;
				default:
					throw new EdoException($"Неизвестный тип документа {edoTask.DocumentType}.");
			}
		}

		private async Task<OrderEdoDocument> SendDocument(DocumentEdoTask edoTask, CancellationToken cancellationToken)
		{
			edoTask.Stage = DocumentEdoTaskStage.Sending;

			var customerEdoDocument = new OrderEdoDocument
			{
				DocumentTaskId = edoTask.Id,
				DocumentType = edoTask.DocumentType,
				Status = EdoDocumentStatus.NotStarted,
				EdoType = EdoType.Taxcom,
				Type = OutgoingEdoDocumentType.Order
			};

			await _uow.SaveAsync(customerEdoDocument, cancellationToken: cancellationToken);
			return customerEdoDocument;
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
