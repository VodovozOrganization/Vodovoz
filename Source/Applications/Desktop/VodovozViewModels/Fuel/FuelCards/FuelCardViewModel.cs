using FuelControl.Library.Services;
using Microsoft.Extensions.Logging;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
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
		private readonly IFuelControlAuthorizationService _fuelControlAuthorizationService;
		private readonly IFuelControlFuelCardsDataService _fuelCardsDataService;
		private readonly IUserSettingsService _userSettingsService;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IFuelControlSettings _fuelControlSettings;

		private CancellationTokenSource _cancellationTokenSource;
		private bool _isCardIdObtainingProcessInWork;

		public FuelCardViewModel(
			ILogger<FuelCardViewModel> logger,
			IFuelRepository fuelRepository,
			IFuelControlAuthorizationService fuelControlAuthorizationService,
			IFuelControlFuelCardsDataService fuelCardsDataService,
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
			_fuelControlAuthorizationService = fuelControlAuthorizationService ?? throw new ArgumentNullException(nameof(fuelControlAuthorizationService));
			_fuelCardsDataService = fuelCardsDataService ?? throw new ArgumentNullException(nameof(fuelCardsDataService));
			_userSettingsService = userSettingsService ?? throw new ArgumentNullException(nameof(userSettingsService));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_fuelControlSettings = fuelControlSettings ?? throw new ArgumentNullException(nameof(fuelControlSettings));
			TabName =
				UoWGeneric.IsNew
				? $"Диалог создания {Entity.GetType().GetClassUserFriendlyName().Genitive}"
				: $"{Entity.GetType().GetClassUserFriendlyName().Nominative.CapitalizeSentence()} №{Entity.Title}";

			SaveCommand = new DelegateCommand(() => Save(true));
			CancelCommand = new DelegateCommand(() => Close(false, CloseSource.Cancel));
			GetCardIdCommand = new DelegateCommand(async () => await SetCardId(), () => IsCanSetCardId);

			ValidationContext.ServiceContainer.AddService(typeof(IUnitOfWorkFactory), unitOfWorkFactory);
			ValidationContext.ServiceContainer.AddService(typeof(IFuelRepository), fuelRepository);

			Entity.PropertyChanged += OnEntityPropertyChanged;
		}

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CancelCommand { get; }
		public DelegateCommand GetCardIdCommand { get; }

		[PropertyChangedAlso(nameof(IsCanSetCardId))]
		public bool IsCardIdObtainingProcessInWork
		{
			get => _isCardIdObtainingProcessInWork;
			set => SetField(ref _isCardIdObtainingProcessInWork, value);
		}

		public bool IsCanSetCardId =>
			Entity.IsCardNumberValid
			&& !IsCardIdObtainingProcessInWork;

		private async Task SetCardId()
		{
			if(_fuelRepository.GetFuelCardsByCardNumber(UoW, Entity.CardNumber).Any(c => c.Id != Entity.Id))
			{
				ShowMessageInGuiThread(
					ImportanceLevel.Error,
					"В базе уже сохранена карта с указанным номером.");

				return;
			}

			if(IsCardIdObtainingProcessInWork)
			{
				ShowMessageInGuiThread(
					ImportanceLevel.Error,
					"Получение значения Id карты уже запущено. Необходимо дождаться окончания процесса.");

				return;
			}

			var cardId = await GetCardId();

			Entity.CardId = cardId;
		}

		private async Task<string> GetCardId()
		{
			if(_cancellationTokenSource != null)
			{
				throw new InvalidOperationException("Получение значения Id карты уже запущено.");
			}

			IsCardIdObtainingProcessInWork = true;
			_cancellationTokenSource = new CancellationTokenSource();

			try
			{
				var sessionId = await GetSessionId(_cancellationTokenSource.Token);

				var fuelCards = await GetAllCardsFromFuelControlService(sessionId, _cancellationTokenSource.Token);

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
			finally
			{
				_cancellationTokenSource.Dispose();
				_cancellationTokenSource = null;

				IsCardIdObtainingProcessInWork = false;
			}

			return string.Empty;
		}

		private async Task<string> GetSessionId(CancellationToken cancellationToken)
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

			var sessionId = await _fuelControlAuthorizationService.Login(
					userSettings.FuelControlApiLogin,
					userSettings.FuelControlApiPassword,
					userSettings.FuelControlApiKey,
					cancellationToken);

			await SaveFuelControlApiSessionData(sessionId);

			return sessionId;
		}

		private async Task SaveFuelControlApiSessionData(string sessionId)
		{
			using(var uow = UnitOfWorkFactory.CreateForRoot<UserSettings>(_userSettingsService.Settings.Id))
			{
				uow.Root.FuelControlApiSessionId = sessionId;
				uow.Root.FuelControlApiSessionExpirationDate = DateTime.Today.AddDays(_fuelControlSettings.ApiSessionLifetime.TotalDays);

				await uow.SaveAsync();
			}
		}

		private async Task<IEnumerable<FuelCard>> GetAllCardsFromFuelControlService(string sessionId, CancellationToken cancellationToken)
		{
			var cards = new List<FuelCard>();

			var cardsInResponseCount = 0;
			var pageLimit = _fuelControlSettings.TransactionsPerQueryLimit;
			var pageOffset = 0;

			_logger.LogDebug("Запрашиваем по API Газпром данные по всем имеющимся топливным картам компании");

			do
			{
				var cardsSet = await _fuelCardsDataService.GetFuelCards(
					sessionId,
					_userSettingsService.Settings.FuelControlApiKey,
					cancellationToken,
					pageLimit,
					pageOffset
					);

				cardsInResponseCount = cardsSet.Count();

				cards.AddRange(cardsSet);

				pageOffset += pageLimit;
			}
			while(cardsInResponseCount == pageLimit);

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

		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.CardNumber))
			{
				OnPropertyChanged(nameof(IsCanSetCardId));
			}
		}

		public override void Dispose()
		{
			_cancellationTokenSource?.Dispose();
			_cancellationTokenSource = null;

			base.Dispose();
		}
	}
}
