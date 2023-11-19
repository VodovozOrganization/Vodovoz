using Microsoft.Extensions.Logging;
using Pacs.Admin.Client;
using Pacs.Core.Messages.Events;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using System;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Domain.Employees;
using Vodovoz.Services;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsDomainSettingsViewModel : WidgetViewModelBase, IObserver<SettingsEvent>
	{
		private readonly ILogger<PacsDomainSettingsViewModel> _logger;
		private readonly Employee _employee;
		private readonly AdminClient _adminClient;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly bool _canEdit;

		private bool _saveInProgress;
		private IDisposable _settingsSubscription;
		private IPacsDomainSettings _originalSettings;
		private IPacsDomainSettings _settingsBeforeChange;
		private int _maxBreakTime;
		private int _maxOperatorsOnBreak;

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CancelCommand { get; }

		public PacsDomainSettingsViewModel(
			ILogger<PacsDomainSettingsViewModel> logger,
			IEmployeeService employeeService,
			IPermissionService permissionService,
			AdminClient adminClient, 
			IObservable<SettingsEvent> settingsPublisher, 
			IGuiDispatcher guiDispatcher)
		{
			if(permissionService is null)
			{
				throw new ArgumentNullException(nameof(permissionService));
			}

			if(settingsPublisher is null)
			{
				throw new ArgumentNullException(nameof(settingsPublisher));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_adminClient = adminClient ?? throw new ArgumentNullException(nameof(adminClient));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));

			_employee = employeeService.GetEmployeeForCurrentUser();
			if(_employee == null)
			{
				throw new AbortCreatingPageException(
					"Должен быть привязан сотрудник к пользователю. Обратитесь в отдел кадров.",
					"Не настроен пользователь");
			}

			_canEdit = permissionService.ValidateUserPresetPermission(Permissions.Pacs.IsAdministrator, _employee.User.Id);

			SaveCommand = new DelegateCommand(() => Save().Wait(), () => CanSave);
			SaveCommand.CanExecuteChangedWith(this, x => x.CanSave);

			CancelCommand = new DelegateCommand(Cancel, () => HasChanges);
			CancelCommand.CanExecuteChangedWith(this, x => x.HasChanges);

			_originalSettings = _adminClient.GetSettings().Result;
			ResetSettings();

			_settingsSubscription = settingsPublisher.Subscribe(this);
		}

		public bool CanEdit => _canEdit;

		public bool HasChanges => CanEdit && _originalSettings != null
			&& MaxBreakTime != (int)_originalSettings.MaxBreakTime.TotalMinutes
			&& MaxOperatorsOnBreak != _originalSettings.MaxOperatorsOnBreak
			;

		public bool HasExternalChanges => _settingsBeforeChange != null 
			&&_settingsBeforeChange != _originalSettings;

		public bool CanSave => HasChanges && !_saveInProgress && !HasExternalChanges;

		#region Settings

		[PropertyChangedAlso(nameof(HasChanges))]
		public virtual int MaxBreakTime
		{
			get => _maxBreakTime;
			set
			{
				StartTrackChanges();
				SetField(ref _maxBreakTime, value);
			}
		}

		public int MaxBreakTimeMinValue => 5;
		public int MaxBreakTimeMaxValue => 1440;

		[PropertyChangedAlso(nameof(HasChanges))]
		public virtual int MaxOperatorsOnBreak
		{
			get => _maxOperatorsOnBreak;
			set
			{
				StartTrackChanges();
				SetField(ref _maxOperatorsOnBreak, value);
			}
		}
		public int MaxOperatorsOnBreakMinValue => 0;
		public int MaxOperatorsOnBreakMaxValue => 500;

		#endregion Settings

		#region Settings subscription

		public void OnCompleted()
		{
			_settingsSubscription.Dispose();
			_settingsSubscription = null;
		}

		public void OnError(Exception error)
		{
			_logger.LogError(error, "Возникло исключение в процессе уведомления об изменении настроек СКУД");
		}

		public void OnNext(SettingsEvent value)
		{
			_guiDispatcher.RunInGuiTread(() => {
				_originalSettings = value.Settings;
				OnPropertyChanged(nameof(HasChanges));
				OnPropertyChanged(nameof(CanSave));
				OnPropertyChanged(nameof(HasExternalChanges));
			});
		}

		#endregion Settings subscription

		private async Task Save()
		{
			try
			{
				_saveInProgress = true;

				var newSettings = new PacsDomainSettings
				{
					AdministratorId = _employee.Id,
					Timestamp = DateTime.Now,
					MaxBreakTime = TimeSpan.FromMinutes(MaxBreakTime),
					MaxOperatorsOnBreak = MaxOperatorsOnBreak,
				};

				await _adminClient.SetSettings(newSettings);
			}
			finally
			{
				_saveInProgress = false;
				StopTrackChanges();
				_guiDispatcher.RunInGuiTread(() =>
				{
					OnPropertyChanged(nameof(HasChanges));
					OnPropertyChanged(nameof(CanSave));
					OnPropertyChanged(nameof(HasExternalChanges));
				});
			}
		}

		public void Cancel()
		{
			ResetSettings();
		}

		private void ResetSettings()
		{
			_maxBreakTime = (int)_originalSettings.MaxBreakTime.TotalMinutes;
			_maxOperatorsOnBreak = _originalSettings.MaxOperatorsOnBreak;
			StopTrackChanges();
			OnPropertyChanged(nameof(HasChanges));
			OnPropertyChanged(nameof(HasExternalChanges));
		}

		private void StartTrackChanges()
		{
			if(_settingsBeforeChange == null)
			{
				_settingsBeforeChange = _originalSettings;
			}
		}

		private void StopTrackChanges()
		{
			_settingsBeforeChange = null;
		}
	}
}
