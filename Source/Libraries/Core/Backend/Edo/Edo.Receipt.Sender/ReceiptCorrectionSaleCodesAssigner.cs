using Edo.Common;
using Edo.Common.Services;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Edo.Receipt.Sender
{
	public class ReceiptCorrectionSaleCodesAssigner
	{
		private readonly ILogger<ReceiptCorrectionSaleCodesAssigner> _logger;
		private readonly ITrueMarkCodesPool _trueMarkCodesPool;
		private readonly ITrueMarkCodesPoolCodeProvider _trueMarkCodesPoolCodeProvider;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;
		private readonly ISaveCodesService _saveCodesService;

		public ReceiptCorrectionSaleCodesAssigner(
			ILogger<ReceiptCorrectionSaleCodesAssigner> logger,
			ITrueMarkCodesPool trueMarkCodesPool,
			ITrueMarkCodesPoolCodeProvider trueMarkCodesPoolCodeProvider,
			ITrueMarkWaterCodeService trueMarkWaterCodeService,
			ISaveCodesService saveCodesService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_trueMarkCodesPool = trueMarkCodesPool ?? throw new ArgumentNullException(nameof(trueMarkCodesPool));
			_trueMarkCodesPoolCodeProvider = trueMarkCodesPoolCodeProvider
				?? throw new ArgumentNullException(nameof(trueMarkCodesPoolCodeProvider));
			_trueMarkWaterCodeService = trueMarkWaterCodeService
				?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
			_saveCodesService = saveCodesService ?? throw new ArgumentNullException(nameof(saveCodesService));
		}

		public async Task AssignAsync(
			IUnitOfWork uow,
			EdoFiscalDocument saleDocument,
			EdoFiscalDocument sourceDocument,
			CancellationToken cancellationToken)
		{
			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(saleDocument == null)
			{
				return;
			}

			var order = saleDocument.ReceiptEdoTask?.FormalEdoRequest?.Order;

			if(saleDocument.DocumentType == FiscalDocumentType.Sale
				|| saleDocument.DocumentType == FiscalDocumentType.SaleCorrection)
			{
				await AssignSaleCodesAsync(uow, saleDocument, sourceDocument, cancellationToken);
			}

			if(order != null)
			{
				await ReconcileOrderPoolCodesAsync(uow, order, cancellationToken);
			}
		}

		public async Task ReconcileOrderPoolCodesAsync(
			IUnitOfWork uow,
			OrderEntity order,
			CancellationToken cancellationToken)
		{
			if(uow == null || order == null)
			{
				return;
			}

			var inFlightIds = GetInFlightResultCodeIds(uow, order.Id);
			var poolCodes = await GetOrderPoolAssignedCodesAsync(uow, order.Id, cancellationToken);
			if(!poolCodes.Any())
			{
				return;
			}

			var keepIds = new HashSet<int>(inFlightIds);
			var available = poolCodes
				.Where(x => x.ResultCode != null && !inFlightIds.Contains(x.ResultCode.Id))
				.OrderByDescending(x => x.Id)
				.ToList();

			var inFlightCodes = poolCodes
				.Where(x => x.ResultCode != null && inFlightIds.Contains(x.ResultCode.Id))
				.ToList();
			var consumedInFlightIds = new HashSet<int>();

			foreach(var orderItem in order.OrderItems ?? Enumerable.Empty<OrderItemEntity>())
			{
				var nomenclature = orderItem?.Nomenclature;
				if(nomenclature == null
					|| !nomenclature.IsAccountableInTrueMark
					|| orderItem.CurrentCount <= 0
					|| nomenclature.Gtins == null
					|| !nomenclature.Gtins.Any())
				{
					continue;
				}

				var gtins = new HashSet<string>(
					nomenclature.Gtins
						.Select(x => x.GtinNumber)
						.Where(x => !string.IsNullOrWhiteSpace(x)),
					StringComparer.OrdinalIgnoreCase);

				var quantity = (int)Math.Round(orderItem.CurrentCount, MidpointRounding.AwayFromZero);

				var coveredByInFlight = inFlightCodes
					.Where(x => !consumedInFlightIds.Contains(x.ResultCode.Id)
						&& gtins.Contains(x.ResultCode.Gtin))
					.Take(quantity)
					.ToList();

				foreach(var productCode in coveredByInFlight)
				{
					consumedInFlightIds.Add(productCode.ResultCode.Id);
				}

				var stillNeed = Math.Max(0, quantity - coveredByInFlight.Count);
				var matched = available
					.Where(x => !keepIds.Contains(x.ResultCode.Id)
						&& gtins.Contains(x.ResultCode.Gtin))
					.Take(stillNeed)
					.ToList();

				foreach(var productCode in matched)
				{
					keepIds.Add(productCode.ResultCode.Id);
				}
			}

			var released = 0;
			foreach(var productCode in available.Where(x => !keepIds.Contains(x.ResultCode.Id)))
			{
				await _saveCodesService.SavePoolResultCodeToPool(productCode, cancellationToken);
				await uow.SaveAsync(productCode, cancellationToken: cancellationToken);
				released++;
			}

			if(released > 0)
			{
				_logger.LogInformation(
					"По заказу {OrderId} возвращено в пул {ReleasedCount} лишних кодов ЧЗ.",
					order.Id,
					released);
			}
		}

		private async Task AssignSaleCodesAsync(
			IUnitOfWork uow,
			EdoFiscalDocument saleDocument,
			EdoFiscalDocument sourceDocument,
			CancellationToken cancellationToken)
		{
			var receiptEdoTask = saleDocument.ReceiptEdoTask;
			if(receiptEdoTask?.FormalEdoRequest == null)
			{
				return;
			}

			var organizationInn = TryGetOrganizationInn(receiptEdoTask);
			if(string.IsNullOrWhiteSpace(organizationInn))
			{
				_logger.LogWarning(
					"Не удалось определить ИНН организации для подбора кодов ЧЗ к документу {DocumentNumber}.",
					saleDocument.DocumentNumber);
				return;
			}

			var order = receiptEdoTask.FormalEdoRequest.Order;
			var reservedResultCodeIds = new HashSet<int>();
			var regulatoryFallback = ResolveRegulatoryDocument(saleDocument, sourceDocument);
			var rebuilt = new List<FiscalInventPosition>();
			var assignedCount = 0;

			foreach(var position in saleDocument.InventPositions.ToList())
			{
				var nomenclature = ResolveNomenclature(position, receiptEdoTask);
				if(HasProductMark(position)
					|| nomenclature == null
					|| !nomenclature.IsAccountableInTrueMark
					|| nomenclature.Gtins == null
					|| !nomenclature.Gtins.Any())
				{
					rebuilt.Add(position);
					continue;
				}

				var units = ExpandToMarkedUnits(position, regulatoryFallback);
				foreach(var unit in units)
				{
					await AttachPoolCodeAsync(
						uow,
						receiptEdoTask,
						order,
						unit,
						nomenclature,
						organizationInn,
						reservedResultCodeIds,
						cancellationToken);
					rebuilt.Add(unit);
					assignedCount++;
				}
			}

			if(assignedCount == 0)
			{
				return;
			}

			saleDocument.InventPositions.Clear();
			foreach(var position in rebuilt)
			{
				saleDocument.InventPositions.Add(position);
			}

			_logger.LogInformation(
				"К документу корректировки {DocumentNumber} из пула назначено {AssignedCount} кодов ЧЗ.",
				saleDocument.DocumentNumber,
				assignedCount);
		}

		private async Task AttachPoolCodeAsync(
			IUnitOfWork uow,
			ReceiptEdoTask receiptEdoTask,
			OrderEntity order,
			FiscalInventPosition inventPosition,
			NomenclatureEntity nomenclature,
			string organizationInn,
			ISet<int> reservedResultCodeIds,
			CancellationToken cancellationToken)
		{
			TrueMarkProductCode productCode = null;

			if(order != null)
			{
				productCode = await FindReusablePoolCodeAsync(
					uow,
					order.Id,
					nomenclature.Gtins.Select(x => x.GtinNumber).Where(x => !string.IsNullOrWhiteSpace(x)).ToList(),
					reservedResultCodeIds,
					cancellationToken);
			}

			if(productCode?.ResultCode != null)
			{
				reservedResultCodeIds.Add(productCode.ResultCode.Id);
			}
			else
			{
				var code = await _trueMarkCodesPoolCodeProvider.TakeValidCodeAsync(
					_trueMarkCodesPool,
					nomenclature.Gtins,
					organizationInn,
					cancellationToken);

				await _trueMarkWaterCodeService.DisaggregateRelatedCodesAsync(uow, code, cancellationToken);

				productCode = new AutoTrueMarkProductCode
				{
					Problem = ProductCodeProblem.Unscanned,
					CustomerEdoRequest = receiptEdoTask.FormalEdoRequest,
					SourceCodeStatus = SourceProductCodeStatus.Changed,
					SourceCode = null,
					ResultCode = code
				};

				await uow.SaveAsync(productCode, cancellationToken: cancellationToken);

				if(receiptEdoTask.FormalEdoRequest.ProductCodes == null)
				{
					receiptEdoTask.FormalEdoRequest.ProductCodes = new ObservableList<TrueMarkProductCode>();
				}

				receiptEdoTask.FormalEdoRequest.ProductCodes.Add(productCode);
				reservedResultCodeIds.Add(code.Id);
			}

			var taskItem = new EdoTaskItem
			{
				CustomerEdoTask = receiptEdoTask,
				ProductCode = productCode
			};

			await uow.SaveAsync(taskItem, cancellationToken: cancellationToken);
			receiptEdoTask.Items.Add(taskItem);
			inventPosition.EdoTaskItem = taskItem;
		}

		private async Task<TrueMarkProductCode> FindReusablePoolCodeAsync(
			IUnitOfWork uow,
			int orderId,
			IList<string> gtins,
			ISet<int> reservedResultCodeIds,
			CancellationToken cancellationToken)
		{
			if(gtins == null || !gtins.Any())
			{
				return null;
			}

			var inFlightIds = GetInFlightResultCodeIds(uow, orderId);

			var gtinSet = new HashSet<string>(gtins, StringComparer.OrdinalIgnoreCase);
			var candidates = await GetOrderPoolAssignedCodesAsync(uow, orderId, cancellationToken);
			return candidates
				.Where(x => x.ResultCode != null
					&& gtinSet.Contains(x.ResultCode.Gtin)
					&& !reservedResultCodeIds.Contains(x.ResultCode.Id)
					&& !inFlightIds.Contains(x.ResultCode.Id))
				.OrderByDescending(x => x.Id)
				.FirstOrDefault();
		}

		private static HashSet<int> GetInFlightResultCodeIds(IUnitOfWork uow, int orderId)
		{
			var result = new HashSet<int>();

			var documents = uow.Session.Query<EdoFiscalDocument>()
				.Where(x => x.ReceiptEdoTask.FormalEdoRequest.Order.Id == orderId
					&& x.Stage != FiscalDocumentStage.Completed
					&& x.Status != FiscalDocumentStatus.Failed
					&& x.Status != FiscalDocumentStatus.SendError)
				.ToList();

			foreach(var document in documents)
			{
				foreach(var position in document.InventPositions ?? Enumerable.Empty<FiscalInventPosition>())
				{
					var resultCodeId = position.EdoTaskItem?.ProductCode?.ResultCode?.Id;
					if(resultCodeId.HasValue)
					{
						result.Add(resultCodeId.Value);
					}
				}
			}

			return result;
		}

		private static async Task<IList<TrueMarkProductCode>> GetOrderPoolAssignedCodesAsync(
			IUnitOfWork uow,
			int orderId,
			CancellationToken cancellationToken)
		{
			var codes = await uow.Session.Query<AutoTrueMarkProductCode>()
				.Where(x => x.CustomerEdoRequest != null
					&& x.CustomerEdoRequest.Order.Id == orderId
					&& x.SourceCodeStatus == SourceProductCodeStatus.Changed
					&& x.ResultCode != null)
				.Fetch(x => x.ResultCode)
				.ToListAsync(cancellationToken);

			return codes.Cast<TrueMarkProductCode>().ToList();
		}

		private static bool HasProductMark(FiscalInventPosition position)
		{
			return position.EdoTaskItem != null || position.GroupCode != null;
		}

		private static NomenclatureEntity ResolveNomenclature(
			FiscalInventPosition position,
			ReceiptEdoTask receiptEdoTask = null)
		{
			var fromOrderItems = position.OrderItems?
				.Select(x => x?.Nomenclature)
				.FirstOrDefault(x => x != null);
			if(fromOrderItems != null)
			{
				return fromOrderItems;
			}

			var order = receiptEdoTask?.FormalEdoRequest?.Order;
			if(order?.OrderItems == null || string.IsNullOrWhiteSpace(position.Name))
			{
				return null;
			}

			return order.OrderItems
				.Where(x => x?.Nomenclature != null && x.CurrentCount > 0)
				.Select(x => x.Nomenclature)
				.FirstOrDefault(n =>
					string.Equals(
						(n.OfficialName ?? n.Name)?.Trim(),
						position.Name.Trim(),
						StringComparison.OrdinalIgnoreCase)
					|| string.Equals(
						n.Name?.Trim(),
						position.Name.Trim(),
						StringComparison.OrdinalIgnoreCase));
		}

		private static IList<FiscalInventPosition> ExpandToMarkedUnits(
			FiscalInventPosition source,
			FiscalIndustryRequisiteRegulatoryDocument regulatoryFallback)
		{
			var quantity = source.Quantity;
			if(quantity <= 0)
			{
				return new List<FiscalInventPosition> { source };
			}

			var unitCount = (int)Math.Round(quantity, MidpointRounding.AwayFromZero);
			if(unitCount <= 0)
			{
				unitCount = 1;
			}

			if(unitCount == 1 && quantity == 1)
			{
				if(source.RegulatoryDocument == null && regulatoryFallback != null)
				{
					source.RegulatoryDocument = regulatoryFallback;
				}

				return new List<FiscalInventPosition> { source };
			}

			var result = new List<FiscalInventPosition>(unitCount);
			var baseDiscount = Math.Round(source.DiscountSum / unitCount, 2);
			var allocatedDiscount = 0m;

			for(var i = 0; i < unitCount; i++)
			{
				var isLast = i == unitCount - 1;
				var unitDiscount = isLast
					? source.DiscountSum - allocatedDiscount
					: baseDiscount;
				allocatedDiscount += unitDiscount;

				result.Add(new FiscalInventPosition
				{
					Name = source.Name,
					Quantity = 1,
					Price = source.Price,
					DiscountSum = unitDiscount,
					Vat = source.Vat,
					RegulatoryDocument = source.RegulatoryDocument ?? regulatoryFallback,
					IndustryRequisiteData = source.IndustryRequisiteData,
					OrderItems = source.OrderItems == null
						? new ObservableList<OrderItemEntity>()
						: new ObservableList<OrderItemEntity>(source.OrderItems)
				});
			}

			return result;
		}

		private static FiscalIndustryRequisiteRegulatoryDocument ResolveRegulatoryDocument(
			EdoFiscalDocument saleDocument,
			EdoFiscalDocument sourceDocument)
		{
			var fromSale = saleDocument.InventPositions?
				.Select(x => x.RegulatoryDocument)
				.FirstOrDefault(x => x != null);
			if(fromSale != null)
			{
				return fromSale;
			}

			var fromTaskReturn = saleDocument.ReceiptEdoTask?.FiscalDocuments?
				.Where(x => x.DocumentType == FiscalDocumentType.Return)
				.SelectMany(x => x.InventPositions ?? Enumerable.Empty<FiscalInventPosition>())
				.Select(x => x.RegulatoryDocument)
				.FirstOrDefault(x => x != null);
			if(fromTaskReturn != null)
			{
				return fromTaskReturn;
			}

			return sourceDocument?.InventPositions?
				.Select(x => x.RegulatoryDocument)
				.FirstOrDefault(x => x != null);
		}

		private static string TryGetOrganizationInn(ReceiptEdoTask receiptEdoTask)
		{
			try
			{
				return receiptEdoTask.FormalEdoRequest?.Order?.Contract?.Organization?.INN;
			}
			catch
			{
				return null;
			}
		}
	}
}
