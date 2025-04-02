using Edo.Common;
using Edo.Contracts.Messages.Events;
using MassTransit;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Edo.Documents
{
	public class ForResaleDocumentEdoTaskHandler : IDisposable
	{
		private readonly IUnitOfWork _uow;
		private readonly ITrueMarkCodeRepository _trueMarkCodeRepository;
		private readonly TrueMarkTaskCodesValidator _trueMarkTaskCodesValidator;
		private readonly TransferRequestCreator _transferRequestCreator;
		private readonly IBus _messageBus;

		public ForResaleDocumentEdoTaskHandler(
			IUnitOfWork uow,
			ITrueMarkCodeRepository trueMarkCodeRepository,
			TrueMarkTaskCodesValidator trueMarkTaskCodesValidator,
			TransferRequestCreator transferRequestCreator,
			IBus messageBus
			)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_trueMarkCodeRepository = trueMarkCodeRepository ?? throw new ArgumentNullException(nameof(trueMarkCodeRepository));
			_trueMarkTaskCodesValidator = trueMarkTaskCodesValidator ?? throw new ArgumentNullException(nameof(trueMarkTaskCodesValidator));
			_transferRequestCreator = transferRequestCreator ?? throw new ArgumentNullException(nameof(transferRequestCreator));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		public async Task HandleNewForResaleFormalDocument(
			DocumentEdoTask documentEdoTask,
			EdoTaskItemTrueMarkStatusProvider trueMarkCodesChecker,
			CancellationToken cancellationToken
			)
		{
			object message = null;

			var order = documentEdoTask.OrderEdoRequest.Order;

			trueMarkCodesChecker.ClearCache();
			var taskValidationResult = await _trueMarkTaskCodesValidator.ValidateAsync(
				documentEdoTask,
				trueMarkCodesChecker,
				cancellationToken
			);

			if(!taskValidationResult.IsAllValid)
			{
				// регистрация проблемы
				throw new InvalidOperationException("Формирование УПД документа для перепродажи. Имеются коды не прошедшие валидацию в ЧЗ");
			}

			if(taskValidationResult.ReadyToSell)
			{
				CreateUpdDocument(documentEdoTask);

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

		public async Task HandleTransferedForResaleFormalDocument(
			DocumentEdoTask documentEdoTask,
			EdoTaskItemTrueMarkStatusProvider trueMarkCodesChecker,
			CancellationToken cancellationToken
			)
		{

			trueMarkCodesChecker.ClearCache();
			var taskValidationResult = await _trueMarkTaskCodesValidator.ValidateAsync(
				documentEdoTask,
				trueMarkCodesChecker,
				cancellationToken
			);

			if(!taskValidationResult.IsAllValid)
			{
				// регистрация проблемы
				throw new InvalidOperationException("Формирование УПД документа для перепродажи. Имеются коды не прошедшие валидацию в ЧЗ");
			}

			if(!taskValidationResult.ReadyToSell)
			{
				// Поставить задачу в ожидание
				throw new InvalidOperationException("Трансфер не завершен, или имеет ошибку");
			}

			CreateUpdDocument(documentEdoTask);

			var customerDocument = await SendDocument(documentEdoTask, cancellationToken);
			documentEdoTask.Status = EdoTaskStatus.InProgress;
			documentEdoTask.Stage = DocumentEdoTaskStage.Sending;
			var message = new OrderDocumentSendEvent { OrderDocumentId = customerDocument.Id };

			await _uow.SaveAsync(documentEdoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);

			await _messageBus.Publish(message, cancellationToken);
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

		private void CreateUpdDocument(DocumentEdoTask documentEdoTask)
		{
			var order = documentEdoTask.OrderEdoRequest.Order;

			var unprocessedCodes = documentEdoTask.Items.ToList();
			var groupCodesWithTaskItems = TakeGroupCodesWithTaskItems(unprocessedCodes);

			var updInventPositions = new List<EdoUpdInventPosition>();

			foreach(var orderItem in order.OrderItems)
			{
				var codeItemsToAssign = new List<EdoUpdInventPositionCode>();

				if(orderItem.Price <= 0 && documentEdoTask.DocumentType == EdoDocumentType.UPD)
				{
					continue;
				}

				var assignedQuantity = 0;
				while(assignedQuantity < orderItem.Count)
				{
					var availableQuantity = orderItem.Count - assignedQuantity;

					var availableGroupGtins = orderItem.Nomenclature.GroupGtins
						.Where(x => x.CodesCount <= availableQuantity)
						.OrderByDescending(x => x.CodesCount);

					TrueMarkWaterGroupCode groupCode = null;
					foreach(var availableGtin in availableGroupGtins)
					{
						groupCode = groupCodesWithTaskItems.Keys.FirstOrDefault(x => x.GTIN == availableGtin.GtinNumber);
						if(groupCode == null)
						{
							continue;
						}

						// установка в ResultCode всем ProductCode в группе
						var groupTaskItems = groupCodesWithTaskItems[groupCode];
						foreach(var groupTaskItem in groupTaskItems)
						{
							groupTaskItem.ProductCode.ResultCode = groupTaskItem.ProductCode.SourceCode;
							groupTaskItem.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Accepted;
						}

						groupCodesWithTaskItems.Remove(groupCode);
						break;
					}

					if(groupCode != null)
					{
						var codesInGroup = groupCode
							.GetAllCodes()
							.Count(x => x.IsTrueMarkWaterIdentificationCode);

						var codeItem = new EdoUpdInventPositionCode
						{
							GroupCode = groupCode,
							Quantity = codesInGroup
						};
						codeItemsToAssign.Add(codeItem);

						assignedQuantity += codesInGroup;
						continue;
					}


					// затем, если ничего не смогли взять из групповых, то ищем и назначаем из индивидуальных
					// в которыех есть заполенный SourceCode, т.е. исключаем неотсканированные позиции задачи

					TrueMarkWaterIdentificationCode individualCode = null;
					var availableGtins = orderItem.Nomenclature.Gtins.Select(x => x.GtinNumber);
					foreach(var availableGtin in availableGtins)
					{
						var availableCode = unprocessedCodes.FirstOrDefault(x => x.ProductCode.SourceCode.GTIN == availableGtin);
						if(availableCode == null)
						{
							continue;
						}

						// ResultCode будет заполнен, если проиходит повторное создание документов
						// индивидуальные коды при этом будут обновлены после валидации
						if(availableCode.ProductCode.ResultCode == null)
						{
							availableCode.ProductCode.ResultCode = availableCode.ProductCode.SourceCode;
							availableCode.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Accepted;
						}

						individualCode = availableCode.ProductCode.ResultCode;
						unprocessedCodes.Remove(availableCode);
						break;
					}

					if(individualCode != null)
					{
						var codeItem = new EdoUpdInventPositionCode
						{
							IndividualCode = individualCode,
							Quantity = 1
						};

						codeItemsToAssign.Add(codeItem);
						assignedQuantity++;
						continue;
					}

					// Недостаточно кодов для назначения для перепродажи
					throw new InvalidOperationException("Недостаточно кодов для назначения для перепродажи");
				}


				var inventPosition = new EdoUpdInventPosition();
				inventPosition.AssignedOrderItem = orderItem;
				inventPosition.Codes = codeItemsToAssign;

				updInventPositions.Add(inventPosition);
			}

			documentEdoTask.UpdInventPositions.Clear();
			foreach(var updInventPosition in updInventPositions)
			{
				documentEdoTask.UpdInventPositions.Add(updInventPosition);
			}
		}

		private IDictionary<TrueMarkWaterGroupCode, IEnumerable<EdoTaskItem>> TakeGroupCodesWithTaskItems(List<EdoTaskItem> unprocessedTaskItems)
		{
			// нашли все индивидуальные коды, которые содержатся в группах
			var codesThatContainedInGroup = unprocessedTaskItems
				.Where(x => x.ProductCode.SourceCode != null)
				.Where(x => x.ProductCode.SourceCode.IsInvalid == false)
				.Where(x => x.ProductCode.SourceCode.ParentWaterGroupCodeId != null)
				.ToList()
				;

			// исключили из обрабатываемого списка все коды, которые содержатся в группах
			// они не подходят для индивидуальной обработки, потому что не имеют CheckCode
			unprocessedTaskItems.RemoveAll(x => codesThatContainedInGroup.Contains(x));

			var groupped = codesThatContainedInGroup
				.GroupBy(x => x.ProductCode.SourceCode.ParentWaterGroupCodeId);

			var parentCodesIds = groupped
				.Select(x => x.Key)
				.Distinct();
			var parentCodes = parentCodesIds
				.Select(x => _trueMarkCodeRepository.GetParentGroupCode(_uow, x.Value))
				.Distinct();

			var result = new Dictionary<TrueMarkWaterGroupCode, IEnumerable<EdoTaskItem>>();

			foreach(var parentCode in parentCodes)
			{
				result.Add(parentCode, codesThatContainedInGroup
					.Where(ctcig => parentCode
						.GetAllCodes()
						.Where(x => x.IsTrueMarkWaterIdentificationCode)
						.Select(x => x.TrueMarkWaterIdentificationCode)
						.Any(x => x.Id == ctcig.ProductCode.SourceCode.Id)));
			}

			// нашли все групповые коды

			return result;
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
