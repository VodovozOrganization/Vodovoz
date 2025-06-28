using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using TrueMark.Contracts.Responses;
using TrueMarkApi.Client;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Models.TrueMark;
using Vodovoz.Settings.Edo;
using VodovozBusiness.Services.TrueMark;
using TrueMarkCodeErrors = Vodovoz.Errors.TrueMark.TrueMarkCode;

namespace Vodovoz.Application.TrueMark
{
	public class TrueMarkWaterCodeService : ITrueMarkWaterCodeService
	{
		private static IList<SourceProductCodeStatus> _successfullyUsedProductCodesStatuses = new List<SourceProductCodeStatus>
		{
			SourceProductCodeStatus.Accepted,
			SourceProductCodeStatus.Changed
		};

		private readonly ILogger<TrueMarkWaterCodeService> _logger;
		private readonly IUnitOfWork _uow;
		private readonly ITrueMarkApiClient _trueMarkApiClient;
		private readonly TrueMarkCodesChecker _trueMarkCodesChecker;
		private readonly TrueMarkWaterCodeParser _trueMarkWaterCodeParser;
		private readonly ITrueMarkTransportCodeFactory _trueMarkTransportCodeFactory;
		private readonly ITrueMarkWaterGroupCodeFactory _trueMarkWaterGroupCodeFactory;
		private readonly ITrueMarkWaterIdentificationCodeFactory _trueMarkWaterIdentificationCodeFactory;
		private readonly IStagingTrueMarkCodeFactory _stagingTrueMarkCodeFactory;
		private readonly IGenericRepository<TrueMarkWaterIdentificationCode> _trueMarkWaterIdentificationCodeRepository;
		private readonly IGenericRepository<TrueMarkProductCode> _trueMarkProductCodeRepository;
		private readonly IGenericRepository<TrueMarkWaterGroupCode> _trueMarkWaterGroupCodeRepository;
		private readonly IGenericRepository<TrueMarkTransportCode> _trueMarkTransportCodeRepository;
		private readonly IGenericRepository<StagingTrueMarkCode> _stagingTrueMarkCodeRepository;
		private readonly IGenericRepository<OrganizationEntity> _organizationRepository;
		private readonly IEdoSettings _edoSettings;

		private IList<string> _organizationsInns;

		public TrueMarkWaterCodeService(
			ILogger<TrueMarkWaterCodeService> logger,
			IUnitOfWork uow,
			ITrueMarkApiClient trueMarkApiClient,
			TrueMarkCodesChecker trueMarkCodesChecker,
			TrueMarkWaterCodeParser trueMarkWaterCodeParser,
			ITrueMarkTransportCodeFactory trueMarkTransportCodeFactory,
			ITrueMarkWaterGroupCodeFactory trueMarkWaterGroupCodeFactory,
			ITrueMarkWaterIdentificationCodeFactory trueMarkWaterIdentificationCodeFactory,
			IStagingTrueMarkCodeFactory stagingTrueMarkCodeFactory,
			IGenericRepository<TrueMarkWaterIdentificationCode> trueMarkWaterIdentificationCodeRepository,
			IGenericRepository<TrueMarkProductCode> trueMarkProductCodeRepository,
			IGenericRepository<TrueMarkWaterGroupCode> trueMarkWaterGroupCodeRepository,
			IGenericRepository<TrueMarkTransportCode> trueMarkTransportCodeRepository,
			IGenericRepository<StagingTrueMarkCode> stagingTrueMarkCodeRepository,
			IGenericRepository<OrganizationEntity> organizationRepository,
			IEdoSettings edoSettings)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_uow = uow
				?? throw new ArgumentNullException(nameof(uow));
			_trueMarkApiClient = trueMarkApiClient
				?? throw new ArgumentNullException(nameof(trueMarkApiClient));
			_trueMarkCodesChecker = trueMarkCodesChecker
				?? throw new ArgumentNullException(nameof(trueMarkCodesChecker));
			_trueMarkWaterCodeParser = trueMarkWaterCodeParser
				?? throw new ArgumentNullException(nameof(trueMarkWaterCodeParser));
			_trueMarkTransportCodeFactory = trueMarkTransportCodeFactory
				?? throw new ArgumentNullException(nameof(trueMarkTransportCodeFactory));
			_trueMarkWaterGroupCodeFactory = trueMarkWaterGroupCodeFactory
				?? throw new ArgumentNullException(nameof(trueMarkWaterGroupCodeFactory));
			_trueMarkWaterIdentificationCodeFactory = trueMarkWaterIdentificationCodeFactory
				?? throw new ArgumentNullException(nameof(trueMarkWaterIdentificationCodeFactory));
			_stagingTrueMarkCodeFactory = stagingTrueMarkCodeFactory
				?? throw new ArgumentNullException(nameof(stagingTrueMarkCodeFactory));
			_trueMarkWaterIdentificationCodeRepository = trueMarkWaterIdentificationCodeRepository
				?? throw new ArgumentNullException(nameof(trueMarkWaterIdentificationCodeRepository));
			_trueMarkProductCodeRepository = trueMarkProductCodeRepository
				?? throw new ArgumentNullException(nameof(trueMarkProductCodeRepository));
			_trueMarkWaterGroupCodeRepository = trueMarkWaterGroupCodeRepository
				?? throw new ArgumentNullException(nameof(trueMarkWaterGroupCodeRepository));
			_trueMarkTransportCodeRepository = trueMarkTransportCodeRepository
				?? throw new ArgumentNullException(nameof(trueMarkTransportCodeRepository));
			_stagingTrueMarkCodeRepository = stagingTrueMarkCodeRepository
				?? throw new ArgumentNullException(nameof(stagingTrueMarkCodeRepository));
			_organizationRepository = organizationRepository
				?? throw new ArgumentNullException(nameof(organizationRepository));
			_edoSettings = edoSettings
				?? throw new ArgumentNullException(nameof(edoSettings));
		}

		public IList<SourceProductCodeStatus> SuccessfullyUsedProductCodesStatuses =>
			_successfullyUsedProductCodesStatuses;

		public TrueMarkWaterIdentificationCode LoadOrCreateTrueMarkWaterIdentificationCode(IUnitOfWork uow, string scannedCode)
		{
			var isScannedCodeValid = _trueMarkWaterCodeParser.TryParse(scannedCode, out TrueMarkWaterCode parsedCode);

			TrueMarkWaterIdentificationCode codeEntity;

			if(isScannedCodeValid)
			{
				if(!TryLoadCode(uow, parsedCode.SourceCode, out codeEntity))
				{
					codeEntity = new TrueMarkWaterIdentificationCode
					{
						IsInvalid = false,
						RawCode = parsedCode.SourceCode.Substring(0, Math.Min(255, parsedCode.SourceCode.Length)),
						GTIN = parsedCode.GTIN,
						SerialNumber = parsedCode.SerialNumber,
						CheckCode = parsedCode.CheckCode
					};
				}
			}
			else
			{
				if(!TryLoadCode(uow, scannedCode, out codeEntity))
				{
					codeEntity = new TrueMarkWaterIdentificationCode
					{
						IsInvalid = true,
						RawCode = scannedCode.Substring(0, Math.Min(255, scannedCode.Length)),
					};
				}
			}


			return codeEntity;
		}

		private bool TryLoadCode(IUnitOfWork uow, string code, out TrueMarkWaterIdentificationCode codeEntity)
		{
			codeEntity = _trueMarkWaterIdentificationCodeRepository
				.Get(uow, x => x.RawCode == code)
				.FirstOrDefault();

			return codeEntity != null;
		}

		public async Task<Result> IsTrueMarkCodeIntroducedAndHasCorrectInn(
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode,
			CancellationToken cancellationToken)
		{
			if(trueMarkWaterIdentificationCode is null)
			{
				throw new ArgumentNullException(nameof(trueMarkWaterIdentificationCode));
			}

			var result = await IsAllTrueMarkCodesIntroducedAndHasCorrectInns(
				new List<TrueMarkWaterIdentificationCode> { trueMarkWaterIdentificationCode },
				cancellationToken);

			return result;
		}

		public async Task<Result> IsAllTrueMarkCodesIntroducedAndHasCorrectInns(
			IEnumerable<TrueMarkWaterIdentificationCode> trueMarkWaterIdentificationCodes,
			CancellationToken cancellationToken)
		{
			if(trueMarkWaterIdentificationCodes is null)
			{
				throw new ArgumentNullException(nameof(trueMarkWaterIdentificationCodes));
			}

			try
			{
				var checkResults = await _trueMarkCodesChecker.CheckCodesAsync(
					trueMarkWaterIdentificationCodes,
					cancellationToken);

				var result = IsAllCodesIntroduced(checkResults);

				if(result.IsFailure)
				{
					return result;
				}

				result = IsAllCodesHasCorrectInn(checkResults);

				if(result.IsFailure)
				{
					return result;
				}

				return Result.Success();
			}
			catch(TrueMarkException ex)
			{
				var error = TrueMarkCodeErrors.CreateTrueMarkApiRequestError(
					$"Запрос к API ЧЗ для проверки кода вернул ответ с ошибкой. " +
					$"{ex.Message}");
				_logger.LogError(ex, error.Message);
				return Result.Failure(error);
			}
			catch(Exception ex)
			{
				var error = TrueMarkCodeErrors.CreateTrueMarkApiRequestError(
					"При выполнении запроса к API ЧЗ для проверки кода возникла непредвиденная ошибка. " +
					"Обратитесь в техподдержку");
				_logger.LogError(ex, error.Message);
				return Result.Failure(error);
			}
		}

		private Result IsAllCodesIntroduced(IEnumerable<TrueMarkCheckResult> checkResults)
		{
			foreach(var checkResult in checkResults)
			{
				if(checkResult.Introduced)
				{
					continue;
				}

				var error = TrueMarkCodeErrors.TrueMarkCodeIsNotIntroduced;
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private Result IsAllCodesHasCorrectInn(IEnumerable<TrueMarkCheckResult> checkResults)
		{
			if(_organizationsInns is null)
			{
				SetOrganizationInns();
			}

			foreach(var checkResult in checkResults)
			{
				if(_organizationsInns.Contains(checkResult.OwnerInn))
				{
					continue;
				}

				var error = TrueMarkCodeErrors.CreateTrueMarkCodeOwnerInnIsNotCorrect(checkResult.OwnerInn);
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private void SetOrganizationInns()
		{
			_organizationsInns =
				_organizationRepository
				.Get(_uow, x => _edoSettings.OrganizationsHavingAccountsInTrueMark.Contains(x.Id)).Select(x => x.INN)
				.ToList();
		}

		public Result IsTrueMarkWaterIdentificationCodeNotUsed(
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode,
			int exceptProductCodeId = 0)
		{
			if(trueMarkWaterIdentificationCode is null)
			{
				throw new ArgumentNullException(nameof(trueMarkWaterIdentificationCode));
			}

			var isCodeAlreadyUsed =
				TrueMarkProductCodesHavingRequiredResultCode(trueMarkWaterIdentificationCode.Id, exceptProductCodeId)
				.Any();

			if(isCodeAlreadyUsed)
			{
				var error = TrueMarkCodeErrors.CreateTrueMarkCodeIsAlreadyUsed(trueMarkWaterIdentificationCode.Id);
				return Result.Failure(error);
			}

			return Result.Success();
		}

		private IEnumerable<TrueMarkProductCode> TrueMarkProductCodesHavingRequiredResultCode(int resultCodeId, int exceptProductCodeId = 0) =>
			_trueMarkProductCodeRepository.Get(
				_uow,
				x => x.ResultCode.Id == resultCodeId
					&& _successfullyUsedProductCodesStatuses.Contains(x.SourceCodeStatus)
				&& (exceptProductCodeId == 0 || x.Id != exceptProductCodeId));

		public Result<TrueMarkAnyCode> TryGetSavedTrueMarkCodeByScannedCode(string scannedCode)
			=> TryGetSavedTrueMarkCodeByScannedCode(_uow, scannedCode);

		public Result<TrueMarkAnyCode> TryGetSavedTrueMarkCodeByScannedCode(IUnitOfWork uow, string scannedCode)
		{
			// Проверяем КИ
			if(_trueMarkWaterIdentificationCodeRepository
				.Get(
					uow,
					x => x.RawCode == scannedCode && !x.IsInvalid,
					1)
				.FirstOrDefault() is TrueMarkWaterIdentificationCode loadedIdentificationCode)
			{
				return Result.Success<TrueMarkAnyCode>(loadedIdentificationCode);
			}

			// Проверяем КИГУ
			if(_trueMarkWaterGroupCodeRepository
				.Get(
					uow,
					x => x.RawCode == scannedCode && !x.IsInvalid,
					1)
				.FirstOrDefault() is TrueMarkWaterGroupCode loadedGroupCode)
			{
				return Result.Success<TrueMarkAnyCode>(loadedGroupCode);
			}

			// Возможно КИТУ
			if(_trueMarkTransportCodeRepository
				.Get(
					uow,
					x => x.RawCode == scannedCode && !x.IsInvalid,
					1)
				.FirstOrDefault() is TrueMarkTransportCode loadedTransportCode)
			{
				return Result.Success<TrueMarkAnyCode>(loadedTransportCode);
			}

			return TrueMarkCodeErrors.MissingPersistedTrueMarkCode;
		}

		public Result<TrueMarkAnyCode> TryGetSavedTrueMarkCodeByScannedCode(IUnitOfWork uow, TrueMarkWaterCode parsedCode)
		{
			// Проверяем КИ
			if(_trueMarkWaterIdentificationCodeRepository
				   .Get(
					   uow,
					   x => x.GTIN == parsedCode.GTIN
							&& x.SerialNumber == parsedCode.SerialNumber
							&& !x.IsInvalid,
					   1)
				   .FirstOrDefault() is TrueMarkWaterIdentificationCode loadedIdentificationCode)
			{
				return Result.Success<TrueMarkAnyCode>(loadedIdentificationCode);
			}

			// Проверяем КИГУ
			if(_trueMarkWaterGroupCodeRepository
				   .Get(
					   uow,
					   x => x.GTIN == parsedCode.GTIN
							&& x.SerialNumber == parsedCode.SerialNumber
							&& !x.IsInvalid,
					   1)
				   .FirstOrDefault() is TrueMarkWaterGroupCode loadedGroupCode)
			{
				return Result.Success<TrueMarkAnyCode>(loadedGroupCode);
			}

			return TrueMarkCodeErrors.MissingPersistedTrueMarkCode;
		}

		public async Task<Result<TrueMarkAnyCode>> GetTrueMarkCodeByScannedCode(IUnitOfWork uow, string scannedCode, CancellationToken cancellationToken = default)
		{
			if(_trueMarkWaterCodeParser.TryParse(scannedCode, out var parsedCode))
			{
				var result = TryGetSavedTrueMarkCodeByScannedCode(uow, parsedCode);

				if(result.IsSuccess
					|| result.Errors.Any(x => x != TrueMarkCodeErrors.MissingPersistedTrueMarkCode))
				{
					return result;
				}
			}
			else
			{
				var result = TryGetSavedTrueMarkCodeByScannedCode(uow, scannedCode);

				if(result.IsSuccess
					|| result.Errors.Any(x => x != TrueMarkCodeErrors.MissingPersistedTrueMarkCode))
				{
					return result;
				}
			}

			// Не нашлось сохраненных кодов и нет других ошибок
			// пытаемся получить коды из API

			ProductInstancesInfoResponse productInstanceInfo = null;

			try
			{
				var requestCode = parsedCode is null ? scannedCode : _trueMarkWaterCodeParser.GetWaterIdentificationCode(parsedCode);

				productInstanceInfo = await _trueMarkApiClient.GetProductInstanceInfoAsync(new string[] { requestCode }, cancellationToken);

				if(!(productInstanceInfo.InstanceStatuses?.FirstOrDefault() is ProductInstanceStatus productInstanceStatus))
				{
					_logger.LogError("Ошибка при запросе к API TrueMark, нет информации о коде");
					return Result.Failure<TrueMarkAnyCode>(Errors.TrueMarkApi.UnknownCode);
				}

				if(productInstanceStatus.Status == ProductInstanceStatusEnum.Emitted
					|| productInstanceStatus.Status == ProductInstanceStatusEnum.Applied
					|| productInstanceStatus.Status == ProductInstanceStatusEnum.AppliedPaid)
				{
					return Result.Failure<TrueMarkAnyCode>(Errors.TrueMarkApi.CodeNotInCorrectStatus);
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при запросе к API TrueMark");
				return Result.Failure<TrueMarkAnyCode>(Errors.TrueMarkApi.CallFailed);
			}

			if((!productInstanceInfo.InstanceStatuses?.Any() ?? true)
			   || !string.IsNullOrWhiteSpace(productInstanceInfo.ErrorMessage))
			{
				return Result.Failure<TrueMarkAnyCode>(Errors.TrueMarkApi.ErrorResponse);
			}

			ProductInstanceStatus instanceStatus = productInstanceInfo.InstanceStatuses.FirstOrDefault();

			if(instanceStatus == null)
			{
				_logger.LogError("Ошибка при запросе к API TrueMark, нет информации о коде, получен пустой список с информацией о кодах");
				return Result.Failure<TrueMarkAnyCode>(Errors.TrueMarkApi.UnknownCode);
			}

			if(_organizationsInns is null)
			{
				SetOrganizationInns();
			}

			TrueMarkAnyCode trueMarkAnyCode = null;

			switch(instanceStatus.GeneralPackageType)
			{
				case GeneralPackageType.Box:
					trueMarkAnyCode = _trueMarkTransportCodeFactory.CreateFromRawCode(scannedCode);
					break;
				case GeneralPackageType.Group:
					trueMarkAnyCode = _trueMarkWaterGroupCodeFactory.CreateFromParsedCode(parsedCode);
					break;
				case GeneralPackageType.Unit:
					trueMarkAnyCode = _trueMarkWaterIdentificationCodeFactory.CreateFromParsedCode(parsedCode);
					break;
			}

			if(trueMarkAnyCode.IsTrueMarkWaterGroupCode || trueMarkAnyCode.IsTrueMarkTransportCode)
			{
				var createCodesResult = await CreateCodesAsync(_trueMarkApiClient, new ProductInstanceStatus[] { instanceStatus }, cancellationToken);

				if(createCodesResult.IsFailure)
				{
					return Result.Failure<TrueMarkAnyCode>(createCodesResult.Errors);
				}

				if(trueMarkAnyCode.IsTrueMarkTransportCode)
				{
					var innerCodes = createCodesResult.Value.ToArray();

					foreach(var innerCode in innerCodes)
					{
						if(innerCode.IsTrueMarkTransportCode)
						{
							trueMarkAnyCode.TrueMarkTransportCode.AddInnerTransportCode(innerCode.TrueMarkTransportCode);
						}

						if(innerCode.IsTrueMarkWaterGroupCode)
						{
							trueMarkAnyCode.TrueMarkTransportCode.AddInnerGroupCode(innerCode.TrueMarkWaterGroupCode);
						}

						if(innerCode.IsTrueMarkWaterIdentificationCode)
						{
							trueMarkAnyCode.TrueMarkTransportCode.AddInnerWaterCode(innerCode.TrueMarkWaterIdentificationCode);
						}
					}
				}

				if(trueMarkAnyCode.IsTrueMarkWaterGroupCode)
				{
					var innerCodes = createCodesResult.Value.ToArray();

					foreach(var innerCode in innerCodes)
					{
						if(innerCode.IsTrueMarkWaterGroupCode)
						{
							trueMarkAnyCode.TrueMarkWaterGroupCode.AddInnerGroupCode(innerCode.TrueMarkWaterGroupCode);
						}
						if(innerCode.IsTrueMarkWaterIdentificationCode)
						{
							trueMarkAnyCode.TrueMarkWaterGroupCode.AddInnerWaterCode(innerCode.TrueMarkWaterIdentificationCode);
						}
					}
				}
			}

			return trueMarkAnyCode;
		}

		public TrueMarkAnyCode GetParentGroupCode(IUnitOfWork unitOfWork, TrueMarkAnyCode trueMarkAnyCode)
		{
			if(trueMarkAnyCode == null)
			{
				throw new ArgumentNullException(nameof(trueMarkAnyCode), "Передано пустое значение в параметр кода");
			}

			return trueMarkAnyCode.Match(
				transportCode =>
				{
					if(transportCode.ParentTransportCodeId != null)
					{
						return GetParentGroupCode(
							unitOfWork,
							_trueMarkTransportCodeRepository
								.Get(
									unitOfWork,
									x => x.Id == transportCode.ParentTransportCodeId,
									1)
								.FirstOrDefault());
					}

					return transportCode;
				},
				groupCode =>
				{
					if(groupCode.ParentTransportCodeId != null)
					{
						return GetParentGroupCode(
							unitOfWork,
							_trueMarkTransportCodeRepository
								.Get(
									unitOfWork,
									x => x.Id == groupCode.ParentTransportCodeId,
									1)
								.FirstOrDefault());
					}

					if(groupCode.ParentWaterGroupCodeId != null)
					{
						return GetParentGroupCode(
							unitOfWork,
							_trueMarkWaterGroupCodeRepository
								.Get(
									unitOfWork,
									x => x.Id == groupCode.ParentWaterGroupCodeId,
									1)
								.FirstOrDefault());
					}

					return groupCode;
				},
				waterCode =>
				{
					if(waterCode.ParentWaterGroupCodeId != null)
					{
						return GetParentGroupCode(unitOfWork,
							_trueMarkWaterGroupCodeRepository
								.Get(
									unitOfWork,
									x => x.Id == waterCode.ParentWaterGroupCodeId,
									1)
								.FirstOrDefault());
					}

					if(waterCode.ParentTransportCodeId != null)
					{
						return GetParentGroupCode(
							unitOfWork,
							_trueMarkTransportCodeRepository
								.Get(
									unitOfWork,
									x => x.Id == waterCode.ParentTransportCodeId,
									1)
								.FirstOrDefault());
					}

					return waterCode;
				});
		}

		private async Task<Result<IEnumerable<TrueMarkAnyCode>>> CreateCodesAsync(ITrueMarkApiClient truemarkClient, ProductInstanceStatus[] instanceStatuses, CancellationToken cancellationToken)
		{
			var nextIterationInstanceCodes = instanceStatuses
				.SelectMany(x => x.Childs);

			ProductInstanceStatus[] innerCodesCheckResults = null;

			List<TrueMarkAnyCode> newCodes = new List<TrueMarkAnyCode>();
			List<TrueMarkAnyCode> newInnerCodes = new List<TrueMarkAnyCode>();

			try
			{
				innerCodesCheckResults = (await truemarkClient.GetProductInstanceInfoAsync(nextIterationInstanceCodes, cancellationToken)).InstanceStatuses.ToArray();

				if(innerCodesCheckResults.Any())
				{
					var innerCodesResult = await CreateCodesAsync(truemarkClient, innerCodesCheckResults, cancellationToken);

					if(innerCodesResult.IsFailure)
					{
						return Result.Failure<IEnumerable<TrueMarkAnyCode>>(innerCodesResult.Errors);
					}

					newInnerCodes.AddRange(innerCodesResult.Value);
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при запросе к API TrueMark");
				return Result.Failure<IEnumerable<TrueMarkAnyCode>>(Errors.TrueMarkApi.CallFailed);
			}

			foreach(var instanceStatus in innerCodesCheckResults)
			{
				if(!_organizationsInns.Contains(instanceStatus.OwnerInn))
				{
					return Result.Failure<IEnumerable<TrueMarkAnyCode>>(TrueMarkCodeErrors.CreateTrueMarkCodeOwnerInnIsNotCorrect(instanceStatus.OwnerInn));
				}

				if(instanceStatus.GeneralPackageType == GeneralPackageType.Box)
				{
					newCodes.Add(_trueMarkTransportCodeFactory.CreateFromProductInstanceStatus(instanceStatus));
				}
				else if(instanceStatus.GeneralPackageType == GeneralPackageType.Group)
				{
					newCodes.Add(_trueMarkWaterGroupCodeFactory.CreateFromProductInstanceStatus(instanceStatus));
				}
				else if(instanceStatus.GeneralPackageType == GeneralPackageType.Unit)
				{
					newCodes.Add(_trueMarkWaterIdentificationCodeFactory.CreateFromProductInstanceStatus(instanceStatus));
				}
			}

			var newTransportCodes = newCodes
				.Where(x => x.IsTrueMarkTransportCode)
				.Select(x => x.TrueMarkTransportCode)
				.ToArray();

			var newGroupCodes = newCodes
				.Where(x => x.IsTrueMarkWaterGroupCode)
				.Select(x => x.TrueMarkWaterGroupCode)
				.ToArray();

			foreach(var innerCode in newInnerCodes)
			{
				innerCode.Match(
					transportCode =>
					{
						newTransportCodes
							.FirstOrDefault(ntc => innerCodesCheckResults
								.FirstOrDefault(iccr => iccr.Childs.Contains(transportCode.RawCode))
								?.IdentificationCode == ntc.RawCode)
							?.AddInnerTransportCode(transportCode);

						return true;
					},
					waterGroupCode =>
					{
						newTransportCodes
							.FirstOrDefault(ntc => innerCodesCheckResults
								.FirstOrDefault(iccr => iccr.Childs.Contains(waterGroupCode.IdentificationCode))
								?.IdentificationCode == ntc.RawCode)
							?.AddInnerGroupCode(waterGroupCode);

						newGroupCodes
							.FirstOrDefault(ngc => innerCodesCheckResults
								.FirstOrDefault(iccr => iccr.Childs.Contains(waterGroupCode.IdentificationCode))
								?.IdentificationCode == ngc.IdentificationCode)
							?.AddInnerGroupCode(waterGroupCode);

						return true;
					},
					waterIdentificationCode =>
					{
						newTransportCodes
							.FirstOrDefault(ntc => innerCodesCheckResults
								.FirstOrDefault(iccr => iccr.Childs.Contains(waterIdentificationCode.RawCode))
								?.IdentificationCode == ntc.RawCode)
							?.AddInnerWaterCode(waterIdentificationCode);

						newGroupCodes
							.FirstOrDefault(ngc => innerCodesCheckResults
								.FirstOrDefault(iccr => iccr.Childs.Contains(waterIdentificationCode.IdentificationCode))
								?.IdentificationCode == ngc.IdentificationCode)
							?.AddInnerWaterCode(waterIdentificationCode);

						return true;
					});
			}

			newInnerCodes.Clear();
			return newCodes;
		}

		public async Task<Result<IDictionary<string, TrueMarkAnyCode>>> GetTrueMarkAnyCodesByScannedCodes(
			IEnumerable<string> scannedCodes,
			CancellationToken cancellationToken = default)
		{
			return await CreateTrueMarkAnyCodesByScannedCodesUsingDataFromTrueMark(scannedCodes, cancellationToken);
		}

		private async Task<Result<IDictionary<string, TrueMarkAnyCode>>> CreateTrueMarkAnyCodesByScannedCodesUsingDataFromTrueMark(
			IEnumerable<string> scannedCodes,
			CancellationToken cancellationToken)
		{
			var scannedCodesData = new Dictionary<string, TrueMarkAnyCode>();
			var codes = new List<TrueMarkAnyCode>();

			var requestCodesData = CreateRequestCodesDataByScannedCodes(scannedCodes);
			var requestCodesInstanseStatusesDataResult = await GetProductInstanceStatuses(requestCodesData.Keys, cancellationToken);

			if(requestCodesInstanseStatusesDataResult.IsFailure)
			{
				return Result.Failure<IDictionary<string, TrueMarkAnyCode>>(requestCodesInstanseStatusesDataResult.Errors);
			}

			var codesInstanseStatuses = requestCodesInstanseStatusesDataResult.Value.Select(x => x.Value).ToList();

			foreach(var codeInstanseStatusData in requestCodesInstanseStatusesDataResult.Value)
			{
				var requestCode = codeInstanseStatusData.Key;
				var instanceStatus = codeInstanseStatusData.Value;

				TrueMarkAnyCode trueMarkAnyCode = null;

				switch(instanceStatus.GeneralPackageType)
				{
					case GeneralPackageType.Box:
						trueMarkAnyCode = _trueMarkTransportCodeFactory.CreateFromRawCode(requestCode);
						break;
					case GeneralPackageType.Group:
						trueMarkAnyCode =
							requestCodesData.ContainsKey(requestCode) && requestCodesData[requestCode].ParsedCode != null
							? _trueMarkWaterGroupCodeFactory.CreateFromParsedCode(requestCodesData[requestCode].ParsedCode)
							: _trueMarkWaterGroupCodeFactory.CreateFromProductInstanceStatus(instanceStatus);
						break;
					case GeneralPackageType.Unit:
						trueMarkAnyCode =
							requestCodesData.ContainsKey(requestCode) && requestCodesData[requestCode].ParsedCode != null
							? _trueMarkWaterIdentificationCodeFactory.CreateFromParsedCode(requestCodesData[requestCode].ParsedCode)
							: _trueMarkWaterIdentificationCodeFactory.CreateFromProductInstanceStatus(instanceStatus);
						break;
					default:
						trueMarkAnyCode = null;
						break;
				}

				if(trueMarkAnyCode != null)
				{
					codes.Add(trueMarkAnyCode);
				}

				if(!requestCodesData.TryGetValue(requestCode, out var scannedCodeData))
				{
					continue;
				}

				if(scannedCodesData.ContainsKey(scannedCodeData.ScannedCode))
				{
					continue;
				}

				scannedCodesData.Add(scannedCodeData.ScannedCode, trueMarkAnyCode);
			}

			var newTransportCodes = codes
				.Where(x => x.IsTrueMarkTransportCode)
				.Select(x => x.TrueMarkTransportCode)
				.ToArray();

			var newGroupCodes = codes
				.Where(x => x.IsTrueMarkWaterGroupCode)
				.Select(x => x.TrueMarkWaterGroupCode)
				.ToArray();

			foreach(var code in codes)
			{
				code.Match(
					transportCode =>
					{
						newTransportCodes
							.FirstOrDefault(ntc => codesInstanseStatuses
								.FirstOrDefault(iccr => iccr.Childs.Contains(transportCode.RawCode))
								?.IdentificationCode == ntc.RawCode)
							?.AddInnerTransportCode(transportCode);

						return true;
					},
					waterGroupCode =>
					{
						newTransportCodes
							.FirstOrDefault(ntc => codesInstanseStatuses
								.FirstOrDefault(iccr => iccr.Childs.Contains(waterGroupCode.IdentificationCode))
								?.IdentificationCode == ntc.RawCode)
							?.AddInnerGroupCode(waterGroupCode);

						newGroupCodes
							.FirstOrDefault(ngc => codesInstanseStatuses
								.FirstOrDefault(iccr => iccr.Childs.Contains(waterGroupCode.IdentificationCode))
								?.IdentificationCode == ngc.IdentificationCode)
							?.AddInnerGroupCode(waterGroupCode);

						return true;
					},
					waterIdentificationCode =>
					{
						newTransportCodes
							.FirstOrDefault(ntc => codesInstanseStatuses
								.FirstOrDefault(iccr => iccr.Childs.Contains(waterIdentificationCode.RawCode))
								?.IdentificationCode == ntc.RawCode)
							?.AddInnerWaterCode(waterIdentificationCode);

						newGroupCodes
							.FirstOrDefault(ngc => codesInstanseStatuses
								.FirstOrDefault(iccr => iccr.Childs.Contains(waterIdentificationCode.IdentificationCode))
								?.IdentificationCode == ngc.IdentificationCode)
							?.AddInnerWaterCode(waterIdentificationCode);

						return true;
					});
			}

			return scannedCodesData;
		}

		private IDictionary<string, (string ScannedCode, TrueMarkWaterCode ParsedCode)> CreateRequestCodesDataByScannedCodes(IEnumerable<string> scannedCodes)
		{
			IList<(string ScannedCode, string RequestCode, TrueMarkWaterCode ParsedCode)> codes = scannedCodes
				.Select(x => (x, _trueMarkWaterCodeParser.TryParse(x, out var parsedCode) ? _trueMarkWaterCodeParser.GetWaterIdentificationCode(parsedCode) : x, parsedCode))
				.ToList();

			var requestCodes = codes
				.GroupBy(x => x.RequestCode)
				.ToDictionary(x => x.Key, x => x.Select(c => (c.ScannedCode, c.ParsedCode)).FirstOrDefault());

			return requestCodes;
		}

		public Result<TrueMarkAnyCode> GetSavedTrueMarkAnyCodesByScannedCodes(IUnitOfWork uow, string scannedCode)
		{
			return
				_trueMarkWaterCodeParser.TryParse(scannedCode, out var parsedCode)
				? TryGetSavedTrueMarkCodeByScannedCode(uow, parsedCode)
				: TryGetSavedTrueMarkCodeByScannedCode(uow, scannedCode);
		}

		private async Task<Result<IDictionary<string, ProductInstanceStatus>>> GetProductInstanceStatuses(IEnumerable<string> requestCodes, CancellationToken cancellationToken)
		{
			var result = new Dictionary<string, ProductInstanceStatus>();

			while(requestCodes != null && requestCodes.Any())
			{
				var statusesResult = await RequestProductInstanceStatuses(requestCodes, cancellationToken);

				if(statusesResult.IsFailure)
				{
					return statusesResult;
				}

				foreach(var status in statusesResult.Value)
				{
					if(result.ContainsKey(status.Key))
					{
						continue;
					}
					result.Add(status.Key, status.Value);
				}

				requestCodes = statusesResult.Value
					.Select(x => x.Value)
					.Where(x => x.GeneralPackageType != GeneralPackageType.Unit)
					.SelectMany(x => x.Childs)
					.ToList();
			}
			;

			return result;
		}

		private async Task<Result<IDictionary<string, ProductInstanceStatus>>> RequestProductInstanceStatuses(IEnumerable<string> requestCodes, CancellationToken cancellationToken)
		{
			if(_organizationsInns is null)
			{
				SetOrganizationInns();
			}

			var instancesStatuses = Enumerable.Empty<ProductInstanceStatus>();

			try
			{
				var productInstancesInfo = await _trueMarkApiClient.GetProductInstanceInfoAsync(requestCodes, cancellationToken);

				if(productInstancesInfo is null
					|| !string.IsNullOrWhiteSpace(productInstancesInfo.ErrorMessage))
				{
					_logger.LogError("Ошибка при запросе к Api TrueMark, ошибка в ответе от Api");
					return Result.Failure<IDictionary<string, ProductInstanceStatus>>(Errors.TrueMarkApi.ErrorResponse);
				}

				if(productInstancesInfo.InstanceStatuses is null
					|| !productInstancesInfo.InstanceStatuses.Any())
				{
					_logger.LogError("Ошибка при запросе к API TrueMark, нет информации о кодах");
					return Result.Failure<IDictionary<string, ProductInstanceStatus>>(Errors.TrueMarkApi.UnknownCode);
				}

				instancesStatuses = productInstancesInfo.InstanceStatuses;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при запросе к API TrueMark");
				return Result.Failure<IDictionary<string, ProductInstanceStatus>>(Errors.TrueMarkApi.CallFailed);
			}

			var result = new Dictionary<string, ProductInstanceStatus>();

			foreach(var instanceStatus in instancesStatuses)
			{
				var requestCode = requestCodes.FirstOrDefault(rc => rc == instanceStatus.IdentificationCode);

				if(string.IsNullOrWhiteSpace(requestCode))
				{
					throw new InvalidOperationException($"No matching code found for IdentificationCode: {instanceStatus.IdentificationCode}");
				}

				if(result.ContainsKey(requestCode))
				{
					continue;
				}

				result.Add(requestCode, instanceStatus);
			}

			return result;
		}

		public virtual async Task<Result<IEnumerable<StagingTrueMarkCode>>> AddStagingTrueMarkCode(
			IUnitOfWork uow,
			string scannedCode,
			StagingTrueMarkCodeRelatedDocumentType relatedDocumentType,
			int relatedDocumentId,
			OrderItemEntity orderItem,
			CancellationToken cancellationToken = default)
		{
			var createCodeResult =
				await CreateStagingTrueMarkCodeByScannedCodeUsingDataFromTrueMark(
					scannedCode,
					relatedDocumentType,
					relatedDocumentId,
					orderItem,
					cancellationToken);

			if(createCodeResult.IsFailure)
			{
				var error = createCodeResult.Errors.FirstOrDefault();
				return Result.Failure<IEnumerable<StagingTrueMarkCode>>(error);
			}

			var stagingTrueMarkCode = createCodeResult.Value;

			var existingRootCodeResult =
				await GetSavedStagingTrueMarkCodesAddedToDocument(uow, stagingTrueMarkCode, cancellationToken);

			if(existingRootCodeResult.IsFailure)
			{
				var error = createCodeResult.Errors.FirstOrDefault();
				return Result.Failure<IEnumerable<StagingTrueMarkCode>>(error);
			}

			return Result.Success<IEnumerable<StagingTrueMarkCode>>(new List<StagingTrueMarkCode>());
		}

		public async Task<Result<StagingTrueMarkCode>> CreateStagingTrueMarkCodeByScannedCodeUsingDataFromTrueMark(
			string scannedCode,
			StagingTrueMarkCodeRelatedDocumentType relatedDocumentType,
			int relatedDocumentId,
			OrderItemEntity orderItem,
			CancellationToken cancellationToken = default)
		{
			var codes = new List<StagingTrueMarkCode>();

			var requestCodesData = CreateRequestCodesDataByScannedCodes(new List<string> { scannedCode });
			var requestCodesInstanseStatusesDataResult = await GetProductInstanceStatuses(requestCodesData.Keys, cancellationToken);

			if(requestCodesInstanseStatusesDataResult.IsFailure)
			{
				return Result.Failure<StagingTrueMarkCode>(requestCodesInstanseStatusesDataResult.Errors);
			}

			var codesInstanseStatuses = requestCodesInstanseStatusesDataResult.Value.Select(x => x.Value).ToList();

			foreach(var instanceStatus in codesInstanseStatuses)
			{
				if(instanceStatus.Status != ProductInstanceStatusEnum.Introduced)
				{
					return Result.Failure<StagingTrueMarkCode>(Errors.TrueMarkApi.CodeNotInCorrectStatus);
				}

				if(!_organizationsInns.Contains(instanceStatus.OwnerInn))
				{
					return Result.Failure<StagingTrueMarkCode>(TrueMarkCodeErrors.CreateTrueMarkCodeOwnerInnIsNotCorrect(instanceStatus.OwnerInn));
				}
			}

			foreach(var codeInstanseStatusData in requestCodesInstanseStatusesDataResult.Value)
			{
				var requestCode = codeInstanseStatusData.Key;
				var instanceStatus = codeInstanseStatusData.Value;

				StagingTrueMarkCode code = null;
				TrueMarkWaterCode parsedCode = null;

				if(requestCodesData.TryGetValue(requestCode, out var requestCodeData))
				{
					parsedCode = requestCodeData.ParsedCode;
				}

				switch(instanceStatus.GeneralPackageType)
				{
					case GeneralPackageType.Box:
						code = _stagingTrueMarkCodeFactory.CreateTransportCodeFromRawCode(requestCode, relatedDocumentType, relatedDocumentId, orderItem);
						break;
					case GeneralPackageType.Group:
						code =
							parsedCode != null
							? _stagingTrueMarkCodeFactory.CreateGroupCodeFromParsedCode(parsedCode, relatedDocumentType, relatedDocumentId, orderItem)
							: _stagingTrueMarkCodeFactory.CreateGroupCodeFromProductInstanceStatus(instanceStatus, relatedDocumentType, relatedDocumentId, orderItem);
						break;
					case GeneralPackageType.Unit:
						code =
							parsedCode != null
							? _stagingTrueMarkCodeFactory.CreateIdentificationCodeFromParsedCode(parsedCode, relatedDocumentType, relatedDocumentId, orderItem)
							: _stagingTrueMarkCodeFactory.CreateIdentificationCodeFromProductInstanceStatus(instanceStatus, relatedDocumentType, relatedDocumentId, orderItem);
						break;
					default:
						throw new NotImplementedException("Указанный в статусе кода тип упаковки не поддерживается");
				}

				codes.Add(code);
			}

			foreach(var code in codes)
			{
				var parentCode = codes
					.FirstOrDefault(c => codesInstanseStatuses
						.FirstOrDefault(cis => cis.Childs.Contains(code.IdentificationCode))
						?.IdentificationCode == c.IdentificationCode);

				parentCode?.AddInnerCode(code);
			}

			var topCode = codes.First(x => x.ParentCodeId == null);

			return topCode;
		}

		public async Task<Result<IEnumerable<StagingTrueMarkCode>>> GetSavedStagingTrueMarkCodesAddedToDocument(
			IUnitOfWork uow,
			IEnumerable<StagingTrueMarkCode> codes,
			CancellationToken cancellationToken)
		{
			var existingCodes = new List<StagingTrueMarkCode>();

			if(!codes.Any())
			{
				return existingCodes;
			}

			var relatedDocumentType = codes.First().RelatedDocumentType;
			var relatedDocumentId = codes.First().RelatedDocumentId;

			var addedCodesResult = await _stagingTrueMarkCodeRepository.GetAsync(
				uow,
				c => c.RelatedDocumentType == relatedDocumentType && c.RelatedDocumentId == relatedDocumentId,
				cancellationToken: cancellationToken);

			if(addedCodesResult.IsFailure)
			{
				return addedCodesResult;
			}

			var addedCodes = addedCodesResult.Value;

			foreach(var code in codes)
			{
				existingCodes.AddRange(addedCodesResult.Value
					.Where(c => ((code.IsTransport && c.RawCode == code.RawCode)
							|| (!code.IsTransport && c.GTIN == code.GTIN && c.SerialNumber == code.SerialNumber))
						&& c.Id != code.Id
						&& c.RelatedDocumentType == code.RelatedDocumentType
						&& c.RelatedDocumentId == code.RelatedDocumentId
						&& c.OrderItem.Id == code.OrderItem.Id)
					.ToList());
			}

			return existingCodes;
		}

		public async Task<Result<IEnumerable<StagingTrueMarkCode>>> GetSavedStagingTrueMarkCodesAddedToDocument(
			IUnitOfWork uow,
			StagingTrueMarkCode code,
			CancellationToken cancellationToken)
		{
			return await _stagingTrueMarkCodeRepository.GetAsync(
				uow,
				c => ((code.IsTransport && c.RawCode == code.RawCode)
						|| (!code.IsTransport && c.GTIN == code.GTIN && c.SerialNumber == code.SerialNumber))
					&& c.Id != code.Id
					&& c.RelatedDocumentType == code.RelatedDocumentType
					&& c.RelatedDocumentId == code.RelatedDocumentId
					&& c.OrderItem.Id == code.OrderItem.Id,
				cancellationToken: cancellationToken);
		}
	}
}
