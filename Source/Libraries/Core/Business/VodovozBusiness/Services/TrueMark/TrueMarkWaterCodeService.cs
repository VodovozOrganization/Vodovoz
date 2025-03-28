﻿using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using TrueMark.Contracts;
using TrueMark.Contracts.Responses;
using TrueMarkApi.Client;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Errors;
using Vodovoz.Models.TrueMark;
using Vodovoz.Settings.Edo;
using TrueMarkCodeErrors = Vodovoz.Errors.TrueMark.TrueMarkCode;

namespace VodovozBusiness.Services.TrueMark
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
		private readonly TrueMarkCodesChecker _trueMarkCodesChecker;
		private readonly TrueMarkWaterCodeParser _trueMarkWaterCodeParser;
		private readonly ITrueMarkApiClientFactory _trueMarkApiClientFactory;
		private readonly IGenericRepository<TrueMarkWaterIdentificationCode> _trueMarkWaterIdentificationCodeRepository;
		private readonly IGenericRepository<TrueMarkProductCode> _trueMarkProductCodeRepository;
		private readonly IGenericRepository<TrueMarkWaterGroupCode> _trueMarkWaterGroupCodeRepository;
		private readonly IGenericRepository<TrueMarkTransportCode> _trueMarkTransportCodeRepository;
		private readonly IGenericRepository<OrganizationEntity> _organizationRepository;
		private readonly IEdoSettings _edoSettings;

		private IList<string> _organizationsInns;

		public TrueMarkWaterCodeService(
			ILogger<TrueMarkWaterCodeService> logger,
			IUnitOfWork uow,
			TrueMarkCodesChecker trueMarkCodesChecker,
			TrueMarkWaterCodeParser trueMarkWaterCodeParser,
			ITrueMarkApiClientFactory trueMarkApiClientFactory,
			IGenericRepository<TrueMarkWaterIdentificationCode> trueMarkWaterIdentificationCodeRepository,
			IGenericRepository<TrueMarkProductCode> trueMarkProductCodeRepository,
			IGenericRepository<TrueMarkWaterGroupCode> trueMarkWaterGroupCodeRepository,
			IGenericRepository<TrueMarkTransportCode> trueMarkTransportCodeRepository,
			IGenericRepository<OrganizationEntity> organizationRepository,
			IEdoSettings edoSettings)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_uow = uow
				?? throw new ArgumentNullException(nameof(uow));
			_trueMarkCodesChecker = trueMarkCodesChecker
				?? throw new ArgumentNullException(nameof(trueMarkCodesChecker));
			_trueMarkWaterCodeParser = trueMarkWaterCodeParser
				?? throw new ArgumentNullException(nameof(trueMarkWaterCodeParser));
			_trueMarkApiClientFactory = trueMarkApiClientFactory
				?? throw new ArgumentNullException(nameof(trueMarkApiClientFactory));
			_trueMarkWaterIdentificationCodeRepository = trueMarkWaterIdentificationCodeRepository
				?? throw new ArgumentNullException(nameof(trueMarkWaterIdentificationCodeRepository));
			_trueMarkProductCodeRepository = trueMarkProductCodeRepository
				?? throw new ArgumentNullException(nameof(trueMarkProductCodeRepository));
			_trueMarkWaterGroupCodeRepository = trueMarkWaterGroupCodeRepository
				?? throw new ArgumentNullException(nameof(trueMarkWaterGroupCodeRepository));
			_trueMarkTransportCodeRepository = trueMarkTransportCodeRepository
				?? throw new ArgumentNullException(nameof(trueMarkTransportCodeRepository));
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
						&& x.CheckCode == parsedCode.CheckCode
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
						&& x.CheckCode == parsedCode.CheckCode
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

			var truemarkClient = _trueMarkApiClientFactory.GetClient();

			try
			{
				var requestCode = parsedCode is null ? scannedCode : _trueMarkWaterCodeParser.GetWaterIdentificationCode(parsedCode);

				productInstanceInfo = await truemarkClient.GetProductInstanceInfoAsync(new string[] { requestCode }, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при запросе к API TrueMark");
				return Result.Failure<TrueMarkAnyCode>(new Error("Temporary.Exception.Error", "Ошибка при запросе к API TrueMark"));
			}

			if((!productInstanceInfo.InstanceStatuses?.Any() ?? true)
			   || !string.IsNullOrWhiteSpace(productInstanceInfo.ErrorMessage))
			{
				return Result.Failure<TrueMarkAnyCode>(new Error("Temporary.Exception.Error", productInstanceInfo?.ErrorMessage ?? "Не удалось получить информацию о коде"));
			}

			ProductInstanceStatus instanceStatus = productInstanceInfo.InstanceStatuses.FirstOrDefault();

			if(instanceStatus == null)
			{
				return Result.Failure<TrueMarkAnyCode>(new Error("Temporary.Exception.Error", "Не удалось получить информацию о коде"));
			}

			if(_organizationsInns is null)
			{
				SetOrganizationInns();
			}

			if(!_organizationsInns.Contains(instanceStatus.OwnerInn))
			{
				return Result.Failure<TrueMarkAnyCode>(TrueMarkCodeErrors.CreateTrueMarkCodeOwnerInnIsNotCorrect(instanceStatus.OwnerInn));
			}

			if(instanceStatus.GeneralPackageType == GeneralPackageType.Unit)
			{
				var newWaterCode = CreateWaterCode(parsedCode);

				uow.Save(newWaterCode);

				return Result.Success<TrueMarkAnyCode>(newWaterCode);
			}

			if(instanceStatus.GeneralPackageType == GeneralPackageType.Group)
			{
				var newGroupCodeResult = await CreateGroupCodeAsync(parsedCode, truemarkClient, instanceStatus, cancellationToken);
				
				if(newGroupCodeResult.IsFailure)
				{
					return Result.Failure<TrueMarkAnyCode>(newGroupCodeResult.Errors);
				}

				uow.Save(newGroupCodeResult.Value);

				return Result.Success<TrueMarkAnyCode>(newGroupCodeResult.Value);
			}

			if(instanceStatus.GeneralPackageType == GeneralPackageType.Box)
			{
				var newTransportCodeResult = await CreateTransportCodeAsync(truemarkClient, instanceStatus, cancellationToken);

				if (newTransportCodeResult.IsFailure)
				{
					return Result.Failure<TrueMarkAnyCode>(newTransportCodeResult.Errors);
				}

				uow.Save(newTransportCodeResult.Value);

				return Result.Success<TrueMarkAnyCode>(newTransportCodeResult.Value);
			}

			return Result.Failure<TrueMarkAnyCode>(new Error("Temporary.Exception.Error", "Не удалось получить информацию о коде"));
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

		private async Task<Result<TrueMarkTransportCode>> CreateTransportCodeAsync(ITrueMarkApiClient truemarkClient, ProductInstanceStatus instanceStatus, CancellationToken cancellationToken)
		{
			var newTransportCode = new TrueMarkTransportCode
			{
				IsInvalid = false,
				RawCode = instanceStatus.IdentificationCode
			};

			ProductInstancesInfoResponse innerCodesCheckResult = null;

			try
			{
				innerCodesCheckResult = await truemarkClient.GetProductInstanceInfoAsync(instanceStatus.Childs, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при запросе к API TrueMark");
				return Result.Failure<TrueMarkTransportCode>(new Error("Temporary.Exception.Error", "Ошибка при запросе к API TrueMark"));
			}

			if (innerCodesCheckResult == null)
			{
				return Result.Failure<TrueMarkTransportCode>(new Error("Temporary.Exception.Error", "Не удалось получить информацию о коде"));
			}

			foreach (var innerCodeCheckResult in innerCodesCheckResult.InstanceStatuses)
			{
				if(!_organizationsInns.Contains(instanceStatus.OwnerInn))
				{
					return Result.Failure<TrueMarkTransportCode>(TrueMarkCodeErrors.CreateTrueMarkCodeOwnerInnIsNotCorrect(instanceStatus.OwnerInn));
				}

				if(innerCodeCheckResult.GeneralPackageType == GeneralPackageType.Box)
				{
					var newInnerTransportCodeResult = await CreateTransportCodeAsync(truemarkClient, innerCodeCheckResult, cancellationToken);

					if(newInnerTransportCodeResult.IsFailure)
					{
						return Result.Failure<TrueMarkTransportCode>(newInnerTransportCodeResult.Errors);
					}

					newTransportCode.AddInnerTransportCode(newInnerTransportCodeResult.Value);
				}
				else if(innerCodeCheckResult.GeneralPackageType == GeneralPackageType.Group)
				{
					var newGroupCodeResult = await CreateGroupCodeAsync(truemarkClient, innerCodeCheckResult, cancellationToken);

					if (newGroupCodeResult.IsFailure)
					{
						return Result.Failure<TrueMarkTransportCode>(newGroupCodeResult.Errors);
					}

					newTransportCode.AddInnerGroupCode(newGroupCodeResult.Value);
				}
				else if(innerCodeCheckResult.GeneralPackageType == GeneralPackageType.Unit)
				{
					var newWaterCode = CreateWaterCode(innerCodeCheckResult);
					newTransportCode.AddInnerWaterCode(newWaterCode);
				}
			}

			return newTransportCode;
		}

		private async Task<Result<TrueMarkWaterGroupCode>> CreateGroupCodeAsync(TrueMarkWaterCode parsedCode, ITrueMarkApiClient truemarkClient, ProductInstanceStatus instanceStatus, CancellationToken cancellationToken)
		{
			var newGroupCode = new TrueMarkWaterGroupCode
			{
				IsInvalid = false,
				RawCode = parsedCode.SourceCode.Substring(0, Math.Min(255, parsedCode.SourceCode.Length)),
				GTIN = parsedCode.GTIN,
				SerialNumber = parsedCode.SerialNumber,
				CheckCode = parsedCode.CheckCode
			};

			ProductInstancesInfoResponse innerCodesCheckResult;

			try
			{
				innerCodesCheckResult = await truemarkClient.GetProductInstanceInfoAsync(instanceStatus.Childs, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при запросе к API TrueMark");
				return Result.Failure<TrueMarkWaterGroupCode>(new Error("Temporary.Exception.Error", "Ошибка при запросе к API TrueMark"));
			}

			if(innerCodesCheckResult == null)
			{
				return Result.Failure<TrueMarkWaterGroupCode>(new Error("Temporary.Exception.Error", "Не удалось получить информацию о коде"));
			}

			foreach(var innerCodeCheckResult in innerCodesCheckResult.InstanceStatuses)
			{
				if(!_organizationsInns.Contains(instanceStatus.OwnerInn))
				{
					return Result.Failure<TrueMarkWaterGroupCode>(TrueMarkCodeErrors.CreateTrueMarkCodeOwnerInnIsNotCorrect(instanceStatus.OwnerInn));
				}

				if(innerCodeCheckResult.GeneralPackageType == GeneralPackageType.Group)
				{
					var newInnerGroupCodeResult = await CreateGroupCodeAsync(truemarkClient, innerCodeCheckResult, cancellationToken);

					if(newInnerGroupCodeResult.IsFailure)
					{
						return Result.Failure<TrueMarkWaterGroupCode>(newInnerGroupCodeResult.Errors);
					}

					newGroupCode.AddInnerGroupCode(newInnerGroupCodeResult.Value);
				}
				else if(innerCodeCheckResult.GeneralPackageType == GeneralPackageType.Unit)
				{
					var newWaterCode = CreateWaterCode(innerCodeCheckResult);
					newGroupCode.AddInnerWaterCode(newWaterCode);
				}
			}

			return newGroupCode;
		}

		private async Task<Result<TrueMarkWaterGroupCode>> CreateGroupCodeAsync(ITrueMarkApiClient truemarkClient, ProductInstanceStatus instanceStatus, CancellationToken cancellationToken)
		{
			var identificationCode = instanceStatus.IdentificationCode;

			var rawCode = "\\u001d" + identificationCode + "\\u001d";

			var serialNumber = identificationCode
				.Replace(instanceStatus.Gtin, "")
				.Replace("0121", "");

			var newGroupCode = new TrueMarkWaterGroupCode
			{
				IsInvalid = false,
				RawCode = rawCode,
				GTIN = instanceStatus.Gtin,
				SerialNumber = serialNumber
			};

			ProductInstancesInfoResponse innerCodesCheckResult = null;

			try
			{
				innerCodesCheckResult = await truemarkClient.GetProductInstanceInfoAsync(instanceStatus.Childs, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при запросе к API TrueMark");
				return Result.Failure<TrueMarkWaterGroupCode>(new Error("Temporary.Exception.Error", "Ошибка при запросе к API TrueMark"));
			}

			if (innerCodesCheckResult == null)
			{
				return Result.Failure<TrueMarkWaterGroupCode>(new Error("Temporary.Exception.Error", "Не удалось получить информацию о коде"));
			}

			foreach (var innerCodeCheckResult in innerCodesCheckResult.InstanceStatuses)
			{
				if(!_organizationsInns.Contains(instanceStatus.OwnerInn))
				{
					return Result.Failure<TrueMarkWaterGroupCode>(TrueMarkCodeErrors.CreateTrueMarkCodeOwnerInnIsNotCorrect(instanceStatus.OwnerInn));
				}

				if(innerCodeCheckResult.GeneralPackageType == GeneralPackageType.Group)
				{
					var newInnerGroupCodeResult = await CreateGroupCodeAsync(truemarkClient, innerCodeCheckResult, cancellationToken);

					if (newInnerGroupCodeResult.IsFailure)
					{
						return Result.Failure<TrueMarkWaterGroupCode>(newInnerGroupCodeResult.Errors);
					}

					newGroupCode.AddInnerGroupCode(newInnerGroupCodeResult.Value);
				}
				else if(innerCodeCheckResult.GeneralPackageType == GeneralPackageType.Unit)
				{
					var newWaterCode = CreateWaterCode(innerCodeCheckResult);
					newGroupCode.AddInnerWaterCode(newWaterCode);
				}
			}

			return newGroupCode;
		}

		private TrueMarkWaterIdentificationCode CreateWaterCode(ProductInstanceStatus productInstanceStatus)
		{
			var identificationCode = productInstanceStatus.IdentificationCode;

			var rawCode = "\\u001d" + identificationCode + "\\u001d";

			var serialNumber = identificationCode
				.Replace(productInstanceStatus.Gtin, "")
				.Replace("0121", "");

			var newWaterCode = new TrueMarkWaterIdentificationCode
			{
				IsInvalid = false,
				RawCode = rawCode,
				GTIN = productInstanceStatus.Gtin,
				SerialNumber = serialNumber
			};

			return newWaterCode;
		}

		private TrueMarkWaterIdentificationCode CreateWaterCode(TrueMarkWaterCode parsedCode)
		{
			var newWaterCode = new TrueMarkWaterIdentificationCode
			{
				IsInvalid = false,
				RawCode = parsedCode.SourceCode.Substring(0, Math.Min(255, parsedCode.SourceCode.Length)),
				GTIN = parsedCode.GTIN,
				SerialNumber = parsedCode.SerialNumber,
				CheckCode = parsedCode.CheckCode
			};

			return newWaterCode;
		}
	}
}
