using Edo.Common;
using Edo.Contracts.Messages.Events;
using Edo.Problems.Custom.Sources;
using Edo.Problems;
using Edo.Problems.Exception;
using MassTransit;
using NHibernate;
using QS.DomainModel.UoW;
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
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Edo.Documents
{
	public class ForOwnNeedDocumentEdoTaskHandler : IDisposable
	{
		private readonly IUnitOfWork _uow;
		private readonly ITrueMarkCodesValidator _trueMarkTaskCodesValidator;
		private readonly ITrueMarkCodeRepository _trueMarkCodeRepository;
		private readonly TransferRequestCreator _transferRequestCreator;
		private readonly ITrueMarkCodesPool _trueMarkCodesPool;
		private readonly EdoProblemRegistrar _edoProblemRegistrar;
		private readonly IBus _messageBus;

		public ForOwnNeedDocumentEdoTaskHandler(
			IUnitOfWork uow,
			ITrueMarkCodesValidator trueMarkTaskCodesValidator,
			ITrueMarkCodeRepository trueMarkCodeRepository,
			TransferRequestCreator transferRequestCreator,
			ITrueMarkCodesPool trueMarkCodesPool,
			EdoProblemRegistrar edoProblemRegistrar,
			IBus messageBus
			)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_trueMarkTaskCodesValidator = trueMarkTaskCodesValidator ?? throw new ArgumentNullException(nameof(trueMarkTaskCodesValidator));
			_trueMarkCodeRepository = trueMarkCodeRepository ?? throw new ArgumentNullException(nameof(trueMarkCodeRepository));
			_transferRequestCreator = transferRequestCreator ?? throw new ArgumentNullException(nameof(transferRequestCreator));
			_trueMarkCodesPool = trueMarkCodesPool ?? throw new ArgumentNullException(nameof(trueMarkCodesPool));
			_edoProblemRegistrar = edoProblemRegistrar ?? throw new ArgumentNullException(nameof(edoProblemRegistrar));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		public async Task HandleNewForOwnNeedsFormalDocument(
			DocumentEdoTask documentEdoTask, 
			EdoTaskItemTrueMarkStatusProvider trueMarkCodesChecker,
			CancellationToken cancellationToken
			)
		{
			if(!IsFormalDocument(documentEdoTask))
			{
				return;
			}

			object message = null;

			var order = documentEdoTask.FormalEdoRequest.Order;
			var reasonForLeaving = order.Client.ReasonForLeaving;

			if(reasonForLeaving == ReasonForLeaving.Resale)
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
				await CreateUpdDocument(documentEdoTask, trueMarkCodesChecker, cancellationToken);

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

						var isGroupCode = codeResult.EdoTaskItem.ProductCode.ResultCode.ParentWaterGroupCodeId != null;

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
							
							var newCode = await LoadCodeFromPool(gtin, cancellationToken);
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
				await CreateUpdDocument(documentEdoTask, trueMarkCodesChecker, cancellationToken);
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


		private async Task CreateUpdDocument(
			DocumentEdoTask documentEdoTask,
			EdoTaskItemTrueMarkStatusProvider trueMarkCodesChecker,
			CancellationToken cancellationToken
			)
		{
			var order = documentEdoTask.FormalEdoRequest.Order;

			var unprocessedCodes = documentEdoTask.Items.ToList();
			var groupCodesWithTaskItems = await TakeGroupCodesWithTaskItems(unprocessedCodes, cancellationToken);

			// ---------------------------------------------------------------------
			// алгоритм назначения кодов на товары в заказе
			// результатом должен быть документ УПД
			// в котором созданы инвентарные позиции в кол-ве равном строкам товаров в заказе
			// к каждой инвентарной позиции привязаны коды в кол-ве равном кол-ву товаров в заказе
			var updInventPositions = new List<EdoUpdInventPosition>();
			var orderItemsByPriceDesc = order.OrderItems.OrderByDescending(x => x.Price).ToArray();
			
			foreach(var orderItem in orderItemsByPriceDesc)
			{
				// Процесс создания инвентарной позиции УПД
				// и поиск и назначение соответствующих кодов

				var codeItemsToAssign = new List<EdoUpdInventPositionCode>();

				if(orderItem.ActualSum <= 0
					&& documentEdoTask.DocumentType == EdoDocumentType.UPD)
				{
					if(orderItem.Nomenclature.IsAccountableInTrueMark && unprocessedCodes.Any())
					{
						var i = 0;
						
						while(i < unprocessedCodes.Count)
						{
							if(unprocessedCodes[i].ProductCode.SourceCode != null
								&& unprocessedCodes[i].ProductCode.ResultCode is null
								&& orderItem.Nomenclature.Gtins.Any(x => x.GtinNumber == unprocessedCodes[i].ProductCode.SourceCode.Gtin))
							{
								await _trueMarkCodesPool.PutCodeAsync(unprocessedCodes[i].ProductCode.SourceCode.Id, cancellationToken);
								documentEdoTask.Items.Remove(unprocessedCodes[i]);
								unprocessedCodes.RemoveAt(i);
							}
							else
							{
								i++;
							}
						}
					}
					
					continue;
				}

				if(orderItem.Nomenclature.IsAccountableInTrueMark)
				{
					var assignedQuantity = 0;
					while(assignedQuantity < orderItem.CurrentCount)
					{
						// Необходимо найти коды для номенклатуры на все кол-во
						// и привязать их к инвентарной позиции УПД

						var availableQuantity = orderItem.CurrentCount - assignedQuantity;

						// сначала ищем и назначем из групповых кодов

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
						var validUnprocessedCodes = unprocessedCodes
							.Where(x => x.ProductCode.SourceCode != null)
							.ToList();

						TrueMarkWaterIdentificationCode individualCode = null;
						var availableGtins = orderItem.Nomenclature.Gtins;
						foreach(var availableGtin in availableGtins)
						{
							var availableCode = validUnprocessedCodes.FirstOrDefault(x => x.ProductCode.SourceCode.Gtin == availableGtin.GtinNumber);
							if(availableCode == null)
							{
								continue;
							}

							// ResultCode будет заполнен, если проиходит повторное создание документов
							// индивидуальные коды при этом будут обновлены после валидации
							if(availableCode.ProductCode.ResultCode == null)
							{
								if(availableCode.ProductCode.Problem == ProductCodeProblem.None
									&& availableCode.ProductCode.SourceCodeStatus != SourceProductCodeStatus.SavedToPool)
								{
									availableCode.ProductCode.ResultCode = availableCode.ProductCode.SourceCode;
									availableCode.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Accepted;
								}
								else
								{
									var newCode = await LoadCodeFromPool(availableGtin, cancellationToken);
									availableCode.ProductCode.ResultCode = newCode;
									availableCode.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Changed;
								}
							}

							individualCode = availableCode.ProductCode.ResultCode;
							validUnprocessedCodes.Remove(availableCode);
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

						// затем, если ничего не смогли взять из индивидуальных, то берем неотсканированную позицию
						// заполняем ее из пула и используем в назначении
						var unscannedCodes = unprocessedCodes
							.Where(x => x.ProductCode.SourceCode == null)
							.ToList();

						var unscannedCode = unscannedCodes.FirstOrDefault();
						if(unscannedCode != null)
						{
							// ResultCode будет заполнен, если проиходит повторное создание документов
							// индивидуальные коды при этом будут обновлены после валидации
							if(unscannedCode.ProductCode.ResultCode == null)
							{
								var forUnscannedAvailableGtins = orderItem.Nomenclature.Gtins;
								var newCode = await LoadCodeFromPool(forUnscannedAvailableGtins, cancellationToken);
								unscannedCode.ProductCode.ResultCode = newCode;
								unscannedCode.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Changed;
							}

							unprocessedCodes.Remove(unscannedCode);

							var codeItem = new EdoUpdInventPositionCode
							{
								IndividualCode = unscannedCode.ProductCode.ResultCode,
								Quantity = 1
							};

							codeItemsToAssign.Add(codeItem);
							assignedQuantity++;
							continue;
						}

						// если не отсканированных нет, но назначить код все еще есть необходимость
						// то создаем новый taskItem и назначаем код из пула в него и в инвентарную позицию УПД
						var forNewAvailableGtins = orderItem.Nomenclature.Gtins;
						var forNewCode = await LoadCodeFromPool(forNewAvailableGtins, cancellationToken);
						
						var newAutoTrueMarkProductCode = new AutoTrueMarkProductCode
						{
							ResultCode = forNewCode,
							SourceCode = forNewCode,
							SourceCodeStatus = SourceProductCodeStatus.Accepted,
							Problem = ProductCodeProblem.None,
							CustomerEdoRequest = documentEdoTask.FormalEdoRequest
						};

						await _uow.SaveAsync(newAutoTrueMarkProductCode, cancellationToken: cancellationToken);

						var newTaskItem = new EdoTaskItem
						{
							ProductCode = newAutoTrueMarkProductCode,
							CustomerEdoTask = documentEdoTask
						};
						documentEdoTask.Items.Add(newTaskItem);

						var forNewCodeItem = new EdoUpdInventPositionCode
						{
							IndividualCode = newTaskItem.ProductCode.ResultCode,
							Quantity = 1
						};

						codeItemsToAssign.Add(forNewCodeItem);
						assignedQuantity++;
					}
				}

				var inventPosition = new EdoUpdInventPosition();
				inventPosition.AssignedOrderItem = orderItem;
				inventPosition.Codes = codeItemsToAssign;

				updInventPositions.Add(inventPosition);
			}

			if(unprocessedCodes.Any())
			{
				// оставшиеся коды удаляем из задачи
				// потому что их не удалось назначить ни на один товар
				foreach(var unproccesCode in unprocessedCodes)
				{
					documentEdoTask.Items.Remove(unproccesCode);
				}
			}

			documentEdoTask.UpdInventPositions.Clear();
			foreach(var updInventPosition in updInventPositions)
			{
				documentEdoTask.UpdInventPositions.Add(updInventPosition);
			}
		}

		private async Task<TrueMarkWaterIdentificationCode> LoadCodeFromPool(GtinEntity gtin, CancellationToken cancellationToken)
		{
			int codeId = 0;
			var problemGtins = new List<EdoProblemGtinItem>();
			EdoCodePoolMissingCodeException exception = null;

			try
			{
				codeId = await _trueMarkCodesPool.TakeCode(gtin.GtinNumber, cancellationToken);
			}
			catch(EdoCodePoolMissingCodeException ex)
			{
				exception = ex;
				if(!problemGtins.Any(x => x.Gtin == gtin))
				{
					problemGtins.Add(new EdoProblemGtinItem
					{
						Gtin = gtin
					});
				}
			}

			if(codeId == 0)
			{
				throw new EdoProblemException(exception, problemGtins);
			}

			return await _uow.Session.GetAsync<TrueMarkWaterIdentificationCode>(codeId, cancellationToken);
		}
		
		private async Task<TrueMarkWaterIdentificationCode> LoadCodeFromPool(
			IEnumerable<GtinEntity> gtins,
			CancellationToken cancellationToken)
		{
			int codeId = 0;
			var problemGtins = new List<EdoProblemGtinItem>();
			EdoCodePoolMissingCodeException exception = null;
			
			// Последний основной, первый резервный
			foreach (var gtin in gtins.Reverse())
			{
				try
				{
					codeId = await _trueMarkCodesPool.TakeCode(gtin.GtinNumber, cancellationToken);
					
					break;
				}
				catch (EdoCodePoolMissingCodeException ex)
				{
					exception = ex;

					if (!problemGtins.Any(x => x.Gtin == gtin))
					{
						problemGtins.Add(new EdoProblemGtinItem
						{
							Gtin = gtin
						});
					}
				}
			}

			if (codeId == 0)
			{
				throw new EdoProblemException(exception, problemGtins);
			}

			return await _uow.Session.GetAsync<TrueMarkWaterIdentificationCode>(codeId, cancellationToken);
		}

		private async Task<IDictionary<TrueMarkWaterGroupCode, IEnumerable<EdoTaskItem>>> TakeGroupCodesWithTaskItems(
			List<EdoTaskItem> unprocessedTaskItems, 
			CancellationToken cancellationToken
			)
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

			var parentCodes = new List<TrueMarkWaterGroupCode>();
			foreach(var parentCodesId in parentCodesIds)
			{
				var parentCode = await _trueMarkCodeRepository.GetGroupCode(parentCodesId.Value, cancellationToken);

				if(parentCode == null)
				{
					continue;
				}

				parentCodes.Add(parentCode);
			}

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
