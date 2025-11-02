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
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Fuel;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.Services;
using Vodovoz.Services.Fuel;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.Fuel.FuelCards
{
	public class FuelCardViewModel : EntityTabViewModelBase<FuelCard>
	{
		private readonly ILogger<FuelCardViewModel> _logger;
		private readonly IFuelApiService _fuelApiService;
		private readonly IFuelRepository _fuelRepository;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly IUserSettingsService _userSettingsService;
		private CancellationTokenSource _cancellationTokenSource;
		private bool _isCardIdObtainingProcessInWork;

		public FuelCardViewModel(
			ILogger<FuelCardViewModel> logger,
			IFuelApiService fuelApiService,
			IFuelRepository fuelRepository,
			IGuiDispatcher guiDispatcher,
			IUserSettingsService userSettingsService,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fuelApiService = fuelApiService ?? throw new ArgumentNullException(nameof(fuelApiService));
			_fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_userSettingsService = userSettingsService ?? throw new ArgumentNullException(nameof(userSettingsService));

			if(!CanRead)
			{
				AbortOpening("У вас недостаточно прав для просмотра");
			}

			TabName =
				UoWGeneric.IsNew
				? $"Диалог создания {Entity.GetType().GetClassUserFriendlyName().Genitive}"
				: $"{Entity.GetType().GetClassUserFriendlyName().Nominative.CapitalizeSentence()} №{Entity.Title}";

			SaveCommand = new DelegateCommand(() => Save(true));
			CancelCommand = new DelegateCommand(() => Close(false, CloseSource.Cancel));
			GetCardIdCommand = new DelegateCommand(async () => await SetCardId(), () => IsCanSetCardId);
			ResetFuelCardIdCommand = new DelegateCommand(ResetFuelCardId);

			Entity.PropertyChanged += OnEntityPropertyChanged;
		}

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CancelCommand { get; }
		public DelegateCommand GetCardIdCommand { get; }
		public DelegateCommand ResetFuelCardIdCommand { get; }

		[PropertyChangedAlso(nameof(IsCanSetCardId))]
		public bool IsCardIdObtainingProcessInWork
		{
			get => _isCardIdObtainingProcessInWork;
			set => SetField(ref _isCardIdObtainingProcessInWork, value);
		}

		public bool IsCanSetCardId =>
			Entity.IsCardNumberValid
			&& !IsCardIdObtainingProcessInWork;

		#region Permissions

		public bool CanCreate => PermissionResult.CanCreate;
		public bool CanRead => PermissionResult.CanRead;
		public bool CanUpdate => PermissionResult.CanUpdate;
		public bool CanDelete => PermissionResult.CanDelete;

		public bool CanCreateOrUpdate => Entity.Id == 0 ? CanCreate : CanUpdate;

		#endregion

		private async Task SetCardId()
		{
			if(_fuelRepository.GetFuelCardsByCardNumber(UoW, Entity.CardNumber).Any(c => c.Id != Entity.Id))
			{
				ShowMessageInGuiThread(
					ImportanceLevel.Error,
					"В базе уже сохранена карта с указанным номером.");

				return;
			}

			if(string.IsNullOrWhiteSpace(_userSettingsService.Settings.FuelControlApiLogin)
				|| string.IsNullOrWhiteSpace(_userSettingsService.Settings.FuelControlApiPassword)
				|| string.IsNullOrWhiteSpace(_userSettingsService.Settings.FuelControlApiKey))
			{
				ShowMessageInGuiThread(
					ImportanceLevel.Error,
					"У Вас не указаны данные для авторизации в сервисе Газпром");

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
				var fuelCards = await _fuelApiService.GetFuelCardsData(_cancellationTokenSource.Token);

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

		private void ResetFuelCardId()
		{
			Entity.CardId = string.Empty;
		}

		public override void Dispose()
		{
			_cancellationTokenSource?.Dispose();
			_cancellationTokenSource = null;

			base.Dispose();
		}
	}
}
