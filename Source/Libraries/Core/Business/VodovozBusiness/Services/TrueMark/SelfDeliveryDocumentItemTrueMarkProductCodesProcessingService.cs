using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using VodovozBusiness.Domain.Client.Specifications;
using NomenclatureErrors = Vodovoz.Errors.Goods.Nomenclature;
using TrueMarkCodeErrors = Vodovoz.Errors.TrueMark.TrueMarkCode;

namespace VodovozBusiness.Services.TrueMark
{
	public class SelfDeliveryDocumentItemTrueMarkProductCodesProcessingService : ISelfDeliveryDocumentItemTrueMarkProductCodesProcessingService
	{
		private readonly IGenericRepository<StagingTrueMarkCode> _stagingTrueMarkCodeRepository;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;

		public SelfDeliveryDocumentItemTrueMarkProductCodesProcessingService(
			IGenericRepository<StagingTrueMarkCode> stagingTrueMarkCodeRepository,
			ITrueMarkWaterCodeService trueMarkWaterCodeService)
		{
			_stagingTrueMarkCodeRepository =
				stagingTrueMarkCodeRepository ?? throw new ArgumentNullException(nameof(stagingTrueMarkCodeRepository));
			_trueMarkWaterCodeService =
				trueMarkWaterCodeService ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
		}

		public async Task AddTrueMarkAnyCodeToSelfDeliveryDocumentItemNoCodeStatusCheck(
			IUnitOfWork uow,
			SelfDeliveryDocumentItemEntity selfDeliveryDocumentItem,
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
					var isCodeAlreadyAdded =
						selfDeliveryDocumentItem.TrueMarkProductCodes.Any(x =>
						x.SourceCode.Gtin == code.TrueMarkWaterIdentificationCode.Gtin
						&& x.SourceCode.SerialNumber == code.TrueMarkWaterIdentificationCode.SerialNumber);

					if(!isCodeAlreadyAdded)
					{
						AddTrueMarkCodeToSelfDeliveryDocumentItem(
							uow,
							selfDeliveryDocumentItem,
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

		private void AddTrueMarkCodeToSelfDeliveryDocumentItem(
			IUnitOfWork uow,
			SelfDeliveryDocumentItemEntity selfDeliveryDocumentItem,
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode,
			SourceProductCodeStatus status,
			ProductCodeProblem problem)
		{
			var productCode = CreateSelfDeliveryDocumentItemTrueMarkProductCode(
				selfDeliveryDocumentItem,
				trueMarkWaterIdentificationCode,
				status,
				problem);

			selfDeliveryDocumentItem.TrueMarkProductCodes.Add(productCode);
			uow.Save(productCode);
		}

		private SelfDeliveryDocumentItemTrueMarkProductCode CreateSelfDeliveryDocumentItemTrueMarkProductCode(
			SelfDeliveryDocumentItemEntity selfDeliveryDocumentItem,
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode,
			SourceProductCodeStatus status,
			ProductCodeProblem problem) =>
			new SelfDeliveryDocumentItemTrueMarkProductCode()
			{
				CreationTime = DateTime.Now,
				SourceCodeStatus = status,
				SourceCode = trueMarkWaterIdentificationCode,
				ResultCode = status == SourceProductCodeStatus.Accepted ? trueMarkWaterIdentificationCode : default,
				SelfDeliveryDocumentItem = selfDeliveryDocumentItem,
				Problem = problem
			};

		public async Task<Result> AddProductCodesToSelfDeliveryDocumentItemAndDeleteStagingCodes(
			IUnitOfWork uow,
			SelfDeliveryDocumentItemEntity selfDeliveryDocumentItem,
			CancellationToken cancellationToken = default)
		{
			if(!selfDeliveryDocumentItem.Nomenclature.IsAccountableInTrueMark)
			{
				throw new InvalidOperationException(
					"Коды ЧЗ можно добавить только к номенклатуре, которая подлежит учету в Честном Знаке");
			}

			var stagingCodes =
				await _trueMarkWaterCodeService.GetAllTrueMarkStagingCodesByRelatedDocument(
				uow,
				StagingTrueMarkCodeRelatedDocumentType.SelfDeliveryDocumentItem,
				selfDeliveryDocumentItem.Id,
				cancellationToken);

			var identificationStagingCodesCount = stagingCodes
				.Count(x => x.CodeType == StagingTrueMarkCodeType.Identification);

			if(identificationStagingCodesCount < selfDeliveryDocumentItem.Amount)
			{
				var error = TrueMarkCodeErrors.NotAllCodesAdded;
				return Result.Failure(error);
			}

			if(identificationStagingCodesCount > selfDeliveryDocumentItem.Amount)
			{
				var error = TrueMarkCodeErrors.TrueMarkCodesCountMoreThenInOrderItem;
				return Result.Failure(error);
			}

			var addProductCodesResult =
				await AddProductCodesToSelfDeliveryDocumentItemFromStagingCodes(
					uow,
					stagingCodes,
					selfDeliveryDocumentItem,
					cancellationToken);

			if(addProductCodesResult.IsFailure)
			{
				var error = addProductCodesResult.Errors.FirstOrDefault();
				return Result.Failure(error);
			}

			var deleteStagingCodesResult =
				await _trueMarkWaterCodeService.DeleteAllTrueMarkStagingCodesByRelatedDocument(
					uow,
					StagingTrueMarkCodeRelatedDocumentType.SelfDeliveryDocumentItem,
					selfDeliveryDocumentItem.Id,
					cancellationToken);

			if(deleteStagingCodesResult.IsFailure)
			{
				var error = deleteStagingCodesResult.Errors.FirstOrDefault();
				return Result.Failure(error);
			}

			var allCodesAddedToOrderResult =
				IsAllSelfDeliveryDocumentItemTrueMarkProductCodesAdded(selfDeliveryDocumentItem);

			if(allCodesAddedToOrderResult.IsFailure)
			{
				var error = allCodesAddedToOrderResult.Errors.FirstOrDefault();
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private async Task<Result> AddProductCodesToSelfDeliveryDocumentItemFromStagingCodes(
			IUnitOfWork uow,
			IEnumerable<StagingTrueMarkCode> stagingCodes,
			SelfDeliveryDocumentItemEntity selfDeliveryDocumentItem,
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
				await AddTrueMarkAnyCodeToSelfDeliveryDocumentItemNoCodeStatusCheck(
					uow,
					selfDeliveryDocumentItem,
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
			SelfDeliveryDocumentItemEntity selfDeliveryDocumentItem,
			CancellationToken cancellationToken = default)
		{
			var createCodeResult =
				await _trueMarkWaterCodeService.CreateStagingTrueMarkCode(
					uow,
					scannedCode,
					StagingTrueMarkCodeRelatedDocumentType.SelfDeliveryDocumentItem,
					selfDeliveryDocumentItem.Id,
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
					selfDeliveryDocumentItem,
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
			SelfDeliveryDocumentItemEntity selfDeliveryDocumentItem,
			CancellationToken cancellationToken = default)
		{
			var existingCodeResult =
				_trueMarkWaterCodeService.GetSavedStagingTrueMarkCodeByScannedCode(
					uow,
					scannedCode,
					StagingTrueMarkCodeRelatedDocumentType.SelfDeliveryDocumentItem,
					selfDeliveryDocumentItem.Id,
					null);

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
			SelfDeliveryDocumentItemEntity selfDeliveryDocumentItem,
			CancellationToken cancellationToken)
		{
			if(stagingTrueMarkCode.RelatedDocumentType != StagingTrueMarkCodeRelatedDocumentType.SelfDeliveryDocumentItem)
			{
				throw new InvalidOperationException("Только коды ЧЗ, отсканированные при отпуске самовывоза, могут быть добавлены");
			}

			var codeCheckingProcessResult = IsNomeclatureAccountableInTrueMark(selfDeliveryDocumentItem.Nomenclature);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			codeCheckingProcessResult = IsNomeclatureGtinContainsCodeGtin(stagingTrueMarkCode, selfDeliveryDocumentItem.Nomenclature);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			codeCheckingProcessResult = IsSelfDeliveryDocumentItemHasNoAddedTrueMarkCodes(selfDeliveryDocumentItem);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			codeCheckingProcessResult = IsStagingTrueMarkCodesCountCanBeAdded(uow, stagingTrueMarkCode, selfDeliveryDocumentItem);

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

		private Result IsSelfDeliveryDocumentItemHasNoAddedTrueMarkCodes(SelfDeliveryDocumentItemEntity selfDeliveryDocumentItemEntity)
		{
			if(selfDeliveryDocumentItemEntity.TrueMarkProductCodes.Count > 0)
			{
				var error = TrueMarkCodeErrors.RelatedDocumentHasTrueMarkCodes;
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private Result IsStagingTrueMarkCodesCountCanBeAdded(
			IUnitOfWork uow,
			StagingTrueMarkCode stagingTrueMarkCode,
			SelfDeliveryDocumentItemEntity selfDeliveryDocumentItem)
		{
			var addedStagingCodesCount = _stagingTrueMarkCodeRepository.GetCount(
				uow,
				StagingTrueMarkCodeSpecification.CreateForRelatedDocumentOrderItemIdentificationCodesExcludeIds(
					StagingTrueMarkCodeRelatedDocumentType.SelfDeliveryDocumentItem,
					selfDeliveryDocumentItem.Id,
					null,
					stagingTrueMarkCode.AllIdentificationCodes.Select(c => c.Id)));

			var newStagingCodesCount = stagingTrueMarkCode.AllIdentificationCodes.Count;

			var isCodeCanBeAdded = addedStagingCodesCount + newStagingCodesCount <= selfDeliveryDocumentItem.Amount;

			if(!isCodeCanBeAdded)
			{
				var error = TrueMarkCodeErrors.TrueMarkCodesCountMoreThenInOrderItem;
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private Result IsNomeclatureAccountableInTrueMark(NomenclatureEntity nomenclature)
		{
			if(!nomenclature.IsAccountableInTrueMark)
			{
				var error = NomenclatureErrors.CreateIsNotAccountableInTrueMark(nomenclature.Name);
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private Result IsNomeclatureGtinContainsCodeGtin(StagingTrueMarkCode stagingTrueMarkCode, NomenclatureEntity nomenclature)
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

		private Result IsAllSelfDeliveryDocumentItemTrueMarkProductCodesAdded(SelfDeliveryDocumentItemEntity selfDeliveryDocumentItem)
		{
			var isAllTrueMarkCodesAdded = selfDeliveryDocumentItem.Amount == selfDeliveryDocumentItem.TrueMarkProductCodes.Count();

			if(!isAllTrueMarkCodesAdded)
			{
				return Result.Failure(TrueMarkCodeErrors.NotAllCodesAdded);
			}

			return Result.Success();
		}
	}
}
