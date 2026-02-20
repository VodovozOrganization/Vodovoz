using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Errors.Goods;
using Vodovoz.Errors.Stores;
using Vodovoz.Errors.TrueMark;
using VodovozBusiness.Domain.Client.Specifications;
using VodovozBusiness.Services.TrueMark;

namespace WarehouseApi.Library.Services
{
	public class CarLoadDocumentTrueMarkCodesProcessingService : ICarLoadDocumentTrueMarkCodesProcessingService
	{
		private readonly IGenericRepository<StagingTrueMarkCode> _stagingTrueMarkCodeRepository;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;
		private readonly IGenericRepository<Order> _orderRepository;

		public CarLoadDocumentTrueMarkCodesProcessingService(
			IGenericRepository<StagingTrueMarkCode> stagingTrueMarkCodeRepository,
			ITrueMarkWaterCodeService trueMarkWaterCodeService,
			IGenericRepository<Order> orderRepository)
		{
			_stagingTrueMarkCodeRepository = stagingTrueMarkCodeRepository;
			_trueMarkWaterCodeService = trueMarkWaterCodeService ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
		}

		public async Task<Result> AddProductCodesToCarLoadDocumentAndDeleteStagingCodes(
			IUnitOfWork uow,
			CarLoadDocument carLoadDocument,
			CancellationToken cancellationToken = default)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(carLoadDocument is null)
			{
				throw new ArgumentNullException(nameof(carLoadDocument));
			}

			foreach(var carLoadDocumentItem in carLoadDocument.Items)
			{
				var addProductCodesResult =
					await AddProductCodesToCarLoadDocumentItemAndDeleteStagingCodes(
						uow,
						carLoadDocumentItem,
						cancellationToken);

				if(addProductCodesResult.IsFailure)
				{
					var error = addProductCodesResult.Errors.FirstOrDefault();
					return Result.Failure(error);
				}
			}

			var isAllTrueMarkCodesAddedResult =
				IsAllTrueMarkCodesInCarLoadDocumentAdded(uow, carLoadDocument);

			if(isAllTrueMarkCodesAddedResult.IsFailure)
			{
				var error = isAllTrueMarkCodesAddedResult.Errors.FirstOrDefault();
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private async Task<Result> AddProductCodesToCarLoadDocumentItemAndDeleteStagingCodes(
			IUnitOfWork uow,
			CarLoadDocumentItem carLoadDocumentItem,
			CancellationToken cancellationToken = default)
		{
			var stagingCodes =
				await _trueMarkWaterCodeService.GetAllTrueMarkStagingCodesByRelatedDocument(
				uow,
				StagingTrueMarkCodeRelatedDocumentType.CarLoadDocumentItem,
				carLoadDocumentItem.Id,
				cancellationToken);

			var addProductCodesResult =
				await AddProductCodesToCarLoadDocumentItemFromStagingCodes(
					uow,
					stagingCodes,
					carLoadDocumentItem,
					cancellationToken);

			if(addProductCodesResult.IsFailure)
			{
				var error = addProductCodesResult.Errors.FirstOrDefault();
				return Result.Failure(error);
			}

			var deleteStagingCodesResult =
				await _trueMarkWaterCodeService.DeleteAllTrueMarkStagingCodesByRelatedDocument(
					uow,
					StagingTrueMarkCodeRelatedDocumentType.CarLoadDocumentItem,
					carLoadDocumentItem.Id,
					cancellationToken);

			if(deleteStagingCodesResult.IsFailure)
			{
				var error = deleteStagingCodesResult.Errors.FirstOrDefault();
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private Result IsAllTrueMarkCodesInCarLoadDocumentAdded(IUnitOfWork uow, CarLoadDocument carLoadDocument)
		{
			var cancelledOrdersIds = GetCarLoadDocumentCancelledOrders(uow, carLoadDocument);

			var isNotAllCodesAdded = carLoadDocument.Items
				.Where(x =>
					x.OrderId != null
					&& !cancelledOrdersIds.Contains(x.OrderId.Value)
					&& x.Nomenclature.IsAccountableInTrueMark
					&& x.Nomenclature.Gtin != null)
				.Any(x => x.TrueMarkCodes.Count < x.Amount);

			if(isNotAllCodesAdded)
			{
				var error = CarLoadDocumentErrors.CreateNotAllTrueMarkCodesWasAddedIntoCarLoadDocument(carLoadDocument.Id);
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private IEnumerable<int> GetCarLoadDocumentCancelledOrders(IUnitOfWork uow, CarLoadDocument carLoadDocument)
		{
			var ordersInDocument = carLoadDocument.Items
				.Where(x => x.OrderId != null)
				.Select(x => x.OrderId.Value)
				.Distinct()
				.ToList();

			var undeliveredStatuses = new OrderStatus[]
			{
				OrderStatus.NotDelivered,
				OrderStatus.DeliveryCanceled,
				OrderStatus.Canceled
			};

			var cancelledOrders =
				_orderRepository.Get(uow, o => ordersInDocument.Contains(o.Id) && undeliveredStatuses.Contains(o.OrderStatus));

			return cancelledOrders.Select(o => o.Id);
		}

		private async Task<Result> AddProductCodesToCarLoadDocumentItemFromStagingCodes(
			IUnitOfWork uow,
			IEnumerable<StagingTrueMarkCode> stagingCodes,
			CarLoadDocumentItem routeListItem,
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
				await AddTrueMarkAnyCodeToCarLoadDocumentItemNoCodeStatusCheck(
					uow,
					routeListItem,
					trueMarkAnyCode,
					SourceProductCodeStatus.Accepted,
					ProductCodeProblem.None,
					cancellationToken);
			}

			return Result.Success();
		}

		private async Task AddTrueMarkAnyCodeToCarLoadDocumentItemNoCodeStatusCheck(
			IUnitOfWork uow,
			CarLoadDocumentItem carLoadDocumentItem,
			TrueMarkAnyCode trueMarkAnyCode,
			SourceProductCodeStatus status,
			ProductCodeProblem problem,
			CancellationToken cancellationToken = default)
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
						carLoadDocumentItem.TrueMarkCodes.Any(x =>
						x.SourceCode.Gtin == code.TrueMarkWaterIdentificationCode.Gtin
						&& x.SourceCode.SerialNumber == code.TrueMarkWaterIdentificationCode.SerialNumber);

					if(!isCodeAlreadyAddedToRouteListItem)
					{
						AddTrueMarkCodeToCarLoadDocumentItem(
							uow,
							carLoadDocumentItem,
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

		private void AddTrueMarkCodeToCarLoadDocumentItem(
			IUnitOfWork uow,
			CarLoadDocumentItem carLoadDocumentItem,
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode,
			SourceProductCodeStatus status,
			ProductCodeProblem problem)
		{
			var productCode = CreateCarLoadDocumentItemTrueMarkProductCode(
				carLoadDocumentItem,
				trueMarkWaterIdentificationCode,
				status,
				problem);

			carLoadDocumentItem.TrueMarkCodes.Add(productCode);
			uow.Save(productCode);
		}

		private CarLoadDocumentItemTrueMarkProductCode CreateCarLoadDocumentItemTrueMarkProductCode(
			CarLoadDocumentItem carLoadDocumentItem,
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode,
			SourceProductCodeStatus status,
			ProductCodeProblem problem) =>
			new CarLoadDocumentItemTrueMarkProductCode()
			{
				CreationTime = DateTime.Now,
				SourceCodeStatus = status,
				SourceCode = trueMarkWaterIdentificationCode,
				ResultCode = status == SourceProductCodeStatus.Accepted ? trueMarkWaterIdentificationCode : default,
				CarLoadDocumentItem = carLoadDocumentItem,
				Problem = problem
			};

		public async Task<Result<StagingTrueMarkCode>> AddStagingTrueMarkCode(
			IUnitOfWork uow,
			string scannedCode,
			CarLoadDocumentItem carLoadDocumentItem,
			CancellationToken cancellationToken = default)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(carLoadDocumentItem is null)
			{
				throw new ArgumentNullException(nameof(carLoadDocumentItem));
			}

			var createCodeResult =
				await _trueMarkWaterCodeService.CreateStagingTrueMarkCode(
					uow,
					scannedCode,
					StagingTrueMarkCodeRelatedDocumentType.CarLoadDocumentItem,
					carLoadDocumentItem.Id,
					null,
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
					carLoadDocumentItem,
					cancellationToken);

			if(isCodeCanBeAddedResult.IsFailure)
			{
				var error = isCodeCanBeAddedResult.Errors.FirstOrDefault();
				return Result.Failure<StagingTrueMarkCode>(error);
			}

			await uow.SaveAsync(stagingTrueMarkCode, cancellationToken: cancellationToken);

			return Result.Success(stagingTrueMarkCode);
		}

		public async Task<Result<StagingTrueMarkCode>> ChangeStagingTrueMarkCode(
			IUnitOfWork uow,
			string newScannedCode,
			string oldScannedCode,
			CarLoadDocumentItem carLoadDocumentItem,
			CancellationToken cancellationToken = default)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(carLoadDocumentItem is null)
			{
				throw new ArgumentNullException(nameof(carLoadDocumentItem));
			}

			var removeCodeResult =
				await RemoveStagingTrueMarkCode(
					uow,
					oldScannedCode,
					carLoadDocumentItem.Id,
					cancellationToken);

			if(removeCodeResult.IsFailure)
			{
				return removeCodeResult;
			}

			var createCodeResult =
				await _trueMarkWaterCodeService.CreateStagingTrueMarkCode(
					uow,
					newScannedCode,
					StagingTrueMarkCodeRelatedDocumentType.CarLoadDocumentItem,
					carLoadDocumentItem.Id,
					null,
					cancellationToken);

			if(createCodeResult.IsFailure)
			{
				return createCodeResult;
			}

			var newStagingCode = createCodeResult.Value;
			var removedStagingCode = removeCodeResult.Value;

			var isCodeCanBeAddedResult =
				await IsStagingTrueMarkCodeCanBeChanged(
					uow,
					newStagingCode,
					removedStagingCode,
					carLoadDocumentItem,
					cancellationToken);

			if(isCodeCanBeAddedResult.IsFailure)
			{
				var error = isCodeCanBeAddedResult.Errors.FirstOrDefault();
				return Result.Failure<StagingTrueMarkCode>(error);
			}

			await uow.SaveAsync(newStagingCode, cancellationToken: cancellationToken);

			return Result.Success(newStagingCode);
		}

		public async Task<Result<StagingTrueMarkCode>> RemoveStagingTrueMarkCode(
			IUnitOfWork uow,
			string scannedCode,
			int carLoadDocumentItemId,
			CancellationToken cancellationToken = default)
		{
			var existingCodeResult =
				_trueMarkWaterCodeService.GetSavedStagingTrueMarkCodeByScannedCode(
					uow,
					scannedCode,
					StagingTrueMarkCodeRelatedDocumentType.CarLoadDocumentItem,
					carLoadDocumentItemId,
					null);

			if(existingCodeResult.IsFailure)
			{
				var error = existingCodeResult.Errors.FirstOrDefault();
				return Result.Failure<StagingTrueMarkCode>(error);
			}

			var codeToRemove = existingCodeResult.Value;

			if(codeToRemove.ParentCodeId != null)
			{
				var error = TrueMarkCodeErrors.AggregatedCode;
				return Result.Failure<StagingTrueMarkCode>(error);
			}

			await uow.DeleteAsync(codeToRemove, cancellationToken: cancellationToken);

			return existingCodeResult;
		}

		private async Task<Result> IsStagingTrueMarkCodeCanBeAdded(
			IUnitOfWork uow,
			StagingTrueMarkCode stagingTrueMarkCode,
			CarLoadDocumentItem carLoadDocumentItem,
			CancellationToken cancellationToken)
		{
			if(stagingTrueMarkCode.RelatedDocumentType != StagingTrueMarkCodeRelatedDocumentType.CarLoadDocumentItem)
			{
				throw new InvalidOperationException("Только коды ЧЗ, отсканированные в складском приложении, могут быть добавлены");
			}

			var codeCheckingProcessResult = IsNomeclatureAccountableInTrueMark(carLoadDocumentItem.Nomenclature);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			codeCheckingProcessResult = IsNomeclatureGtinContainsCodeGtin(stagingTrueMarkCode, carLoadDocumentItem.Nomenclature);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			codeCheckingProcessResult = IsCarLoadDocumentItemHaveNoAddedCodes(carLoadDocumentItem);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			codeCheckingProcessResult = IsStagingTrueMarkCodesCountCanBeAdded(uow, stagingTrueMarkCode, carLoadDocumentItem);

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

		private async Task<Result> IsStagingTrueMarkCodeCanBeChanged(
			IUnitOfWork uow,
			StagingTrueMarkCode newStagingTrueMarkCode,
			StagingTrueMarkCode oldStagingTrueMarkCode,
			CarLoadDocumentItem carLoadDocumentItem,
			CancellationToken cancellationToken)
		{
			if(newStagingTrueMarkCode.RelatedDocumentType != StagingTrueMarkCodeRelatedDocumentType.CarLoadDocumentItem)
			{
				throw new InvalidOperationException("Только коды ЧЗ, отсканированные в складском приложении, могут быть добавлены");
			}

			var codeCheckingProcessResult = IsNomeclatureAccountableInTrueMark(carLoadDocumentItem.Nomenclature);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			codeCheckingProcessResult = IsNomeclatureGtinContainsCodeGtin(newStagingTrueMarkCode, carLoadDocumentItem.Nomenclature);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			codeCheckingProcessResult = IsCarLoadDocumentItemHaveNoAddedCodes(carLoadDocumentItem);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			codeCheckingProcessResult =
				await IsStagingTrueMarkCodesCountCanBeChanged(uow, newStagingTrueMarkCode, oldStagingTrueMarkCode, carLoadDocumentItem);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			codeCheckingProcessResult =
				await _trueMarkWaterCodeService.IsStagingTrueMarkCodeAlreadyUsedInProductCodes(uow, newStagingTrueMarkCode, cancellationToken);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			return Result.Success();
		}

		private Result IsCarLoadDocumentItemHaveNoAddedCodes(CarLoadDocumentItem carLoadDocumentItem)
		{
			if(carLoadDocumentItem?.TrueMarkCodes.Count > 0)
			{
				var error = TrueMarkCodeErrors.RelatedDocumentHasTrueMarkCodes;
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private Result IsStagingTrueMarkCodesCountCanBeAdded(
			IUnitOfWork uow,
			StagingTrueMarkCode stagingTrueMarkCode,
			CarLoadDocumentItem carLoadDocumentItem)
		{
			var carLoadDocumentItemId = stagingTrueMarkCode.RelatedDocumentId;

			var addedStagingCodesCount = _stagingTrueMarkCodeRepository.GetCount(
				uow,
				StagingTrueMarkCodeSpecification.CreateForRelatedDocumentOrderItemIdentificationCodesExcludeIds(
					StagingTrueMarkCodeRelatedDocumentType.CarLoadDocumentItem,
					carLoadDocumentItemId,
					null,
					stagingTrueMarkCode.AllIdentificationCodes.Select(c => c.Id)));

			var newStagingCodesCount = stagingTrueMarkCode.AllIdentificationCodes.Count;

			var isCodeCanBeAdded =
				addedStagingCodesCount + newStagingCodesCount <= carLoadDocumentItem.Amount;

			if(!isCodeCanBeAdded)
			{
				var error = TrueMarkCodeErrors.TrueMarkCodesCountMoreThenInOrderItem;
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private async Task<Result> IsStagingTrueMarkCodesCountCanBeChanged(
			IUnitOfWork uow,
			StagingTrueMarkCode newStagingTrueMarkCode,
			StagingTrueMarkCode oldStagingTrueMarkCode,
			CarLoadDocumentItem carLoadDocumentItem)
		{
			var carLoadDocumentItemId = newStagingTrueMarkCode.RelatedDocumentId;
			var allRemoveingCodes = oldStagingTrueMarkCode.AllIdentificationCodes.Select(c => c.Id).ToList();

			var addedStagingCodesCount = (await _stagingTrueMarkCodeRepository.GetAsync(
				uow,
				StagingTrueMarkCodeSpecification.CreateForRelatedDocumentOrderItemIdentificationCodesExcludeIds(
					StagingTrueMarkCodeRelatedDocumentType.CarLoadDocumentItem,
					carLoadDocumentItemId,
					null,
					newStagingTrueMarkCode.AllIdentificationCodes.Select(c => c.Id))))
				.Value
				.Where(x => !allRemoveingCodes.Contains(x.Id))
				.Count();

			var newStagingCodesCount = newStagingTrueMarkCode.AllIdentificationCodes.Count;

			var isCodeCanBeAdded =
				addedStagingCodesCount + newStagingCodesCount <= carLoadDocumentItem.Amount;

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
	}
}
