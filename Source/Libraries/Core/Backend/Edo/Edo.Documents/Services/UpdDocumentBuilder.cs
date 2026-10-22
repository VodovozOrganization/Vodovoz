using Edo.Common;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Controllers;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Edo.Upd;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Edo.Documents.Services
{
	public partial class UpdDocumentBuilder : IUpdDocumentBuilder
	{
		private readonly IUnitOfWork _uow;
		private readonly ITrueMarkCodesPool _trueMarkCodesPool;
		private readonly ITrueMarkCodeRepository _trueMarkCodeRepository;
		private readonly ITrueMarkCodesPoolCodeProvider _trueMarkCodesPoolCodeProvider;
		private readonly IUpdTransportCodeService _updTransportCodeService;
		private readonly ICounterpartyEdoAccountEntityController _counterpartyEdoAccountEntityController;
		private readonly IPaymentRepository _paymentRepository;
		private readonly ILogger<UpdDocumentBuilder> _logger;

		public UpdDocumentBuilder(
			IUnitOfWork uow,
			ITrueMarkCodesPool trueMarkCodesPool,
			ITrueMarkCodeRepository trueMarkCodeRepository,
			ITrueMarkCodesPoolCodeProvider trueMarkCodesPoolCodeProvider,
			IUpdTransportCodeService updTransportCodeService,
			ICounterpartyEdoAccountEntityController counterpartyEdoAccountEntityController,
			IPaymentRepository paymentRepository,
			ILogger<UpdDocumentBuilder> logger
		)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_trueMarkCodesPool = trueMarkCodesPool ?? throw new ArgumentNullException(nameof(trueMarkCodesPool));
			_trueMarkCodeRepository = trueMarkCodeRepository ?? throw new ArgumentNullException(nameof(trueMarkCodeRepository));
			_trueMarkCodesPoolCodeProvider = trueMarkCodesPoolCodeProvider ?? throw new ArgumentNullException(nameof(trueMarkCodesPoolCodeProvider));
			_updTransportCodeService = updTransportCodeService ?? throw new ArgumentNullException(nameof(updTransportCodeService));
			_counterpartyEdoAccountEntityController = counterpartyEdoAccountEntityController ?? throw new ArgumentNullException(nameof(counterpartyEdoAccountEntityController));
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task BuildForOwnNeedUpdDocumentAsync(
			DocumentEdoTask documentEdoTask,
			CancellationToken cancellationToken
		)
		{
			if(documentEdoTask is null)
			{
				throw new ArgumentNullException(nameof(documentEdoTask));
			}

			if(documentEdoTask.Upd != null)
			{
				return;
			}

			var order = documentEdoTask.FormalEdoRequest.Order;

			if(order.Client.PersonType == PersonType.natural)
			{
				throw new InvalidOperationException("Нельзя сделать УПД для физического лица");
			}

			var unprocessedCodes = documentEdoTask.Items.ToList();
			var groupCodesWithTaskItems = await TakeGroupCodesWithTaskItems(unprocessedCodes, cancellationToken);

			// ---------------------------------------------------------------------
			// алгоритм назначения кодов на товары в заказе
			// результатом должен быть документ УПД
			// в котором созданы инвентарные позиции в кол-ве равном строкам товаров в заказе
			// к каждой инвентарной позиции привязаны коды в кол-ве равном кол-ву товаров в заказе
			var context = new UpdDocumentCreationContext
			{
				DocumentEdoTask = documentEdoTask,
				Order = order,
				UnprocessedCodes = unprocessedCodes,
				GroupCodesWithTaskItems = groupCodesWithTaskItems,
				OrderItemsByPriceDesc = order.OrderItems.OrderByDescending(x => x.Price).ToArray()
			};

			// 1. Создаем инвентарные позиции и собираем информацию о необходимых кодах
			var updInventPositions = await BuildInventPositionsAsync(context, cancellationToken);

			// 2. Пакетно получаем коды из пула
			var loadedCodesByGtin = await LoadCodesFromPoolAsync(context, cancellationToken);

			// 3. Создаем TaskItem'ы и привязываем полученные коды
			await AssignLoadedCodesAsync(context, loadedCodesByGtin, cancellationToken);

			// 4. Очищаем неиспользованные коды
			CleanupUnusedCodes(context);

			// 5. Создаем документ
			var updDocument = CreateUpdDocument(context, updInventPositions);

			documentEdoTask.Upd = updDocument;

			_logger.LogInformation(
				"Документ УПД для задачи {TaskId} успешно создан. Инвентарных позиций: {PositionCount}, кодов: {CodeCount}",
				documentEdoTask.Id,
				updInventPositions.Count,
				updInventPositions.Sum(p => p.Codes.Count)
			);
		}

		public async Task RebuildForOwnNeedUpdDocumentAsync(
			DocumentEdoTask documentEdoTask,
			CancellationToken cancellationToken
		)
		{
			if(documentEdoTask is null)
			{
				throw new ArgumentNullException(nameof(documentEdoTask));
			}

			await RemoveUpdDocumentAsync(documentEdoTask, cancellationToken);
			await BuildForOwnNeedUpdDocumentAsync(documentEdoTask, cancellationToken);
		}

		private async Task RemoveUpdDocumentAsync(DocumentEdoTask documentEdoTask, CancellationToken cancellationToken)
		{
			var oldUpdDocument = documentEdoTask.Upd;

			if(oldUpdDocument is null)
			{
				return;
			}

			documentEdoTask.Upd = null;

			// Каскад many-to-one не удаляет замененный документ,
			// поэтому удаляем явно вместе с инвентарными позициями и их кодами
			if(oldUpdDocument.Id != 0)
			{
				await _uow.DeleteAsync(oldUpdDocument, cancellationToken);
			}
		}

		public EdoUpdDocument GetUpdDocumentWithoutCodes(DocumentEdoTask documentEdoTask)
		{
			if(documentEdoTask is null)
			{
				throw new ArgumentNullException(nameof(documentEdoTask));
			}

			var order = documentEdoTask.FormalEdoRequest.Order;

			if(order.Client.PersonType == PersonType.natural)
			{
				throw new InvalidOperationException("Нельзя сделать УПД для физического лица");
			}

			var context = new UpdDocumentCreationContext
			{
				DocumentEdoTask = documentEdoTask,
				Order = order,
				OrderItemsByPriceDesc = order.OrderItems.OrderByDescending(x => x.Price).ToArray()
			};

			var updInventPositions = BuildInventPositionsWithoutCodes(context);

			var updDocument = CreateUpdDocument(context, updInventPositions);
			return updDocument;
		}

		private async Task<List<UpdInventPosition>> BuildInventPositionsAsync(
			UpdDocumentCreationContext context,
			CancellationToken cancellationToken
		)
		{
			var updInventPositions = new List<UpdInventPosition>();

			foreach(var orderItem in context.OrderItemsByPriceDesc)
			{
				var codeItemsToAssign = new List<UpdProductCode>();
				var inventPosition = CreateInventPosition(context.Order, orderItem);

				// Процесс создания инвентарной позиции УПД
				// и поиск и назначение соответствующих кодов

				// Обработка нулевых позиций
				if(await HandleZeroPriceItemAsync(context, orderItem, cancellationToken))
				{
					continue;
				}

				if(orderItem.Nomenclature.IsAccountableInTrueMark)
				{
					await ProcessAccountableOrderItemAsync(
						context,
						orderItem,
						codeItemsToAssign,
						cancellationToken
					);
				}

				inventPosition.Codes = codeItemsToAssign;

				updInventPositions.Add(inventPosition);
			}

			return updInventPositions;
		}

		private List<UpdInventPosition> BuildInventPositionsWithoutCodes(UpdDocumentCreationContext context)
		{
			var updInventPositions = new List<UpdInventPosition>();

			foreach(var orderItem in context.OrderItemsByPriceDesc)
			{
				var inventPosition = CreateInventPosition(context.Order, orderItem);
				updInventPositions.Add(inventPosition);
			}

			return updInventPositions;
		}

		public UpdInventPosition CreateInventPosition(OrderEntity order, OrderItemEntity orderItem)
		{
			var isService = orderItem.Nomenclature.Category == NomenclatureCategory.master
					|| orderItem.Nomenclature.Category == NomenclatureCategory.service;

			var inventPosition = new UpdInventPosition
			{
				AssignedOrderItem = orderItem,
				NomenclatureId = orderItem.Nomenclature.Id,
				Name = orderItem.Nomenclature.OfficialName,
				Count = orderItem.CurrentCount,
				Price = orderItem.Price,
				DiscountMoney = orderItem.DiscountMoney,
				IncludeVat = orderItem.IncludeNDS ?? 0,
				ValueAddedTax = orderItem.ValueAddedTax,
				OKEI = orderItem.Nomenclature.Unit.OKEI,
				UnitName = orderItem.Nomenclature.Unit.Name,
				IsService = isService
			};

			var clientSpecialNomenclature = order.Client.SpecialNomenclatures
				.FirstOrDefault(x => x.Nomenclature.Id == orderItem.Nomenclature.Id);

			if(order.Client.UseSpecialDocFields && clientSpecialNomenclature != null)
			{
				inventPosition.SpecialCustomerNomenclatureId = clientSpecialNomenclature.SpecialId;
			}

			return inventPosition;
		}

		private async Task<bool> HandleZeroPriceItemAsync(
			UpdDocumentCreationContext context,
			OrderItemEntity orderItem,
			CancellationToken cancellationToken
		)
		{
			if(orderItem.ActualSum <= 0 && context.DocumentEdoTask.DocumentType is EdoDocumentType.UPD)
			{
				if(orderItem.Nomenclature.IsAccountableInTrueMark && context.UnprocessedCodes.Any())
				{
					var i = 0;
					while(i < context.UnprocessedCodes.Count)
					{
						if(context.UnprocessedCodes[i].ProductCode.SourceCode != null
							&& context.UnprocessedCodes[i].ProductCode.ResultCode is null
							&& orderItem.Nomenclature.Gtins.Any(x => x.GtinNumber == context.UnprocessedCodes[i].ProductCode.SourceCode?.Gtin))
						{
							await _trueMarkCodesPool.PutCodeAsync(context.UnprocessedCodes[i].ProductCode.SourceCode.Id, cancellationToken);
							context.DocumentEdoTask.Items.Remove(context.UnprocessedCodes[i]);
							context.UnprocessedCodes.RemoveAt(i);
						}
						else
						{
							i++;
						}
					}
				}
				return true;
			}
			return false;
		}

		private async Task ProcessAccountableOrderItemAsync(
			UpdDocumentCreationContext context,
			OrderItemEntity orderItem,
			List<UpdProductCode> codeItemsToAssign,
			CancellationToken cancellationToken)
		{
			int assignedQuantity = 0;

			while(assignedQuantity < orderItem.CurrentCount)
			{
				// Необходимо найти коды для номенклатуры на все кол-во
				// и привязать их к инвентарной позиции УПД

				var availableQuantity = (int)(orderItem.CurrentCount - assignedQuantity);

				// сначала ищем и назначем из групповых кодов
				var assignedGroupCount = await TryAssignGroupCodeAsync(
					context,
					orderItem,
					availableQuantity,
					codeItemsToAssign,
					cancellationToken
				);

				if(assignedGroupCount > 0)
				{
					assignedQuantity += assignedGroupCount;
					continue;
				}

				// затем, если ничего не смогли взять из групповых, то ищем и назначаем из индивидуальных
				// в которых есть заполенный SourceCode, т.е. исключаем неотсканированные позиции задачи
				if(TryAssignExistingIndividualCode(context, orderItem, codeItemsToAssign))
				{
					var individualCode = codeItemsToAssign.Last().IndividualCode;
					if(individualCode?.ParentWaterGroupCodeId != null)
					{
						await _updTransportCodeService.DisaggregateCodeAsync(individualCode, cancellationToken);
					}
					assignedQuantity ++;
					continue;
				}

				// затем, если ничего не смогли взять из индивидуальных, то берем неотсканированную позицию
				// заполняем ее из пула и используем в назначении
				if(TryAssignUnscannedCode(context, orderItem, codeItemsToAssign))
				{
					assignedQuantity ++;
					continue;
				}

				// если не отсканированных нет, но назначить код все еще есть необходимость
				// то создаем новый taskItem и назначаем код из пула в него и в инвентарную позицию УПД
				var remainingCount = (int)(orderItem.CurrentCount - assignedQuantity);
				CreateNewCodesRequest(
					context,
					orderItem,
					remainingCount,
					codeItemsToAssign
				);

				assignedQuantity = (int)orderItem.CurrentCount;
			}
		}

		private async Task<int> TryAssignGroupCodeAsync(
			UpdDocumentCreationContext context,
			OrderItemEntity orderItem,
			int availableQuantity,
			List<UpdProductCode> codeItemsToAssign,
			CancellationToken cancellationToken
		)
		{
			var availableGroupGtins = orderItem.Nomenclature.GroupGtins
				.Where(x => x.CodesCount <= availableQuantity)
				.OrderByDescending(x => x.CodesCount);

			TrueMarkWaterGroupCode groupCode = null;

			var isGroupCodeAssigned = false;

			foreach(var availableGtin in availableGroupGtins)
			{
				groupCode = context.GroupCodesWithTaskItems.Keys
					.FirstOrDefault(x => x.GTIN == availableGtin.GtinNumber);
				if(groupCode is null)
				{
					continue;
				}

				var isGroupCodeUsedInEdoDocument = _trueMarkCodeRepository.IsGroupCodeUsedInEdoDocument(groupCode.Id);
				if(isGroupCodeUsedInEdoDocument)
				{
					// Уже используется в каком-то ЭДО документе, пропускаем
					context.GroupCodesWithTaskItems.Remove(groupCode);
					continue;
				}

				// установка в ResultCode всем ProductCode в группе
				var groupTaskItems = context.GroupCodesWithTaskItems[groupCode];

				if(availableQuantity < groupTaskItems.Count())
				{
					context.GroupCodesWithTaskItems.Remove(groupCode);
					await _updTransportCodeService.DisaggregateCodeAsync(groupCode, cancellationToken);
					continue;
				}

				foreach(var groupTaskItem in groupTaskItems)
				{
					groupTaskItem.ProductCode.ResultCode = groupTaskItem.ProductCode.SourceCode;
					groupTaskItem.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Accepted;
				}

				context.GroupCodesWithTaskItems.Remove(groupCode);

				isGroupCodeAssigned = true;

				break;
			}

			if(isGroupCodeAssigned)
			{
				var codesInGroup = groupCode
					.GetAllCodes()
					.Count(x => x.IsTrueMarkWaterIdentificationCode);

				var codeItem = new UpdProductCode{ 
					GroupCode = groupCode, 
					Quantity = codesInGroup 
				};
				codeItemsToAssign.Add(codeItem);

				return codesInGroup;
			}

			return 0;
		}

		private bool TryAssignExistingIndividualCode(
			UpdDocumentCreationContext context,
			OrderItemEntity orderItem,
			List<UpdProductCode> codeItemsToAssign
		)
		{
			var validUnprocessedCodes = context.UnprocessedCodes
				.Where(x => x.ProductCode.SourceCode != null)
				.ToList();

			TrueMarkWaterIdentificationCode individualCode = null;
			var availableGtins = orderItem.Nomenclature.Gtins;

			foreach(var availableGtin in availableGtins)
			{
				var availableCode = validUnprocessedCodes
					.FirstOrDefault(x => x.ProductCode.SourceCode?.Gtin == availableGtin.GtinNumber);

				if(availableCode is null)
				{
					continue;
				}

				// ResultCode будет заполнен, если проиходит повторное создание документов
				// индивидуальные коды при этом будут обновлены после валидации
				if(availableCode.ProductCode.ResultCode is null)
				{
					if(availableCode.ProductCode.Problem is ProductCodeProblem.None
						&& availableCode.ProductCode.SourceCodeStatus != SourceProductCodeStatus.SavedToPool)
					{
						availableCode.ProductCode.ResultCode = availableCode.ProductCode.SourceCode;
						availableCode.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Accepted;
					}
					else
					{
						AddCodeRequirement(context, availableGtin, orderItem, codeItemsToAssign, edoTaskItemForChange: availableCode);
						availableCode.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Changed;

						context.UnprocessedCodes.Remove(availableCode);

						return true;
					}
				}

				individualCode = availableCode.ProductCode.ResultCode;
				validUnprocessedCodes.Remove(availableCode);
				context.UnprocessedCodes.Remove(availableCode);
				break;
			}

			if(individualCode != null)
			{
				var codeItem = new UpdProductCode { 
					IndividualCode = individualCode, 
					Quantity = 1 
				};
				codeItemsToAssign.Add(codeItem);
				return true;
			}

			return false;
		}

		private bool TryAssignUnscannedCode(
			UpdDocumentCreationContext context,
			OrderItemEntity orderItem,
			List<UpdProductCode> codeItemsToAssign
		)
		{
			var unscannedCodes = context.UnprocessedCodes
				.Where(x => x.ProductCode.SourceCode is null)
				.ToList();

			var unscannedCode = unscannedCodes.FirstOrDefault();
			if(unscannedCode is null)
			{
				return false;
			}

			// ResultCode будет заполнен, если проиходит повторное создание документов
			// индивидуальные коды при этом будут обновлены после валидации
			if(unscannedCode.ProductCode.ResultCode is null)
			{
				var forUnscannedAvailableGtins = orderItem.Nomenclature.Gtins;
				var gtinEntity = forUnscannedAvailableGtins.OrderBy(g => g.Priority).FirstOrDefault();

				if(gtinEntity != null)
				{
					AddCodeRequirement(context, gtinEntity, orderItem, codeItemsToAssign, edoTaskItemForChange: unscannedCode);
					unscannedCode.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Changed;
				}
			}

			context.UnprocessedCodes.Remove(unscannedCode);

			var codeItem = new UpdProductCode { 
				IndividualCode = unscannedCode.ProductCode.ResultCode, 
				Quantity = 1 
			};
			codeItemsToAssign.Add(codeItem);
			return true;
		}

		private void CreateNewCodesRequest(
			UpdDocumentCreationContext context,
			OrderItemEntity orderItem,
			int count,
			List<UpdProductCode> codeItemsToAssign)
		{
			var forNewAvailableGtins = orderItem.Nomenclature.Gtins;
			var forNewGtinEntity = forNewAvailableGtins.OrderBy(g => g.Priority).FirstOrDefault();

			if(forNewGtinEntity is null)
			{
				_logger.LogWarning(
					"Для номенклатуры {NomenclatureId} не найдено подходящих GTIN для заказа {OrderId}",
					orderItem.Nomenclature.Id,
					context.DocumentEdoTask.FormalEdoRequest.Order.Id);
				return;
			}

			AddCodeRequirement(context, forNewGtinEntity, orderItem, codeItemsToAssign, count);
		}

		private void AddCodeRequirement(
			UpdDocumentCreationContext context,
			GtinEntity gtinEntity,
			OrderItemEntity orderItem,
			List<UpdProductCode> codeItemsToAssign,
			int count = 1,
			EdoTaskItem edoTaskItemForChange = null
		)
		{
			if(context.CodesNeeded.ContainsKey(gtinEntity))
			{
				context.CodesNeeded[gtinEntity] += count;
			}
			else
			{
				context.CodesNeeded[gtinEntity] = count;
			}

			for(int i = 0; i < count; i++)
			{
				var codeItem = new UpdProductCode
				{
					Quantity = 1
				};
				codeItemsToAssign.Add(codeItem);

				context.PendingCodeItems.Add(new UpdPendingCodeItem
				{
					CodeItem = codeItem,
					Gtin = gtinEntity,
					OrderItem = orderItem,
					EdoTaskItemForCodeChange = edoTaskItemForChange
				});
			}
		}

		private async Task<Dictionary<string, List<TrueMarkWaterIdentificationCode>>> LoadCodesFromPoolAsync(
			UpdDocumentCreationContext context,
			CancellationToken cancellationToken
		) 
		{
			if(!context.CodesNeeded.Any())
			{
				_logger.LogDebug("Нет необходимых кодов для загрузки из пула");
				return new Dictionary<string, List<TrueMarkWaterIdentificationCode>>();
			}

			_logger.LogInformation(
				"Начинаем пакетную загрузку {TotalCodes} кодов для {GtinCount} GTIN",
				context.CodesNeeded.Values.Sum(),
				context.CodesNeeded.Count);

			var gtinCounts = context.CodesNeeded.ToDictionary(kv => kv.Key.GtinNumber, kv => kv.Value);

			var loadedCodes = await _trueMarkCodesPoolCodeProvider.TakeValidCodesBatchAsync(
				_trueMarkCodesPool,
				gtinCounts,
				context.Order.Contract.Organization.INN,
				cancellationToken
			);

			if(loadedCodes.IsFailure)
			{
				var errorMessage = loadedCodes.Errors.FirstOrDefault()?.Message ?? "Неизвестная ошибка";
				_logger.LogError(
					"Ошибка при загрузке кодов из пула: {ErrorMessage}",
					errorMessage);

				return new Dictionary<string, List<TrueMarkWaterIdentificationCode>>();
			}

			context.CodesNeeded.Clear();

			return loadedCodes.Value.ToDictionary(
				kv => kv.Key,
				kv => kv.Value.ToList());
		}

		private async Task AssignLoadedCodesAsync(
			UpdDocumentCreationContext context,
			Dictionary<string, List<TrueMarkWaterIdentificationCode>> loadedCodesByGtin,
			CancellationToken cancellationToken)
		{
			if(!context.PendingCodeItems.Any())
			{
				return;
			}

			if(!loadedCodesByGtin.Any())
			{
				return;
			}

			var availableCodes = loadedCodesByGtin
				.ToDictionary(kv => kv.Key, kv => kv.Value.ToList());

			var codesToDisaggregate = new List<TrueMarkWaterIdentificationCode>();

			foreach(var pendingItem in context.PendingCodeItems)
			{
				var gtinNumber = pendingItem.Gtin.GtinNumber;

				if(availableCodes.TryGetValue(gtinNumber, out var codes) && codes.Any())
				{
					var code = codes.First();
					codes.RemoveAt(0);
					codesToDisaggregate.Add(code);

					var taskItem = await CreateOrUpdateTaskItemAsync(
						context.DocumentEdoTask,
						code,
						pendingItem.EdoTaskItemForCodeChange,
						cancellationToken);

					pendingItem.CodeItem.IndividualCode = code;

					_logger.LogDebug(
						"Код {CodeId} назначен для OrderItem {OrderItemId}",
						code.Id,
						pendingItem.OrderItem.Id);
				}
				else
				{
					_logger.LogWarning(
						"Не удалось получить код для GTIN {Gtin} для OrderItem {OrderItemId}",
						gtinNumber,
						pendingItem.OrderItem.Id);

					if(pendingItem.EdoTaskItemForCodeChange is null)
					{
						await CreateTaskItemWithProblemAsync(
							context.DocumentEdoTask,
							cancellationToken);
					}
				}
			}

			if(codesToDisaggregate.Any())
			{
				_logger.LogInformation(
					"Выполняем дизагрегацию для {Count} кодов",
					codesToDisaggregate.Count);

				foreach(var code in codesToDisaggregate)
				{
					await _updTransportCodeService.DisaggregateCodeAsync(code, cancellationToken);
				}
			}

			await ReturnUnusedCodesAsync(availableCodes, cancellationToken);
		}

		private async Task<EdoTaskItem> CreateOrUpdateTaskItemAsync(
			DocumentEdoTask documentEdoTask,
			TrueMarkWaterIdentificationCode code,
			EdoTaskItem edoTaskItemToCodeChange,
			CancellationToken cancellationToken)
		{
			var productCode = edoTaskItemToCodeChange?.ProductCode
				?? new AutoTrueMarkProductCode
			{
				SourceCode = code,
				SourceCodeStatus = SourceProductCodeStatus.Accepted,
				Problem = ProductCodeProblem.None,
				CustomerEdoRequest = documentEdoTask.FormalEdoRequest
			};

			productCode.ResultCode = code;

			await _uow.SaveAsync(productCode, cancellationToken: cancellationToken);

			var taskItem = edoTaskItemToCodeChange
				?? new EdoTaskItem
			{
				ProductCode = productCode,
				CustomerEdoTask = documentEdoTask
			};

			if(!documentEdoTask.Items.Contains(taskItem))
			{
				documentEdoTask.Items.Add(taskItem);
			}

			return taskItem;
		}

		private async Task CreateTaskItemWithProblemAsync(
			DocumentEdoTask documentEdoTask,
			CancellationToken cancellationToken)
		{
			var productCode = new AutoTrueMarkProductCode
			{
				ResultCode = null,
				SourceCode = null,
				SourceCodeStatus = SourceProductCodeStatus.New,
				Problem = ProductCodeProblem.Unscanned,
				CustomerEdoRequest = documentEdoTask.FormalEdoRequest
			};

			await _uow.SaveAsync(productCode, cancellationToken: cancellationToken);

			var taskItem = new EdoTaskItem
			{
				ProductCode = productCode,
				CustomerEdoTask = documentEdoTask
			};
			documentEdoTask.Items.Add(taskItem);
		}

		private async Task ReturnUnusedCodesAsync(
			Dictionary<string, List<TrueMarkWaterIdentificationCode>> loadedCodesByGtin,
			CancellationToken cancellationToken)
		{
			foreach(var gtinCodes in loadedCodesByGtin)
			{
				if(!gtinCodes.Value.Any())
				{
					continue;
				}

				_logger.LogWarning(
					"Остались неиспользованные коды для GTIN {Gtin}: {Count} шт. Возвращаем в пул.",
					gtinCodes.Key,
					gtinCodes.Value.Count);

				foreach(var code in gtinCodes.Value)
				{
					await _trueMarkCodesPool.PutCodeAsync(code.Id, cancellationToken);
				}
			}
		}

		private void CleanupUnusedCodes(UpdDocumentCreationContext context)
		{
			if(!context.UnprocessedCodes.Any())
			{
				return;
			}

			_logger.LogInformation(
				"Удаляем {Count} неиспользованных кодов из задачи",
				context.UnprocessedCodes.Count);

			// оставшиеся коды удаляем из задачи
			// потому что их не удалось назначить ни на один товар
			foreach(var unprocessedCode in context.UnprocessedCodes)
			{
				context.DocumentEdoTask.Items.Remove(unprocessedCode);
			}
		}

		private EdoUpdDocument CreateUpdDocument(
			UpdDocumentCreationContext context,
			List<UpdInventPosition> updInventPositions
		)
		{
			var order = context.Order;

			var seller = order.Contract.Organization;
			var sellerOrganization = new UpdOrganization {
				Name = seller.Name,
				CountryCode = "643",
				CountryName = "Россия",
				Address = seller.ActiveOrganizationVersion.JurAddress,
				Inn = seller.INN,
				Kpp = seller.KPP,
				Ogrn = seller.OGRN,
				OgrnDate = seller.OGRNDate,
				EdoAccountId = seller.TaxcomEdoSettings.EdoAccount,
				IsWithoutVat = order.IsCashlessPaymentTypeAndOrganizationWithoutVAT
			};

			var counterpartyEdoAccount = _counterpartyEdoAccountEntityController.GetDefaultCounterpartyEdoAccountByOrganizationId(
				order.Client,
				order.Contract.Organization.Id
			);

			var customer = order.Client;
			var customerOrganization = BuildCustomerOrganization(customer, counterpartyEdoAccount);
			var consigneeOrganization = BuildConsigneeOrganization(order, counterpartyEdoAccount);

			var orderPayments = _paymentRepository.GetOrderPayments(_uow, order.Id)
				.Select(payment => payment.GetFirstParentRefundedPaymentOrCurrent())
				.Where(x => order.DeliveryDate.HasValue && x.Date < order.DeliveryDate.Value.AddDays(1))
				.Distinct()
				.ToList();

			

			var updDocument = new EdoUpdDocument
			{
				Guid = Guid.NewGuid(),
				Number = DocumentNumberBuilder.Build(order, DocumentContainerType.Upd),
				Date = order.DeliveryDate.Value,
				Sum = updInventPositions.Sum(x => x.Sum),
				Seller = sellerOrganization,
				Customer = customerOrganization,
				Consignee = consigneeOrganization,
				GovContract = string.IsNullOrWhiteSpace(order.Client.GovContract) ? null : order.Client.GovContract,
				ReasonForLeaving = customer.ReasonForLeaving,
				OrderId = order.Id
			};

			updDocument.InventPositions = updInventPositions;

			updDocument.Payments = orderPayments.Select(payment => new UpdPayment
			{
				PaymentNum = payment.PaymentNum.ToString(),
				PaymentDate = payment.Date.ToString("dd.MM.yyyy"),
			}).ToList();

			AddShipingDocument(updDocument, order);
			AddShipingReasonDocument(updDocument, order);

			if(!string.IsNullOrWhiteSpace(customer.CargoReceiver) && customer.UseSpecialDocFields)
			{
				updDocument.SpecialConsignee = customer.CargoReceiver;
			}

			if(!string.IsNullOrWhiteSpace(order.CounterpartyExternalOrderId) && customer.UseSpecialDocFields)
			{
				updDocument.CustomerExternalOrderId = customer.SpecialNomenclatures.Any()
					? $"{order.CounterpartyExternalOrderId}"
					: $"N{order.CounterpartyExternalOrderId}";
			}

			return updDocument;
		}

		private UpdOrganization BuildCustomerOrganization(
			CounterpartyEntity customer,
			CounterpartyEdoAccountEntity counterpartyEdoAccount
		)
		{
			var customerName = customer.FullName;
			var customerKpp = customer.KPP;

			if(customer.UseSpecialDocFields)
			{
				if(!string.IsNullOrWhiteSpace(customer.SpecialCustomer))
				{
					customerName = customer.SpecialCustomer;
				}
				if(!string.IsNullOrWhiteSpace(customer.PayerSpecialKPP))
				{
					customerKpp = customer.PayerSpecialKPP;
				}
			}
			var customerOrganization = new UpdOrganization
			{
				Name = customerName,
				CountryCode = "643",
				CountryName = "Россия",
				Address = customer.JurAddress,
				Inn = customer.INN,
				Kpp = customerKpp,
				Ogrn = customer.OGRN,
				OgrnDate = customer.OGRNDate,
				EdoAccountId = counterpartyEdoAccount.PersonalAccountIdInEdo
			};

			return customerOrganization;
		}

		private UpdOrganization BuildConsigneeOrganization(
			OrderEntity orderEntity,
			CounterpartyEdoAccountEntity counterpartyEdoAccount
		)
		{
			var customer = orderEntity.Client;
			var deliveryPoint = orderEntity.DeliveryPoint;

			var consignee = BuildCustomerOrganization(customer, counterpartyEdoAccount);

			if(customer.CargoReceiverSource == CargoReceiverSource.FromDeliveryPoint)
			{
				consignee.Address = deliveryPoint != null ? deliveryPoint.ShortAddress : customer.JurAddress;
				consignee.Kpp = deliveryPoint?.KPP ?? customer.KPP;
				return consignee;
			}

			var useSpecialDoc = !string.IsNullOrWhiteSpace(customer.CargoReceiver) && customer.UseSpecialDocFields;
			if(customer.CargoReceiverSource == CargoReceiverSource.Special && useSpecialDoc)
			{
				consignee.Address = customer.CargoReceiver;
				consignee.Kpp = customer.PayerSpecialKPP ?? customer.KPP;
				return consignee;
			}

			return consignee;
		}

		private void AddShipingDocument(EdoUpdDocument updDocument, OrderEntity order)
		{
			if(order.Client.UseSpecialDocFields
				&& !string.IsNullOrWhiteSpace(order.Client.SpecialContractName)
				&& !string.IsNullOrWhiteSpace(order.Client.SpecialContractNumber)
				&& order.Client.SpecialContractDate.HasValue)
			{
				updDocument.ShippingDocument = order.Client.SpecialContractName;
				updDocument.ShippingDocumentNumber = order.Client.SpecialContractNumber;
				updDocument.ShippingDocumentDate = $"{order.Client.SpecialContractDate.Value:dd.MM.yyyy}";
				return;
			}

			if(order.Client.UseSpecialDocFields && !string.IsNullOrWhiteSpace(order.Client.SpecialContractName))
			{
				updDocument.ShippingDocument = "Без документа-основания";
				return;
			}

			if(order.Contract != null)
			{
				updDocument.ShippingDocument = "Договор";
				updDocument.ShippingDocumentNumber = order.Contract.Number;
				updDocument.ShippingDocumentDate = $"{order.Contract.IssueDate:dd.MM.yyyy}";
			}
			else
			{
				updDocument.ShippingDocument = "Без документа-основания";
			}
		}

		private void AddShipingReasonDocument(EdoUpdDocument updDocument, OrderEntity order)
		{
			var customer = order.Client;

			if(customer.UseSpecialDocFields
			   && !string.IsNullOrWhiteSpace(customer.SpecialContractName)
			   && !string.IsNullOrWhiteSpace(customer.SpecialContractNumber)
			   && customer.SpecialContractDate.HasValue)
			{
				updDocument.ShippingReasonDocument = customer.SpecialContractName;
				updDocument.ShippingReasonDocumentNumber = customer.SpecialContractNumber;
				updDocument.ShippingReasonDocumentDate = customer.SpecialContractDate.Value.ToString("dd.MM.yyyy");
				return;
			}

			if(customer.UseSpecialDocFields && !string.IsNullOrWhiteSpace(customer.SpecialContractName))
			{
				updDocument.ShippingReasonDocument = "Без документа-основания";
				return;
			}

			if(order.Contract != null)
			{
				updDocument.ShippingReasonDocument = "Договор";
				updDocument.ShippingReasonDocumentNumber = order.Contract.Number;
				updDocument.ShippingReasonDocumentDate = order.Contract.IssueDate.ToString("dd.MM.yyyy");
			}
		}


		private async Task<Dictionary<TrueMarkWaterGroupCode, IEnumerable<EdoTaskItem>>> TakeGroupCodesWithTaskItems(
			List<EdoTaskItem> unprocessedTaskItems,
			CancellationToken cancellationToken)
		{
			// нашли все индивидуальные коды, которые содержатся в группах
			var codesThatContainedInGroup = unprocessedTaskItems
				.Where(x => x.ProductCode.SourceCode != null)
				.Where(x => x.ProductCode.SourceCode.IsInvalid == false)
				.Where(x => x.ProductCode.SourceCode.ParentWaterGroupCodeId != null)
				.ToList();

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

				if(parentCode is null)
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
						.Any(x => x.Id == ctcig.ProductCode.SourceCode?.Id)));
			}

			// нашли все групповые коды

			return result;
		}
	}
}
