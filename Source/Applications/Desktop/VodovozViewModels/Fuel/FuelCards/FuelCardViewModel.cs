using FuelControl.Library.Services;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Fuel;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.Services;
using Vodovoz.Settings.Fuel;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.Fuel.FuelCards
{
	public class FuelCardViewModel : EntityTabViewModelBase<FuelCard>
	{
		private readonly ILogger<FuelCardViewModel> _logger;
		private readonly IFuelRepository _fuelRepository;
		private readonly IFuelManagmentAuthorizationService _fuelManagmentAuthorizationService;
		private readonly IFuelCardsGeneralInfoService _fuelCardsGeneralInfoService;
		private readonly IUserSettingsService _userSettingsService;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IFuelControlSettings _fuelControlSettings;

		public FuelCardViewModel(
			ILogger<FuelCardViewModel> logger,
			IFuelRepository fuelRepository,
			IFuelManagmentAuthorizationService fuelManagmentAuthorizationService,
			IFuelCardsGeneralInfoService fuelCardsGeneralInfoService,
			IUserSettingsService userSettingsService,
			IGuiDispatcher guiDispatcher,
			IFuelControlSettings fuelControlSettings,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation = null)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			_fuelManagmentAuthorizationService = fuelManagmentAuthorizationService ?? throw new ArgumentNullException(nameof(fuelManagmentAuthorizationService));
			_fuelCardsGeneralInfoService = fuelCardsGeneralInfoService ?? throw new ArgumentNullException(nameof(fuelCardsGeneralInfoService));
			_userSettingsService = userSettingsService ?? throw new ArgumentNullException(nameof(userSettingsService));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_fuelControlSettings = fuelControlSettings ?? throw new ArgumentNullException(nameof(fuelControlSettings));
			TabName =
				UoWGeneric.IsNew
				? $"Диалог создания {Entity.GetType().GetClassUserFriendlyName().Genitive}"
				: $"{Entity.GetType().GetClassUserFriendlyName().Nominative.CapitalizeSentence()} №{Entity.Title}";

			SaveCommand = new DelegateCommand(() => Save(true));
			CancelCommand = new DelegateCommand(() => Close(false, CloseSource.Cancel));
			GetCardIdCommand = new DelegateCommand(async () => await SetCardId(), () => Entity.IsCardNumberValid);

			ValidationContext.ServiceContainer.AddService(typeof(IUnitOfWorkFactory), unitOfWorkFactory);
			ValidationContext.ServiceContainer.AddService(typeof(IFuelRepository), fuelRepository);
		}

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CancelCommand { get; }
		public DelegateCommand GetCardIdCommand { get; }

		private async Task SetCardId()
		{
			if(_fuelRepository.GetFuelCardsByCardNumber(UoW, Entity.CardNumber).Any(c => c.Id != Entity.Id))
			{
				ShowMessageInGuiThread(
					ImportanceLevel.Error,
					"В базе уже сохранена карта с указанным номером.");

				return;
			}

			var cardId = await GetCardId();

			Entity.CardId = cardId;
		}

		private async Task<string> GetCardId()
		{
			try
			{
				var sessionId = await GetSessionId();

				var fuelCards = await GetAllCardsFromFuelControlService(sessionId);

				var card = fuelCards.Where(c => c.CardNumber == Entity.CardNumber).FirstOrDefault();

				if(card is null)
				{
					var errorMessage = $"Карта с номером {Entity.CardNumber} не найдена среди карт договора компании на сервере Газпром";

					_logger.LogError(errorMessage);
					ShowMessageInGuiThread(ImportanceLevel.Error, errorMessage);
				}

				return card?.CardId ?? string.Empty;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message);
				ShowMessageInGuiThread(ImportanceLevel.Error, ex.Message);
			}

			return string.Empty;
		}

		private async Task<string> GetSessionId()
		{
			var userSettings = _userSettingsService.Settings;

			if(!userSettings.IsUserHasAuthDataForFuelControlApi)
			{
				throw new ArgumentException("У текущего пользователя не указаны данные для авторизации в сервисе Газпром");
			}

			if(!userSettings.IsNeedToLoginFuelControlApi)
			{
				return userSettings.FuelControlApiSessionId;
			}

			_logger.LogDebug("Необходима авторизация в сервисе API управления топливными картами");

			var sessionId = await _fuelManagmentAuthorizationService.Login(
					userSettings.FuelControlApiLogin,
					userSettings.FuelControlApiPassword,
					userSettings.FuelControlApiKey);

			using(var uow = UnitOfWorkFactory.CreateForRoot<UserSettings>(userSettings.Id))
			{
				uow.Root.FuelControlApiSessionId = sessionId;
				uow.Root.FuelControlApiSessionExpirationDate = DateTime.Today.AddDays(_fuelControlSettings.ApiSessionLifetime.TotalDays);

				await uow.SaveAsync();
			}

			return sessionId;
		}

		private async Task<IEnumerable<FuelCard>> GetAllCardsFromFuelControlService(string sessionId)
		{
			var cards = new List<FuelCard>();

			var cardsSetCount = 0;
			var pageLimit = _fuelControlSettings.TransactionsPerQueryLimit;
			var pageOffset = 0;

			_logger.LogDebug("Запрашиваем по API Газпром данные по всем имеющимся топливным картам компании");

			do
			{
				var cardsSet = await _fuelCardsGeneralInfoService.GetFuelCards(
				sessionId,
				_userSettingsService.Settings.FuelControlApiKey);

				cardsSetCount = cardsSet.Count();

				cards.AddRange(cardsSet);

				pageOffset += pageLimit;
			}
			while(cardsSetCount == pageLimit);

			_logger.LogDebug("Получены данные по {CardsCount} картам",
				cards.Count);

			return cards;
		}

		private void ShowMessageInGuiThread(ImportanceLevel level, string message)
		{
			_guiDispatcher.RunInGuiTread(() =>
			{
				CommonServices.InteractiveService.ShowMessage(level, message);
			});
		}
	}
}
