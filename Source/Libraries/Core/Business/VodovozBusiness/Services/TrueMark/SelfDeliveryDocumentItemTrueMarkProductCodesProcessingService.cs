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
using Vodovoz.Domain.Documents;
using NomenclatureErrors = Vodovoz.Errors.Goods.NomenclatureErrors;
using TrueMarkCodeErrors = Vodovoz.Errors.TrueMark.TrueMarkCodeErrors;

namespace VodovozBusiness.Services.TrueMark
{
	public class SelfDeliveryDocumentItemTrueMarkProductCodesProcessingService : ISelfDeliveryDocumentItemTrueMarkProductCodesProcessingService
	{
		private readonly IGenericRepository<StagingTrueMarkCode> _stagingTrueMarkCodeRepository;
		private readonly IGenericRepository<NomenclatureEntity> _nomenclatureRepository;
		private readonly IGenericRepository<GtinEntity> _gtinRepository;
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;

		public SelfDeliveryDocumentItemTrueMarkProductCodesProcessingService(
			IGenericRepository<StagingTrueMarkCode> stagingTrueMarkCodeRepository,
			IGenericRepository<NomenclatureEntity> nomenclatureRepository,
			IGenericRepository<GtinEntity> gtinRepository,
			ITrueMarkWaterCodeService trueMarkWaterCodeService)
		{
			_stagingTrueMarkCodeRepository =
				stagingTrueMarkCodeRepository ?? throw new ArgumentNullException(nameof(stagingTrueMarkCodeRepository));
			_nomenclatureRepository =
				nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_gtinRepository =
				gtinRepository ?? throw new ArgumentNullException(nameof(gtinRepository));
			_trueMarkWaterCodeService =
				trueMarkWaterCodeService ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
		}

		public async Task<IEnumerable<StagingTrueMarkCode>> GetStagingTrueMarkCodesBySelfDeliveryDocumentItem(
			IUnitOfWork uow,
			int selfDeliveryDocumentItemId,
			CancellationToken cancellationToken = default) =>
			await _trueMarkWaterCodeService.GetAllTrueMarkStagingCodesByRelatedDocument(
				uow,
				StagingTrueMarkCodeRelatedDocumentType.SelfDeliveryDocumentItem,
				selfDeliveryDocumentItemId,
				cancellationToken);

		public async Task AddTrueMarkAnyCodeToSelfDeliveryDocumentItemNoCodeStatusCheck(
			IUnitOfWork uow,
			SelfDeliveryDocumentItem selfDeliveryDocumentItem,
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

		public async Task<Result> AddProductCodesToSelfDeliveryDocumentItem(
			IUnitOfWork uow,
			SelfDeliveryDocumentItem selfDeliveryDocumentItem,
			IEnumerable<StagingTrueMarkCode> stagingCodes,
			CancellationToken cancellationToken = default)
		{
			if(!selfDeliveryDocumentItem.Nomenclature.IsAccountableInTrueMark)
			{
				throw new InvalidOperationException(
					"Коды ЧЗ можно добавить только к номенклатуре, которая подлежит учету в Честном Знаке");
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

			return Result.Success();
		}

		private async Task<Result> AddProductCodesToSelfDeliveryDocumentItemFromStagingCodes(
			IUnitOfWork uow,
			IEnumerable<StagingTrueMarkCode> stagingCodes,
			SelfDeliveryDocumentItem selfDeliveryDocumentItem,
			CancellationToken cancellationToken = default)
		{
			var trueMarkAnyCodesResult =
				await _trueMarkWaterCodeService.CreateTrueMarkAnyCodesFromStagingCodes(
					uow,
					stagingCodes.SelectMany(x => x.AllCodes),
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

		public async Task<Result<StagingTrueMarkCode>> CreateStagingTrueMarkCode(
			IUnitOfWork uow,
			string scannedCode,
			int selfDeliveryDocumentItemId,
			CancellationToken cancellationToken = default) =>
			await _trueMarkWaterCodeService.CreateStagingTrueMarkCode(
				uow,
				scannedCode,
				StagingTrueMarkCodeRelatedDocumentType.SelfDeliveryDocumentItem,
				selfDeliveryDocumentItemId,
				null,
				cancellationToken);

		public async Task<Result> IsStagingTrueMarkCodeCanBeAddedToItemOfNomenclature(
			IUnitOfWork uow,
			StagingTrueMarkCode stagingTrueMarkCode,
			int nomeclatureId,
			CancellationToken cancellationToken)
		{
			if(stagingTrueMarkCode.RelatedDocumentType != StagingTrueMarkCodeRelatedDocumentType.SelfDeliveryDocumentItem)
			{
				throw new InvalidOperationException("Только коды ЧЗ, отсканированные при отпуске самовывоза, могут быть добавлены");
			}

			var nomeclature = _nomenclatureRepository.GetFirstOrDefault(uow, x => x.Id == nomeclatureId);

			var codeCheckingProcessResult = IsNomeclatureAccountableInTrueMark(nomeclature);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			codeCheckingProcessResult = await IsNomeclatureGtinContainsCodeGtin(uow, stagingTrueMarkCode, nomeclatureId, cancellationToken);

			if(codeCheckingProcessResult.IsFailure)
			{
				return codeCheckingProcessResult;
			}

			return Result.Success();
		}

		public async Task<Result> IsStagingTrueMarkCodeAlreadyUsedInProductCodes(
			IUnitOfWork uow,
			StagingTrueMarkCode stagingTrueMarkCode,
			CancellationToken cancellationToken) =>
			await _trueMarkWaterCodeService.IsStagingTrueMarkCodeAlreadyUsedInProductCodes(uow, stagingTrueMarkCode, cancellationToken);

		private Result IsNomeclatureAccountableInTrueMark(NomenclatureEntity nomenclature)
		{
			if(!nomenclature.IsAccountableInTrueMark)
			{
				var error = NomenclatureErrors.CreateIsNotAccountableInTrueMark(nomenclature.Name);
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private async Task<Result> IsNomeclatureGtinContainsCodeGtin(
			IUnitOfWork uow,
			StagingTrueMarkCode stagingTrueMarkCode,
			int nomenclatureId,
			CancellationToken cancellationToken = default)
		{
			var nomenclatureGtins =
				(await _gtinRepository.GetAsync(uow, x => x.Nomenclature.Id == nomenclatureId, cancellationToken: cancellationToken))
				.Value
				.Select(x => x.GtinNumber);

			var codesGtin = stagingTrueMarkCode.AllIdentificationCodes
				.Select(x => x.Gtin)
				.FirstOrDefault();

			if(!nomenclatureGtins.Contains(codesGtin))
			{
				var codeNomeclatureName = await GetNomenclatureNameByGtin(uow, codesGtin, cancellationToken);

				var error = TrueMarkCodeErrors.CreateGtinNomenclatureNotFoundInOrder(codeNomeclatureName, codesGtin);
				return Result.Failure<StagingTrueMarkCode>(error);
			}

			return Result.Success();
		}

		public Result IsAllSelfDeliveryDocumentItemTrueMarkProductCodesAdded(SelfDeliveryDocumentItemEntity selfDeliveryDocumentItem)
		{
			var isAllTrueMarkCodesAdded = selfDeliveryDocumentItem.Amount == selfDeliveryDocumentItem.TrueMarkProductCodes.Count();

			if(!isAllTrueMarkCodesAdded)
			{
				return Result.Failure(TrueMarkCodeErrors.NotAllCodesAdded);
			}

			return Result.Success();
		}

		private async Task<string> GetNomenclatureNameByGtin(IUnitOfWork uow, string gtin, CancellationToken cancellationToken)
		{
			var nomenclatureName = (await _nomenclatureRepository
				.GetAsync(uow, x => x.Gtins.Any(g => g.GtinNumber == gtin), 1, cancellationToken))
				.Value
				.Select(x => x.Name)
				.FirstOrDefault() ?? string.Empty;
			return nomenclatureName;
		}
	}
}
