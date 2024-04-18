using FuelControl.Library.Services;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Fuel;
using Vodovoz.Services;
using Vodovoz.Services.Fuel;
using Vodovoz.Settings.Fuel;

namespace Vodovoz.ViewModels.Infrastructure.Services.Fuel
{
	public class FuelApiService : IFuelApiService
	{
		private readonly ILogger<FuelApiService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IFuelControlAuthorizationService _fuelControlAuthorizationService;
		private readonly IFuelControlFuelCardsDataService _fuelCardsDataService;
		private readonly IUserSettingsService _userSettingsService;
		private readonly IFuelControlSettings _fuelControlSettings;

		public FuelApiService(
			IUnitOfWorkFactory unitOfWorkFactory,
			ILogger<FuelApiService> logger,
			IFuelControlAuthorizationService fuelControlAuthorizationService,
			IFuelControlFuelCardsDataService fuelCardsDataService,
			IUserSettingsService userSettingsService,
			IFuelControlSettings fuelControlSettings)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fuelControlAuthorizationService = fuelControlAuthorizationService ?? throw new ArgumentNullException(nameof(fuelControlAuthorizationService));
			_fuelCardsDataService = fuelCardsDataService ?? throw new ArgumentNullException(nameof(fuelCardsDataService));
			_userSettingsService = userSettingsService ?? throw new ArgumentNullException(nameof(userSettingsService));
			_fuelControlSettings = fuelControlSettings ?? throw new ArgumentNullException(nameof(fuelControlSettings));
		}

		public async Task<(string SessionId, DateTime SessionExpirationDate)> Login(CancellationToken cancellationToken)
		{
			var userSettings = _userSettingsService.Settings;

			if(!userSettings.IsUserHasAuthDataForFuelControlApi)
			{
				throw new ArgumentException("У текущего пользователя не указаны данные для авторизации в сервисе API управления топливом");
			}

			try
			{
				var session = await _fuelControlAuthorizationService.Login(
						userSettings.FuelControlApiLogin,
						userSettings.FuelControlApiPassword,
						userSettings.FuelControlApiKey,
						cancellationToken);

				return session;
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}

		public async Task<IEnumerable<FuelCard>> GetFuelCardsData(CancellationToken cancellationToken)
		{
			var sessionId = await GetSessionIdOrLogin(cancellationToken);

			var allCards = await GetAllCardsFromFuelControlService(sessionId, cancellationToken);

			return allCards;
		}

		private async Task<IEnumerable<FuelCard>> GetAllCardsFromFuelControlService(string sessionId, CancellationToken cancellationToken)
		{
			var cards = new List<FuelCard>();
			var pageLimit = _fuelControlSettings.TransactionsPerQueryLimit;
			var pageOffset = 0;
			var cardsInResponseCount = 0;

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
			try
			{
				var cardsSet = await _fuelCardsDataService.GetFuelCards(
						sessionId,
						_userSettingsService.Settings.FuelControlApiKey,
						cancellationToken,
						pageLimit,
						pageOffset
						);

				return cardsSet;
			}
			catch(Exception ex )
			{
				throw ex;
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
			var session = await Login(cancellationToken);

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
	}
}
