using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Logistics;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using VodovozBusiness.Domain.Client.Specifications;
using TrueMarkCodeErrors = Vodovoz.Errors.TrueMark.TrueMarkCodeErrors;
using NomenclatureErrors = Vodovoz.Errors.Goods.NomenclatureErrors;

namespace VodovozBusiness.Services.TrueMark
{
	public class RouteListItemTrueMarkProductCodesProcessingService : IRouteListItemTrueMarkProductCodesProcessingService
	{
		private readonly IOrderRepository _orderRepository;
		private readonly IGenericRepository<RouteListItemEntity> _routeListItemRepository;
		private readonly IGenericRepository<StagingTrueMarkCode> _stagingTrueMarkCodeRepository;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;

		public RouteListItemTrueMarkProductCodesProcessingService(
			IOrderRepository orderRepository,
			IGenericRepository<RouteListItemEntity> routeListItemRepository,
			IGenericRepository<StagingTrueMarkCode> stagingTrueMarkCodeRepository,
			ITrueMarkWaterCodeService trueMarkWaterCodeService)
		{
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_stagingTrueMarkCodeRepository = stagingTrueMarkCodeRepository ?? throw new ArgumentNullException(nameof(stagingTrueMarkCodeRepository));
			_trueMarkWaterCodeService = trueMarkWaterCodeService;
		}
		
		public Result ValidateTrueMarkCodeIsInAggregationCode(TrueMarkAnyCode trueMarkCodeResult)
		{
			if((trueMarkCodeResult.IsTrueMarkTransportCode
					&& trueMarkCodeResult.TrueMarkTransportCode?.ParentTransportCodeId != null)
				|| (trueMarkCodeResult.IsTrueMarkWaterGroupCode
					&& (trueMarkCodeResult.TrueMarkWaterGroupCode?.ParentTransportCodeId != null
						|| trueMarkCodeResult.TrueMarkWaterGroupCode?.ParentWaterGroupCodeId != null))
				|| (trueMarkCodeResult.IsTrueMarkWaterIdentificationCode
					&& (trueMarkCodeResult.TrueMarkWaterIdentificationCode?.ParentTransportCodeId != null
						|| trueMarkCodeResult.TrueMarkWaterIdentificationCode?.ParentWaterGroupCodeId != null)))
			{
				return Result.Failure(TrueMarkCodeErrors.AggregatedCode);
			}

			return Result.Success();
		}

		public void AddTrueMarkCodeToRouteListItem(
			IUnitOfWork uow,
			RouteListItemEntity routeListAddress,
			int vodovozOrderItemId,
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode,
			SourceProductCodeStatus status,
			ProductCodeProblem problem)
		{
			var productCode = CreateRouteListItemTrueMarkProductCode(
				routeListAddress,
				trueMarkWaterIdentificationCode,
				status,
				problem);

			routeListAddress.TrueMarkCodes.Add(productCode);
			uow.Save(productCode);

			var trueMarkCodeOrderItem = new TrueMarkProductCodeOrderItem
			{
				TrueMarkProductCodeId = productCode.Id,
				OrderItemId = vodovozOrderItemId
			};
			uow.Save(trueMarkCodeOrderItem);
		}

		/// <inheritdoc/>
		public async Task AddTrueMarkAnyCodeToRouteListItemNoCodeStatusCheck(
			IUnitOfWork uow,
			RouteListItemEntity routeListAddress,
			int orderSaleItemId,
			TrueMarkAnyCode trueMarkAnyCode,
			SourceProductCodeStatus status,
			ProductCodeProblem problem,
			CancellationToken cancellationToken = default
		)
		{
			IEnumerable<TrueMarkAnyCode> trueMarkAnyCodes = trueMarkAnyCode.Match(
				transportCode => trueMarkAnyCodes = transportCode.GetAllCodes(),
				groupCode => trueMarkAnyCodes = groupCode.GetAllCodes(),
				waterCode => new TrueMarkAnyCode[] { waterCode })
				.ToList();

			foreach(var code in trueMarkAnyCodes)
			{
				if(code.IsTrueMarkWaterIdentificationCode)
				{
					var isCodeAlreadyAddedToRouteListItem =
						routeListAddress.TrueMarkCodes.Any(x =>
						x.SourceCode.Gtin == code.TrueMarkWaterIdentificationCode.Gtin
						&& x.SourceCode.SerialNumber == code.TrueMarkWaterIdentificationCode.SerialNumber);

					if(!isCodeAlreadyAddedToRouteListItem)
					{
						AddTrueMarkCodeToRouteListItem(
							uow,
							routeListAddress,
							orderSaleItemId,
							code.TrueMarkWaterIdentificationCode,
							status,
							problem);
					}
				}

				await code.Match(
					async transportCode =>
					{
						if(transportCode.Id == 0)
						{
							await uow.SaveAsync(transportCode, cancellationToken: cancellationToken);
						}

						return true;
					},
					async groupCode =>
					{
						if(groupCode.Id == 0)
						{
							await uow.SaveAsync(groupCode, cancellationToken: cancellationToken);
						}

						return true;
					},
					async waterCode =>
					{
						if(waterCode.Id == 0)
						{
							await uow.SaveAsync(waterCode, cancellationToken: cancellationToken);
						}

						return true;
					});
			}
		}

		private RouteListItemTrueMarkProductCode CreateRouteListItemTrueMarkProductCode(
			RouteListItemEntity routeListAddress,
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode,
			SourceProductCodeStatus status,
			ProductCodeProblem problem) =>
			new RouteListItemTrueMarkProductCode()
			{
				CreationTime = DateTime.Now,
				SourceCodeStatus = status,
				SourceCode = trueMarkWaterIdentificationCode,
				ResultCode = status == SourceProductCodeStatus.Accepted ? trueMarkWaterIdentificationCode : default,
				RouteListItem = routeListAddress,
				Problem = problem
			};

		public async Task<Result> IsAllStagingTrueMarkCodesAddedToRouteListItem(
			IUnitOfWork uow,
			RouteListItem routeListItem,
			CancellationToken cancellationToken = default)
		{
			var order = routeListItem.Order;

			var stagingCodes =
				await _trueMarkWaterCodeService.GetAllTrueMarkStagingCodesByRelatedDocument(
				uow,
				StagingTrueMarkCodeRelatedDocumentType.RouteListItem,
				routeListItem.Id,
				cancellationToken);

			return IsAllStagingTrueMarkCodesAdded(order, stagingCodes);
		}

		private Result IsAllStagingTrueMarkCodesAdded(
			Order order,
			IEnumerable<StagingTrueMarkCode> stagingCodes)
		{
			var orderItemStaginCodes = stagingCodes
				.GroupBy(x => x.OrderItemId)
				.ToDictionary(x => x.Key, x => x.ToList());

			foreach(var orderItem in order.OrderItems)
			{
				if(!orderItem.Nomenclature.IsAccountableInTrueMark)
				{
					continue;
				}

				if(!orderItemStaginCodes.TryGetValue(orderItem.Id, out var stagingCodesForOrderItem))
				{
					var error = TrueMarkCodeErrors.NotAllCodesAdded;
					return Result.Failure(error);
				}

				var stagingCodesForOrderItemCount = stagingCodesForOrderItem
					.Count(x => x.CodeType == StagingTrueMarkCodeType.Identification);

				var orderItemCount = orderItem.ActualCount ?? orderItem.Count;

				if(stagingCodesForOrderItemCount < orderItemCount)
				{
					var error = TrueMarkCodeErrors.NotAllCodesAdded;
					return Result.Failure(error);
				}

				if(stagingCodesForOrderItemCount > orderItemCount)
				{
					var error = TrueMarkCodeErrors.TrueMarkCodesCountMoreThenInOrderItem;
					return Result.Failure(error);
				}
			}

			return Result.Success();
		}

		public async Task<Result> AddProductCodesToRouteListItemAndDeleteStagingCodes(
			IUnitOfWork uow,
			RouteListItem routeListItem,
			CancellationToken cancellationToken = default)
		{
			var order = routeListItem.Order;

			var stagingCodes =
				await _trueMarkWaterCodeService.GetAllTrueMarkStagingCodesByRelatedDocument(
				uow,
				StagingTrueMarkCodeRelatedDocumentType.RouteListItem,
				routeListItem.Id,
				cancellationToken);

			var orderItemStaginCodes = stagingCodes
				.GroupBy(x => x.OrderItemId)
				.ToDictionary(x => x.Key, x => x.ToList());

			foreach(var orderItem in order.OrderItems)
			{
				if(!orderItem.Nomenclature.IsAccountableInTrueMark)
				{
					continue;
				}

				if(!orderItemStaginCodes.TryGetValue(orderItem.Id, out var stagingCodesForOrderItem))
				{
					var error = TrueMarkCodeErrors.NotAllCodesAdded;
					return Result.Failure(error);
				}

				var stagingCodesForOrderItemCount = stagingCodesForOrderItem
					.Count(x => x.CodeType == StagingTrueMarkCodeType.Identification);

				var orderItemCount = orderItem.ActualCount ?? orderItem.Count;

				if(stagingCodesForOrderItemCount < orderItemCount)
				{
					var error = TrueMarkCodeErrors.NotAllCodesAdded;
					return Result.Failure(error);
				}

				if(stagingCodesForOrderItemCount > orderItemCount)
				{
					var error = TrueMarkCodeErrors.TrueMarkCodesCountMoreThenInOrderItem;
					return Result.Failure(error);
				}

				var addProductCodesResult =
					await AddProductCodesToRouteListItemFromStagingCodes(
						uow,
						stagingCodesForOrderItem,
						routeListItem,
						orderItem.Id,
						cancellationToken);

				if(addProductCodesResult.IsFailure)
				{
					var error = addProductCodesResult.Errors.FirstOrDefault();
					return Result.Failure(error);
				}
			}

			var allCodesAddedToOrderResult =
				IsAllRouteListItemTrueMarkProductCodesAddedToOrder(uow, routeListItem.Order.Id);

			if(allCodesAddedToOrderResult.IsFailure)
			{
				var error = allCodesAddedToOrderResult.Errors.FirstOrDefault();
				return Result.Failure(error);
			}

			var deleteStagingCodesResult =
				await _trueMarkWaterCodeService.DeleteAllTrueMarkStagingCodesByRelatedDocument(
					uow,
					StagingTrueMarkCodeRelatedDocumentType.RouteListItem,
					routeListItem.Id,
					cancellationToken);

			if(deleteStagingCodesResult.IsFailure)
			{
				var error = deleteStagingCodesResult.Errors.FirstOrDefault();
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private async Task<Result> AddProductCodesToRouteListItemFromStagingCodes(
			IUnitOfWork uow,
			IEnumerable<StagingTrueMarkCode> stagingCodes,
			RouteListItemEntity routeListItem,
			int orderItemId,
			CancellationToken cancellationToken = default)
		{
			var trueMarkAnyCodesResult =
				await _trueMarkWaterCodeService.CreateTrueMarkAnyCodesFromStagingCodes(
					uow,
					stagingCodes,
					cancellationToken);

			if(trueMarkAnyCodesResult.IsFailure)
			{
				var error = trueMarkAnyCodesResult.Errors.FirstOrDefault();
				return Result.Failure(error);
			}

			foreach(var trueMarkAnyCode in trueMarkAnyCodesResult.Value)
			{
				await AddTrueMarkAnyCodeToRouteListItemNoCodeStatusCheck(
					uow,
					routeListItem,
					orderItemId,
					trueMarkAnyCode,
					SourceProductCodeStatus.Accepted,
					ProductCodeProblem.None,
					cancellationToken);
			}

			return Result.Success();
		}

		public async Task<Result<StagingTrueMarkCode>> AddStagingTrueMarkCode(
			IUnitOfWork uow,
			string scannedCode,
			int routeListItemId,
			OrderItem orderItem,
			CancellationToken cancellationToken = default)
		{
			var createCodeResult =
				await _trueMarkWaterCodeService.CreateStagingTrueMarkCode(
					uow,
					scannedCode,
					StagingTrueMarkCodeRelatedDocumentType.RouteListItem,
					routeListItemId,
					orderItem.Id,
					cancellationToken);

			if(createCodeResult.IsFailure)
			{
				return createCodeResult;
			}

			var stagingTrueMarkCode = createCodeResult.Value;

			var isCodeCanBeAddedResult =
				await IsStagingTrueMarkCodeCanBeAdded(
					uow,
					stagingTrueMarkCode,
					orderItem,
					cancellationToken);

			if(isCodeCanBeAddedResult.IsFailure)
			{
				var error = isCodeCanBeAddedResult.Errors.FirstOrDefault();
				return Result.Failure<StagingTrueMarkCode>(error);
			}

			await uow.SaveAsync(stagingTrueMarkCode, cancellationToken: cancellationToken);

			return Result.Success(stagingTrueMarkCode);
		}

		public async Task<Result> RemoveStagingTrueMarkCode(
			IUnitOfWork uow,
			string scannedCode,
			int routeListItemId,
			int orderItemId,
			CancellationToken cancellationToken = default)
		{
			var existingCodeResult =
				_trueMarkWaterCodeService.GetSavedStagingTrueMarkCodeByScannedCode(
					uow,
					scannedCode,
					StagingTrueMarkCodeRelatedDocumentType.RouteListItem,
					routeListItemId,
					orderItemId);

			if(existingCodeResult.IsFailure)
			{
				var error = existingCodeResult.Errors.FirstOrDefault();
				return Result.Failure(error);
			}

			var codeToRemove = existingCodeResult.Value;

			if(codeToRemove.ParentCodeId != null)
			{
				var error = TrueMarkCodeErrors.AggregatedCode;
				return Result.Failure(error);
			}

			await uow.DeleteAsync(codeToRemove, cancellationToken: cancellationToken);

			return Result.Success();
		}

		private async Task<Result> IsStagingTrueMarkCodeCanBeAdded(
			IUnitOfWork uow,
			StagingTrueMarkCode stagingTrueMarkCode,
			OrderItem orderItem,
			CancellationToken cancellationToken)
		{
			if(stagingTrueMarkCode.RelatedDocumentType != StagingTrueMarkCodeRelatedDocumentType.RouteListItem)
			{
				throw new InvalidOperationException("Только коды ЧЗ, отсканированные в водительском приложении, могут быть добавлены");
			}
			
			var codeCheckingProcessResult = IsNomeclatureAccountableInTrueMark(orderItem.Nomenclature);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			codeCheckingProcessResult = IsNomeclatureGtinContainsCodeGtin(stagingTrueMarkCode, orderItem.Nomenclature);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			codeCheckingProcessResult = IsRouteListItemHaveNoAddedCodes(uow, stagingTrueMarkCode.RelatedDocumentId);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			codeCheckingProcessResult = IsStagingTrueMarkCodesCountCanBeAdded(uow, stagingTrueMarkCode, orderItem);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			codeCheckingProcessResult =
				await _trueMarkWaterCodeService.IsStagingTrueMarkCodeAlreadyUsedInProductCodes(uow, stagingTrueMarkCode, cancellationToken);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			return Result.Success();
		}

		private Result IsRouteListItemHaveNoAddedCodes(IUnitOfWork uow, int routeListItemid)
		{
			var routeListItem = _routeListItemRepository.GetFirstOrDefault(
				uow,
				x => x.Id == routeListItemid);

			if(routeListItem?.TrueMarkCodes.Count > 0)
			{
				var error = TrueMarkCodeErrors.RelatedDocumentHasTrueMarkCodes;
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private Result IsStagingTrueMarkCodesCountCanBeAdded(
			IUnitOfWork uow,
			StagingTrueMarkCode stagingTrueMarkCode,
			OrderItem orderItem)
		{
			var routeListItemId = stagingTrueMarkCode.RelatedDocumentId;
			var orderItemId = stagingTrueMarkCode.OrderItemId;

			var addedStagingCodesCount = _stagingTrueMarkCodeRepository.GetCount(
				uow,
				StagingTrueMarkCodeSpecification.CreateForRelatedDocumentOrderItemIdentificationCodesExcludeIds(
					StagingTrueMarkCodeRelatedDocumentType.RouteListItem,
					routeListItemId,
					orderItem.Id,
					stagingTrueMarkCode.AllIdentificationCodes.Select(c => c.Id)));

			var newStagingCodesCount = stagingTrueMarkCode.AllIdentificationCodes.Count;

			var isCodeCanBeAdded = addedStagingCodesCount + newStagingCodesCount <= (orderItem.ActualCount ?? orderItem.Count);

			if(!isCodeCanBeAdded)
			{
				var error = TrueMarkCodeErrors.TrueMarkCodesCountMoreThenInOrderItem;
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private Result IsNomeclatureAccountableInTrueMark(Nomenclature nomenclature)
		{
			if(!nomenclature.IsAccountableInTrueMark)
			{
				var error = NomenclatureErrors.CreateIsNotAccountableInTrueMark(nomenclature.Name);
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private Result IsNomeclatureGtinContainsCodeGtin(StagingTrueMarkCode stagingTrueMarkCode, Nomenclature nomenclature)
		{
			var nomenclatureGtins = nomenclature.Gtins
				.Select(x => x.GtinNumber)
				.ToList();

			var codesGtin = stagingTrueMarkCode.AllIdentificationCodes
				.Select(x => x.Gtin)
				.FirstOrDefault();

			if(!nomenclatureGtins.Contains(codesGtin))
			{
				var error = TrueMarkCodeErrors.CreateTrueMarkCodeGtinIsNotEqualsNomenclatureGtin(stagingTrueMarkCode.RawCode);
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private Result IsAllRouteListItemTrueMarkProductCodesAddedToOrder(IUnitOfWork uow, int orderId)
		{
			var isAllTrueMarkCodesAdded =
				_orderRepository.IsAllRouteListItemTrueMarkProductCodesAddedToOrder(uow, orderId);

			if(!isAllTrueMarkCodesAdded)
			{
				return Result.Failure(TrueMarkCodeErrors.NotAllCodesAdded);
			}

			return Result.Success();
		}
	}
}
