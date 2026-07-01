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
using Vodovoz.Core.Domain.Edo;
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
		private readonly ILogger<UpdDocumentBuilder> _logger;

		public UpdDocumentBuilder(
			IUnitOfWork uow,
			ITrueMarkCodesPool trueMarkCodesPool,
			ITrueMarkCodeRepository trueMarkCodeRepository,
			ITrueMarkCodesPoolCodeProvider trueMarkCodesPoolCodeProvider,
			ILogger<UpdDocumentBuilder> logger)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_trueMarkCodesPool = trueMarkCodesPool ?? throw new ArgumentNullException(nameof(trueMarkCodesPool));
			_trueMarkCodeRepository = trueMarkCodeRepository ?? throw new ArgumentNullException(nameof(trueMarkCodeRepository));
			_trueMarkCodesPoolCodeProvider = trueMarkCodesPoolCodeProvider ?? throw new ArgumentNullException(nameof(trueMarkCodesPoolCodeProvider));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task BuildUpdDocumentAsync(
			DocumentEdoTask documentEdoTask,
			CancellationToken cancellationToken)
		{
			if(documentEdoTask is null)
			{
				throw new ArgumentNullException(nameof(documentEdoTask));
			}

			var order = documentEdoTask.FormalEdoRequest.Order;
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
				UnprocessedCodes = unprocessedCodes,
				GroupCodesWithTaskItems = groupCodesWithTaskItems,
				OrderItemsByPriceDesc = order.OrderItems.OrderByDescending(x => x.Price).ToArray(),
				CodesNeeded = new Dictionary<GtinEntity, int>(),
				PendingCodeItems = new List<UpdPendingCodeItem>(),
				OrganizationInn = GetOrderOrganizationInn(documentEdoTask)
			};

			// 1. Создаем инвентарные позиции и собираем информацию о необходимых кодах
			var updInventPositions = await BuildInventPositionsAsync(context, cancellationToken);

			// 2. Пакетно получаем коды из пула
			var loadedCodesByGtin = await LoadCodesFromPoolAsync(context, cancellationToken);

			// 3. Создаем TaskItem'ы и привязываем полученные коды
			await AssignLoadedCodesAsync(context, loadedCodesByGtin, cancellationToken);

			// 4. Очищаем неиспользованные коды
			CleanupUnusedCodes(context);

			// 5. Обновляем документ
			UpdateDocument(documentEdoTask, updInventPositions);

			_logger.LogInformation(
				"Документ УПД для задачи {TaskId} успешно создан. Инвентарных позиций: {PositionCount}, кодов: {CodeCount}",
				documentEdoTask.Id,
				updInventPositions.Count,
				updInventPositions.Sum(p => p.Codes.Count));
		}

		private async Task<List<EdoUpdInventPosition>> BuildInventPositionsAsync(
			UpdDocumentCreationContext context,
			CancellationToken cancellationToken)
		{
			var updInventPositions = new List<EdoUpdInventPosition>();

			foreach(var orderItem in context.OrderItemsByPriceDesc)
			{
				var codeItemsToAssign = new List<EdoUpdInventPositionCode>();

				// Процесс создания инвентарной позиции УПД
				// и поиск и назначение соответствующих кодов

				// Обработка нулевых позиций
				if(await HandleZeroPriceItemAsync(context, orderItem, cancellationToken))
				{
					continue;
				}

				if(orderItem.Nomenclature.IsAccountableInTrueMark)
				{
					ProcessAccountableOrderItemAsync(
						context,
						orderItem,
						codeItemsToAssign);
				}

				var inventPosition = new EdoUpdInventPosition
				{
					AssignedOrderItem = orderItem,
					Codes = codeItemsToAssign
				};

				updInventPositions.Add(inventPosition);
			}

			return updInventPositions;
		}

		private async Task<bool> HandleZeroPriceItemAsync(
			UpdDocumentCreationContext context,
			OrderItemEntity orderItem,
			CancellationToken cancellationToken)
		{
			if(orderItem.ActualSum <= 0
				&& context.DocumentEdoTask.DocumentType is EdoDocumentType.UPD)
			{
				if(orderItem.Nomenclature.IsAccountableInTrueMark && context.UnprocessedCodes.Any())
				{
					var i = 0;
					while(i < context.UnprocessedCodes.Count)
					{
						if(context.UnprocessedCodes[i].ProductCode.SourceCode is null
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

		private void ProcessAccountableOrderItemAsync(
			UpdDocumentCreationContext context,
			OrderItemEntity orderItem,
			List<EdoUpdInventPositionCode> codeItemsToAssign)
		{
			int assignedQuantity = 0;

			while(assignedQuantity < orderItem.CurrentCount)
			{
				// Необходимо найти коды для номенклатуры на все кол-во
				// и привязать их к инвентарной позиции УПД

				var availableQuantity = (int)(orderItem.CurrentCount - assignedQuantity);

				// сначала ищем и назначем из групповых кодов
				if(TryAssignGroupCode(context, orderItem, availableQuantity, codeItemsToAssign, out var assignedGroupCount))
				{
					assignedQuantity += assignedGroupCount;
					continue;
				}

				// затем, если ничего не смогли взять из групповых, то ищем и назначаем из индивидуальных
				// в которыех есть заполенный SourceCode, т.е. исключаем неотсканированные позиции задачи
				if(TryAssignExistingIndividualCode(context, orderItem, codeItemsToAssign, out var assignedIndividualCount))
				{
					assignedQuantity += assignedIndividualCount;
					continue;
				}

				// затем, если ничего не смогли взять из индивидуальных, то берем неотсканированную позицию
				// заполняем ее из пула и используем в назначении
				if(TryAssignUnscannedCode(context, orderItem, codeItemsToAssign, out var assignedUnscannedCount))
				{
					assignedQuantity += assignedUnscannedCount;
					continue;
				}

				// если не отсканированных нет, но назначить код все еще есть необходимость
				// то создаем новый taskItem и назначаем код из пула в него и в инвентарную позицию УПД
				var remainingCount = (int)(orderItem.CurrentCount - assignedQuantity);
				CreateNewCodesRequest(
					context,
					orderItem,
					remainingCount,
					codeItemsToAssign);

				assignedQuantity = (int)orderItem.CurrentCount;
			}
		}

		private bool TryAssignGroupCode(
			UpdDocumentCreationContext context,
			OrderItemEntity orderItem,
			int availableQuantity,
			List<EdoUpdInventPositionCode> codeItemsToAssign,
			out int assignedCount)
		{
			assignedCount = 0;

			var availableGroupGtins = orderItem.Nomenclature.GroupGtins
				.Where(x => x.CodesCount <= availableQuantity)
				.OrderByDescending(x => x.CodesCount);

			TrueMarkWaterGroupCode groupCode = null;
			foreach(var availableGtin in availableGroupGtins)
			{
				groupCode = context.GroupCodesWithTaskItems.Keys
					.FirstOrDefault(x => x.GTIN == availableGtin.GtinNumber);
				if(groupCode is null)
				{
					continue;
				}

				// установка в ResultCode всем ProductCode в группе
				var groupTaskItems = context.GroupCodesWithTaskItems[groupCode];
				foreach(var groupTaskItem in groupTaskItems)
				{
					groupTaskItem.ProductCode.ResultCode = groupTaskItem.ProductCode.SourceCode;
					groupTaskItem.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Accepted;
				}

				context.GroupCodesWithTaskItems.Remove(groupCode);
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

				assignedCount = codesInGroup;
				return true;
			}

			return false;
		}

		private bool TryAssignExistingIndividualCode(
			UpdDocumentCreationContext context,
			OrderItemEntity orderItem,
			List<EdoUpdInventPositionCode> codeItemsToAssign,
			out int assignedCount)
		{
			assignedCount = 0;

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
						// Если код требует замены - добавляем в словарь необходимых кодов
						AddCodeRequirement(context, availableGtin, orderItem, codeItemsToAssign);
						availableCode.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Changed;

						// Удаляем код из необработанных
						context.UnprocessedCodes.Remove(availableCode);

						assignedCount = 1;
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
				var codeItem = new EdoUpdInventPositionCode
				{
					IndividualCode = individualCode,
					Quantity = 1
				};

				codeItemsToAssign.Add(codeItem);
				assignedCount = 1;
				return true;
			}

			return false;
		}

		private bool TryAssignUnscannedCode(
			UpdDocumentCreationContext context,
			OrderItemEntity orderItem,
			List<EdoUpdInventPositionCode> codeItemsToAssign,
			out int assignedCount)
		{
			assignedCount = 0;

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
					AddCodeRequirement(context, gtinEntity, orderItem, codeItemsToAssign);
					unscannedCode.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Changed;
				}
			}

			context.UnprocessedCodes.Remove(unscannedCode);

			var codeItem = new EdoUpdInventPositionCode
			{
				IndividualCode = unscannedCode.ProductCode.ResultCode,
				Quantity = 1
			};
			codeItemsToAssign.Add(codeItem);

			assignedCount = 1;
			return true;
		}

		private void CreateNewCodesRequest(
			UpdDocumentCreationContext context,
			OrderItemEntity orderItem,
			int count,
			List<EdoUpdInventPositionCode> codeItemsToAssign)
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
			List<EdoUpdInventPositionCode> codeItemsToAssign,
			int count = 1)
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
				var codeItem = new EdoUpdInventPositionCode
				{
					IndividualCode = null,
					Quantity = 1
				};
				codeItemsToAssign.Add(codeItem);

				context.PendingCodeItems.Add(new UpdPendingCodeItem
				{
					CodeItem = codeItem,
					Gtin = gtinEntity,
					OrderItem = orderItem
				});
			}
		}

		private async Task<Dictionary<string, List<TrueMarkWaterIdentificationCode>>> LoadCodesFromPoolAsync(
			UpdDocumentCreationContext context,
			CancellationToken cancellationToken)
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
				context.OrganizationInn,
				cancellationToken);

			context.CodesNeeded.Clear();

			return loadedCodes.ToDictionary(
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

			// Создаем копию словаря для мутации
			var availableCodes = loadedCodesByGtin
				.ToDictionary(kv => kv.Key, kv => kv.Value.ToList());

			foreach(var pendingItem in context.PendingCodeItems)
			{
				var gtinNumber = pendingItem.Gtin.GtinNumber;

				if(availableCodes.TryGetValue(gtinNumber, out var codes) && codes.Any())
				{
					var code = codes.First();
					codes.RemoveAt(0);

					var taskItem = await CreateTaskItemAsync(
						context.DocumentEdoTask,
						code,
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

					await CreateTaskItemWithProblemAsync(
						context.DocumentEdoTask,
						cancellationToken);
				}
			}

			// Возвращаем неиспользованные коды в пул
			await ReturnUnusedCodesAsync(availableCodes, cancellationToken);
		}

		private async Task<EdoTaskItem> CreateTaskItemAsync(
			DocumentEdoTask documentEdoTask,
			TrueMarkWaterIdentificationCode code,
			CancellationToken cancellationToken)
		{
			var productCode = new AutoTrueMarkProductCode
			{
				ResultCode = code,
				SourceCode = code,
				SourceCodeStatus = SourceProductCodeStatus.Accepted,
				Problem = ProductCodeProblem.None,
				CustomerEdoRequest = documentEdoTask.FormalEdoRequest
			};

			await _uow.SaveAsync(productCode, cancellationToken: cancellationToken);

			var taskItem = new EdoTaskItem
			{
				ProductCode = productCode,
				CustomerEdoTask = documentEdoTask
			};
			documentEdoTask.Items.Add(taskItem);

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

		private void UpdateDocument(
			DocumentEdoTask documentEdoTask,
			List<EdoUpdInventPosition> updInventPositions)
		{
			documentEdoTask.UpdInventPositions.Clear();
			foreach(var updInventPosition in updInventPositions)
			{
				documentEdoTask.UpdInventPositions.Add(updInventPosition);
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

		private static string GetOrderOrganizationInn(DocumentEdoTask documentEdoTask)
		{
			return documentEdoTask.FormalEdoRequest.Order.Contract.Organization.INN;
		}
	}
}
