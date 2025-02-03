using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
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
		private readonly ITrueMarkWaterCodeCheckService _trueMarkWaterCodeCheckService;

		public RouteListItemTrueMarkProductCodesProcessingService(
			IOrderRepository orderRepository,
			ITrueMarkWaterCodeCheckService trueMarkWaterCodeCheckService)
		{
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_trueMarkWaterCodeCheckService = trueMarkWaterCodeCheckService;
		}

		public async Task<Result> AddTrueMarkCodeToRouteListItemWithCodeChecking(
			IUnitOfWork uow,
			RouteListItem routeListAddress,
			Order vodovozOrder,
			OrderItem vodovozOrderItem,
			string scannedCode,
			SourceProductCodeStatus status,
			CancellationToken cancellationToken,
			bool isCheckForCodeChange = false)
		{
			var trueMarkWaterIdentificationCode =
				_trueMarkWaterCodeCheckService.LoadOrCreateTrueMarkWaterIdentificationCode(uow, scannedCode);

			uow.Save(trueMarkWaterIdentificationCode);

			var codeCheckingResult = await IsTrueMarkCodeCanBeAddedToRouteListItem(
				uow,
				trueMarkWaterIdentificationCode,
				routeListAddress,
				vodovozOrder,
				vodovozOrderItem,
				cancellationToken,
				isCheckForCodeChange);

			if(codeCheckingResult.IsFailure)
			{
				return codeCheckingResult;
			}

			AddTrueMarkCodeToRouteListItem(
				uow,
				routeListAddress,
				vodovozOrderItem.Id,
				trueMarkWaterIdentificationCode,
				status);

			uow.Save(routeListAddress);

			return Result.Success();
		}

		public void AddTrueMarkCodeToRouteListItem(
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
			Order vodovozOrder,
			OrderItem vodovozOrderItem,
			string oldScannedCode,
			string newScannedCode,
			SourceProductCodeStatus status,
			CancellationToken cancellationToken)
		{
			var oldTrueMarkWaterIdentificationCode =
				_trueMarkWaterCodeCheckService.LoadOrCreateTrueMarkWaterIdentificationCode(uow, oldScannedCode);

			var newTrueMarkWaterIdentificationCode =
				_trueMarkWaterCodeCheckService.LoadOrCreateTrueMarkWaterIdentificationCode(uow, newScannedCode);

			if(oldTrueMarkWaterIdentificationCode.GTIN != newTrueMarkWaterIdentificationCode.GTIN)
			{
				var error = TrueMarkCodeErrors.CreateTrueMarkCodesGtinsNotEqual(oldScannedCode, newScannedCode);
				return Result.Failure(error);
			}

			var oldCodeRemovingResult =
				RemoveTrueMarkCodeFromRouteListItem(uow, routeListAddress, vodovozOrderItem, oldScannedCode);

			if(oldCodeRemovingResult.IsFailure)
			{
				var error = oldCodeRemovingResult.Errors.FirstOrDefault();
				return Result.Failure(error);
			}

			var newCodeAddingResult =
				await AddTrueMarkCodeToRouteListItemWithCodeChecking(
					uow,
					routeListAddress,
					vodovozOrder,
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

		public Result RemoveTrueMarkCodeFromRouteListItem(
			IUnitOfWork uow,
			RouteListItem routeListAddress,
			OrderItem vodovozOrderItem,
			string scannedCode)
		{
			var trueMarkWaterIdentificationCode =
				_trueMarkWaterCodeCheckService.LoadOrCreateTrueMarkWaterIdentificationCode(uow, scannedCode);

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

			var productCodeOrderItem = _orderRepository.GetTrueMarkCodesAddedByDriverToOrderItemByOrderItemId(uow, vodovozOrderItem.Id)
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
			Order order,
			OrderItem orderItem,
			CancellationToken cancellationToken,
			bool isCheckForCodeChange = false)
		{
			if(trueMarkWaterIdentificationCode.IsInvalid)
			{
				var error = TrueMarkCodeErrors.CreateTrueMarkCodeStringIsNotValid(trueMarkWaterIdentificationCode.RawCode);
				return Result.Failure(error);
			}

			if(trueMarkWaterIdentificationCode.GTIN != orderItem.Nomenclature.Gtin)
			{
				var error = TrueMarkCodeErrors.CreateTrueMarkCodeGtinIsNotEqualsNomenclatureGtin(trueMarkWaterIdentificationCode.RawCode);
				return Result.Failure(error);
			}

			var addedCodesHavingRequiredGtin = routeListAddress.TrueMarkCodes
				.Where(x => x.SourceCode.GTIN == trueMarkWaterIdentificationCode.GTIN)
			.ToList();
			var orderItemBottlesCount = order.OrderItems
				.Where(x => x.Id == orderItem.Id)
			.FirstOrDefault()?.Count ?? 0;

			var addedToOrderItemCodes = _orderRepository.GetTrueMarkCodesAddedByDriverToOrderItemByOrderItemId(uow, orderItem.Id);
			var addedToOrderItemCodesCount = _orderRepository.GetTrueMarkCodesAddedByDriverToOrderItemByOrderItemId(uow, orderItem.Id).Count;

			if(!isCheckForCodeChange && addedToOrderItemCodesCount >= orderItemBottlesCount)
			{
				var error = TrueMarkCodeErrors.AllCodesAlreadyAdded;
				return Result.Failure(error);
			}

			if(addedCodesHavingRequiredGtin.Select(x => x.SourceCode).Any(x => x.RawCode == trueMarkWaterIdentificationCode.RawCode))
			{
				var error = TrueMarkCodeErrors.TrueMarkCodeIsAlreadyUsed;
				return Result.Failure(error);
			}

			var codeCheckingProcessResult =
				_trueMarkWaterCodeCheckService.IsTrueMarkWaterIdentificationCodeNotUsed(trueMarkWaterIdentificationCode);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			codeCheckingProcessResult =
				await _trueMarkWaterCodeCheckService.IsTrueMarkCodeIntroducedAndHasCorrectInn(trueMarkWaterIdentificationCode, cancellationToken);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
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
