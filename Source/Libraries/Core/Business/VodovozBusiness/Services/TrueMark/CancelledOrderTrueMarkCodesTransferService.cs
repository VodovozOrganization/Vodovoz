using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.TrueMark;

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

			var edoRequest = CreateEdoRequest(targetOrder);
			uow.Save(edoRequest);

			var createdProductCodes = CreateProductCodes(uow, edoRequest, sourceProductCodes, targetOrderItemsBySourceCode.Value);
			edoRequest.ProductCodes = new ObservableList<TrueMarkProductCode>(createdProductCodes);
			uow.Save(edoRequest);

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
				errors.Add(CreateError(nameof(sourceOrderId), "Не указан заказ-источник."));
			}

			if(targetOrderId <= 0)
			{
				errors.Add(CreateError(nameof(targetOrderId), "Не указан заказ, в который нужно перенести коды."));
			}

			if(sourceOrderId == targetOrderId)
			{
				errors.Add(CreateError(nameof(targetOrderId), "Нельзя перенести коды в тот же самый заказ."));
			}

			return errors.Any() ? Result.Failure(errors) : Result.Success();
		}

		private Result ValidateOrders(Order sourceOrder, Order targetOrder)
		{
			var errors = new List<Error>();

			if(sourceOrder is null)
			{
				errors.Add(CreateError(nameof(sourceOrder), "Заказ-источник не найден."));
			}
			else if(sourceOrder.OrderStatus != OrderStatus.Canceled)
			{
				errors.Add(CreateError(nameof(sourceOrder), "Переносить коды можно только из полностью отмененного заказа."));
			}

			if(targetOrder is null)
			{
				errors.Add(CreateError(nameof(targetOrder), "Целевой заказ не найден."));
			}
			else if(targetOrder.OrderStatus == OrderStatus.Canceled || targetOrder.OrderStatus == OrderStatus.DeliveryCanceled)
			{
				errors.Add(CreateError(nameof(targetOrder), "Нельзя перенести коды в отмененный заказ."));
			}

			return errors.Any() ? Result.Failure(errors) : Result.Success();
		}

		private Result ValidateSourceCodes(IUnitOfWork uow, IList<TrueMarkProductCode> sourceProductCodes)
		{
			if(!sourceProductCodes.Any())
			{
				return CreateError(nameof(sourceProductCodes), "В отмененном заказе нет отклоненных кодов для переноса.");
			}

			var sourceCodeIds = new HashSet<int>();
			var excludedProductCodeIds = new HashSet<int>();

			foreach(var sourceProductCode in sourceProductCodes)
			{
				excludedProductCodeIds.Add(sourceProductCode.Id);

				if(!sourceCodeIds.Add(sourceProductCode.SourceCode.Id))
				{
					return CreateError(nameof(sourceProductCodes), "В отмененном заказе есть повторяющиеся коды. Перенос отменен.");
				}
			}

			var usedProductCodes = _trueMarkRepository.GetProductCodesByIdentificationCodeIds(
				uow,
				sourceCodeIds,
				excludedProductCodeIds);
			
			if(usedProductCodes.Any())
			{
				return CreateError(
					nameof(sourceProductCodes),
					"Часть кодов уже используется в другом заказе или документе. Перенос отменен.");
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
				return CreateError(nameof(targetOrder), "В целевом заказе нет товаров, требующих коды маркировки.");
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
					return CreateError(
						nameof(targetOrder),
						$"В целевом заказе недостаточно товаров с GTIN {sourceCode.Gtin} для переноса кодов.");
				}

				result.Add(sourceProductCode.Id, targetItem.OrderItem);
			}

			return Result.Success<IDictionary<int, OrderItem>>(result);
		}

		private static ManualEdoRequest CreateEdoRequest(Order targetOrder)
		{
			return new ManualEdoRequest
			{
				Order = targetOrder,
				Source = EdoRequestSource.Manual,
				Time = DateTime.Now,
				DocumentType = EdoDocumentType.UPD,
				Type = CustomerEdoRequestType.Order
			};
		}

		private static IList<TrueMarkProductCode> CreateProductCodes(
			IUnitOfWork uow,
			FormalEdoRequest edoRequest,
			IList<TrueMarkProductCode> sourceProductCodes,
			IDictionary<int, OrderItem> targetOrderItemsBySourceCode)
		{
			var createdProductCodes = new List<TrueMarkProductCode>();

			foreach(var sourceProductCode in sourceProductCodes)
			{
				var productCode = new AutoTrueMarkProductCode
				{
					CreationTime = DateTime.Now,
					LastModified = DateTime.Now,
					SourceCode = sourceProductCode.SourceCode,
					ResultCode = sourceProductCode.SourceCode,
					SourceCodeStatus = SourceProductCodeStatus.Accepted,
					Problem = ProductCodeProblem.None,
					CustomerEdoRequest = edoRequest
				};

				uow.Save(productCode);

				var productCodeOrderItem = new TrueMarkProductCodeOrderItem
				{
					TrueMarkProductCodeId = productCode.Id,
					OrderItemId = targetOrderItemsBySourceCode[sourceProductCode.Id].Id
				};

				uow.Save(productCodeOrderItem);

				createdProductCodes.Add(productCode);
			}

			return createdProductCodes;
		}

		private static Error CreateError(string fieldName, string message) =>
			new Error(typeof(CancelledOrderTrueMarkCodesTransferService), fieldName, message);
	}
}
