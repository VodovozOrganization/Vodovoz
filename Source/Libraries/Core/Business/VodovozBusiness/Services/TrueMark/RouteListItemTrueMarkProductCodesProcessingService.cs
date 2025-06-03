using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Logistics;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using TrueMarkCodeErrors = Vodovoz.Errors.TrueMark.TrueMarkCode;

namespace VodovozBusiness.Services.TrueMark
{
	public class RouteListItemTrueMarkProductCodesProcessingService : IRouteListItemTrueMarkProductCodesProcessingService
	{
		private readonly IOrderRepository _orderRepository;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;

		public RouteListItemTrueMarkProductCodesProcessingService(
			IOrderRepository orderRepository,
			ITrueMarkWaterCodeService trueMarkWaterCodeService)
		{
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_trueMarkWaterCodeService = trueMarkWaterCodeService;
		}

		public async Task<Result> AddTrueMarkCodeToRouteListItemWithCodeChecking(
			IUnitOfWork uow,
			RouteListItem routeListAddress,
			OrderItem vodovozOrderItem,
			string scannedCode,
			SourceProductCodeStatus status,
			CancellationToken cancellationToken,
			bool isCheckForCodeChange = false,
			bool skipCodeIntroducedAndHasCorrectInnCheck = false)
		{
			var trueMarkCodeResult =
				await _trueMarkWaterCodeService.GetTrueMarkCodeByScannedCode(uow, scannedCode, cancellationToken);

			uow.Commit();
			uow.Session.BeginTransaction();

			if(trueMarkCodeResult.IsFailure)
			{
				return Result.Failure(trueMarkCodeResult.Errors);
			}

			var aggregationValidationResult = ValidateTrueMarkCodeIsInAggregationCode(trueMarkCodeResult.Value);

			if(aggregationValidationResult.IsFailure)
			{
				return Result.Failure(aggregationValidationResult.Errors);
			}

			IEnumerable<TrueMarkAnyCode> trueMarkAnyCodes = trueMarkCodeResult.Value.Match(
				transportCode => trueMarkAnyCodes = transportCode.GetAllCodes(),
				groupCode => trueMarkAnyCodes = groupCode.GetAllCodes(),
				waterCode => new TrueMarkAnyCode[] { waterCode });

			foreach(var trueMarkAnyCode in trueMarkAnyCodes)
			{
				if(!trueMarkAnyCode.IsTrueMarkWaterIdentificationCode)
				{
					continue;
				}

				var codeCheckingResult = await IsTrueMarkCodeCanBeAddedToRouteListItem(
					uow,
					trueMarkAnyCode.TrueMarkWaterIdentificationCode,
					routeListAddress,
					vodovozOrderItem,
					cancellationToken,
					isCheckForCodeChange,
					skipCodeIntroducedAndHasCorrectInnCheck);

				if(codeCheckingResult.IsFailure)
				{
					return codeCheckingResult;
				}

				AddTrueMarkCodeToRouteListItem(
					uow,
					routeListAddress,
					vodovozOrderItem.Id,
					trueMarkAnyCode.TrueMarkWaterIdentificationCode,
					status,
					ProductCodeProblem.None);
			}

			uow.Save(routeListAddress);

			return Result.Success();
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

		public async Task<Result> IsTrueMarkCodeCanBeAddedToRouteListItem(
			IUnitOfWork uow,
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode,
			RouteListItem routeListAddress,
			OrderItem orderItem,
			CancellationToken cancellationToken,
			bool isCheckForCodeChange = false,
			bool skipCodeIntroducedAndHasCorrectInnCheck = false)
		{
			var codeCheckingProcessResult = IsTrueMarkWaterIdentificationCodeValid(trueMarkWaterIdentificationCode);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			codeCheckingProcessResult = IsNomeclatureGtinContainsCodeGtin(trueMarkWaterIdentificationCode, orderItem.Nomenclature);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			if(!isCheckForCodeChange)
			{
				codeCheckingProcessResult = IsNotAllTrueMarkCodesAdded(uow, orderItem);

				if(codeCheckingProcessResult.IsFailure)
				{
					return codeCheckingProcessResult;
				}
			}

			codeCheckingProcessResult = IsCodeAlreadyAddedToRouteListItem(trueMarkWaterIdentificationCode, routeListAddress);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			codeCheckingProcessResult =
				_trueMarkWaterCodeService.IsTrueMarkWaterIdentificationCodeNotUsed(trueMarkWaterIdentificationCode);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			if(!skipCodeIntroducedAndHasCorrectInnCheck)
			{
				codeCheckingProcessResult =
				await _trueMarkWaterCodeService.IsTrueMarkCodeIntroducedAndHasCorrectInn(trueMarkWaterIdentificationCode, cancellationToken);

				if(codeCheckingProcessResult.IsFailure)
				{
					return codeCheckingProcessResult;
				}
			}

			return Result.Success();
		}

		/// <inheritdoc/>
		public async Task<Result> AddProductCodesToRouteListItemNoCodeStatusCheck(
			IUnitOfWork uow,
			RouteListItemEntity routeListAddress,
			int orderSaleItemId,
			IEnumerable<string> scannedCodes,
			SourceProductCodeStatus status,
			ProductCodeProblem problem)
		{
			var scannedCodesDataResult = await _trueMarkWaterCodeService.GetTrueMarkAnyCodesByScannedCodes(uow, scannedCodes);

			if(scannedCodesDataResult.IsFailure)
			{
				return scannedCodesDataResult;
			}

			foreach(var code in scannedCodesDataResult.Value)
			{
				if(code.IsTrueMarkWaterIdentificationCode)
				{
					var isCodeAlreadyAddedToRouteListItem =
						routeListAddress.TrueMarkCodes.Any(x =>
						x.SourceCode.GTIN == code.TrueMarkWaterIdentificationCode.GTIN
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

				code.Match(
					transportCode =>
					{
						if(transportCode.Id == 0)
						{
							uow.Save(transportCode);
						}

						return true;
					},
					groupCode =>
					{
						if(groupCode.Id == 0)
						{
							uow.Save(groupCode);
						}

						return true;
					},
					waterCode =>
					{
						if(waterCode.Id == 0)
						{
							uow.Save(waterCode);
						}

						return true;
					});
			}

			return Result.Success();
		}

		private static Result IsTrueMarkWaterIdentificationCodeValid(TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode)
		{
			if(trueMarkWaterIdentificationCode.IsInvalid)
			{
				var error = TrueMarkCodeErrors.CreateTrueMarkCodeStringIsNotValid(trueMarkWaterIdentificationCode.RawCode);
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private static Result IsCodeAlreadyAddedToRouteListItem(TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode, RouteListItem routeListAddress)
		{
			if(routeListAddress.TrueMarkCodes.Select(x => x.SourceCode).Any(x => x.RawCode == trueMarkWaterIdentificationCode.RawCode))
			{
				var error = TrueMarkCodeErrors.TrueMarkCodeIsAlreadyUsed;
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private Result IsNotAllTrueMarkCodesAdded(IUnitOfWork uow, OrderItem orderItem)
		{
			var addedToOrderItemCodesCount = _orderRepository.GetTrueMarkCodesAddedByDriverToOrderItemByOrderItemId(uow, orderItem.Id).Count;

			var isAllCodesAdded = addedToOrderItemCodesCount >= (orderItem.ActualCount ?? orderItem.Count);

			if(isAllCodesAdded)
			{
				var error = TrueMarkCodeErrors.AllCodesAlreadyAdded;
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private Result IsNomeclatureGtinContainsCodeGtin(TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode, Nomenclature nomenclature)
		{
			var nomenclatureGtins = nomenclature.Gtins
				.Select(x => x.GtinNumber)
				.ToList();

			if(!nomenclatureGtins.Contains(trueMarkWaterIdentificationCode.GTIN))
			{
				var error = TrueMarkCodeErrors.CreateTrueMarkCodeGtinIsNotEqualsNomenclatureGtin(trueMarkWaterIdentificationCode.RawCode);
				return Result.Failure(error);
			}

			return Result.Success();
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
	}
}
