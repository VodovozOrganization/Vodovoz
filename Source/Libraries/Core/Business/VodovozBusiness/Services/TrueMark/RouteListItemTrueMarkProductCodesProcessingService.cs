using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Logistics;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using TrueMarkCodeErrors = Vodovoz.Errors.TrueMark.TrueMarkCodeErrors;

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
		
		public Result ValidateTrueMarkCodeIsInAggregationCode(TrueMarkAnyCode trueMarkCodeResult)
		{
			// Разрешаем индивидуальные коды, даже если они имеют родителей
			if (trueMarkCodeResult.IsTrueMarkWaterIdentificationCode)
			{
				return Result.Success();
			}

			// Блокируем только группы и транспортные коды, если они сами входят в агрегацию
			if ((trueMarkCodeResult.IsTrueMarkTransportCode
			     && trueMarkCodeResult.TrueMarkTransportCode?.ParentTransportCodeId != null)
			    || (trueMarkCodeResult.IsTrueMarkWaterGroupCode
			        && (trueMarkCodeResult.TrueMarkWaterGroupCode?.ParentTransportCodeId != null
			            || trueMarkCodeResult.TrueMarkWaterGroupCode?.ParentWaterGroupCodeId != null)))
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
			IEnumerable<TrueMarkWaterIdentificationCode> trueMarkWaterIdentificationCodes,
			RouteListItem routeListAddress,
			OrderItem orderItem,
			CancellationToken cancellationToken,
			bool isCheckForCodeChange = false
		)
		{
			Result codeCheckingProcessResult;

			foreach(var trueMarkWaterIdentificationCode in trueMarkWaterIdentificationCodes)
			{
				codeCheckingProcessResult = IsTrueMarkWaterIdentificationCodeValid(trueMarkWaterIdentificationCode);

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

				codeCheckingProcessResult = _trueMarkWaterCodeService
					.IsTrueMarkWaterIdentificationCodeNotUsed(trueMarkWaterIdentificationCode);

				if(codeCheckingProcessResult.IsFailure)
				{
					return codeCheckingProcessResult;
				}
			}

			codeCheckingProcessResult = await _trueMarkWaterCodeService.IsAllTrueMarkCodesValid(
				trueMarkWaterIdentificationCodes, 
				cancellationToken
			);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			return Result.Success();
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
				waterCode => new TrueMarkAnyCode[] { waterCode });

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

			if(!nomenclatureGtins.Contains(trueMarkWaterIdentificationCode.Gtin))
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
