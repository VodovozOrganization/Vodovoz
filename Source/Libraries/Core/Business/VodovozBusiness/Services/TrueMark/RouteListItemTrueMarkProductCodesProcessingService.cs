using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Errors;
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
			bool isCheckForCodeChange = false)
		{
			var trueMarkCodeResult = await _trueMarkWaterCodeService.GetTrueMarkCodeByScannedCode(uow, scannedCode);

			if(trueMarkCodeResult.IsFailure)
			{
				return trueMarkCodeResult;
			}

			IEnumerable<TrueMarkAnyCode> trueMarkAnyCodes = trueMarkCodeResult.Value.Match(
				transportCode => trueMarkAnyCodes = transportCode.GetAllCodes(),
				groupCode => trueMarkAnyCodes = groupCode.GetAllCodes(),
				identificationCode => new TrueMarkAnyCode[] { identificationCode });

			foreach(var anyCode in trueMarkAnyCodes)
			{
				if(!anyCode.IsTrueMarkWaterIdentificationCode)
				{
					continue;
				}

				var addSingleCodeResult = await AddSingleTrueMarkCodeToRouteListItemWithCodeChecking(
					uow,
					routeListAddress,
					vodovozOrderItem,
					anyCode.TrueMarkWaterIdentificationCode,
					status, cancellationToken,
					isCheckForCodeChange);

				if(addSingleCodeResult.IsFailure)
				{
					return addSingleCodeResult;
				}
			}

			return Result.Success();
		}

		private async Task<Result> AddSingleTrueMarkCodeToRouteListItemWithCodeChecking(
			IUnitOfWork uow,
			RouteListItem routeListAddress,
			OrderItem vodovozOrderItem,
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode,
			SourceProductCodeStatus status,
			CancellationToken cancellationToken,
			bool isCheckForCodeChange = false)
		{
			var codeCheckingResult = await IsTrueMarkCodeCanBeAddedToRouteListItem(
				uow,
				trueMarkWaterIdentificationCode,
				routeListAddress,
				vodovozOrderItem,
				cancellationToken,
				isCheckForCodeChange);

			if(codeCheckingResult.IsFailure)
			{
				return codeCheckingResult;
			}

			AddSingleTrueMarkCodeToRouteListItem(
				uow,
				routeListAddress,
				vodovozOrderItem.Id,
				trueMarkWaterIdentificationCode,
				status);

			uow.Save(routeListAddress);

			return Result.Success();
		}

		public void AddSingleTrueMarkCodeToRouteListItem(
			IUnitOfWork uow,
			RouteListItem routeListAddress,
			int vodovozOrderItemId,
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode,
			SourceProductCodeStatus status)
		{
			var productCode = CreateRouteListItemTrueMarkProductCode(
				routeListAddress,
				trueMarkWaterIdentificationCode,
				status);

			routeListAddress.TrueMarkCodes.Add(productCode);
			uow.Save(productCode);

			var trueMarkCodeOrderItem = new TrueMarkProductCodeOrderItem
			{
				TrueMarkProductCodeId = productCode.Id,
				OrderItemId = vodovozOrderItemId
			};
			uow.Save(trueMarkCodeOrderItem);
		}

		public async Task<Result> ChangeTrueMarkCodeToRouteListItemWithCodeChecking(
			IUnitOfWork uow,
			RouteListItem routeListAddress,
			OrderItem vodovozOrderItem,
			string oldScannedCode,
			string newScannedCode,
			SourceProductCodeStatus status,
			CancellationToken cancellationToken)
		{
			var oldCodeRemovingResult =
				await RemoveTrueMarkCodeFromRouteListItem(uow, routeListAddress, vodovozOrderItem.Id, oldScannedCode);

			if(oldCodeRemovingResult.IsFailure)
			{
				var error = oldCodeRemovingResult.Errors.FirstOrDefault();
				return Result.Failure(error);
			}

			var newCodeAddingResult =
				await AddTrueMarkCodeToRouteListItemWithCodeChecking(
					uow,
					routeListAddress,
					vodovozOrderItem,
					newScannedCode,
					status,
					cancellationToken,
					true);

			if(newCodeAddingResult.IsFailure)
			{
				var error = newCodeAddingResult.Errors.FirstOrDefault();
				return Result.Failure(error);
			}

			return Result.Success();
		}

		public async Task<Result> RemoveTrueMarkCodeFromRouteListItem(
			IUnitOfWork uow,
			RouteListItem routeListAddress,
			int vodovozOrderItemId,
			string scannedCode)
		{
			var trueMarkCodeResult = await _trueMarkWaterCodeService.GetTrueMarkCodeByScannedCode(uow, scannedCode);

			if(trueMarkCodeResult.IsFailure)
			{
				return trueMarkCodeResult;
			}

			IEnumerable<TrueMarkAnyCode> trueMarkAnyCodes = trueMarkCodeResult.Value.Match(
				transportCode => trueMarkAnyCodes = transportCode.GetAllCodes(),
				groupCode => trueMarkAnyCodes = groupCode.GetAllCodes(),
				identificationCode => new TrueMarkAnyCode[] { identificationCode });

			foreach(var anyCode in trueMarkAnyCodes)
			{
				if(!anyCode.IsTrueMarkWaterIdentificationCode)
				{
					continue;
				}

				var removeSingleCodeResult = RemoveSingleTrueMarkCodeFromRouteListItem(
					uow,
					routeListAddress,
					vodovozOrderItemId,
					anyCode.TrueMarkWaterIdentificationCode);

				if(removeSingleCodeResult.IsFailure)
				{
					return removeSingleCodeResult;
				}
			}

			return Result.Success();
		}

		private Result RemoveSingleTrueMarkCodeFromRouteListItem(
			IUnitOfWork uow,
			RouteListItem routeListAddress,
			int vodovozOrderItemId,
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode)
		{
			var productCode =
				routeListAddress.TrueMarkCodes
				.Where(x => x.SourceCode.Id == trueMarkWaterIdentificationCode.Id)
				.FirstOrDefault();

			if(productCode is null)
			{
				var error = TrueMarkCodeErrors.TrueMarkCodeForRouteListItemNotFound;
				return Result.Failure(error);
			}

			routeListAddress.TrueMarkCodes.Remove(productCode);

			var productCodeOrderItem = _orderRepository.GetTrueMarkCodesAddedByDriverToOrderItemByOrderItemId(uow, vodovozOrderItemId)
				.Where(x => x.TrueMarkProductCodeId == productCode.Id)
				.FirstOrDefault();

			if(productCodeOrderItem != null)
			{
				uow.Delete(productCodeOrderItem);
			}

			uow.Save(routeListAddress);

			return Result.Success();
		}

		private async Task<Result> IsTrueMarkCodeCanBeAddedToRouteListItem(
			IUnitOfWork uow,
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode,
			RouteListItem routeListAddress,
			OrderItem orderItem,
			CancellationToken cancellationToken,
			bool isCheckForCodeChange = false)
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

			codeCheckingProcessResult =
				await _trueMarkWaterCodeService.IsTrueMarkCodeIntroducedAndHasCorrectInn(trueMarkWaterIdentificationCode, cancellationToken);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
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
			RouteListItem routeListAddress,
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode,
			SourceProductCodeStatus status) =>
			new RouteListItemTrueMarkProductCode()
			{
				CreationTime = DateTime.Now,
				SourceCodeStatus = status,
				SourceCode = trueMarkWaterIdentificationCode,
				ResultCode = status == SourceProductCodeStatus.Accepted ? trueMarkWaterIdentificationCode : default,
				RouteListItem = routeListAddress
			};
	}
}
