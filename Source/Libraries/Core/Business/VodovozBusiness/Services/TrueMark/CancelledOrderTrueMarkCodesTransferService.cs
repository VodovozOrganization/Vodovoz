using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
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

			var targetOrderItemsBySourceCode = MatchTargetOrderItems(uow, targetOrder, sourceProductCodes);

			if(targetOrderItemsBySourceCode.IsFailure)
			{
				return Result.Failure<CancelledOrderTrueMarkCodesTransferResult>(targetOrderItemsBySourceCode.Errors);
			}

			var createdProductCodes = CreateProductCodes(sourceProductCodes);
			var edoRequest = ManualEdoRequestFactory.Create(targetOrder, createdProductCodes.Values);
			uow.Save(edoRequest);

			CreateProductCodeOrderItems(uow, createdProductCodes, targetOrderItemsBySourceCode.Value);

			return Result.Success(new CancelledOrderTrueMarkCodesTransferResult
			{
				TargetOrderId = targetOrderId,
				EdoRequestId = edoRequest.Id,
				TransferredCodesCount = createdProductCodes.Count
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
			
			if(usedProductCodes.Any())
			{
				return EdoErrors.ProductCodesAlreadyUsed;
			}

			return Result.Success();
		}

		private Result<IDictionary<int, OrderItem>> MatchTargetOrderItems(
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

			var result = new Dictionary<int, OrderItem>();

			foreach(var sourceProductCode in sourceProductCodes)
			{
				var sourceCode = sourceProductCode.SourceCode;
				var targetItem = availableItems.FirstOrDefault(x =>
					x.AvailableCount > result.Values.Count(item => item.Id == x.OrderItem.Id)
					&& x.Gtins.Contains(sourceCode.Gtin));

				if(targetItem is null)
				{
					return EdoErrors.CreateInsufficientTargetOrderItems(sourceCode.Gtin);
				}

				result.Add(sourceProductCode.Id, targetItem.OrderItem);
			}

			return Result.Success<IDictionary<int, OrderItem>>(result);
		}

		private static IDictionary<int, TrueMarkProductCode> CreateProductCodes(
			IList<TrueMarkProductCode> sourceProductCodes)
		{
			var now = DateTime.Now;

			return sourceProductCodes.ToDictionary(
				sourceProductCode => sourceProductCode.Id,
				sourceProductCode => (TrueMarkProductCode)new AutoTrueMarkProductCode
			{
				CreationTime = now,
				LastModified = now,
				SourceCode = sourceProductCode.SourceCode,
				ResultCode = sourceProductCode.SourceCode,
				SourceCodeStatus = SourceProductCodeStatus.Accepted,
				Problem = ProductCodeProblem.None
			});
		}

		private static void CreateProductCodeOrderItems(
			IUnitOfWork uow,
			IDictionary<int, TrueMarkProductCode> createdProductCodes,
			IDictionary<int, OrderItem> targetOrderItemsBySourceCode)
		{
			foreach(var createdProductCode in createdProductCodes)
			{
				var productCodeOrderItem = new TrueMarkProductCodeOrderItem
				{
					TrueMarkProductCodeId = createdProductCode.Value.Id,
					OrderItemId = targetOrderItemsBySourceCode[createdProductCode.Key].Id
				};

				uow.Save(productCodeOrderItem);
			}
		}
	}
}
