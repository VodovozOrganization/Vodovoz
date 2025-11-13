using FuelControl.Library.Services;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Users.Settings;
using Vodovoz.Domain.Fuel;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.Services;
using Vodovoz.Services.Fuel;
using Vodovoz.Settings.Fuel;

namespace Vodovoz.ViewModels.Infrastructure.Services.Fuel
{
	public class FuelApiService : IFuelApiService
	{
		private const int _savingErrorMessageMaxLength = 1000;

		private readonly ILogger<FuelApiService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IFuelControlAuthorizationService _fuelControlAuthorizationService;
		private readonly IFuelControlFuelCardsDataService _fuelCardsDataService;
		private readonly IFuelLimitsManagementService _fuelLimitsManagementService;
		private readonly IFuelControlFuelCardProductRestrictionService _productRestrictionService;
		private readonly IUserSettingsService _userSettingsService;
		private readonly IFuelControlSettings _fuelControlSettings;
		private readonly IFuelRepository _fuelRepository;

		public FuelApiService(
			IUnitOfWorkFactory unitOfWorkFactory,
			ILogger<FuelApiService> logger,
			IFuelControlAuthorizationService fuelControlAuthorizationService,
			IFuelControlFuelCardsDataService fuelCardsDataService,
			IFuelLimitsManagementService fuelLimitsManagementService,
			IFuelControlFuelCardProductRestrictionService productRestrictionService,
			IUserSettingsService userSettingsService,
			IFuelControlSettings fuelControlSettings,
			IFuelRepository fuelRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fuelControlAuthorizationService = fuelControlAuthorizationService ?? throw new ArgumentNullException(nameof(fuelControlAuthorizationService));
			_fuelCardsDataService = fuelCardsDataService ?? throw new ArgumentNullException(nameof(fuelCardsDataService));
			_fuelLimitsManagementService = fuelLimitsManagementService ?? throw new ArgumentNullException(nameof(fuelLimitsManagementService));
			_productRestrictionService = productRestrictionService ?? throw new ArgumentNullException(nameof(productRestrictionService));
			_userSettingsService = userSettingsService ?? throw new ArgumentNullException(nameof(userSettingsService));
			_fuelControlSettings = fuelControlSettings ?? throw new ArgumentNullException(nameof(fuelControlSettings));
			_fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
		}

		public async Task<(string SessionId, DateTime SessionExpirationDate)> Login(
			string login,
			string password,
			string apiKey,
			CancellationToken cancellationToken)
		{
			var request = CreateFuelApiRequestData(FuelApiRequestFunction.Login);

			try
			{
				var session = await _fuelControlAuthorizationService.Login(login, password, apiKey, cancellationToken);

				request.ResponseResult = FuelApiResponseResult.Success;

				return session;
			}
			catch(Exception ex)
			{
				request.ResponseResult = FuelApiResponseResult.Error;
				request.ErrorResponseMessage = GetErrorMessageFromException(ex);

				throw ex;
			}
			finally
			{
				await SaveFuelApiRequestData(request);
			}
		}

		public async Task<IEnumerable<FuelCard>> GetFuelCardsData(CancellationToken cancellationToken)
		{
			var sessionId = await GetSessionIdOrLogin(cancellationToken);

			var allCards = await GetAllCardsFromFuelControlService(sessionId, cancellationToken);

			return allCards;
		}

		public async Task<IEnumerable<FuelLimit>> GetFuelLimitsByCardId(string cardId, CancellationToken cancellationToken)
		{
			var sessionId = await GetSessionIdOrLogin(cancellationToken);

			var requestData = CreateFuelApiRequestData(FuelApiRequestFunction.FuelCardsLimitsData);

			try
			{
				var limits = await _fuelLimitsManagementService.GetFuelLimitsByCardId(
					cardId,
					sessionId,
					_userSettingsService.Settings.FuelControlApiKey,
					cancellationToken);

				requestData.ResponseResult = FuelApiResponseResult.Success;

				return limits;
			}
			catch(Exception ex)
			{
				requestData.ResponseResult = FuelApiResponseResult.Error;
				requestData.ErrorResponseMessage = GetErrorMessageFromException(ex);

				throw ex;
			}
			finally
			{
				await SaveFuelApiRequestData(requestData);
			}
		}

		public async Task<bool> RemoveFuelLimitById(string limitId, CancellationToken cancellationToken)
		{
			var sessionId = await GetSessionIdOrLogin(cancellationToken);

			var requestData = CreateFuelApiRequestData(FuelApiRequestFunction.FuelCardsLimitsDelete);

			try
			{
				var isRemoved = await _fuelLimitsManagementService.RemoveFuelLimitById(
					limitId,
					sessionId,
					_userSettingsService.Settings.FuelControlApiKey,
					cancellationToken);

				requestData.ResponseResult = FuelApiResponseResult.Success;

				return isRemoved;
			}
			catch(Exception ex)
			{
				requestData.ResponseResult = FuelApiResponseResult.Error;
				requestData.ErrorResponseMessage = GetErrorMessageFromException(ex);

				throw ex;
			}
			finally
			{
				await SaveFuelApiRequestData(requestData);
			}
		}

		public async Task<IEnumerable<string>> SetFuelLimit(FuelLimit fuelLimit, CancellationToken cancellationToken)
		{
			var sessionId = await GetSessionIdOrLogin(cancellationToken);

			var requestData = CreateFuelApiRequestData(FuelApiRequestFunction.FuelCardsLimitCreate);

			try
			{
				var createdLimits = await _fuelLimitsManagementService.SetFuelLimit(
					fuelLimit,
					sessionId,
					_userSettingsService.Settings.FuelControlApiKey,
					cancellationToken);

				requestData.ResponseResult = FuelApiResponseResult.Success;

				return createdLimits;
			}
			catch(Exception ex)
			{
				requestData.ResponseResult = FuelApiResponseResult.Error;
				requestData.ErrorResponseMessage = GetErrorMessageFromException(ex);

				throw ex;
			}
			finally
			{
				await SaveFuelApiRequestData(requestData);
			}
		}

		private async Task<IEnumerable<FuelCard>> GetAllCardsFromFuelControlService(string sessionId, CancellationToken cancellationToken)
		{
			var cards = new List<FuelCard>();
			var pageLimit = _fuelControlSettings.TransactionsPerQueryLimit;
			var pageOffset = 0;
			var cardsInResponseCount = 0;

			if(pageLimit <= 0)
			{
				throw new InvalidOperationException("Значение лимита возвращаемого количества транзакций за один запрос должно быть больше нуля");
			}

			_logger.LogDebug("Запрашиваем по API Газпром данные по всем имеющимся топливным картам компании");

			do
			{
				var cardsSet = await GetCardsSetFromFuelControlService(sessionId, pageLimit, pageOffset, cancellationToken);

				cardsInResponseCount = cardsSet.Count();

				cards.AddRange(cardsSet);

				pageOffset += pageLimit;
			}
			while(cardsInResponseCount == pageLimit);

			_logger.LogDebug("Получены данные по {CardsCount} картам",
				cards.Count);

			return cards;
		}

		private async Task<IEnumerable<FuelCard>> GetCardsSetFromFuelControlService(
			string sessionId,
			int pageLimit,
			int pageOffset,
			CancellationToken cancellationToken)
		{
			var request = CreateFuelApiRequestData(FuelApiRequestFunction.FuelCardsData);

			try
			{
				var cardsSet = await _fuelCardsDataService.GetFuelCards(
						sessionId,
						_userSettingsService.Settings.FuelControlApiKey,
						cancellationToken,
						pageLimit,
						pageOffset
						);

				request.ResponseResult = FuelApiResponseResult.Success;

				return cardsSet;
			}
			catch(Exception ex)
			{
				request.ResponseResult = FuelApiResponseResult.Error;
				request.ErrorResponseMessage = GetErrorMessageFromException(ex);

				throw ex;
			}
			finally
			{
				await SaveFuelApiRequestData(request);
			}
		}

		public async Task SetProductRestrictionsAndRemoveExistingByCardId(
			string cardId,
			CancellationToken cancellationToken,
			IEnumerable<string> productGroupIds = null)
		{
			productGroupIds = productGroupIds ?? Enumerable.Empty<string>();

			var existingRestrictionIds =
				await GetProductRestrictionByCardId(cardId, cancellationToken);

			foreach(var restrictionId in existingRestrictionIds)
			{
				await RemoveProductRestrictionById(restrictionId, cancellationToken);
			}

			if(!productGroupIds.Any())
			{
				await SetProductRestriction(cardId, cancellationToken);
				return;
			}

			foreach(var productGroupId in productGroupIds)
			{
				await SetProductRestriction(cardId, cancellationToken, productGroupId);
			}
		}

		private async Task<IEnumerable<string>> GetProductRestrictionByCardId(
			string cardId,
			CancellationToken cancellationToken)
		{
			var sessionId = await GetSessionIdOrLogin(cancellationToken);

			var requestData = CreateFuelApiRequestData(FuelApiRequestFunction.None);

			try
			{
				var restrictions = await _productRestrictionService.GetProductRestrictionsByCardId(
					cardId,
					sessionId,
					_userSettingsService.Settings.FuelControlApiKey,
					cancellationToken);

				requestData.ResponseResult = FuelApiResponseResult.Success;

				return restrictions;
			}
			catch(Exception ex)
			{
				requestData.ResponseResult = FuelApiResponseResult.Error;
				requestData.ErrorResponseMessage = GetErrorMessageFromException(ex);

				throw;
			}
			finally
			{
				await SaveFuelApiRequestData(requestData);
			}
		}

		private async Task<bool> RemoveProductRestrictionById(string restrictionId, CancellationToken cancellationToken)
		{
			var sessionId = await GetSessionIdOrLogin(cancellationToken);

			var requestData = CreateFuelApiRequestData(FuelApiRequestFunction.None);

			try
			{
				var isRemoved = await _productRestrictionService.RemoveProductRestictionById(
					restrictionId,
					sessionId,
					_userSettingsService.Settings.FuelControlApiKey,
					cancellationToken);

				requestData.ResponseResult = FuelApiResponseResult.Success;

				return isRemoved;
			}
			catch(Exception ex)
			{
				requestData.ResponseResult = FuelApiResponseResult.Error;
				requestData.ErrorResponseMessage = GetErrorMessageFromException(ex);

				throw;
			}
			finally
			{
				await SaveFuelApiRequestData(requestData);
			}
		}

		private async Task<IEnumerable<long>> SetProductRestriction(
			string cardId,
			CancellationToken cancellationToken,
			string productGroupId = null)
		{
			var sessionId = await GetSessionIdOrLogin(cancellationToken);

			var requestData = CreateFuelApiRequestData(FuelApiRequestFunction.None);

			var createdRestrictions = Enumerable.Empty<long>();

			try
			{
				if(string.IsNullOrWhiteSpace(productGroupId))
				{
					createdRestrictions = await _productRestrictionService.SetCommonFuelRestriction(
						cardId,
						sessionId,
						_userSettingsService.Settings.FuelControlApiKey,
						cancellationToken);
				}
				else
				{
					createdRestrictions = await _productRestrictionService.SetFuelProductGroupRestriction(
						cardId,
						productGroupId,
						sessionId,
						_userSettingsService.Settings.FuelControlApiKey,
						cancellationToken);
				}

				requestData.ResponseResult = FuelApiResponseResult.Success;

				return createdRestrictions;
			}
			catch(Exception ex)
			{
				requestData.ResponseResult = FuelApiResponseResult.Error;
				requestData.ErrorResponseMessage = GetErrorMessageFromException(ex);

				throw;
			}
			finally
			{
				await SaveFuelApiRequestData(requestData);
			}
		}

		private async Task<string> GetSessionIdOrLogin(CancellationToken cancellationToken)
		{
			var userSettings = _userSettingsService.Settings;

			if(!userSettings.IsNeedToLoginFuelControlApi)
			{
				return userSettings.FuelControlApiSessionId;
			}

			_logger.LogDebug("Необходима авторизация в сервисе API управления топливными картами");

			var session = await LoginAndSaveSessionData(cancellationToken);

			return session.SessionId;
		}

		private async Task<(string SessionId, DateTime SessionExpirationDate)> LoginAndSaveSessionData(
			CancellationToken cancellationToken)
		{
			var userSettings = _userSettingsService.Settings;

			var session = await Login(
				userSettings.FuelControlApiLogin,
				userSettings.FuelControlApiPassword,
				userSettings.FuelControlApiKey,
				cancellationToken);

			await SaveFuelControlApiSessionData(session.SessionId, session.SessionExpirationDate);

			return session;
		}

		private async Task SaveFuelControlApiSessionData(string sessionId, DateTime sessionExpirationDate)
		{
			using(var uow = _unitOfWorkFactory.CreateForRoot<UserSettings>(_userSettingsService.Settings.Id))
			{
				uow.Root.FuelControlApiSessionId = sessionId;
				uow.Root.FuelControlApiSessionExpirationDate = sessionExpirationDate;

				await uow.SaveAsync();
			}
		}

		private FuelApiRequest CreateFuelApiRequestData(FuelApiRequestFunction requestFunction)
		{
			return new FuelApiRequest
			{
				RequestDateTime = DateTime.Now,
				Author = _userSettingsService.Settings.User,
				RequestFunction = requestFunction
			};
		}

		private async Task SaveFuelApiRequestData(FuelApiRequest fuelApiRequest)
		{
			if(fuelApiRequest is null)
			{
				throw new ArgumentNullException(nameof(fuelApiRequest));
			}

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("Сохранение информации о запросе к API управления топливом"))
			{
				await _fuelRepository.SaveFuelApiRequest(uow, fuelApiRequest);
			}
		}

		private string GetErrorMessageFromException(Exception ex)
		{
			var errorMessage =
				ex.Message.Length > _savingErrorMessageMaxLength
				? ex.Message.Substring(0, _savingErrorMessageMaxLength)
				: ex.Message;

			return errorMessage;
		}
	}
}
