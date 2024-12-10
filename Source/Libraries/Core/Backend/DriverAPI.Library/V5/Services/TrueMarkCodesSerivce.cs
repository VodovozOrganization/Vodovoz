using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Errors;
using Vodovoz.Models.TrueMark;
using Vodovoz.Settings.Edo;
using TrueMarkCodeErrors = Vodovoz.Errors.TrueMark.TrueMarkCode;

namespace DriverAPI.Library.V5.Services
{
	/// <summary>
	/// Сервис проверки кодов ЧЗ для водительского приложения
	/// </summary>
	public class TrueMarkCodesSerivce : ITrueMarkCodesSerivce
	{
		private readonly IList<SourceProductCodeStatus> _productCodesStatusesToCheckDuplicates = new List<SourceProductCodeStatus>
		{
			SourceProductCodeStatus.Accepted,
			SourceProductCodeStatus.Accepted
		};

		private readonly ILogger<TrueMarkCodesSerivce> _logger;
		private readonly TrueMarkWaterCodeParser _trueMarkWaterCodeParser;
		private readonly IGenericRepository<TrueMarkWaterIdentificationCode> _trueMarkIdentificationCodeRepository;
		private readonly IGenericRepository<RouteListItemTrueMarkProductCode> _routeListItemTrueMarkProductCodeRepository;
		private readonly TrueMarkCodesChecker _trueMarkCodesChecker;

		private readonly IList<string> _organizationsInns;

		public TrueMarkCodesSerivce(
			ILogger<TrueMarkCodesSerivce> logger,
			IUnitOfWork uow,
			TrueMarkWaterCodeParser trueMarkWaterCodeParser,
			IGenericRepository<TrueMarkWaterIdentificationCode> trueMarkIdentificationCodeRepository,
			IGenericRepository<Organization> organizationRepository,
			IGenericRepository<RouteListItemTrueMarkProductCode> routeListItemTrueMarkProductCodeRepository,
			TrueMarkCodesChecker trueMarkCodesChecker,
			IEdoSettings edoSettings)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(organizationRepository is null)
			{
				throw new ArgumentNullException(nameof(organizationRepository));
			}

			if(edoSettings is null)
			{
				throw new ArgumentNullException(nameof(edoSettings));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_trueMarkWaterCodeParser = trueMarkWaterCodeParser ?? throw new System.ArgumentNullException(nameof(trueMarkWaterCodeParser));
			_trueMarkIdentificationCodeRepository = trueMarkIdentificationCodeRepository ?? throw new System.ArgumentNullException(nameof(trueMarkIdentificationCodeRepository));
			_routeListItemTrueMarkProductCodeRepository = routeListItemTrueMarkProductCodeRepository ?? throw new ArgumentNullException(nameof(routeListItemTrueMarkProductCodeRepository));
			_trueMarkCodesChecker = trueMarkCodesChecker ?? throw new ArgumentNullException(nameof(trueMarkCodesChecker));

			_organizationsInns =
				organizationRepository
				.Get(uow, x => edoSettings.OrganizationsHavingAccountsInTrueMark.Contains(x.Id)).Select(x => x.INN)
				.ToList();
		}

		/// <summary>
		/// Преобразует коллекцию строк отсканированных кодов ЧЗ в коллекцию сущностей кодов ЧЗ.\n
		/// Если код найден в базе, то возвращается найденная в базе сущность
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="scannedCodes">Коллекция строк отсканированных кодов ЧЗ</param>
		/// <returns>Коллекция сущностей кодов ЧЗ</returns>
		public IEnumerable<TrueMarkWaterIdentificationCode> CreateTrueMarkWaterIdentificationCodesFromScannedCodes(
			IUnitOfWork uow,
			IEnumerable<string> scannedCodes)
		{
			var identificationCodes = new List<TrueMarkWaterIdentificationCode>();

			foreach(var scannedCode in scannedCodes)
			{
				identificationCodes.Add(CreateTrueMarkWaterIdentificationCodeFromScannedCode(uow, scannedCode));
			}

			return identificationCodes;
		}

		private TrueMarkWaterIdentificationCode CreateTrueMarkWaterIdentificationCodeFromScannedCode(
			IUnitOfWork uow,
			string scannedCode)
		{
			TrueMarkWaterIdentificationCode codeEntity;

			var isValidTrueMarkCode = _trueMarkWaterCodeParser.TryParse(scannedCode, out TrueMarkWaterCode parsedCode);

			if(isValidTrueMarkCode)
			{
				var isCodeExistsInDatabase = TryLoadTrueMarkIdentificationCode(uow, parsedCode.SourceCode, out codeEntity);

				if(!isCodeExistsInDatabase)
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
				var isCodeExistsInDatabase = TryLoadTrueMarkIdentificationCode(uow, scannedCode, out codeEntity);

				if(!isCodeExistsInDatabase)
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

		private bool TryLoadTrueMarkIdentificationCode(IUnitOfWork uow, string code, out TrueMarkWaterIdentificationCode codeEntity)
		{
			codeEntity = _trueMarkIdentificationCodeRepository
				.GetValue(uow, x => x, x => x.RawCode == code)
				.SingleOrDefault();

			return codeEntity != null;
		}

		/// <summary>
		/// Проверяется, что кол-во кодов соответствует кол-ву бутылей в строке заказа
		/// </summary>
		/// <param name="codes">Список сущностей кодов ЧЗ</param>
		/// <param name="orderItem">Строка заказа</param>
		/// <returns>Результат проверки</returns>
		public Result IsAllTrueMarkCodesAddedToOrderItem(
			IEnumerable<TrueMarkWaterIdentificationCode> codes,
			OrderItem orderItem)
		{
			var errors = new List<Error>();

			var nomenclature = orderItem.Nomenclature;

			if(!nomenclature.IsAccountableInTrueMark)
			{
				return Result.Success();
			}

			if(codes.Count() != orderItem.Count)
			{
				var error = TrueMarkCodeErrors.ScannedTrueMarkCodesCountNotEqualOrderItemCountError;
				errors.Add(error);
				LogError(error);
			}

			return errors.Any()
					? Result.Failure(errors)
					: Result.Success();
		}

		/// <summary>
		/// Проверяется, что GTIN кодов соответствуют GTIN номенклатуре строки заказа
		/// </summary>
		/// <param name="codes">Список сущностей кодов ЧЗ</param>
		/// <param name="orderItem">Строка заказа</param>
		/// <returns>Результат проверки</returns>
		public Result IsAllTrueMarkCodeGtinsMatchesToNomenclatureGtin(
			IEnumerable<TrueMarkWaterIdentificationCode> codes,
			OrderItem orderItem)
		{
			var errors = new List<Error>();

			var nomenclature = orderItem.Nomenclature;

			if(!nomenclature.IsAccountableInTrueMark)
			{
				return Result.Success();
			}

			var counter = 1;

			foreach(var code in codes)
			{
				if(code.GTIN != nomenclature.Gtin)
				{
					var error = TrueMarkCodeErrors.CreateTrueMarkCodeGtinIsNotEqualsNomenclatureGtin($"Порядковый номер {counter}");
					errors.Add(error);
					LogError(error);

					counter++;
				}
			}

			return errors.Any()
					? Result.Failure(errors)
					: Result.Success();
		}

		/// <summary>
		/// У всех кодов отсутсвуют дубликаты
		/// </summary>
		/// <param name="uow">UOW</param>
		/// <param name="codes">Список сущностей кодов ЧЗ</param>
		/// <returns>Результат проверки</returns>
		public Result IsAllTrueMarkCodesHasNoDuplicates(
			IUnitOfWork uow,
			IEnumerable<TrueMarkWaterIdentificationCode> codes)
		{
			var errors = new List<Error>();

			var codeIds = codes.Where(x => x.Id != 0).Select(x => x.Id).Distinct().ToList();

			var duplicateIds =
				_routeListItemTrueMarkProductCodeRepository
				.Get(uow, x => codeIds.Contains(x.ResultCode.Id) && _productCodesStatusesToCheckDuplicates.Contains(x.SourceCodeStatus))
				.Select(x => x.Id);

			var counter = 1;

			foreach(var code in codes)
			{
				if(duplicateIds.Contains(code.Id))
				{
					var error = TrueMarkCodeErrors.CreateTrueMarkCodeIsAlreadyExists($"Порядковый номер {counter}");
					LogError(error);
					errors.Add(error);
				}

				counter++;
			}

			return errors.Any()
					? Result.Failure(errors)
					: Result.Success();
		}

		/// <summary>
		/// Проверяется, что все коды ЧЗ в статусе Introduced и владелец кода одна из наших организаций
		/// </summary>
		/// <param name="codes">Коллекция кодов</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат проверки</returns>
		public async Task<Result> IsAllTrueMarkCodesIntroducedAndHasCorrectInn(
			IEnumerable<TrueMarkWaterIdentificationCode> codes,
			CancellationToken cancellationToken)
		{
			var errors = new List<Error>();

			try
			{
				var checkResults = await _trueMarkCodesChecker.CheckCodesAsync(codes, cancellationToken);

				var counter = 1;

				foreach(var checkResult in checkResults)
				{
					if(!checkResult.Introduced)
					{
						var error = TrueMarkCodeErrors.CreateTrueMarkCodeIsNotIntroduced($"Порядковый номер {counter}");
						LogError(error);
						errors.Add(error);
					}

					if(!_organizationsInns.Contains(checkResult.OwnerInn))
					{
						var error = TrueMarkCodeErrors.CreateTrueMarkCodeIsNotIntroduced($"Порядковый номер {counter}");
						LogError(error);
						errors.Add(error);
					}

					counter++;
				}

				return errors.Any()
					? Result.Failure(errors)
					: Result.Success();
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

		public IList<RouteListItemTrueMarkProductCode> CreateAcceptedNoProblemTrueMarkProductCodesFromIdentificationCodes(
			IEnumerable<TrueMarkWaterIdentificationCode> codes,
			RouteListItem routeListItem)
		{
			var productCodes = new List<RouteListItemTrueMarkProductCode>();

			foreach(var code in codes)
			{
				var productCode = new RouteListItemTrueMarkProductCode
				{
					CreationTime = DateTime.Now,
					RouteListItem = routeListItem,
					SourceCode = code,
					ResultCode = code,
					SourceCodeStatus = SourceProductCodeStatus.Accepted,
					Problem = ProductCodeProblem.None
				};
			}

			return productCodes;
		}

		//public IList<RouteListItemTrueMarkProductCode> GetRouteListItemTrueMarkProductCodesFromIdentificationCodes(
		//	IUnitOfWork uow,
		//	IEnumerable<TrueMarkWaterIdentificationCode> codes,
		//	RouteListItem routeListItem,
		//	bool isDefectBottle = false)
		//{
		//	var productCodes = new List<RouteListItemTrueMarkProductCode>();

		//	foreach(var code in codes)
		//	{
		//		RouteListItemTrueMarkProductCode existingProductCode = null;

		//		TryGetCodeDuplicate(uow, code.Id, out existingProductCode);

		//		var productCode = new RouteListItemTrueMarkProductCode
		//		{
		//			CreationTime = DateTime.Now,
		//			RouteListItem = routeListItem,
		//			SourceCode = code
		//		};

		//		if(existingProductCode != null)
		//		{
		//			productCode.SourceCodeStatus = SourceProductCodeStatus.Problem;
		//			productCode.Problem = ProductCodeProblem.Duplicate;

		//			productCodes.Add(productCode);
		//			continue;
		//		}

		//		if(!code.IsInvalid)
		//		{
		//			if(isDefectBottle)
		//			{
		//				productCode.SourceCodeStatus = SourceProductCodeStatus.Problem;
		//				productCode.Problem = ProductCodeProblem.Defect;
		//			}
		//			else
		//			{
		//				productCode.SourceCodeStatus = SourceProductCodeStatus.Accepted;
		//				productCode.Problem = ProductCodeProblem.None;
		//			}

		//			productCode.ResultCode = code;

		//			productCodes.Add(productCode);
		//			continue;
		//		}
		//		else
		//		{
		//			productCode.SourceCodeStatus = SourceProductCodeStatus.Problem;
		//			productCode.Problem = isDefectBottle ? ProductCodeProblem.Defect : ProductCodeProblem.None;

		//			productCodes.Add(productCode);
		//			continue;
		//		}

		//		productCodes.Add(productCode);
		//	}
		//}

		private bool TryGetCodeDuplicate(
			IUnitOfWork uow,
			int identificationTrueMarkCodeId,
			out RouteListItemTrueMarkProductCode productCode)
		{
			productCode =
				_routeListItemTrueMarkProductCodeRepository
				.Get(
					uow,
					x =>
						x.ResultCode.Id == identificationTrueMarkCodeId
						&& (x.SourceCodeStatus == SourceProductCodeStatus.Accepted
							|| x.SourceCodeStatus == SourceProductCodeStatus.Changed))
				.FirstOrDefault();

			return productCode != null;
		}

		private void LogError(Error error)
		{
			_logger.LogError(error.Message);
		}
	}
}
