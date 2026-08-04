using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Errors;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.TrueMark;
using Vodovoz.Errors.Edo;
using VodovozBusiness.Services.Edo;

namespace VodovozBusiness.Services.TrueMark
{
	/// <summary>
	/// Сервис переноса отклоненных кодов маркировки из отмененного заказа в другой заказ.
	/// </summary>
	public class CancelledOrderTrueMarkCodesTransferService : ICancelledOrderTrueMarkCodesTransferService
	{
		private readonly ITrueMarkRepository _trueMarkRepository;

		/// <summary>
		/// Создает экземпляр сервиса переноса отклоненных кодов маркировки.
		/// </summary>
		/// <param name="trueMarkRepository">Репозиторий кодов маркировки</param>
		public CancelledOrderTrueMarkCodesTransferService(ITrueMarkRepository trueMarkRepository)
		{
			_trueMarkRepository = trueMarkRepository ?? throw new ArgumentNullException(nameof(trueMarkRepository));
		}

		/// <inheritdoc />
		public Result<CancelledOrderTrueMarkCodesTransferResult> TransferCodes(
			IUnitOfWork uow,
			int sourceOrderId,
			int targetOrderId)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			var validationResult = ValidateOrderIds(sourceOrderId, targetOrderId);

			if(validationResult.IsFailure)
			{
				return Result.Failure<CancelledOrderTrueMarkCodesTransferResult>(validationResult.Errors);
			}

			var sourceOrder = uow.GetById<Order>(sourceOrderId);
			var targetOrder = uow.GetById<Order>(targetOrderId);

			validationResult = ValidateOrders(sourceOrder, targetOrder);

			if(validationResult.IsFailure)
			{
				return Result.Failure<CancelledOrderTrueMarkCodesTransferResult>(validationResult.Errors);
			}

			var sourceProductCodes = _trueMarkRepository.GetRejectedProductCodesByOrder(uow, sourceOrderId);

			validationResult = ValidateSourceCodes(uow, sourceProductCodes);

			if(validationResult.IsFailure)
			{
				return Result.Failure<CancelledOrderTrueMarkCodesTransferResult>(validationResult.Errors);
			}

			validationResult = ValidateTargetOrderItems(uow, targetOrder, sourceProductCodes);

			if(validationResult.IsFailure)
			{
				return Result.Failure<CancelledOrderTrueMarkCodesTransferResult>(validationResult.Errors);
			}

			ClearSourceProductCodeResults(sourceProductCodes);
			FlushClearedResultCodes(uow);
			var transferredProductCodes = CreateTransferredProductCodes(sourceProductCodes);
			var edoRequest = ManualEdoRequestFactory.Create(targetOrder, transferredProductCodes);
			uow.Save(edoRequest);

			return Result.Success(new CancelledOrderTrueMarkCodesTransferResult
			{
				TargetOrderId = targetOrderId,
				EdoRequestId = edoRequest.Id,
				TransferredCodesCount = sourceProductCodes.Count
			});
		}

		private Result ValidateOrderIds(int sourceOrderId, int targetOrderId)
		{
			var errors = new List<Error>();

			if(sourceOrderId <= 0)
			{
				errors.Add(EdoErrors.SourceOrderIdMissing);
			}

			if(targetOrderId <= 0)
			{
				errors.Add(EdoErrors.TargetOrderIdMissing);
			}

			if(sourceOrderId == targetOrderId)
			{
				errors.Add(EdoErrors.SameTransferOrder);
			}

			return errors.Any() ? Result.Failure(errors) : Result.Success();
		}

		private Result ValidateOrders(Order sourceOrder, Order targetOrder)
		{
			var errors = new List<Error>();

			if(sourceOrder is null)
			{
				errors.Add(EdoErrors.SourceOrderNotFound);
			}
			else if(sourceOrder.OrderStatus != OrderStatus.Canceled)
			{
				errors.Add(EdoErrors.SourceOrderNotCanceled);
			}

			if(targetOrder is null)
			{
				errors.Add(EdoErrors.TargetOrderNotFound);
			}
			else if(targetOrder.OrderStatus == OrderStatus.Canceled || targetOrder.OrderStatus == OrderStatus.DeliveryCanceled)
			{
				errors.Add(EdoErrors.TargetOrderCanceled);
			}

			return errors.Any() ? Result.Failure(errors) : Result.Success();
		}

		private Result ValidateSourceCodes(IUnitOfWork uow, IList<TrueMarkProductCode> sourceProductCodes)
		{
			if(!sourceProductCodes.Any())
			{
				return EdoErrors.RejectedCodesNotFound;
			}

			var sourceCodeIds = new HashSet<int>();
			var excludedProductCodeIds = new HashSet<int>();

			foreach(var sourceProductCode in sourceProductCodes)
			{
				excludedProductCodeIds.Add(sourceProductCode.Id);

				if(!sourceCodeIds.Add(sourceProductCode.SourceCode.Id))
				{
					return EdoErrors.DuplicateRejectedCodes;
				}
			}

			var usedProductCodes = _trueMarkRepository.GetProductCodesByIdentificationCodeIds(
				uow,
				sourceCodeIds,
				excludedProductCodeIds);

			if(!usedProductCodes.Any())
			{
				return Result.Success();
			}

			var orderId = usedProductCodes
				.Select(GetProductCodeOrderId)
				.Where(x => x > 0)
				.FirstOrDefault();

			return orderId > 0
				? TrueMarkCodeErrors.CreateTrueMarkCodeIsAlreadyUsedInOrder(orderId)
				: EdoErrors.ProductCodesAlreadyUsed;

		}

		private static int GetProductCodeOrderId(TrueMarkProductCode productCode)
		{
			switch(productCode)
			{
				case RouteListItemTrueMarkProductCode routeListProductCode:
					return routeListProductCode.RouteListItem?.Order?.Id ?? 0;
				case CarLoadDocumentItemTrueMarkProductCode carLoadProductCode:
					return carLoadProductCode.CarLoadDocumentItem?.OrderId ?? 0;
				case SelfDeliveryDocumentItemTrueMarkProductCode selfDeliveryProductCode:
					return selfDeliveryProductCode.SelfDeliveryDocumentItem?.Document?.Order?.Id ?? 0;
				default:
					return productCode.CustomerEdoRequest?.Order?.Id ?? 0;
			}
		}

		private Result ValidateTargetOrderItems(
			IUnitOfWork uow,
			Order targetOrder,
			IList<TrueMarkProductCode> sourceProductCodes)
		{
			var targetOrderItems = targetOrder.OrderItems
				.Where(x => x.IsTrueMarkCodesMustBeAdded)
				.Where(x => x.Nomenclature.Gtins.Any())
				.ToList();

			if(!targetOrderItems.Any())
			{
				return EdoErrors.TargetOrderItemsNotFound;
			}

			var productCodesCountByOrderItems = _trueMarkRepository.GetProductCodesCountByOrderItems(
				uow,
				targetOrderItems.Select(x => x.Id).ToArray());

			var availableItems = targetOrderItems
				.Select(x => new
				{
					OrderItem = x,
					Gtins = new HashSet<string>(x.Nomenclature.Gtins.Select(g => g.GtinNumber)),
					AvailableCount = Math.Max(
						0,
						(int)(x.ActualCount ?? x.Count) - (productCodesCountByOrderItems.TryGetValue(x.Id, out var orderItem) ? orderItem : 0))
				})
				.ToList();

			var assignedProductCodesCountByOrderItemId = new Dictionary<int, int>();

			foreach(var sourceProductCode in sourceProductCodes)
			{
				var sourceCode = sourceProductCode.SourceCode;
				var targetItem = availableItems.FirstOrDefault(x =>
					x.AvailableCount > GetAssignedProductCodesCount(
						assignedProductCodesCountByOrderItemId,
						x.OrderItem.Id)
					&& x.Gtins.Contains(sourceCode.Gtin));

				if(targetItem is null)
				{
					return EdoErrors.CreateInsufficientTargetOrderItems(sourceCode.Gtin);
				}

				assignedProductCodesCountByOrderItemId[targetItem.OrderItem.Id] =
					GetAssignedProductCodesCount(
						assignedProductCodesCountByOrderItemId,
						targetItem.OrderItem.Id) + 1;
			}

			return Result.Success();
		}

		private static int GetAssignedProductCodesCount(
			IDictionary<int, int> assignedProductCodesCountByOrderItemId,
			int orderItemId) =>
			assignedProductCodesCountByOrderItemId.TryGetValue(orderItemId, out var count) ? count : 0;

		private static IList<TrueMarkProductCode> CreateTransferredProductCodes(
			IList<TrueMarkProductCode> sourceProductCodes)
		{
			var transferredProductCodes = new List<TrueMarkProductCode>();
			var now = DateTime.Now;

			foreach(var sourceProductCode in sourceProductCodes)
			{
				var transferredCode = sourceProductCode.SourceCode;

				transferredProductCodes.Add(new AutoTrueMarkProductCode
				{
					CreationTime = now,
					LastModified = now,
					SourceCode = transferredCode,
					ResultCode = transferredCode,
					SourceCodeStatus = SourceProductCodeStatus.Accepted,
					Problem = ProductCodeProblem.None
				});
			}

			return transferredProductCodes;
		}

		private static void ClearSourceProductCodeResults(IEnumerable<TrueMarkProductCode> sourceProductCodes)
		{
			foreach(var sourceProductCode in sourceProductCodes)
			{
				sourceProductCode.ResultCode = null;
			}
		}

		private static void FlushClearedResultCodes(IUnitOfWork uow)
		{
			uow.OpenTransaction();
			uow.Session.Flush();
		}
	}
}
